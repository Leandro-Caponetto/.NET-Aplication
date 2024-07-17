using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.Shared.Configuration;
using Core.V1.CorreoArgentino.UpdateTrackingNumber.Models;
using Core.V1.TiendaNube.RegisterTienda.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Core.V1.CorreoArgentino.UpdateTrackingNumber
{
    public class UpdateTrackingNumberRequestHandler : IRequestHandler<UpdateTrackingNumberRequest, ErrorTrackingNumberModel>
    {
        private readonly UserManager<User> userManager;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly IDocumentTypeRepository documentTypeRepository;
        private readonly ITiendaNubeRequestService tiendaNubeRequestService;
        private readonly TrackingSettingsCorreoArgentino trackingSettingsCorreoArgentino;

        private readonly ILogger logger;

        public UpdateTrackingNumberRequestHandler(
            UserManager<User> userManager,
            IApiMiCorreo correoArgentinoRequestService,
            ISellerRepository sellerRepository,
            IDocumentTypeRepository documentTypeRepository,
            ITiendaNubeRequestService tiendaNubeRequestService,
            TrackingSettingsCorreoArgentino trackingSettingsCorreoArgentino,
            ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.correoArgentinoRequestService = correoArgentinoRequestService ?? throw new ArgumentNullException(nameof(correoArgentinoRequestService));
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
            this.tiendaNubeRequestService = tiendaNubeRequestService ?? throw new ArgumentNullException(nameof(tiendaNubeRequestService));
            this.trackingSettingsCorreoArgentino = trackingSettingsCorreoArgentino ?? throw new ArgumentNullException(nameof(trackingSettingsCorreoArgentino));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ErrorTrackingNumberModel> Handle(UpdateTrackingNumberRequest request, CancellationToken cancellationToken)
        {
            logger.Information("UpdateTrackingNumber - Finding order by idClient = " + request.customerId.ToString() + "; idPedidoPaqueteClient = " + request.extOrderId);
            try
            {
                var dborder = await sellerRepository.FindOrderAsync(request.customerId.ToString(), request.extOrderId, "OK");
                if (dborder == null)
                {
                    logger.Error("UpdateTrackingNumber - Order not found in DB for: idClient = " + request.customerId.ToString() + "; idPedidoPaqueteClient  = " + request.extOrderId);
                    return new ErrorTrackingNumberModel()
                    {
                        code = "11",
                        error = "No se encontro orden " + request.extOrderId + " para el cliente id = " + request.customerId
                    };
                }

                var old_seller = await sellerRepository.FindByIdAsync(dborder.SellerId);
                if (old_seller == null)
                    logger.Error($"UpdateTrackingNumber - old_seller is null. Finding seller by dborder.SellerId = {dborder.SellerId}");

                var seller = old_seller == null ? null : await sellerRepository.FindByTNUserIdForceAsync(old_seller.TNUserId);
                if (seller == null)
                {
                    logger.Error($"UpdateTrackingNumber - seller is null. Finding seller by old_seller.TNUserId = {old_seller.TNUserId}; idClient = {request.customerId.ToString()}; order.id = {dborder.Id}");
                    return new ErrorTrackingNumberModel()
                    {
                        code = "10",
                        error = "No hay ninguna tienda asociada para la orden: <idClient = " + request.customerId + "; idPedidoPaqueteCliente = " + request.extOrderId + ">"
                    };
                }
               /*
              logger.Information($"UpdateTrackingNumber - Finding seller by idClient = {request.customerId.ToString()}: old_seller.id = {old_seller.Id}, seller.id = {seller.Id}");

              var order = await tiendaNubeRequestService.GetOrderById("UpdateTrackingNumber", seller.TNAccessToken, seller.TNUserId, request.extOrderId);
              if (order.id == 0)
              {
                  logger.Error("UpdateTrackingNumber: Order not found on TiendaNube: idClient = " + request.customerId.ToString() + "  seller store (TNUserId) = " + seller.TNUserId + "; order.id = " + request.extOrderId);
                  return new ErrorTrackingNumberModel()
                  {
                      code = "20",
                      error = "No hay una orden disponible"
                  };
              }
              if (order.id.ToString() != request.extOrderId)
                  logger.Error("UpdateTrackingNumber - order.id = " + order.id.ToString() + " != " + request.extOrderId + " = request.idPedidoPaqueteCliente");
              */

                var orderUpdate = new UpdateOrderModel()
                {
                    shipping_tracking_number = request.trackingNumber,
                    shipping_tracking_url = trackingSettingsCorreoArgentino.UrlShipping + "id=" + request.trackingNumber,
                    notify_customer = true
                };

                var order = await tiendaNubeRequestService.UpdateTrackingNumber("UpdateTrackingNumber", seller.TNUserId, request.extOrderId, orderUpdate, seller.TNAccessToken);
                if (order.code != 0 && order.id == 0)
                {
                    logger.Error("UpdateTrackingNumber: Order not found on TiendaNube: idClient = " + request.customerId.ToString() + "  seller store (TNUserId) = " + seller.TNUserId + "; order.id = " + request.extOrderId);
                    return new ErrorTrackingNumberModel()
                    {
                        code = order.code.ToString(),
                        error = order.description != null ? order.message + " - " + order.description : order.message
                    };
                } else
                {
                    logger.Debug("UpdateTrackingNumber - OK storeId = " + seller.TNUserId + "; orderId = " + request.extOrderId + "; TN = " + request.trackingNumber);
                }

                //tiendaNubeRequestService.UpdateTrackingNumber("UpdateTrackingNumber", seller.TNUserId, request.extOrderId, orderUpdate, seller.TNAccessToken);
                //logger.Debug("UpdateTrackingNumber - OK storeId = " + seller.TNUserId +"; orderId = " + request.extOrderId + "; TN = " + request.trackingNumber);
            }
            catch (Exception ex)
            {
                logger.Error("UpdateTrackingNumber - Error updating tracking number for idClient = " + request.customerId.ToString() + "; idPedidoPaqueteClient = " + request.extOrderId 
                    + "\nMessage: " + ex.Message);
                logger.Error("UpdateTrackingNumber - Trace: " + ex.StackTrace);
                return new ErrorTrackingNumberModel()
                {
                    code = "0",
                    error = ex.Message
                };
            }




            return new ErrorTrackingNumberModel();
        }
    }
}
