using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.RequestsHTTP.Models.TiendaNube;
using Core.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.ShippingCarrier
{
    public class ShippingCarrierRequestHandler : IRequestHandler<ShippingCarrierRequest>
    {
        private readonly UserManager<User> userManager;
        private readonly ITiendaNubeRequestService tiendaNubeRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly ILogger logger;
        

        public ShippingCarrierRequestHandler(UserManager<User> userManager, ITiendaNubeRequestService tiendaNubeRequestService, ISellerRepository sellerRepository, ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.tiendaNubeRequestService = tiendaNubeRequestService ?? throw new ArgumentNullException(nameof(tiendaNubeRequestService));
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(ShippingCarrierRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = Stopwatch.StartNew();
            logger.Information($"Start service: TiendaNube/ShippingCarrier... code: {request.Code}");

            var clientData = await sellerRepository.FindByCodeAsync(request.Code);

            try
            {
                List<ShippingCarrierResponseModel> lscarrier = await tiendaNubeRequestService.GetTiendaNubeShippingCarriers("ShippingCarrier", clientData.TNAccessToken, clientData.TNUserId);

                if (lscarrier == null)
                    logger.Information($"ShippingCarrier - GET not Found: code {request.Code}, id {clientData.TNUserId}: <{clientData.TNAccessToken}>");
                else
                {
                    logger.Information($"ShippingCarrier - GET Found: code {request.Code}, id {clientData.TNUserId}, shipping carriers count = {lscarrier.Count}: <{clientData.TNAccessToken}>");
                    foreach (ShippingCarrierResponseModel icarrier in lscarrier)
                    {
                        logger.Information($"ShippingCarrier - calling delete for: id {clientData.TNUserId}, shipping carriers id = {icarrier.id}");
                        if (!await tiendaNubeRequestService.DeleteShippingCarrier("ShippingCarrier", clientData.TNAccessToken, clientData.TNUserId, icarrier.id.ToString()))
                            logger.Error($"ShippingCarrier - Error deleting for: id {clientData.TNUserId}, shipping carrier id: {icarrier.id}");
                    }
                }
            }
            catch (Exception ex)
            {
                try
                {
                    StackTrace st = new StackTrace(ex, true);
                    StackFrame sfr = st.GetFrame(0);
                    logger.Error($"ShippingCarrier - Error Getting shipping carrier id {clientData.TNUserId} {sfr.GetFileName()}[{sfr.GetFileLineNumber()}] - {sfr.GetMethod().Name} : {ex.Message}");
                }
                catch (Exception ex2)
                {
                    logger.Error($"ShippingCarrier - Error Getting shipping carrier id {clientData.TNUserId}: {ex.Message}, {ex2.Message}");
                }
            }

            ShippingCarrierResponseModel scarrier = await tiendaNubeRequestService.PostTiendaNubeShippingCarrier("ShippingCarrier", clientData.TNAccessToken, clientData.TNUserId);

            if (scarrier == null || scarrier.id == 0)
            {
                logger.Error($"ShippingCarrier - code {request.Code}, id {clientData.TNUserId}: <{clientData.TNAccessToken}>");
                if (scarrier == null)
                    logger.Error("ShippingCarrier - response id is null");
                else if (scarrier.id == 0)
                    logger.Error($"ShippingCarrier - response id is zero: {JsonConvert.SerializeObject(scarrier)}");
            }

            foreach(var option in ShippingOptions.Options)
            {
                await tiendaNubeRequestService.SetShippingCarrierOptions(
                    "ShippingCarrier", clientData.TNAccessToken, clientData.TNUserId, scarrier.id.ToString(), option);
            }

            te.Stop();

            logger.Information("End service TiendaNube/ShippingCarrier elapsed time {0}ms", te.ElapsedMilliseconds);

            return Unit.Value;
        }
    }
}
