using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.Shared.Configuration;
using Core.V1.TiendaNube.GetBranchOffices.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Serilog;
using System.Diagnostics;
using Dimension = Core.RequestsHTTP.Models.ApiMiCorreo.Dimension;

namespace Core.V1.TiendaNube.GetBranchOffices
{
    public class GetBranchOfficesRequestHandler : IRequestHandler<GetBranchOfficesRequest, RatesModelResponse>
    {
        private readonly ISellerRepository sellerRepository;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly IAgencies agencies;
        // private readonly MinMaxDeliveryDays minMaxDeliveryDays;
        private readonly ILogger logger;

        private static readonly DateTime _initService = DateTime.Now;
        private static DimensionLimits _dimLimits = null;
        private static DateTime _dimLimits_validTo = DateTime.Now;

        public static DateTime GetInitializeService()
        {
            return _initService;
        }

        public GetBranchOfficesRequestHandler(
            ISellerRepository sellerRepository,
            IApiMiCorreo correoArgentinoRequestService,
            IAgencies agencies,
            MinMaxDeliveryDays minMaxDeliveryDays,
            ILogger logger)
        {
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.correoArgentinoRequestService = correoArgentinoRequestService ?? throw new ArgumentNullException(nameof(correoArgentinoRequestService));
            this.agencies = agencies ?? throw new ArgumentNullException(nameof(agencies));
            // this.minMaxDeliveryDays = minMaxDeliveryDays ?? throw new ArgumentNullException(nameof(minMaxDeliveryDays));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private bool ValidateDimensionsAsync(Dimension dim)
        {
            try
            {
                if (_dimLimits == null || _dimLimits_validTo < DateTime.Now)
                {
                    Task<DimensionLimits> taskdl = correoArgentinoRequestService.GetDimensionsLimits("GetBrachOffices");
                    if (taskdl != null)
                    {
                        _dimLimits = taskdl.Result;
                        if (19 < _dimLimits.validTo.Length) // Trunc Timezone
                            _dimLimits.validTo = _dimLimits.validTo[..19];

                        _dimLimits_validTo = DateTime.ParseExact(_dimLimits.validTo, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                    }
                    else
                        _dimLimits = null;
                }
            }
            catch (Exception ex)
            {
                _dimLimits = null;
                logger.Error($"api/TiendaNubeApi/GetBrachOffices/GetDimensionsLimits: {ex.Message}");
            }


            if (_dimLimits == null)
                return true;

            if (0 < _dimLimits.maxWeight && (dim.weight < _dimLimits.minWeight || _dimLimits.maxWeight < dim.weight))
                return false;

            if (0 < _dimLimits.maxLength && (_dimLimits.maxLength < dim.length || _dimLimits.maxLength < dim.width || _dimLimits.maxLength < dim.height))
                return false;

            if (0 < _dimLimits.maxVolumetricLength && (_dimLimits.maxVolumetricLength < dim.length + dim.width + dim.height))
                return false;

            return true;
        }

        public async Task<RatesModelResponse> Handle(GetBranchOfficesRequest request, CancellationToken cancellationToken)
        {
            var te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/TiendaNubeApi/GetBrachOffices...");

            if (request == null || request.destination == null || string.IsNullOrEmpty(request.destination.province))
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/GetBrachOffices elapsed time {0}ms. Province destination is null", te.ElapsedMilliseconds);
                return null;
            }

            if (request.destination.country == null || !request.destination.country.ToUpper().Equals("AR"))
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/GetBrachOffices elapsed time {0}ms. Country destination is not AR", te.ElapsedMilliseconds);
                return null;
            }

            // FIXME: Don't forget to remove this line
            // var seller = new Seller() { CAUserId = "0000550395" };
            var seller = await this.sellerRepository.FindByTNUserIdAsync(request.store_id.ToString());

            if (seller == null)
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/GetBrachOffices elapsed time {0}ms. Seller DB not found store_id = {1}", te.ElapsedMilliseconds, request.store_id);
                return null;
            }

            int[] x = null;
            try
            {
                var volume = new Algorith_calc_volume();
                x = volume.Stack(request.items);
            }
            catch (Exception ex)
            {
                logger.Error($"GetBrachOffices - {request.items.Count()} - Error calculating volume for items: {ex.Message}\n{ex.StackTrace}");
                foreach (ItemModel item in request.items)
                {
                    if (item.dimensions != null)
                        logger.Error($"GetBrachOffices - calculating item: ({item.dimensions.width}, {item.dimensions.depth}, {item.dimensions.height}) x {item.quantity}");
                    else
                        logger.Error($"GetBrachOffices - calculating item: (item.dimensions == null) x {item.quantity}");
                }
                x = new int[3] { 1, 1, 1 };
            }

            var dimension = new Dimension()
            {
                weight = request.items.Sum(x => x.grams * x.quantity).Value,
                length = x[0],
                width = x[1],
                height = x[2]
            };

            if (!ValidateDimensionsAsync(dimension))
            {
                te.Stop();
                logger.Error($"End service api/TiendaNubeApi/GetBrachOffices elapsed time {te.ElapsedMilliseconds}ms. Invalid dimensions found: store_id = {request.store_id}: ({dimension.weight}, {dimension.length}, {dimension.width}, {dimension.height})");
                return null;
            }

            var getPriceModel = new ShipPriceModel()
            {
                customerId = seller.CAUserId,
                postalCodeDestination = request.destination.postal_code,
                postalCodeOrigin = request.origin.postal_code,
                dimensions = dimension
            };

            ShipPriceResponseModel response = await this.correoArgentinoRequestService.Rates("GetBrachOffices", getPriceModel);

            IEnumerable<BranchOffice> branches = null;

            if (response.rates != null)
            {
                if (response.rates.Exists(r => r.deliveredType == "S"))
                {
                    branches = await agencies.findAgenciesAsync(request.destination.province, request.destination.postal_code, seller.CAUserId);
                    logger.Information("GetBrachOffices - {0} Branches found for postal code {1}", branches != null ? branches.Count() : 0, request.destination.postal_code);
                }
            }
            else
            {
                logger.Information("GetBrachOffices - Rates not found for postal code {0}!", request.destination.postal_code);
            }

            var listnewBranches = new RatesModelResponse();

            foreach (var rate in response.rates)
            {
                if (rate.deliveredType == "S" && (branches != null && branches.Any()))
                {
                    foreach (var branch in branches)
                    {
                        listnewBranches.rates.Add(new RatesResponse()
                        {
                            name = rate.productName + " - " + branch.name,
                            type = "pickup",
                            code = rate.productType + rate.deliveredType,
                            price = (double)(decimal)rate.price,
                            price_merchant = (double)(decimal)rate.price,
                            currency = "ARS",
                            phone_required = true,
                            // min_delivery_date = DateTime.Today.AddDays(minMaxDeliveryDays.MinDays),
                            // max_delivery_date = DateTime.Today.AddDays(minMaxDeliveryDays.MaxDays),


                            min_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMin)),
                            max_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMax)),

                            //min_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMin)).ToString("o"),
                            //max_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMax)).ToString("o"),
                            availability = true,
                            reference = branch.code,
                            hours = branch.schedules,
                            address = new AddressModel()
                            {
                                address = branch.streetName,
                                number = branch.streetNumber,
                                floor = null,
                                city = branch.city,
                                locality = branch.locality,
                                province = branch.province,
                                country = "AR",
                                zipcode = branch.spostalCode,
                                longitude = branch.longitude,
                                latitude = branch.latitude,
                                phone = branch.phone,
                            }
                        });
                    }

                }
                else if (rate.deliveredType == "D")
                {
                    listnewBranches.rates.Add(new RatesResponse()
                    {
                        name = rate.productName + " - Envio a domicilio",
                        type = "ship",
                        code = rate.productType + rate.deliveredType,
                        price = (double)(decimal)rate.price,
                        price_merchant = (double)(decimal)rate.price,
                        currency = "ARS",
                        phone_required = true,
                        // min_delivery_date = DateTime.Today.AddDays(minMaxDeliveryDays.MinDays),
                        // max_delivery_date = DateTime.Today.AddDays(minMaxDeliveryDays.MaxDays),

                        min_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMin)),
                        max_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMax)),

                        //min_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMin)).ToString("o"),
                        //max_delivery_date = DateTime.Today.AddDays(Convert.ToDouble(rate.deliveryTimeMax)).ToString("o"),
                        availability = true,
                        reference = "ref123"
                    });
                }
            }

            te.Stop();

            logger.Information($"End service api/TiendaNubeApi/GetBrachOffices elapsed time {te.ElapsedMilliseconds}ms");

            return listnewBranches;
        }
    }
}
