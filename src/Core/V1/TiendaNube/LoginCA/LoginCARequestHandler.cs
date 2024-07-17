using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.LoginCA
{
    public class LoginCARequestHandler : IRequestHandler<LoginCARequest, string>
    {
        private readonly UserManager<User> userManager;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly IDocumentTypeRepository documentTypeRepository;
        private readonly ILogger logger;

        public LoginCARequestHandler(
            UserManager<User> userManager,
            IApiMiCorreo correoArgentinoRequestService,
            ISellerRepository sellerRepository,
            IDocumentTypeRepository documentTypeRepository,
            ILogger logger)
        {
            this.userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            this.correoArgentinoRequestService = correoArgentinoRequestService ?? throw new ArgumentNullException(nameof(correoArgentinoRequestService));
            this.sellerRepository = sellerRepository ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.documentTypeRepository = documentTypeRepository ?? throw new ArgumentNullException(nameof(documentTypeRepository));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<string> Handle(LoginCARequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/TiendaNubeApi/LoginUserCA...");

            var responseCorreo = await correoArgentinoRequestService.Login("LoginUserCA", new LoginModel(request.Username, request.Password));

            if (responseCorreo == null)
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/LoginUserCA response ApiMiCorreo is null elapsed time {0}ms", te.ElapsedMilliseconds);
                return "Service ApiMiCorreo/login Unavailable";
            }
            else if (responseCorreo.customerId == null)
            {
                te.Stop();
                logger.Error($"End service api/TiendaNubeApi/LoginUserCA response ApiMiCorreo without customerId {responseCorreo.message} elapsed time {te.ElapsedMilliseconds}ms");
                return "Service ApiMiCorreo/login error: " + responseCorreo.message;
            }

            var responseSeller = await sellerRepository.FindByCodeAsync(request.Code);

            if (responseSeller.Id == Guid.Empty)
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/LoginUserCA elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new BusinessException("No se encontro el seller Id");
            }
            responseSeller.UpdatedOn = DateTime.Now;
            responseSeller.CAUserId = responseCorreo.customerId;
            responseSeller.CADocNro = "Unavailable";

            /*var responseDocumentType = await documentTypeRepository.FindByNameAsync(responseCorreo.documentType.ToUpper());

            if(responseDocumentType != null)
                responseSeller.CADocTypeId = responseDocumentType.Id;//*/

            sellerRepository.Update(responseSeller);

            te.Stop();
            logger.Information("End service api/TiendaNubeApi/LoginUserCA elapsed time {0}ms", te.ElapsedMilliseconds);
            return null;
        }
    }
}
