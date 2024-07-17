using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.V1.TiendaNube.RegisterTienda.Models;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Serilog;

namespace Core.V1.TiendaNube.RegisterTienda
{
    public class RegisterTiendaRequestHandler : IRequestHandler<RegisterTiendaRequest>
    {
        private readonly UserManager<User> userManager;
        //private static readonly HttpClient client = new HttpClient();
        private readonly ITiendaNubeRequestService tiendaNubeRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly ILogger logger;

        public RegisterTiendaRequestHandler(
            UserManager<User> userManager,
            ITiendaNubeRequestService tiendaNubeRequestService,
            ISellerRepository sellerRepository,
            ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.tiendaNubeRequestService = tiendaNubeRequestService ?? throw new ArgumentNullException(nameof(tiendaNubeRequestService));
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Unit> Handle(RegisterTiendaRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/TiendaNubeApi/AutorizeNewTienda...");

            var response = await tiendaNubeRequestService.GetTiendaNubeAuthorization("AutorizeNewTienda", request.Code);

            if (response.access_token == null)
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/AutorizeNewTienda elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new BusinessException("No hubo respuesta del servidor intente nuevamente");
            }

            var modelSeller = new Seller()
            {
                TNAccessToken = response.access_token,
                TNCode = request.Code,
                TNScope = response.scope,
                TNTokenType = response.token_type,
                TNUserId = response.user_id,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
            };

            sellerRepository.Add(modelSeller);
            logger.Information($"api/TiendaNubeApi/AutorizeNewTienda - New seller add to data base: id = {modelSeller.Id}, TNUserId = {modelSeller.TNUserId}, CAUserId = {modelSeller.CAUserId}, CreatedOn = {modelSeller.CreatedOn}");

            te.Stop();
            logger.Information("End service api/TiendaNubeApi/AutorizeNewTienda elapsed time {0}ms", te.ElapsedMilliseconds);
            return Unit.Value;
        }
    }
}
