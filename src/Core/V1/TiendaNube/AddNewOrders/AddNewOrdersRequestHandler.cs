using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using Core.RequestsHTTP.Models.TiendaNube;
using Core.Shared;
using Core.V1.TiendaNube.AddNewOrdersToDatabase;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Core.V1.TiendaNube.GetBranchOffices;
using System.Net.Http;

namespace Core.V1.TiendaNube.AddNewOrders
{
    public class AddNewOrdersRequestHandler : IRequestHandler<AddNewOrderProcNextRequest, OrderStatus>
    {
        private readonly UserManager<User> userManager;
        private readonly ITiendaNubeRequestService tiendaNubeRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly IOrderRepository orderRepository;
        private readonly IProvinceRepository provinceRepository;
        private readonly IShippingTypeRepository shippingTypeRepository;
        private readonly ITNOrderStatusRepository tNOrderStatusRepository;
        private readonly IMediator mediator;

        private readonly ILogger logger;

        private static Object _lock = new Object();

        protected class OPStore
        {
            public OPStore(Seller dbseller)
            {
                id = dbseller.Id;
                access_token = dbseller.TNAccessToken;
                code = dbseller.TNCode;
                causerid = dbseller.CAUserId;
                createdOn = dbseller.CreatedOn;
                dtused = DateTime.Now;
            }

            public Guid id { get; set; }
            public String access_token { get; set; }
            public String code { get; set; }
            public String causerid { get; set; }
            public DateTimeOffset createdOn { get; set; }
            public DateTime dtused { get; set; }
        }


        private static OrderStatus _OSProcessing = new OrderStatus(null, false, "process...");

        private static DateTime _lastClear = DateTime.Now;
        private static ConcurrentDictionary<int, OPStore> _stores = new ConcurrentDictionary<int, OPStore>();

        private static int _cron_checkrun_sec               = 60 * 10;  // 10 minutes
        private static int _order_maxtime_inmem_sec         = 60 * 1;   //  1 minute
        private static int _max_attempt_tnb_get_orders      = 3;
        private static int _sleep_when_tnb_get_orders_fail  = 500;      // ms
        private static int _sleep_when_tnb_get_orders_exception = 10000; // 10s


        private static ConcurrentDictionary<Guid, bool> _sellers_updateToken = new ConcurrentDictionary<Guid, bool>();



        public static int get_importorders_stores()
        {
            return _stores.Count;
        }

        public static DateTime get_last_clear()
        {
            return _lastClear;
        }

        public static int get_sellers_updateToken()
        {
            return _sellers_updateToken.Count;
        }


        protected class order_request
        {
            public order_request(int strId, int ordId, OPStore sllr)
            {
                storeId = strId;
                id = ordId;
                seller = sllr;
            }

            public int storeId { get; set; }
            public int id { get; set; }
            public OPStore seller { get; set; }
        };

        public AddNewOrdersRequestHandler(
            UserManager<User> userManager,
            ITiendaNubeRequestService tiendaNubeRequestService,
            ISellerRepository sellerRepository,
            IApiMiCorreo correoArgentinoRequestService,
            IOrderRepository orderRepository,
            IProvinceRepository provinceRepository,
            IShippingTypeRepository shippingTypeRepository,
            ITNOrderStatusRepository tNOrderStatusRepository,
            IMediator mediator,
            ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.tiendaNubeRequestService = tiendaNubeRequestService ?? throw new ArgumentNullException(nameof(tiendaNubeRequestService));
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.correoArgentinoRequestService = correoArgentinoRequestService ?? throw new ArgumentNullException(nameof(correoArgentinoRequestService));
            this.orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
            this.provinceRepository = provinceRepository ?? throw new ArgumentNullException(nameof(provinceRepository));
            this.shippingTypeRepository = shippingTypeRepository ?? throw new ArgumentNullException(nameof(shippingTypeRepository));
            this.tNOrderStatusRepository = tNOrderStatusRepository ?? throw new ArgumentNullException(nameof(tNOrderStatusRepository));
            this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }



        private static void cron_clearMemory(ILogger logger)
        {
            // check more than _cron_checkrun_sec seconds:
            if ((DateTime.Now - _lastClear).TotalSeconds < _cron_checkrun_sec)
                return;

            // Every _cron_checkrun_sec + 1 seconds:

            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information($"Running AddNewOrders/cron_clearMemory... last clear run: {_lastClear}");

            int count = 0;
            lock (_lock)
            {
                foreach (KeyValuePair<int, OPStore> store in _stores)
                {
                    if (_order_maxtime_inmem_sec < (DateTime.Now - store.Value.dtused).TotalSeconds)
                    {
                        ++count;
                        logger.Information($"AddNewOrders/cron_clearMemory: Deleted memory Store {store.Key} - {store.Value.causerid}");
                        OPStore val;
                        _stores.TryRemove(store.Key, out val);
                    }
                }
            }

            _lastClear = DateTime.Now;
            te.Stop();
            logger.Information($"End AddNewOrders/cron_clearMemory deleted {count} memory orders, elapsed time {te.ElapsedMilliseconds}ms");
        }

        public async Task<OrderStatus> Handle(AddNewOrderProcNextRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information($"Start service: TiendaNube/AddNewOrders-ProcessNext... ({request.storeId}, {request.orderId})");

            cron_clearMemory(logger);

            OrderStatus os = null;

            OPStore store = null;
            lock (_lock)
                _stores.TryGetValue(request.storeId, out store);

            if (store == null)
            {
                Seller seller = null;
                try
                {
                    seller = await sellerRepository.FindByTNUserIdForceAsync(request.storeId.ToString());
                }
                catch (Exception ex)
                {
                    logger.Error($"TiendaNube/AddNewOrders-ProcessNext - ({request.storeId.ToString()}, {request.orderId}) - Exception: {ex.Message}\n -> {ex.StackTrace}");
                }

                if (seller != null && logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                    logger.Debug("TiendaNube/AddNewOrders-ProcessNext - Seller: " + JsonConvert.SerializeObject(seller));
                else
                    logger.Information("TiendaNube/AddNewOrders-ProcessNext - Seller: {0}", seller != null && seller.CAUserId != null ? seller.CAUserId : "<null>");

                if (seller == null)
                {
                    te.Stop();
                    logger.Error($"End service TiendaNube/AddNewOrders-ProcessNext ({request.storeId}, {request.orderId}) elapsed time {te.ElapsedMilliseconds}ms");
                    return new OrderStatus($"id: {request.orderId}", true, "Vendedor no registrado");
                }

                store = new OPStore(seller);
                lock (_lock)
                    _stores.TryAdd(request.storeId, store);
            }
            else
                store.dtused = DateTime.Now;

            try
            {
                order_request oreq = new order_request(request.storeId, request.orderId, store);
                os = await process_order(oreq);
            }
            catch (Exception ex)
            {
                os = new OrderStatus($"id: {request.orderId}", true, $"{ex.Message}");
            }
            te.Stop();
            logger.Information("End service TiendaNube/AddNewOrders-ProcessNext {1} elapsed time {0}ms", te.ElapsedMilliseconds, request.storeId);

            return os;
        }

        protected async Task<OrderStatus> process_order(order_request order)
        {
            logger.Information("Starting new task process_order for store id ({0], {1})", order.storeId, order.id);

            OrderStatus sProc = null;

            OrderResponseModel tnorder = null;
            try
            {
                for (int nTry = 0; nTry < _max_attempt_tnb_get_orders && (tnorder == null || tnorder.code != 0); ++nTry)
                {
                    try
                    {
                        tnorder = await tiendaNubeRequestService.GetOrderById($"process_order ({order.storeId},{order.id})", order.seller.access_token, order.storeId.ToString(), order.id.ToString());

                        // TODO: Inject json file -- > REMOVE
                        // var json = "{\n    \"id\": 717752809,\n    \"token\": \"7bc5c6458e1f6d79ba887c343144595ddeb174f6\",\n    \"store_id\": \"737415\",\n    \"contact_name\": \"María Candela Lallana\",\n    \"contact_phone\": \"+543424484280\",\n    \"contact_identification\": \"37333231\",\n    \"shipping_min_days\": 5,\n    \"shipping_max_days\": 6,\n    \"billing_name\": \"María\",\n    \"billing_phone\": \"+543424484280\",\n    \"billing_address\": \"San Lorenzo\",\n    \"billing_number\": \"2584\",\n    \"billing_floor\": \"\",\n    \"billing_locality\": \"\",\n    \"billing_zipcode\": \"3000\",\n    \"billing_city\": \"Santa Fe\",\n    \"billing_province\": \"Santa Fe\",\n    \"billing_country\": \"AR\",\n    \"shipping_cost_owner\": \"492.31\",\n    \"shipping_cost_customer\": \"0.00\",\n    \"coupon\": [],\n    \"promotional_discount\": {\n        \"id\": null,\n        \"store_id\": 737415,\n        \"order_id\": \"717752809\",\n        \"created_at\": \"2022-09-13T15:29:28+0000\",\n        \"total_discount_amount\": \"0.00\",\n        \"contents\": [],\n        \"promotions_applied\": []\n    },\n    \"subtotal\": \"7300.00\",\n    \"discount\": \"1095.00\",\n    \"discount_coupon\": \"0.00\",\n    \"discount_gateway\": \"1095.00\",\n    \"total\": \"6205.00\",\n    \"total_usd\": \"46.88\",\n    \"checkout_enabled\": true,\n    \"weight\": \"1.400\",\n    \"currency\": \"ARS\",\n    \"language\": \"es\",\n    \"gateway\": \"offline\",\n    \"gateway_id\": null,\n    \"gateway_name\": \"Transferencia/ Deposito bancario\",\n    \"shipping\": \"api_128000\",\n    \"shipping_option\": \"Punto de retiro\",\n    \"shipping_option_code\": \"standardPickup\",\n    \"shipping_option_reference\": \"S0000\",\n    \"shipping_pickup_details\": {\n        \"name\": \"Correo Argentino - SANTA FE\",\n        \"address\": {\n            \"address\": \"MENDOZA\",\n            \"number\": \"2430\",\n            \"floor\": null,\n            \"locality\": \"SANTA FE\",\n            \"city\": \"LA CAPITAL\",\n            \"province\": \"SANTA FE\",\n            \"zipcode\": \"S3000ZAE\",\n            \"country\": \"argentina\",\n            \"latitude\": \"-31.6495675\",\n            \"longitude\": \"-60.7055354\",\n            \"phone\": \"(0342) 4500852\"\n        },\n        \"hours\": [\n            {\n                \"day\": 1,\n                \"start\": \"0800\",\n                \"end\": \"1800\"\n            },\n            {\n                \"day\": 2,\n                \"start\": \"0800\",\n                \"end\": \"1800\"\n            },\n            {\n                \"day\": 3,\n                \"start\": \"0800\",\n                \"end\": \"1800\"\n            },\n            {\n                \"day\": 4,\n                \"start\": \"0800\",\n                \"end\": \"1800\"\n            },\n            {\n                \"day\": 5,\n                \"start\": \"0800\",\n                \"end\": \"1800\"\n            },\n            {\n                \"day\": 6,\n                \"start\": \"0800\",\n                \"end\": \"1400\"\n            }\n        ]\n    },\n    \"shipping_tracking_number\": \"0000655234871002C39C001\",\n    \"shipping_tracking_url\": null,\n    \"shipping_store_branch_name\": null,\n    \"shipping_pickup_type\": \"pickup\",\n    \"shipping_suboption\": {\n        \"id\": \"9579177200f68e9890cd2f1fa6da6225\"\n    },\n    \"extra\": {},\n    \"storefront\": \"store\",\n    \"note\": \"\",\n    \"created_at\": \"2022-09-12T14:37:42+0000\",\n    \"updated_at\": \"2022-09-13T12:58:58+0000\",\n    \"completed_at\": {\n        \"date\": \"2022-09-12 14:37:42.000000\",\n        \"timezone_type\": 3,\n        \"timezone\": \"UTC\"\n    },\n    \"next_action\": \"noop\",\n    \"payment_details\": {\n        \"method\": \"custom\",\n        \"credit_card_company\": null,\n        \"installments\": \"1\"\n    },\n    \"attributes\": [],\n    \"customer\": {\n        \"id\": 100830492,\n        \"name\": \"María Candela Lallana\",\n        \"email\": \"mcandelalallana@gmail.com\",\n        \"identification\": \"37333231\",\n        \"phone\": \"+543424484280\",\n        \"note\": null,\n        \"default_address\": {\n            \"address\": null,\n            \"city\": \"La Capital\",\n            \"country\": \"AR\",\n            \"created_at\": \"2022-09-12T14:37:41+0000\",\n            \"default\": true,\n            \"floor\": null,\n            \"id\": 76422356,\n            \"locality\": \"Barranquitas\",\n            \"name\": \"María Candela Lallana\",\n            \"number\": null,\n            \"phone\": \"+543424484280\",\n            \"province\": \"Santa Fe\",\n            \"updated_at\": \"2022-09-12T14:37:41+0000\",\n            \"zipcode\": \"3000\"\n        },\n        \"addresses\": [\n            {\n                \"address\": null,\n                \"city\": \"La Capital\",\n                \"country\": \"AR\",\n                \"created_at\": \"2022-09-11T13:56:31+0000\",\n                \"default\": false,\n                \"floor\": null,\n                \"id\": 76370254,\n                \"locality\": \"Barranquitas\",\n                \"name\": \"María Candela Lallana\",\n                \"number\": null,\n                \"phone\": \"\",\n                \"province\": \"Santa Fe\",\n                \"updated_at\": \"2022-09-11T13:56:31+0000\",\n                \"zipcode\": \"3000\"\n            },\n            {\n                \"address\": null,\n                \"city\": \"La Capital\",\n                \"country\": \"AR\",\n                \"created_at\": \"2022-09-12T14:37:41+0000\",\n                \"default\": true,\n                \"floor\": null,\n                \"id\": 76422356,\n                \"locality\": \"Barranquitas\",\n                \"name\": \"María Candela Lallana\",\n                \"number\": null,\n                \"phone\": \"+543424484280\",\n                \"province\": \"Santa Fe\",\n                \"updated_at\": \"2022-09-12T14:37:41+0000\",\n                \"zipcode\": \"3000\"\n            }\n        ],\n        \"billing_name\": \"María\",\n        \"billing_phone\": \"+543424484280\",\n        \"billing_address\": \"San Lorenzo\",\n        \"billing_number\": \"2584\",\n        \"billing_floor\": \"\",\n        \"billing_locality\": \"\",\n        \"billing_zipcode\": \"3000\",\n        \"billing_city\": \"Santa Fe\",\n        \"billing_province\": \"Santa Fe\",\n        \"billing_country\": \"AR\",\n        \"extra\": {},\n        \"total_spent\": \"6205.00\",\n        \"total_spent_currency\": \"ARS\",\n        \"last_order_id\": 717752809,\n        \"active\": true,\n        \"first_interaction\": \"2022-09-12T14:37:42+0000\",\n        \"created_at\": \"2022-09-11T13:56:31+0000\",\n        \"updated_at\": \"2022-09-12T15:22:54+0000\",\n        \"accepts_marketing\": true,\n        \"accepts_marketing_updated_at\": \"2022-09-11T13:55:24+0000\"\n    },\n    \"products\": [\n        {\n            \"id\": 998624935,\n            \"depth\": \"0.00\",\n            \"height\": \"0.00\",\n            \"name\": \"MUSCULOSA PADS (Blanco, 1)\",\n            \"price\": \"900.00\",\n            \"compare_at_price\": \"2250.00\",\n            \"product_id\": 61289973,\n            \"image\": {\n                \"id\": 114751593,\n                \"product_id\": 61289973,\n                \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/737/415/products/4347fc08-2aad-4ca9-bbb1-ed7b7e66ed17_nube-297ccffe70c6290b8615983953995944-1024-1024.jpg\",\n                \"position\": 1,\n                \"alt\": [],\n                \"created_at\": \"2020-08-25T22:43:24+0000\",\n                \"updated_at\": \"2020-08-25T22:43:24+0000\"\n            },\n            \"quantity\": \"1\",\n            \"free_shipping\": false,\n            \"weight\": \"0.20\",\n            \"width\": \"0.00\",\n            \"variant_id\": \"184733107\",\n            \"variant_values\": [\n                \"Blanco\",\n                \"1\"\n            ],\n            \"properties\": [],\n            \"sku\": \"MusPadBlaT1\",\n            \"barcode\": null\n        },\n        {\n            \"id\": 998625026,\n            \"depth\": \"0.00\",\n            \"height\": \"0.00\",\n            \"name\": \"MUSCULOSA PADS (Gris, 1)\",\n            \"price\": \"900.00\",\n            \"compare_at_price\": \"2250.00\",\n            \"product_id\": 61289973,\n            \"image\": {\n                \"id\": 114751593,\n                \"product_id\": 61289973,\n                \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/737/415/products/4347fc08-2aad-4ca9-bbb1-ed7b7e66ed17_nube-297ccffe70c6290b8615983953995944-1024-1024.jpg\",\n                \"position\": 1,\n                \"alt\": [],\n                \"created_at\": \"2020-08-25T22:43:24+0000\",\n                \"updated_at\": \"2020-08-25T22:43:24+0000\"\n            },\n            \"quantity\": \"1\",\n            \"free_shipping\": false,\n            \"weight\": \"0.20\",\n            \"width\": \"0.00\",\n            \"variant_id\": \"184733109\",\n            \"variant_values\": [\n                \"Gris\",\n                \"1\"\n            ],\n            \"properties\": [],\n            \"sku\": \"MusPadGriT1\",\n            \"barcode\": null\n        },\n        {\n            \"id\": 998625090,\n            \"depth\": \"0.00\",\n            \"height\": \"0.00\",\n            \"name\": \"MUSCULOSA PADS (Negro, 1)\",\n            \"price\": \"900.00\",\n            \"compare_at_price\": \"2250.00\",\n            \"product_id\": 61289973,\n            \"image\": {\n                \"id\": 114751593,\n                \"product_id\": 61289973,\n                \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/737/415/products/4347fc08-2aad-4ca9-bbb1-ed7b7e66ed17_nube-297ccffe70c6290b8615983953995944-1024-1024.jpg\",\n                \"position\": 1,\n                \"alt\": [],\n                \"created_at\": \"2020-08-25T22:43:24+0000\",\n                \"updated_at\": \"2020-08-25T22:43:24+0000\"\n            },\n            \"quantity\": \"1\",\n            \"free_shipping\": false,\n            \"weight\": \"0.20\",\n            \"width\": \"0.00\",\n            \"variant_id\": \"184733110\",\n            \"variant_values\": [\n                \"Negro\",\n                \"1\"\n            ],\n            \"properties\": [],\n            \"sku\": \"MusPadNegT1\",\n            \"barcode\": null\n        },\n        {\n            \"id\": 998627598,\n            \"depth\": \"0.00\",\n            \"height\": \"0.00\",\n            \"name\": \"TOP HOT (Gris, 2)\",\n            \"price\": \"1125.00\",\n            \"compare_at_price\": \"2250.00\",\n            \"product_id\": 62782589,\n            \"image\": {\n                \"id\": 117532683,\n                \"product_id\": 62782589,\n                \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/737/415/products/fe3b4589-155e-438d-9231-d49421f3556d_nube-fb4786381f1606853615996623706233-1024-1024.jpg\",\n                \"position\": 1,\n                \"alt\": [],\n                \"created_at\": \"2020-09-09T14:39:35+0000\",\n                \"updated_at\": \"2020-09-09T14:39:35+0000\"\n            },\n            \"quantity\": \"1\",\n            \"free_shipping\": false,\n            \"weight\": \"0.20\",\n            \"width\": \"0.00\",\n            \"variant_id\": \"192895361\",\n            \"variant_values\": [\n                \"Gris\",\n                \"2\"\n            ],\n            \"properties\": [],\n            \"sku\": \"TopHotGriT2\",\n            \"barcode\": null\n        },\n        {\n            \"id\": 998626990,\n            \"depth\": \"0.00\",\n            \"height\": \"0.00\",\n            \"name\": \"BIKER HOT (2, Gris)\",\n            \"price\": \"1475.00\",\n            \"compare_at_price\": \"2950.00\",\n            \"product_id\": 63015543,\n            \"image\": {\n                \"id\": 117957050,\n                \"product_id\": 63015543,\n                \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/737/415/products/bikerssss1-b7fe7a3d15f7cc4a3515998373903460-1024-1024.jpeg\",\n                \"position\": 1,\n                \"alt\": [],\n                \"created_at\": \"2020-09-11T15:10:36+0000\",\n                \"updated_at\": \"2020-09-11T15:16:51+0000\"\n            },\n            \"quantity\": \"1\",\n            \"free_shipping\": false,\n            \"weight\": \"0.20\",\n            \"width\": \"0.00\",\n            \"variant_id\": \"194101103\",\n            \"variant_values\": [\n                \"2\",\n                \"Gris\"\n            ],\n            \"properties\": [],\n            \"sku\": \"BikHotGriT2\",\n            \"barcode\": null\n        },\n        {\n            \"id\": 998624361,\n            \"depth\": \"0.00\",\n            \"height\": \"0.00\",\n            \"name\": \"BLAZER TAILOR (Beige, 1)\",\n            \"price\": \"2000.00\",\n            \"compare_at_price\": \"4700.00\",\n            \"product_id\": 84087864,\n            \"image\": {\n                \"id\": 167350338,\n                \"product_id\": 84087864,\n                \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/737/415/products/image-97f930a8653ccad23316191263665097-1024-1024.\",\n                \"position\": 1,\n                \"alt\": [],\n                \"created_at\": \"2021-04-22T21:19:31+0000\",\n                \"updated_at\": \"2021-04-22T21:50:02+0000\"\n            },\n            \"quantity\": \"1\",\n            \"free_shipping\": false,\n            \"weight\": \"0.40\",\n            \"width\": \"0.00\",\n            \"variant_id\": \"326946271\",\n            \"variant_values\": [\n                \"Beige\",\n                \"1\"\n            ],\n            \"properties\": [],\n            \"sku\": null,\n            \"barcode\": null\n        }\n    ],\n    \"clearsale\": {\n        \"CodigoIntegracao\": false,\n        \"IP\": \"181.117.96.76\",\n        \"Estado\": \"S\"\n    },\n    \"number\": 1572,\n    \"cancel_reason\": null,\n    \"owner_note\": null,\n    \"cancelled_at\": null,\n    \"closed_at\": \"2022-09-12T22:58:24+0000\",\n    \"read_at\": null,\n    \"status\": \"closed\",\n    \"payment_status\": \"paid\",\n    \"gateway_link\": null,\n    \"shipping_carrier_name\": \"Correo Argentino Shipping\",\n    \"shipping_address\": {\n        \"address\": \"No informado\",\n        \"city\": \"La Capital\",\n        \"country\": \"AR\",\n        \"created_at\": \"2022-09-12T14:07:29+0000\",\n        \"default\": false,\n        \"floor\": \"\",\n        \"id\": 0,\n        \"locality\": \"Barranquitas\",\n        \"name\": \"María Candela Lallana\",\n        \"number\": \"\",\n        \"phone\": \"+543424484280\",\n        \"province\": \"Santa Fe\",\n        \"updated_at\": \"2022-09-13T12:58:58+0000\",\n        \"zipcode\": \"3000\",\n        \"customs\": null\n    },\n    \"shipping_status\": \"shipped\",\n    \"shipped_at\": \"2022-09-12T22:58:22+0000\",\n    \"paid_at\": \"2022-09-12T18:07:36+0000\",\n    \"landing_url\": \"https://www.zoesetton.com.ar/\",\n    \"client_details\": {\n        \"browser_ip\": \"181.117.96.76\",\n        \"user_agent\": \"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/105.0.0.0 Safari/537.36\"\n    },\n    \"app_id\": null\n}";
                        // var json = "{\n  \"id\": 707979677,\n  \"token\": \"66451b66c3165d75d4411024ea8bfb5bdedd0d65\",\n  \"store_id\": \"417567\",\n  \"contact_name\": \"Jaquelina Vicario\",\n  \"contact_phone\": \"+542664840030\",\n  \"contact_identification\": \"26980158\",\n  \"shipping_min_days\": 3,\n  \"shipping_max_days\": 4,\n  \"billing_name\": \"Jaquelina\",\n  \"billing_phone\": \"+542664840030\",\n  \"billing_address\": \"modulo 7 manzana 21 casa 17 La Punta.  San Luis \",\n  \"billing_number\": \"17\",\n  \"billing_floor\": \"\",\n  \"billing_locality\": \"El nogal\",\n  \"billing_zipcode\": \"5710\",\n  \"billing_city\": \"la punta\",\n  \"billing_province\": \"San Luis\",\n  \"billing_country\": \"AR\",\n  \"shipping_cost_owner\": \"647.60\",\n  \"shipping_cost_customer\": \"647.60\",\n  \"coupon\": [],\n  \"promotional_discount\": {\n    \"id\": 27374689,\n    \"store_id\": 417567,\n    \"order_id\": 707979677,\n    \"created_at\": \"2022-09-07T16:58:36-0300\",\n    \"total_discount_amount\": \"989.60\",\n    \"contents\": [\n      {\n        \"id\": 993842616,\n        \"discount_script_type\": \"NAtX%off\",\n        \"scope_type\": \"categories\",\n        \"scope_value_id\": 3852205,\n        \"scope_value_name\": \"MUSCULOSAS\",\n        \"quantity\": 1,\n        \"original_price\": 2450,\n        \"final_price\": 1169.1,\n        \"discount_amount\": 129.9,\n        \"total_discount_amount\": 129.9,\n        \"begin_date\": null,\n        \"end_date\": null\n      },\n      {\n        \"id\": 993843208,\n        \"discount_script_type\": \"NAtX%off\",\n        \"scope_type\": \"categories\",\n        \"scope_value_id\": 3852205,\n        \"scope_value_name\": \"MUSCULOSAS\",\n        \"quantity\": 1,\n        \"original_price\": 2450,\n        \"final_price\": 1169.1,\n        \"discount_amount\": 129.9,\n        \"total_discount_amount\": 129.9,\n        \"begin_date\": null,\n        \"end_date\": null\n      },\n      {\n        \"id\": 993836429,\n        \"discount_script_type\": \"NAtX%off\",\n        \"scope_type\": \"categories\",\n        \"scope_value_id\": 3688847,\n        \"scope_value_name\": \"Vestidos\",\n        \"quantity\": 1,\n        \"original_price\": 6000,\n        \"final_price\": 3149.1,\n        \"discount_amount\": 349.9,\n        \"total_discount_amount\": 349.9,\n        \"begin_date\": null,\n        \"end_date\": null\n      },\n      {\n        \"id\": 993838565,\n        \"discount_script_type\": \"NAtX%off\",\n        \"scope_type\": \"categories\",\n        \"scope_value_id\": 3688847,\n        \"scope_value_name\": \"Vestidos\",\n        \"quantity\": 1,\n        \"original_price\": 4140,\n        \"final_price\": 3419.1,\n        \"discount_amount\": 379.9,\n        \"total_discount_amount\": 379.9,\n        \"begin_date\": null,\n        \"end_date\": null\n      }\n    ],\n    \"promotions_applied\": [\n      {\n        \"scope_value_id\": 3852205,\n        \"discount_script_type\": \"NAtX%off\",\n        \"total_discount_amount\": 259.8,\n        \"total_discount_amount_short\": \"$259,80\",\n        \"total_discount_amount_long\": \"$259,80 ARS\",\n        \"total_discount_amount_to_cents\": 25980,\n        \"scope_value_name\": \"MUSCULOSAS\",\n        \"selected_threshold\": {\n          \"quantity\": 1,\n          \"discount_decimal_percentage\": 0.1\n        }\n      },\n      {\n        \"scope_value_id\": 3688847,\n        \"discount_script_type\": \"NAtX%off\",\n        \"total_discount_amount\": 729.8,\n        \"total_discount_amount_short\": \"$729,80\",\n        \"total_discount_amount_long\": \"$729,80 ARS\",\n        \"total_discount_amount_to_cents\": 72980,\n        \"scope_value_name\": \"Vestidos\",\n        \"selected_threshold\": {\n          \"quantity\": 1,\n          \"discount_decimal_percentage\": 0.1\n        }\n      }\n    ]\n  },\n  \"subtotal\": \"9896.00\",\n  \"discount\": \"989.60\",\n  \"discount_coupon\": \"0.00\",\n  \"discount_gateway\": \"0.00\",\n  \"total\": \"9554.00\",\n  \"total_usd\": \"72.17\",\n  \"checkout_enabled\": true,\n  \"weight\": \"0.450\",\n  \"currency\": \"ARS\",\n  \"language\": \"es\",\n  \"gateway\": \"mercadopago\",\n  \"gateway_id\": \"5745117671\",\n  \"gateway_name\": \"Mercado Pago\",\n  \"shipping\": \"api_385262\",\n  \"shipping_option\": \"Correo Argentino - Envio a domicilio\",\n  \"shipping_option_code\": \"standard\",\n  \"shipping_option_reference\": \"ref123\",\n  \"shipping_pickup_details\": null,\n  \"shipping_tracking_number\": null,\n  \"shipping_tracking_url\": null,\n  \"shipping_store_branch_name\": null,\n  \"shipping_pickup_type\": \"ship\",\n  \"shipping_suboption\": [],\n  \"extra\": {},\n  \"storefront\": \"mobile\",\n  \"note\": \"\",\n  \"created_at\": \"2022-09-07T17:01:32+0000\",\n  \"updated_at\": \"2022-09-12T13:02:16+0000\",\n  \"completed_at\": {\n    \"date\": \"2022-09-07 17:01:32.000000\",\n    \"timezone_type\": 3,\n    \"timezone\": \"UTC\"\n  },\n  \"next_action\": \"waiting_shipment\",\n  \"payment_details\": {\n    \"method\": \"credit_card\",\n    \"credit_card_company\": null,\n    \"installments\": \"1\"\n  },\n  \"attributes\": [],\n  \"customer\": {\n    \"id\": 100570730,\n    \"name\": \"Jaquelina Vicario\",\n    \"email\": \"jaquelinavicario@gmail.com\",\n    \"identification\": \"26980158\",\n    \"phone\": \"+542664840030\",\n    \"note\": null,\n    \"default_address\": {\n      \"address\": \"modulo 7 manzana 21 casa 17 La Punta.  San Luis \",\n      \"city\": \"La Punta\",\n      \"country\": \"AR\",\n      \"created_at\": \"2022-09-07T17:01:32+0000\",\n      \"default\": true,\n      \"floor\": \"\",\n      \"id\": 76157811,\n      \"locality\": \"El nogal\",\n      \"name\": \"Jaquelina Vicario\",\n      \"number\": \"17\",\n      \"phone\": \"+542664840030\",\n      \"province\": \"San Luis\",\n      \"updated_at\": \"2022-09-07T17:01:32+0000\",\n      \"zipcode\": \"5710\"\n    },\n    \"addresses\": [\n      {\n        \"address\": \"modulo 7 manzana 21 casa 17 La Punta.  San Luis \",\n        \"city\": \"La Punta\",\n        \"country\": \"AR\",\n        \"created_at\": \"2022-09-07T17:01:32+0000\",\n        \"default\": true,\n        \"floor\": \"\",\n        \"id\": 76157811,\n        \"locality\": \"El nogal\",\n        \"name\": \"Jaquelina Vicario\",\n        \"number\": \"17\",\n        \"phone\": \"+542664840030\",\n        \"province\": \"San Luis\",\n        \"updated_at\": \"2022-09-07T17:01:32+0000\",\n        \"zipcode\": \"5710\"\n      }\n    ],\n    \"billing_name\": \"Jaquelina\",\n    \"billing_phone\": \"+542664840030\",\n    \"billing_address\": \"modulo 7 manzana 21 casa 17 La Punta.  San Luis \",\n    \"billing_number\": \"17\",\n    \"billing_floor\": \"\",\n    \"billing_locality\": \"El nogal\",\n    \"billing_zipcode\": \"5710\",\n    \"billing_city\": \"la punta\",\n    \"billing_province\": \"San Luis\",\n    \"billing_country\": \"AR\",\n    \"extra\": {},\n    \"total_spent\": \"9554.00\",\n    \"total_spent_currency\": \"ARS\",\n    \"last_order_id\": 707979677,\n    \"active\": true,\n    \"first_interaction\": \"2022-09-07T17:01:32+0000\",\n    \"created_at\": \"2022-09-07T17:01:32+0000\",\n    \"updated_at\": \"2022-09-07T17:02:26+0000\",\n    \"accepts_marketing\": true,\n    \"accepts_marketing_updated_at\": \"2022-09-07T16:57:52+0000\"\n  },\n  \"products\": [\n    {\n      \"id\": 993838565,\n      \"depth\": \"1.00\",\n      \"height\": \"20.00\",\n      \"name\": \"VESTIDO FAUNA (CAMEL)\",\n      \"price\": \"3799.00\",\n      \"compare_at_price\": \"4140.00\",\n      \"product_id\": 118357100,\n      \"image\": {\n        \"id\": 310264812,\n        \"product_id\": 118357100,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/417/567/products/80756ff4-f38c-4862-85ea-fbd6d7921ece1-c95f1f8b08431619cd16511838458872-1024-1024.jpeg\",\n        \"position\": 1,\n        \"alt\": [],\n        \"created_at\": \"2022-04-28T22:10:12+0000\",\n        \"updated_at\": \"2022-07-19T01:50:52+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.10\",\n      \"width\": \"20.00\",\n      \"variant_id\": \"457089268\",\n      \"variant_values\": [\"CAMEL\"],\n      \"properties\": [],\n      \"sku\": \"1002220-CA\",\n      \"barcode\": null\n    },\n    {\n      \"id\": 993842616,\n      \"depth\": \"10.00\",\n      \"height\": \"1.00\",\n      \"name\": \"MUSCULOSA BERNARD (Negro, M)\",\n      \"price\": \"1299.00\",\n      \"compare_at_price\": \"2450.00\",\n      \"product_id\": 129450067,\n      \"image\": {\n        \"id\": 355044397,\n        \"product_id\": 129450067,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/417/567/products/913bece5-07cc-4e09-a4cb-8ed27960e7621-876160208d72c5607e16604885016933-1024-1024.jpeg\",\n        \"position\": 2,\n        \"alt\": [],\n        \"created_at\": \"2022-08-14T14:28:54+0000\",\n        \"updated_at\": \"2022-08-14T14:51:23+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.10\",\n      \"width\": \"10.00\",\n      \"variant_id\": \"506707327\",\n      \"variant_values\": [\"Negro\", \"M\"],\n      \"properties\": [],\n      \"sku\": \"1002638-NE-M\",\n      \"barcode\": null\n    },\n    {\n      \"id\": 993843208,\n      \"depth\": \"10.00\",\n      \"height\": \"1.00\",\n      \"name\": \"MUSCULOSA BERNARD (Rosa, M)\",\n      \"price\": \"1299.00\",\n      \"compare_at_price\": \"2450.00\",\n      \"product_id\": 129450067,\n      \"image\": {\n        \"id\": 355044454,\n        \"product_id\": 129450067,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/417/567/products/56554c39-9b92-4b90-bf73-7c5e9f714cca1-8a516603bcd29aacbc16604885032797-1024-1024.jpeg\",\n        \"position\": 3,\n        \"alt\": [],\n        \"created_at\": \"2022-08-14T14:29:14+0000\",\n        \"updated_at\": \"2022-08-14T14:51:23+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.10\",\n      \"width\": \"10.00\",\n      \"variant_id\": \"506707338\",\n      \"variant_values\": [\"Rosa\", \"M\"],\n      \"properties\": [],\n      \"sku\": \"1002638-RS-M\",\n      \"barcode\": null\n    },\n    {\n      \"id\": 993836429,\n      \"depth\": \"20.00\",\n      \"height\": \"2.00\",\n      \"name\": \"VESTIDO PEAK (FUCSIA)\",\n      \"price\": \"3499.00\",\n      \"compare_at_price\": \"6000.00\",\n      \"product_id\": 129841033,\n      \"image\": {\n        \"id\": 356980800,\n        \"product_id\": 129841033,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/417/567/products/d327aa94-71d1-4ca1-a826-9904ebc01a721-d47cd3a90abd65926016608722700293-1024-1024.jpeg\",\n        \"position\": 2,\n        \"alt\": [],\n        \"created_at\": \"2022-08-19T01:24:06+0000\",\n        \"updated_at\": \"2022-08-23T14:50:11+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.15\",\n      \"width\": \"20.00\",\n      \"variant_id\": \"508538870\",\n      \"variant_values\": [\"FUCSIA\"],\n      \"properties\": [],\n      \"sku\": \"1002646-FU\",\n      \"barcode\": null\n    }\n  ],\n  \"clearsale\": {\n    \"CodigoIntegracao\": false,\n    \"IP\": \"190.115.119.182\",\n    \"Estado\": \"D\"\n  },\n  \"number\": 69173,\n  \"cancel_reason\": null,\n  \"owner_note\": \"lista 9/9 guada\\r\\n\\r\\nfauna camel cambio por bordo - 9/9 cande\",\n  \"cancelled_at\": null,\n  \"closed_at\": null,\n  \"read_at\": null,\n  \"status\": \"open\",\n  \"payment_status\": \"paid\",\n  \"gateway_link\": \"https://www.mercadopago.com.ar/activities?q=707979677\",\n  \"shipping_carrier_name\": \"Correo Argentino Shipping\",\n  \"shipping_address\": {\n    \"address\": \"modulo 7 manzana 21 casa 17 La Punta.  San Luis \",\n    \"city\": \"La Punta\",\n    \"country\": \"AR\",\n    \"created_at\": \"2022-09-07T16:47:40+0000\",\n    \"default\": false,\n    \"floor\": \"\",\n    \"id\": 0,\n    \"locality\": \"El nogal\",\n    \"name\": \"Jaquelina Vicario\",\n    \"number\": \"17\",\n    \"phone\": \"+542664840030\",\n    \"province\": \"San Luis\",\n    \"updated_at\": \"2022-09-12T13:02:16+0000\",\n    \"zipcode\": \"5710\",\n    \"customs\": null\n  },\n  \"shipping_status\": \"unshipped\",\n  \"shipped_at\": null,\n  \"paid_at\": \"2022-09-07T17:01:33+0000\",\n  \"landing_url\": \"https://www.sorjuana.com.ar/productos/vestido-monaca/?pf=fbc&utm_source=IGShopping&utm_medium=Social\",\n  \"client_details\": {\n    \"browser_ip\": \"190.115.119.182\",\n    \"user_agent\": \"Mozilla/5.0 (Linux; Android 11; SM-A115M Build/RP1A.200720.012; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/104.0.5112.97 Mobile Safari/537.36 Instagram 250.0.0.21.109 Android (30/11; 280dpi; 720x1411; samsung; SM-A115M; a11q; qcom; es_U\"\n  },\n  \"app_id\": null\n}\n";

                        // var json = "{\n  \"id\": 613394445,\n  \"token\": \"0d9348758d2d8ecb289c5df060db287f91f77fce\",\n  \"store_id\": \"934891\",\n  \"contact_name\": \"Natalia Colovatto\",\n  \"contact_phone\": \"+543412822551\",\n  \"contact_identification\": \"26999747\",\n  \"shipping_min_days\": 2,\n  \"shipping_max_days\": 4,\n  \"billing_name\": \"Natalia\",\n  \"billing_phone\": \"+543412822551\",\n  \"billing_address\": \"Sarmiento \",\n  \"billing_number\": \"565\",\n  \"billing_floor\": \"\",\n  \"billing_locality\": \"\",\n  \"billing_zipcode\": \"2121\",\n  \"billing_city\": \"Pérez \",\n  \"billing_province\": \"Santa Fe\",\n  \"billing_country\": \"AR\",\n  \"shipping_cost_owner\": \"544.44\",\n  \"shipping_cost_customer\": \"544.44\",\n  \"coupon\": [],\n  \"promotional_discount\": {\n    \"id\": null,\n    \"store_id\": 934891,\n    \"order_id\": \"613394445\",\n    \"created_at\": \"2022-09-21T13:14:11+0000\",\n    \"total_discount_amount\": \"0.00\",\n    \"contents\": [],\n    \"promotions_applied\": []\n  },\n  \"subtotal\": \"33300.00\",\n  \"discount\": \"0.00\",\n  \"discount_coupon\": \"0.00\",\n  \"discount_gateway\": \"0.00\",\n  \"total\": \"33844.44\",\n  \"total_usd\": \"255.67\",\n  \"checkout_enabled\": true,\n  \"weight\": \"1.100\",\n  \"currency\": \"ARS\",\n  \"language\": \"es\",\n  \"gateway\": \"mercadopago\",\n  \"gateway_id\": \"25814529279\",\n  \"gateway_name\": \"Mercado Pago\",\n  \"shipping\": \"api_323326\",\n  \"shipping_option\": \"Punto de retiro\",\n  \"shipping_option_code\": \"standardPickup\",\n  \"shipping_option_reference\": \"S0410\",\n  \"shipping_pickup_details\": {\n    \"name\": \"Correo Argentino - PEREZ\",\n    \"address\": {\n      \"address\": \"DR ALBERTO CHIAVARINI\",\n      \"number\": \"1199\",\n      \"floor\": null,\n      \"locality\": \"PEREZ\",\n      \"city\": \"ROSARIO\",\n      \"province\": \"SANTA FE\",\n      \"zipcode\": \"S2121ZAA\",\n      \"country\": \"argentina\",\n      \"latitude\": \"-32.9977226\",\n      \"longitude\": \"-60.7661552\",\n      \"phone\": \"(0341) 4951395\"\n    },\n    \"hours\": [\n      {\n        \"day\": 1,\n        \"start\": \"0800\",\n        \"end\": \"1500\"\n      },\n      {\n        \"day\": 2,\n        \"start\": \"0800\",\n        \"end\": \"1500\"\n      },\n      {\n        \"day\": 3,\n        \"start\": \"0800\",\n        \"end\": \"1500\"\n      },\n      {\n        \"day\": 4,\n        \"start\": \"0800\",\n        \"end\": \"1500\"\n      },\n      {\n        \"day\": 5,\n        \"start\": \"0800\",\n        \"end\": \"1500\"\n      }\n    ]\n  },\n  \"shipping_tracking_number\": \"00006566746GIXTLE19A301\",\n  \"shipping_tracking_url\": null,\n  \"shipping_store_branch_name\": null,\n  \"shipping_pickup_type\": \"pickup\",\n  \"shipping_suboption\": {\n    \"id\": \"5c94375ac72588d9ce74383684902317\"\n  },\n  \"extra\": {},\n  \"storefront\": \"mobile\",\n  \"note\": \"\",\n  \"created_at\": \"2022-09-15T16:18:44+0000\",\n  \"updated_at\": \"2022-09-20T21:14:23+0000\",\n  \"completed_at\": {\n    \"date\": \"2022-09-15 16:18:44.000000\",\n    \"timezone_type\": 3,\n    \"timezone\": \"UTC\"\n  },\n  \"next_action\": \"noop\",\n  \"payment_details\": {\n    \"method\": \"credit_card\",\n    \"credit_card_company\": \"master\",\n    \"installments\": \"6\"\n  },\n  \"attributes\": [],\n  \"customer\": {\n    \"id\": 58353874,\n    \"name\": \"Natalia Colovatto\",\n    \"email\": \"colovattonatalia@gmail.com\",\n    \"identification\": \"26999747\",\n    \"phone\": \"+543412822551\",\n    \"note\": null,\n    \"default_address\": {\n      \"address\": null,\n      \"city\": \"Rosario\",\n      \"country\": \"AR\",\n      \"created_at\": \"2021-08-12T15:56:00+0000\",\n      \"default\": true,\n      \"floor\": null,\n      \"id\": 50056518,\n      \"locality\": \"Perez\",\n      \"name\": \"Natalia Colovatto\",\n      \"number\": null,\n      \"phone\": \"\",\n      \"province\": \"Santa Fe\",\n      \"updated_at\": \"2021-08-12T15:56:00+0000\",\n      \"zipcode\": \"2121\"\n    },\n    \"addresses\": [\n      {\n        \"address\": \"Sarmiento \",\n        \"city\": \"Pérez \",\n        \"country\": \"AR\",\n        \"created_at\": \"2021-03-09T17:31:02+0000\",\n        \"default\": false,\n        \"floor\": \"\",\n        \"id\": 39506388,\n        \"locality\": \"\",\n        \"name\": \"Natalia Colovatto\",\n        \"number\": \"565\",\n        \"phone\": \"+543412822551\",\n        \"province\": \"Santa Fe\",\n        \"updated_at\": \"2021-03-09T17:31:02+0000\",\n        \"zipcode\": \"2121\"\n      },\n      {\n        \"address\": \"Sarmiento \",\n        \"city\": \"Pérez \",\n        \"country\": \"AR\",\n        \"created_at\": \"2021-05-06T19:09:13+0000\",\n        \"default\": false,\n        \"floor\": \"\",\n        \"id\": 43217496,\n        \"locality\": \"\",\n        \"name\": \"Natalia Colovatto\",\n        \"number\": \"565\",\n        \"phone\": \"+5493412822551\",\n        \"province\": \"Santa Fe\",\n        \"updated_at\": \"2021-05-06T19:09:13+0000\",\n        \"zipcode\": \"2121\"\n      },\n      {\n        \"address\": null,\n        \"city\": \"Rosario\",\n        \"country\": \"AR\",\n        \"created_at\": \"2021-08-12T15:56:00+0000\",\n        \"default\": true,\n        \"floor\": null,\n        \"id\": 50056518,\n        \"locality\": \"Perez\",\n        \"name\": \"Natalia Colovatto\",\n        \"number\": null,\n        \"phone\": \"\",\n        \"province\": \"Santa Fe\",\n        \"updated_at\": \"2021-08-12T15:56:00+0000\",\n        \"zipcode\": \"2121\"\n      }\n    ],\n    \"billing_name\": \"Natalia\",\n    \"billing_phone\": \"+543412822551\",\n    \"billing_address\": \"Sarmiento \",\n    \"billing_number\": \"565\",\n    \"billing_floor\": \"\",\n    \"billing_locality\": \"\",\n    \"billing_zipcode\": \"2121\",\n    \"billing_city\": \"Perez\",\n    \"billing_province\": \"Santa Fe\",\n    \"billing_country\": \"AR\",\n    \"extra\": {},\n    \"total_spent\": \"103450.32\",\n    \"total_spent_currency\": \"ARS\",\n    \"last_order_id\": 613394445,\n    \"active\": true,\n    \"first_interaction\": \"2021-05-18T11:27:42+0000\",\n    \"created_at\": \"2021-03-09T17:31:02+0000\",\n    \"updated_at\": \"2022-04-08T13:33:53+0000\",\n    \"accepts_marketing\": true,\n    \"accepts_marketing_updated_at\": \"2021-09-23T02:51:46+0000\"\n  },\n  \"products\": [\n    {\n      \"id\": 1002297253,\n      \"depth\": \"20.00\",\n      \"height\": \"10.00\",\n      \"name\": \"Remera Just (Blanco)\",\n      \"price\": \"2500.00\",\n      \"compare_at_price\": \"2500.00\",\n      \"product_id\": 131349164,\n      \"image\": {\n        \"id\": 362343839,\n        \"product_id\": 131349164,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/934/891/products/179f267f-c142-47d9-bce2-8e2abdcd6433-35947b8d2a24070beb16619686170958-1024-1024.jpeg\",\n        \"position\": 1,\n        \"alt\": [],\n        \"created_at\": \"2022-08-31T17:56:19+0000\",\n        \"updated_at\": \"2022-08-31T19:51:41+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.20\",\n      \"width\": \"20.00\",\n      \"variant_id\": \"514726366\",\n      \"variant_values\": [\"Blanco\"],\n      \"properties\": [],\n      \"sku\": null,\n      \"barcode\": null\n    },\n    {\n      \"id\": 1002290111,\n      \"depth\": \"20.00\",\n      \"height\": \"10.00\",\n      \"name\": \"Camisa Merengue (Fucsia, 38)\",\n      \"price\": \"12000.00\",\n      \"compare_at_price\": \"13800.00\",\n      \"product_id\": 131375748,\n      \"image\": {\n        \"id\": 368120821,\n        \"product_id\": 131375748,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/934/891/products/43799f56-6cbd-4376-b7d7-76e9f6e8bf40-9e25793a0d98a7b62716630306874230-1024-1024.jpeg\",\n        \"position\": 12,\n        \"alt\": [],\n        \"created_at\": \"2022-09-13T00:55:59+0000\",\n        \"updated_at\": \"2022-09-20T14:50:13+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.30\",\n      \"width\": \"20.00\",\n      \"variant_id\": \"514791698\",\n      \"variant_values\": [\"Fucsia\", \"38\"],\n      \"properties\": [],\n      \"sku\": null,\n      \"barcode\": null\n    },\n    {\n      \"id\": 1002290267,\n      \"depth\": \"20.00\",\n      \"height\": \"10.00\",\n      \"name\": \"Camisa Merengue (Beneton, 38)\",\n      \"price\": \"12000.00\",\n      \"compare_at_price\": \"13800.00\",\n      \"product_id\": 131375748,\n      \"image\": {\n        \"id\": 362616982,\n        \"product_id\": 131375748,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/934/891/products/bb0b95e4-d274-4ba3-81d4-3dd7d2dced4a-3e3c59029477fbc5df16619956780765-1024-1024.jpeg\",\n        \"position\": 1,\n        \"alt\": [],\n        \"created_at\": \"2022-09-01T01:27:04+0000\",\n        \"updated_at\": \"2022-09-20T14:50:13+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.30\",\n      \"width\": \"20.00\",\n      \"variant_id\": \"514791702\",\n      \"variant_values\": [\"Beneton\", \"38\"],\n      \"properties\": [],\n      \"sku\": null,\n      \"barcode\": null\n    },\n    {\n      \"id\": 1002309546,\n      \"depth\": \"20.00\",\n      \"height\": \"10.00\",\n      \"name\": \"Short Felipe (32)\",\n      \"price\": \"6800.00\",\n      \"compare_at_price\": \"6800.00\",\n      \"product_id\": 131708277,\n      \"image\": {\n        \"id\": 369468497,\n        \"product_id\": 131708277,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/934/891/products/image-20d5c8e13f304d037416632581011585-1024-1024.jpg\",\n        \"position\": 1,\n        \"alt\": [],\n        \"created_at\": \"2022-09-15T16:07:15+0000\",\n        \"updated_at\": \"2022-09-15T16:08:28+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.30\",\n      \"width\": \"20.00\",\n      \"variant_id\": \"516210369\",\n      \"variant_values\": [\"32\"],\n      \"properties\": [],\n      \"sku\": null,\n      \"barcode\": null\n    }\n  ],\n  \"clearsale\": {\n    \"CodigoIntegracao\": false,\n    \"IP\": \"181.14.119.156\",\n    \"Estado\": \"S\"\n  },\n  \"number\": 1222,\n  \"cancel_reason\": null,\n  \"owner_note\": null,\n  \"cancelled_at\": null,\n  \"closed_at\": \"2022-09-20T21:14:23+0000\",\n  \"read_at\": null,\n  \"status\": \"closed\",\n  \"payment_status\": \"paid\",\n  \"gateway_link\": \"https://www.mercadopago.com.ar/activities?q=613394445\",\n  \"shipping_carrier_name\": \"Correo Argentino Shipping\",\n  \"shipping_address\": {\n    \"address\": \"\",\n    \"city\": \"Rosario\",\n    \"country\": \"AR\",\n    \"created_at\": \"2022-07-21T21:19:15+0000\",\n    \"default\": false,\n    \"floor\": \"\",\n    \"id\": 0,\n    \"locality\": \"Perez\",\n    \"name\": \"Natalia Colovatto\",\n    \"number\": \"\",\n    \"phone\": \"\",\n    \"province\": \"Santa Fe\",\n    \"updated_at\": \"2022-09-20T21:14:23+0000\",\n    \"zipcode\": \"2121\",\n    \"customs\": null\n  },\n  \"shipping_status\": \"shipped\",\n  \"shipped_at\": \"2022-09-20T12:14:03+0000\",\n  \"paid_at\": \"2022-09-15T16:18:45+0000\",\n  \"landing_url\": \"https://www.elbauldelola.com.ar/\",\n  \"client_details\": {\n    \"browser_ip\": \"181.14.119.156\",\n    \"user_agent\": \"Mozilla/5.0 (Linux; Android 10; SM-A307G Build/QP1A.190711.020; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/103.0.5060.71 Mobile Safari/537.36 Instagram 244.0.0.17.110 Android (29/10; 280dpi; 720x1423; samsung; SM-A307G; a30s; exynos7904\"\n  },\n  \"app_id\": null\n}\n";
                        // var json = "{\n  \"id\": 734963839,\n  \"token\": \"df9ffd3b05481605b12eb5d952e277a5afd1ee2f\",\n  \"store_id\": \"934891\",\n  \"contact_name\": \"Claudia karina Gomez\",\n  \"contact_phone\": \"+541141979704\",\n  \"contact_identification\": \"20001420\",\n  \"shipping_min_days\": 3,\n  \"shipping_max_days\": 4,\n  \"billing_name\": \"Claudia karina\",\n  \"billing_phone\": \"+541141979704\",\n  \"billing_address\": \"San lorenzo\",\n  \"billing_number\": \"2272\",\n  \"billing_floor\": \"10 A\",\n  \"billing_locality\": \"\",\n  \"billing_zipcode\": \"1650\",\n  \"billing_city\": \"San Martin\",\n  \"billing_province\": \"Buenos Aires\",\n  \"billing_country\": \"AR\",\n  \"shipping_cost_owner\": \"726.93\",\n  \"shipping_cost_customer\": \"726.93\",\n  \"coupon\": [],\n  \"promotional_discount\": {\n    \"id\": null,\n    \"store_id\": 1401216,\n    \"order_id\": \"734963839\",\n    \"created_at\": \"2022-09-21T13:09:57+0000\",\n    \"total_discount_amount\": \"0.00\",\n    \"contents\": [],\n    \"promotions_applied\": []\n  },\n  \"subtotal\": \"20232.00\",\n  \"discount\": \"0.00\",\n  \"discount_coupon\": \"0.00\",\n  \"discount_gateway\": \"0.00\",\n  \"total\": \"20958.93\",\n  \"total_usd\": \"158.33\",\n  \"checkout_enabled\": true,\n  \"weight\": \"0.550\",\n  \"currency\": \"ARS\",\n  \"language\": \"es\",\n  \"gateway\": \"mercadopago\",\n  \"gateway_id\": \"25957678718\",\n  \"gateway_name\": \"Mercado Pago\",\n  \"shipping\": \"api_393597\",\n  \"shipping_option\": \"Correo Argentino - Envio a domicilio\",\n  \"shipping_option_code\": \"standard\",\n  \"shipping_option_reference\": \"ref123\",\n  \"shipping_pickup_details\": null,\n  \"shipping_tracking_number\": \"SD357678225\",\n  \"shipping_tracking_url\": null,\n  \"shipping_store_branch_name\": null,\n  \"shipping_pickup_type\": \"ship\",\n  \"shipping_suboption\": [],\n  \"extra\": {},\n  \"storefront\": \"mobile\",\n  \"note\": \"\",\n  \"created_at\": \"2022-09-20T15:18:02+0000\",\n  \"updated_at\": \"2022-09-21T12:06:57+0000\",\n  \"completed_at\": {\n    \"date\": \"2022-09-20 15:18:02.000000\",\n    \"timezone_type\": 3,\n    \"timezone\": \"UTC\"\n  },\n  \"next_action\": \"close\",\n  \"payment_details\": {\n    \"method\": \"credit_card\",\n    \"credit_card_company\": \"visa\",\n    \"installments\": \"12\"\n  },\n  \"attributes\": [],\n  \"customer\": {\n    \"id\": 98973638,\n    \"name\": \"Claudia karina Gomez\",\n    \"email\": \"claudiakgomez@hotmail.com\",\n    \"identification\": \"27200014206\",\n    \"phone\": \"+541141979704\",\n    \"note\": null,\n    \"default_address\": {\n      \"address\": \"San lorenzo \",\n      \"city\": \"San Martin\",\n      \"country\": \"AR\",\n      \"created_at\": \"2022-08-16T11:04:08+0000\",\n      \"default\": true,\n      \"floor\": \"10 A\",\n      \"id\": 74959729,\n      \"locality\": \"\",\n      \"name\": \"Claudia karina Gomez\",\n      \"number\": \"2272\",\n      \"phone\": \"+541141979704\",\n      \"province\": \"Buenos Aires\",\n      \"updated_at\": \"2022-08-16T11:04:08+0000\",\n      \"zipcode\": \"1650\"\n    },\n    \"addresses\": [\n      {\n        \"address\": \"San lorenzo \",\n        \"city\": \"San Martin\",\n        \"country\": \"AR\",\n        \"created_at\": \"2022-08-16T11:04:08+0000\",\n        \"default\": true,\n        \"floor\": \"10 A\",\n        \"id\": 74959729,\n        \"locality\": \"\",\n        \"name\": \"Claudia karina Gomez\",\n        \"number\": \"2272\",\n        \"phone\": \"+541141979704\",\n        \"province\": \"Buenos Aires\",\n        \"updated_at\": \"2022-08-16T11:04:08+0000\",\n        \"zipcode\": \"1650\"\n      }\n    ],\n    \"billing_name\": \"Claudia karina\",\n    \"billing_phone\": \"+541141979704\",\n    \"billing_address\": \"San lorenzo \",\n    \"billing_number\": \"2272\",\n    \"billing_floor\": \"10 A\",\n    \"billing_locality\": \"\",\n    \"billing_zipcode\": \"1650\",\n    \"billing_city\": \"San Martin\",\n    \"billing_province\": \"Buenos Aires\",\n    \"billing_country\": \"AR\",\n    \"extra\": {},\n    \"total_spent\": \"41201.82\",\n    \"total_spent_currency\": \"ARS\",\n    \"last_order_id\": 734963839,\n    \"active\": false,\n    \"first_interaction\": \"2022-09-20T15:18:02+0000\",\n    \"created_at\": \"2022-08-16T11:04:08+0000\",\n    \"updated_at\": \"2022-08-16T11:04:08+0000\",\n    \"accepts_marketing\": true,\n    \"accepts_marketing_updated_at\": \"2022-08-16T11:01:37+0000\"\n  },\n  \"products\": [\n    {\n      \"id\": 1007440942,\n      \"depth\": \"0.00\",\n      \"height\": \"0.00\",\n      \"name\": \"Remeron Sandy de lanilla\",\n      \"price\": \"12982.00\",\n      \"compare_at_price\": \"12982.00\",\n      \"product_id\": 118755593,\n      \"image\": {\n        \"id\": 311206759,\n        \"product_id\": 118755593,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/001/401/216/products/bfc46526-7815-427f-bbc5-1e16f7455be9-597aae4b205e77097116514132382204-1024-1024.jpeg\",\n        \"position\": 1,\n        \"alt\": [],\n        \"created_at\": \"2022-05-01T13:29:05+0000\",\n        \"updated_at\": \"2022-05-01T13:54:03+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.25\",\n      \"width\": \"0.00\",\n      \"variant_id\": \"459563333\",\n      \"variant_values\": [],\n      \"properties\": [],\n      \"sku\": null,\n      \"barcode\": null\n    },\n    {\n      \"id\": 1007427269,\n      \"depth\": \"0.00\",\n      \"height\": \"0.00\",\n      \"name\": \"Remeron ARIANA\",\n      \"price\": \"7250.00\",\n      \"compare_at_price\": \"7250.00\",\n      \"product_id\": 127454276,\n      \"image\": {\n        \"id\": 346025128,\n        \"product_id\": 127454276,\n        \"src\": \"https://d3ugyf2ht6aenh.cloudfront.net/stores/001/401/216/products/43e82989-2283-4c1c-a01f-cddfee8fe064-f5fe4d2770781105c416586571837002-1024-1024.jpeg\",\n        \"position\": 1,\n        \"alt\": [],\n        \"created_at\": \"2022-07-24T10:05:46+0000\",\n        \"updated_at\": \"2022-07-24T10:06:28+0000\"\n      },\n      \"quantity\": \"1\",\n      \"free_shipping\": false,\n      \"weight\": \"0.30\",\n      \"width\": \"0.00\",\n      \"variant_id\": \"497541676\",\n      \"variant_values\": [],\n      \"properties\": [],\n      \"sku\": null,\n      \"barcode\": null\n    }\n  ],\n  \"clearsale\": {\n    \"CodigoIntegracao\": false,\n    \"IP\": \"200.5.98.50\",\n    \"Estado\": \"BX\"\n  },\n  \"number\": 5318,\n  \"cancel_reason\": null,\n  \"owner_note\": null,\n  \"cancelled_at\": null,\n  \"closed_at\": null,\n  \"read_at\": null,\n  \"status\": \"open\",\n  \"payment_status\": \"paid\",\n  \"gateway_link\": \"https://www.mercadopago.com.ar/activities?q=734963839\",\n  \"shipping_carrier_name\": \"Correo Argentino Shipping\",\n  \"shipping_address\": {\n    \"address\": \"San lorenzo\",\n    \"city\": \"San Martin\",\n    \"country\": \"AR\",\n    \"created_at\": \"2022-09-20T13:52:14+0000\",\n    \"default\": false,\n    \"floor\": \"10 A\",\n    \"id\": 0,\n    \"locality\": \"\",\n    \"name\": \"Claudia karina Gomez\",\n    \"number\": \"2272\",\n    \"phone\": \"+541141979704\",\n    \"province\": \"Buenos Aires\",\n    \"updated_at\": \"2022-09-21T12:06:57+0000\",\n    \"zipcode\": \"1650\",\n    \"customs\": null\n  },\n  \"shipping_status\": \"shipped\",\n  \"shipped_at\": \"2022-09-21T12:06:57+0000\",\n  \"paid_at\": \"2022-09-20T15:18:03+0000\",\n  \"landing_url\": \"https://andraurgat.ar/productos/remeron-marta-de-morley4/?pf=fbc&utm_source=IGShopping&utm_medium=Social\",\n  \"client_details\": {\n    \"browser_ip\": \"200.5.98.50\",\n    \"user_agent\": \"Mozilla/5.0 (Linux; Android 12; SM-A127M Build/SP1A.210812.016; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/105.0.5195.136 Mobile Safari/537.36 Instagram 252.0.0.17.111 Android (31/12; 300dpi; 720x1467; samsung; SM-A127M; a12s; exynos850\"\n  },\n  \"app_id\": null\n}\n";
                        // tnorder = JsonConvert.DeserializeObject<OrderResponseModel>(json);
                        // >>>>>>>>>>>>>>>>>>>>>>

                        if (tnorder != null && tnorder.code != 0)
                        {
                            logger.Error($"process_order - Attempt {nTry + 1}. Sleep for {_sleep_when_tnb_get_orders_fail * (2 * nTry + 1)}ms. The order ({order.storeId}, {order.id}) -> response from Tienda nube: tnorder.id = {tnorder.id}");
                            if (tnorder.code == 401)
                            {
                                logger.Error($"process_order - ({order.storeId}, {order.id} -> code: {tnorder.code}) - Can not Get order from TiendaNube. Message: {tnorder.message}, descrip: {tnorder.description}, DB seller id: {order.seller.id}, seller token: {order.seller.access_token}");
                                Seller dbseller = await sellerRepository.FindByTNUserIdForceAsync(order.storeId.ToString());
                                if (order.seller.id != dbseller.Id)
                                {
                                    order.seller.id = dbseller.Id;
                                    order.seller.causerid = dbseller.CAUserId;
                                    order.seller.code = dbseller.TNCode;
                                    order.seller.access_token = dbseller.TNAccessToken;
                                    order.seller.createdOn = dbseller.CreatedOn;
                                    logger.Information($"process_order - getted new seller from repository: id = {order.seller.id}, TNUserId = {dbseller.TNUserId}, CAUserId = {order.seller.causerid}, token = {order.seller.access_token}");
                                    continue; // Skeeping Sleep
                                }
                                else
                                {
                                    logger.Error($"process_order - ({order.storeId}, {order.id}) - getted some seller from repository: id = {order.seller.id}, TNUserId = {order.storeId}, CAUserId = {order.seller.causerid}, token = {order.seller.access_token}");
                                    OPStore news = await updateToken(dbseller);
                                    if (news != null)
                                    {
                                        order.seller.id = news.id;
                                        order.seller.causerid = news.causerid;
                                        order.seller.code = news.code;
                                        order.seller.access_token = news.access_token;
                                        order.seller.createdOn = news.createdOn;
                                        logger.Information($"process_order - ({order.storeId}, {order.id}) - getted new token from tiendanube: id = {order.seller.id}, TNUserId = {dbseller.TNUserId}, CAUserId = {order.seller.causerid}, token = {order.seller.access_token}");
                                        continue; // Skeeping Sleep
                                    }
                                    logger.Error($"process_order - ({order.storeId}, {order.id}) can not got a new token!");
                                }
                            }
                            else
                                logger.Error($"process_order - ({order.storeId}, {order.id} -> code: {tnorder.code}) - Can not Get order from TiendaNube. Message: {tnorder.message}, descrip: {tnorder.description}");
                            Thread.Sleep(_sleep_when_tnb_get_orders_fail * (2 * nTry + 1));
                        }
                        else if (sProc != null)
                            sProc = null;
                    }
                    catch (Exception ex)
                    {
                        tnorder = null;
                        sProc = new OrderStatus($"id:{order.id}", true, $"{ex.Message} buscando la orden en Tiendanube. Por favor intente más tarde");
                        logger.Error($"process_order - Attempt {nTry + 1}. Sleep for {_sleep_when_tnb_get_orders_exception * (2 * nTry + 1)}ms. Exception executing: Get Order ID ({order.storeId}, {order.id}) Exception: {ex.Message} -> {ex.StackTrace}");
                        Thread.Sleep(_sleep_when_tnb_get_orders_exception * (2 * nTry + 1));
                    }
                }

                if (tnorder != null)
                {
                    if (tnorder.code != 0)
                    {
                        switch (tnorder.code)
                        {
                            case 401: // Unauthorized, Invalid access token
                                sProc = new OrderStatus($"id:{order.id}", true, "No se puede recuperar esta orden de Tiendanube. Token expirado. Intente más tarde");
                                break;
                            case 404: // Not Found, Order not found
                                sProc = new OrderStatus($"id:{order.id}", true, "No se puede recuperar esta orden de Tiendanube.");
                                break;
                            case 429: // Too Many Requests, Too many requests from you
                                sProc = new OrderStatus($"id:{order.id}", true, "No se pudo recuperar la orden de Tiendanube. Intente más tarde");
                                break;
                            case 500: // Internal Server Error, <null>
                                sProc = new OrderStatus($"id:{order.id}", true, "No se pudo recuperar la orden de Tiendanube. Intente más tarde. Error en el servidor de Tiendanube");
                                break;
                            default:
                                sProc = new OrderStatus($"id:{order.id}", true, $"no se pudo recuperar la orden de Tiendanube. {tnorder.code}:{tnorder.message}");
                                break;
                        }
                        logger.Error($"process_order - ({order.storeId}, {order.id} -> {tnorder.code}) - Can not Get order from TiendaNube. Message: {tnorder.message}, descrip: {tnorder.description}");
                    }
                    else if (!IsAllowedOrder(tnorder))
                    {
                        sProc = new OrderStatus(tnorder.number, true, "Esta orden no corresponde a un envío del Correo Argentino");
                        logger.Error($"process_order - The order ({order.storeId}, {order.id} -> {tnorder.number}) not is for Correo Argentino: " +
                            $"\"shipping_pickup_type\": \"{tnorder.shipping_pickup_type}\", \"shipping_option\": \"{tnorder.shipping_option}\", " +
                            $"\"shipping_option_code\": \"{tnorder.shipping_option_code}\", " +
                            $"\"shipping_pickup_details.name\": \"{tnorder.shipping_pickup_details?.name}\"");
                    }

                    if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug) && tnorder.id != 0)
                        logger.Debug($"process_order ->> Get Order ID ({order.storeId}, {order.id}): {JsonConvert.SerializeObject(tnorder)}");
                    else if (tnorder.id == 0)
                        logger.Error($"process_order ->> Get Order ID ({order.storeId}, {order.id}) return id = 0: {JsonConvert.SerializeObject(tnorder)}");
                    else
                        logger.Information($"process_order ->> Get Order ID ({order.storeId}, {order.id} -> {tnorder.number}) ...");

                    if (sProc != null)
                        tnorder = null;
                }
            }
            catch (Exception ex)
            {
                sProc = new OrderStatus($"id:{order.id}", true, "Excepcion buscando la orden id en Tiendanube");
                logger.Error($"process_order ->> Get Order ID ({order.storeId}, {order.id}) Exception: {ex.Message} -> {ex.StackTrace}");
            }

            if (sProc == null)
            {
                try
                {
                    var tnorders = new List<OrderResponseModel>();
                    tnorders.Add(tnorder);
                    var requestToDatabase = new AddNewOrdersToDatabaseRequest()
                    {
                        Orders = tnorders,
                        seller_id = order.seller.id,
                        seller_causerid = order.seller.causerid,
                        seller_tnstoreid = order.storeId
                    };

                    /// TODO: Commented because faild with multithreads:
                    await mediator.Send(requestToDatabase);
                    logger.Information($"process_order - The ({order.storeId}, {order.id} -> {tnorder.number}) orders save successful!");
                }
                catch (Exception ex)
                {
                    sProc = new OrderStatus(tnorder.number, true, "DB error en el plugin Tiendanube");
                    logger.Error($"process_order - Exception cathed saving to DB ({order.storeId}, {order.id} -> {tnorder.number}): {ex.Message}\n -> {ex.StackTrace}");
                }


                RegisterSendModelEx caorder = null;
                try
                {
                    AddressBase shippingAddress = null;

                    if (tnorder.shipping_address != null || (tnorder.shipping_pickup_type == ShippingOptions.Pickup && tnorder.shipping_pickup_details != null))
                    {
                        if (tnorder.shipping_pickup_type == ShippingOptions.Ship)
                        {
                            if (tnorder.shipping_address != null && tnorder.shipping_address.country != null && tnorder.shipping_address.country.ToUpper() == "AR")
                            {
                                shippingAddress = new AddressBase()
                                {
                                    streetNumber = tnorder.shipping_address.number,
                                    streetName = tnorder.shipping_address.address,
                                    postalCode = tnorder.shipping_address.zipcode,
                                    locality = tnorder.shipping_address.locality,
                                    city = tnorder.shipping_address.city,
                                    floor = tnorder.shipping_address.floor != null ? 
                                                tnorder.shipping_address.floor.Length < 51 ? tnorder.shipping_address.floor : tnorder.shipping_address.floor.Substring(0, 50) : null,
                                    apartment = null,
                                    provinceCode = Shared.Province.GetProvince(tnorder.shipping_address.province).ToString()
                                };
                            }
                            else if (tnorder.shipping_address != null && (tnorder.shipping_address.country == null || tnorder.shipping_address.country.ToUpper() != "AR"))
                            {
                                sProc = new OrderStatus(tnorder.number, true, "Correo Argentino no realiza servicios internacionales a través de MiCorreo");
                                logger.Error($"process_order - Error Shipping address ({order.storeId}, {order.id} -> {tnorder.number}): Country {tnorder.shipping_address.country}");
                            }
                            else
                            {
                                sProc = new OrderStatus(tnorder.number, true, "La dirección de envio está incompleta");
                                logger.Error($"process_order - Error Shipping address ({order.storeId}, {order.id} -> {tnorder.number}): address {tnorder.shipping_address}");
                            }
                        }
                        else
                        {
                            if (tnorder.shipping_pickup_details != null && tnorder.shipping_pickup_details.address != null && tnorder.shipping_address != null
                                    && tnorder.shipping_address.country.ToUpper() == "AR")
                            {
                                shippingAddress = new AddressBase()
                                {
                                    streetNumber = tnorder.shipping_pickup_details.address.number,
                                    streetName = tnorder.shipping_pickup_details.address.address,
                                    postalCode = tnorder.shipping_pickup_details.address.zipcode,
                                    locality = tnorder.shipping_pickup_details.address.locality,
                                    city = tnorder.shipping_pickup_details.address.city,
                                    floor = tnorder.shipping_pickup_details.address.floor != null ?
                                                tnorder.shipping_pickup_details.address.floor.Length < 51 ? tnorder.shipping_pickup_details.address.floor : tnorder.shipping_pickup_details.address.floor.Substring(0, 50) : null,
                                    apartment = null,
                                    provinceCode = Shared.Province.GetProvince(tnorder.shipping_pickup_details.address.province).ToString()
                                };
                            }
                            else if (tnorder.shipping_pickup_details != null && tnorder.shipping_pickup_details.address != null && (tnorder.shipping_address == null 
                                    || tnorder.shipping_address.country == null || tnorder.shipping_pickup_details.address.country.ToUpper() != "AR"))
                            {
                                sProc = new OrderStatus(tnorder.number, true, "Correo Argentino no realiza servicios internacionales a través de MiCorreo");
                                logger.Error($"process_order - Error Shipping pickup address ({order.storeId}, {order.id} -> {tnorder.number}): Country {tnorder.shipping_pickup_details.address.country}");
                            }
                            else
                            {
                                sProc = new OrderStatus(tnorder.number, true, "La dirección de envío está incompleta");
                                logger.Error($"process_order - Error Shipping pickup address ({order.storeId}, {order.id} -> {tnorder.number}): tnorder.shipping_pickup_details {tnorder.shipping_pickup_details}");
                            }
                        }
                    }
                    else
                    {
                        logger.Error($"process_order - ({order.storeId}, {order.id} -> {tnorder.number}) - domEntrega is not setted tnorder: {JsonConvert.SerializeObject(tnorder)}");
                        sProc = new OrderStatus(tnorder.number, true, "Problemas con la dirección de envío");
                    }

                    if (sProc == null)
                    {
                        int[] x = null;
                        try
                        {
                            Algorith_calc_volume volume = new Algorith_calc_volume();
                            x = volume.Stack(tnorder.products);
                        }
                        catch (Exception ex)
                        {
                            logger.Error($"process_order - ({order.storeId}, {order.id} -> {tnorder.number}) - {tnorder.products.Count()} Error calculating volume for product: {ex.Message}\n{ex.StackTrace}");
                            foreach (ProductsModel prod in tnorder.products)
                                logger.Error($"process_order - ({order.storeId}, {order.id} -> {tnorder.number}) - calculating prod: ({prod.width}, {prod.depth}, {prod.height}) x {prod.quantity}");
                            x = new int[3] { 1, 1, 1 };
                        }

                        var productType = tnorder.shipping_option_code.StartsWith("standard") ? "CP" : tnorder.shipping_option_code.Substring(0, 2);
                        var deliveryType = tnorder.shipping_pickup_type == ShippingOptions.Ship ? "D" : "S";

                        Shipping shipping = new Shipping()
                        {
                            productType = productType,
                            deliveryType = deliveryType,
                            //sucursal = item?.shipping_store_branch_name ?? item?.shipping_pickup_details?.name 
                            agency = deliveryType == "S" ? tnorder.shipping_option_reference : tnorder.shipping_store_branch_name ?? tnorder.shipping_pickup_details?.name,
                            address = shippingAddress,
                            declaredValue = (float)tnorder.products.Sum(x => Convert.ToDouble(x.price, new CultureInfo("en-US")) * x.quantity),
                            length = x[0],
                            width = x[1],
                            height = x[2],
                            weight = (int)Math.Ceiling(tnorder.products.Sum(x => Convert.ToDouble(x.weight, new CultureInfo("en-US")) * x.quantity) * 1000)
                        };

                        string name = "";
                        { // set name to shipping:
                            string lastname = null;
                            if (!string.IsNullOrEmpty(tnorder.shipping_address?.last_name))
                                lastname = tnorder.shipping_address.last_name.Trim();
                            string firstname = null;
                            if (!string.IsNullOrEmpty(tnorder.shipping_address?.first_name))
                                firstname = tnorder.shipping_address.first_name.Trim();

                            if (string.IsNullOrEmpty(lastname) && string.IsNullOrEmpty(firstname))
                            {
                                if (!string.IsNullOrEmpty(tnorder.shipping_address?.name))
                                    name = tnorder.shipping_address.name.Trim();
                            }
                            else if (!string.IsNullOrEmpty(lastname) && !string.IsNullOrEmpty(firstname))
                                name = lastname + ", " + firstname;
                            else if (!string.IsNullOrEmpty(lastname))
                                name = lastname;
                            else
                                name = firstname;
                        }
                        if (string.IsNullOrEmpty(name))
                            name = tnorder.contact_name;

                        logger.Debug($"process_order - ({order.storeId}, {order.id} -> {tnorder.number}) name to shipping address = '{name}'");

                        if (shippingAddress != null && !string.IsNullOrEmpty(shippingAddress.floor))
                            shippingAddress.floor = normalizeDeliveryAddressFloor(shippingAddress.floor.Trim());

                        var recipient = new Recipient()
                        {
                            //celular = item.shipping_address != null ? item?.shipping_address?.phone : item.customer.default_address.phone,
                            cellPhone = tnorder.contact_phone,
                            //telefono = item.shipping_address != null ? item?.shipping_address?.phone : item.customer.default_address.phone,
                            phone = tnorder.contact_phone,
                            name = name,
                            email = tnorder.contact_email,
                        };

                        caorder = new RegisterSendModelEx()
                        {
                            order = tnorder.number,
                            send = new RegisterSendModel()
                            {
                                customerId = order.seller.causerid,
                                extOrderId = tnorder.id.ToString(),
                                orderNumber = tnorder.number,
                                sender = new Sender(),
                                recipient = recipient,
                                shipping = shipping
                            }
                        };
                    }
                }
                catch (Exception ex)
                {
                    StackTrace st = new StackTrace(ex, true);
                    sProc = new OrderStatus(tnorder.number, true, $"Error [{st.GetFrame(0)?.GetFileLineNumber()}] generando la orden para el Correo Argentino");
                    logger.Error($"process_order - Exception cathed saving to DB line number {st.GetFrame(0)?.GetFileLineNumber()} ({order.storeId}, {order.id} -> {tnorder.number}): {ex.Message}\n-> {ex.StackTrace}");
                }

                if (caorder != null)
                {
                    try
                    {
                        logger.Information($"process_order - ({order.storeId}, {order.id} -> {caorder.order}): Sending order to Cotizador: {JsonConvert.SerializeObject(caorder.send)}");

                        RequestsHTTP.Models.ApiMiCorreo.RegisterSendResponseModel respCA = await correoArgentinoRequestService.importShipping($"process_order ({order.storeId},{order.id})", caorder.send);

                        if (respCA == null)
                        {
                            sProc = new OrderStatus(caorder.order, true, "Error de conexión con el importador, intente más tarde");
                            logger.Error($"process_order - ({order.storeId}, {order.id} -> {caorder.order}): Cotizador response for order: {JsonConvert.SerializeObject(respCA)}");

                            Order reporder = await orderRepository.FindByCAOrderIdAsync(caorder.send.extOrderId);
                            reporder.ErrorCode = "400";
                            reporder.ErrorDescription = "Server Not found!";

                            orderRepository.Update(reporder);
                        }
                        else if (respCA.code != 0)
                        {
                            sProc = new OrderStatus(caorder.order, true, $"{respCA.message}");
                            logger.Error($"process_order - ({order.storeId}, {order.id} -> {caorder.order}): error Cotizador response for order: {JsonConvert.SerializeObject(respCA)}");

                            Order reporder = await orderRepository.FindByCAOrderIdAsync(caorder.send.extOrderId);
                            reporder.ErrorCode = respCA.code.ToString();
                            reporder.ErrorDescription = 50 < respCA.message.Length ? respCA.message.Substring(0, 50) : respCA.message;

                            orderRepository.Update(reporder);
                        }
                        else
                        {
                            logger.Information($"process_order - ({order.storeId}, {order.id} -> {caorder.order}): Cotizador response for order: {JsonConvert.SerializeObject(respCA)}");
                            sProc = new OrderStatus(caorder.order, false, "Importacion exitosa!");
                        }

                        await orderRepository.SavaChangesContext();
                    }
                    catch (Exception ex)
                    {
                        sProc = new OrderStatus(caorder.order, true, ex.Message);
                        logger.Error($"process_order - ({order.storeId}, {order.id} -> {caorder.order}): exception: {ex.Message}\n -> {ex.StackTrace}");
                        logger.Error($"process_order ->> TiendaNube GET orders ({order.storeId}, {order.id}): {JsonConvert.SerializeObject(tnorder)}");
                    }
                }
            }

            logger.Information("End process_order for store id ({0}, {1})", order.storeId, order.id);
            return sProc;
        }



        private async Task<OPStore> updateToken(Seller seller)
        {
            bool bloop = false;
            bool bvalue = false;
            OPStore rval = null;
            while (_sellers_updateToken.TryGetValue(seller.Id, out bvalue))
            {
                bloop = true;
                Thread.Sleep(500);
            }
            if (bloop)
                return rval;

            _sellers_updateToken.TryAdd(seller.Id, true);

            try
            {
                AuthorizeResponseModel rspauth = await tiendaNubeRequestService.GetTiendaNubeAuthorization("updateToken", seller.TNCode);
                if (rspauth.access_token != null)
                {
                    var modelSeller = new Seller()
                    {
                        Id = seller.Id,
                        TNCode = seller.TNCode,
                        TNAccessToken = rspauth.access_token,
                        TNTokenType = rspauth.token_type,
                        TNUserId = rspauth.user_id,
                        TNScope = rspauth.scope,
                        CAUserId = seller.CAUserId,
                        CAUserDate = seller.CAUserDate,
                        CADocTypeId = seller.CADocTypeId,
                        CADocNro = seller.CADocNro,
                        CreatedOn = seller.CreatedOn,
                        UpdatedOn = DateTime.Now,
                        DocumentType = seller.DocumentType
                    };
                    sellerRepository.Update(modelSeller);

                    rval = new OPStore(modelSeller);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"updateToken Exception: - ({seller.Id}, {seller.TNUserId}, {seller.CAUserId}) -> code: {seller.TNCode} invalid token: {seller.TNAccessToken}: {ex.Message}");
            }

            _sellers_updateToken.TryRemove(seller.Id, out bvalue);
            return rval;
        }


        private static bool IsAllowedOrder(OrderResponseModel order)
        {
            //bool isAllowed;
            //if(order.shipping_pickup_type == "ship")//domicilio
            //{
            //    isAllowed = order.shipping_option.StartsWith(ShippingOptions.ShippingName) && 
            //        (order.shipping_option_code == ShippingOptions.StandardCode || order.shipping_option_code == ShippingOptions.StandardPickupCode);
            //}
            //else//sucursal
            //{
            //    isAllowed = order.shipping_pickup_details.name.StartsWith(ShippingOptions.ShippingName) &&
            //        (order.shipping_option_code == ShippingOptions.StandardCode || order.shipping_option_code == ShippingOptions.StandardPickupCode);
            //}

            //return isAllowed;
            
            return ShippingOptions.Options.Exists(o => o.code == order.shipping_option_code) || order.shipping_option_code.StartsWith("standard");
        }



        private static string normalizeDeliveryAddressFloor(string floor)
        {
            if (string.IsNullOrEmpty(floor))
                return floor;

            string vret = null;
            string []splspace = floor.Split(' ');
            foreach (string selement in splspace)
            {
                if (!string.IsNullOrEmpty(selement))
                {
                    string se = selement.Trim();
                    if (!string.IsNullOrEmpty(se))
                    {
                        if (!string.IsNullOrEmpty(vret))
                            vret += " ";
                        vret += se;
                    }
                }
            }
            return vret;
        }
    }
}
