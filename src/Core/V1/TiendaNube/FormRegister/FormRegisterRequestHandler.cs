using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.FormRegister
{
    public class FormRegisterRequestHandler : IRequestHandler<FormRegisterRequest, string>
    {
        private readonly UserManager<User> userManager;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly IDocumentTypeRepository documentTypeRepository;
        private readonly ILogger logger;

        public FormRegisterRequestHandler(
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

        public async Task<string> Handle(FormRegisterRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/TiendaNubeApi/RegisterProcess...");

            //hacer registro contra MiCorreo
            NewUserModel newUser = new NewUserModel()
            {
                firstName = request.DocumentType != "DNI" ? request.CompanyName?.ToUpper() : request.FirstName?.ToUpper(), //EL NOMBRE / RAZON SOCIAL / APELLIDO TIENE Q SER ENVIADO EN MAYUSCULAS SI O SI
                lastName = request.LastName?.ToUpper(),
                email = request.Email,
                password = request.Password,
                documentType = request.DocumentType?.ToUpper(),
                documentId = request.DocumentNumber,
                phone = request.CellPhone,
                cellPhone = request.CellPhone,
                address = new AddressBase()
                {
                    streetName = request.Street,
                    streetNumber = request.StreetNumber,
                    floor = request.Floor,
                    apartment = request.Depto,
                    city = request.Location,
                    provinceCode = request.City,
                    postalCode = request.CPA
                }
            };

            var response = await correoArgentinoRequestService.Register("RegisterProcess", newUser);

            if (!string.IsNullOrEmpty(response.message))
            {
                te.Stop();
                logger.Error($"End service api/TiendaNubeApi/RegisterProcess {response.message} elapsed time {te.ElapsedMilliseconds}ms");
                return response.message;
            }

            var responseSeller = await sellerRepository.FindByCodeAsync(request.Code);

            if (responseSeller.Id == Guid.Empty)
            {
                te.Stop();
                logger.Error("End service api/TiendaNubeApi/RegisterProcess elapsed time {0}ms", te.ElapsedMilliseconds);
                throw new BusinessException("Error al tratar de traer el seller Id");
            }

            responseSeller.UpdatedOn = DateTime.Now;
            responseSeller.CAUserId = response.customerId;
            responseSeller.CADocNro = newUser.documentId;

            var responseDocumentType = await documentTypeRepository.FindByNameAsync(newUser.documentType.ToUpper());

            if (responseDocumentType != null)
                responseSeller.CADocTypeId = responseDocumentType.Id;

            sellerRepository.Update(responseSeller);

            te.Stop();
            logger.Information("End service api/TiendaNubeApi/RegisterProcess elapsed time {0}ms", te.ElapsedMilliseconds);
            return "";
        }
    }
}
