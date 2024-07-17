using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using Core.Shared;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.AddNewOrdersToDatabase
{
    public class AddNewOrdersToDatabaseRequestHandler : IRequestHandler<AddNewOrdersToDatabaseRequest>
    {
        private readonly UserManager<User> userManager;
        private readonly ITiendaNubeRequestService tiendaNubeRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly IOrderRepository orderRepository;
        private readonly IProvinceRepository provinceRepository;
        private readonly IShippingTypeRepository shippingTypeRepository;
        private readonly ITNOrderStatusRepository tNOrderStatusRepository;
        private readonly ILogger logger;

        public AddNewOrdersToDatabaseRequestHandler(
            UserManager<User> userManager,
            ITiendaNubeRequestService tiendaNubeRequestService,
            ISellerRepository sellerRepository,
            IApiMiCorreo correoArgentinoRequestService,
            IOrderRepository orderRepository,
            IProvinceRepository provinceRepository,
            IShippingTypeRepository shippingTypeRepository,
            ITNOrderStatusRepository tNOrderStatusRepository,
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
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(AddNewOrdersToDatabaseRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start internal service: AddNewOrdersToDatabaseRequest...");

            if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                logger.Debug("request AddNewOrdersToDatabaseRequest {0} orders: " + JsonConvert.SerializeObject(request), request?.Orders?.Count());
            else
                logger.Information("request AddNewOrdersToDatabaseRequest {0} orders", request?.Orders?.Count());

            var listIds = new List<string>();

            foreach (var item in request.Orders)
            {
                listIds.AddRange(item.products.Select(x => x.product_id.ToString()));
            }

            //var listProductsNames = await tiendaNubeRequestService.GetProductNamesByIds(listIds.Select(x => x), request.Seller.TNAccessToken, request.Seller.TNUserId);

            var provinces = await provinceRepository.GetAll();
            var shippingTypeIds = await shippingTypeRepository.GetAll();
            var orderStatuses = await tNOrderStatusRepository.GetAll();


            if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                logger.Debug("provinces: " + JsonConvert.SerializeObject(provinces));
                logger.Debug("shippingTypeIds: " + JsonConvert.SerializeObject(shippingTypeIds));
                logger.Debug("orderStatuses: " + JsonConvert.SerializeObject(orderStatuses));
            }


            #region Create orders to save into database
            foreach (var ord in request.Orders)
            {
                var listProducts = new List<OrderProduct>();


                foreach (var y in ord.products)
                {
                    listProducts.Add(new OrderProduct()
                    {
                        TNDepth = Convert.ToDecimal(y.depth, new CultureInfo("en-US")),
                        TNHeight = Convert.ToDecimal(y.height, new CultureInfo("en-US")),
                        TNPrice = Convert.ToDecimal(y.price, new CultureInfo("en-US")),
                        TNProductId = y.product_id,
                        TNWidth = Convert.ToDecimal(y.width, new CultureInfo("en-US")),
                        //TNProductName = listProductsNames.FirstOrDefault(x => y.product_id.ToString() == x.id)?.name.es,
                        TNProductName = "",
                        TNQuantity = y.quantity,
                        TNWeight = Convert.ToDecimal(y.weight, new CultureInfo("en-US"))
                    });
                }

                var orderNueva = new Order();

                if (ord?.shipping_address == null)
                {
                    orderNueva = new Order()
                    {
                        CAIdClientId = request.seller_causerid,
                        CAOrderId = ord.id.ToString(),
                        OrderProducts = listProducts.ToList(),
                        TNTotalPrice = ord.products.Sum(q => Convert.ToDecimal(q.price, new CultureInfo("en-US"))),
                        TNTotalWeight = ord.products.Sum(q => Convert.ToDecimal(q.weight, new CultureInfo("en-US"))),
                        SellerId = request.seller_id,
                        ShippingTypeId = shippingTypeIds.FirstOrDefault(y => y.TypeName == ord.shipping_pickup_type).Id,
                        TNCreatedOn = ord.created_at,
                        TNOrderId = ord.id,
                        TNUpdatedOn = ord.updated_at,
                        TNOrderStatusId = orderStatuses.FirstOrDefault(y => y.StatusName == ord.status).Id,
                        TotalDepth = 0,
                        TotalHeight = 0,
                        TotalWidth = 0,
                        CreatedOn = DateTime.Now,
                        ErrorCode = "NA",
                        ErrorDescription = "OK"
                    };
                } else
                {
                    var ReceiverProvinceId = ord?.shipping_pickup_type == ShippingOptions.Ship ? provinces?.FirstOrDefault(y => y.Name.ToUpper() == ord?.shipping_address.province.ToUpper())?.Id : provinces.FirstOrDefault(y => y.Name.ToUpper() == ord?.shipping_pickup_details.address.province.ToUpper())?.Id;
                    var TNTotalPrice = ord?.products?.Sum(q => Convert.ToDecimal(q.price, new CultureInfo("en-US")));
                    var TNTotalWeight = ord?.products?.Sum(q => Convert.ToDecimal(q.weight, new CultureInfo("en-US")));
                    var ShippingTypeId = shippingTypeIds?.FirstOrDefault(y => y.TypeName == ord.shipping_pickup_type)?.Id;
                    var ord_status = ord?.status?.ToLower() == "cancelled" ? "canceled" : ord?.status.ToLower();
                    var TNOrderStatusId = orderStatuses?.FirstOrDefault(y => y.StatusName.ToLower() == ord_status)?.Id;
                    if (ReceiverProvinceId == null)
                        logger.Error("ReceiverProvinceId is null (CAIdClientId: {0}; CAOrderId: {1}; SellerId: {2})", request.seller_causerid, ord.id, request.seller_id);
                    if (TNTotalPrice == null)
                        logger.Error("TNTotalPrice is null (CAIdClientId: {0}; CAOrderId: {1}; SellerId: {2})", request.seller_causerid, ord.id, request.seller_id);
                    if (TNTotalWeight == null)
                        logger.Error("TNTotalWeight is null (CAIdClientId: {0}; CAOrderId: {1}; SellerId: {2})", request.seller_causerid, ord.id, request.seller_id);
                    if (ShippingTypeId == null)
                        logger.Error("ShippingTypeId is null (CAIdClientId: {0}; CAOrderId: {1}; SellerId: {2})", request.seller_causerid, ord.id, request.seller_id);
                    if (TNOrderStatusId == null)
                        logger.Error("TNOrderStatusId is null (CAIdClientId: {0}; CAOrderId: {1}; SellerId: {2})", request.seller_causerid, ord.id, request.seller_id);

                    orderNueva = new Order()
                    {
                        CAIdClientId = request.seller_causerid,
                        CAOrderId = ord.id.ToString(),
                        SenderProvinceId = null,
                        ReceiverCellPhone = ord?.shipping_address.phone,
                        ReceiverCASucursal = ord.shipping_pickup_type == ShippingOptions.Ship ? ord?.shipping_store_branch_name : ord?.shipping_pickup_details?.name,
                        ReceiverDpto = "",
                        ReceiverFloor = ord.shipping_pickup_type == ShippingOptions.Ship ? ord?.shipping_address.floor : ord.shipping_pickup_details.address.floor,
                        ReceiverHeight = 0,
                        ReceiverLocality = ord.shipping_pickup_type == ShippingOptions.Ship ? ord?.shipping_address.locality : ord.shipping_pickup_details.address.locality,
                        ReceiverMail = ord?.contact_email,
                        ReceiverName = ord?.contact_name,
                        ReceiverPhone = ord.shipping_pickup_type == ShippingOptions.Ship ? ord?.shipping_address.phone : ord.shipping_pickup_details.address.phone,
                        ReceiverPostalCode = ord.shipping_pickup_type == ShippingOptions.Ship ? ord?.shipping_address.zipcode : ord.shipping_pickup_details.address.zipcode,
                        ReceiverProvinceId = ReceiverProvinceId,
                        ReceiverStreet = ord.shipping_pickup_type == ShippingOptions.Ship ? ord?.shipping_address.address : ord.shipping_pickup_details.address.address,
                        OrderProducts = listProducts.ToList(),
                        TNTotalPrice = (decimal)TNTotalPrice,
                        TNTotalWeight = (decimal)TNTotalWeight,
                        SellerId = request.seller_id,
                        ShippingTypeId = (int)ShippingTypeId,
                        TNCreatedOn = ord.created_at,
                        TNOrderId = ord.id,
                        TNUpdatedOn = ord.updated_at,
                        TNOrderStatusId = (int)TNOrderStatusId,
                        TotalDepth = 0,
                        TotalHeight = 0,
                        TotalWidth = 0,
                        CreatedOn = DateTime.Now,
                        ErrorCode = "NA",
                        ErrorDescription = "OK"
                    };
                }

                this.orderRepository.Add(orderNueva);
            }
            #endregion

            te.Stop();
            logger.Information("End internal service AddNewOrdersToDatabaseRequest elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
