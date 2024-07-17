using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using Core.Exceptions;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1.TiendaNube.DocumentTypeList
{
    public class DocumentTypeListRequestHandler : IRequestHandler<DocumentTypeListRequest, IList<DocumentType>>
    {
        private readonly UserManager<User> userManager;
        private readonly IApiMiCorreo correoArgentinoRequestService;
        private readonly ISellerRepository sellerRepository;
        private readonly IDocumentTypeRepository documentTypeRepository;
        private readonly ILogger logger;

        public DocumentTypeListRequestHandler(
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

        public async Task<IList<DocumentType>> Handle(DocumentTypeListRequest request, CancellationToken cancellationToken)
        {
            Stopwatch te = new Stopwatch();
            te.Start();
            logger.Information("Start service: api/TiendaNubeApi/RegisterUserCA...");

            var response = await documentTypeRepository.GetAll();

            te.Stop();
            logger.Information("End service api/TiendaNubeApi/RegisterUserCA elapsed time {0}ms", te.ElapsedMilliseconds);
            return response;
        }
    }
}
