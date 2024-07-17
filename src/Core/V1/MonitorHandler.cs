using Core.Data.Repositories;
using Core.Data.RequestServices;
using Core.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Core.V1
{

    public class MonitorRequest : IRequest<Monitor>
    {
        public MonitorRequest()
        {
        }
    }

    public class MonitorHandler : IRequestHandler<MonitorRequest, Monitor>
    {
        private readonly ISellerRepository sellerRepository;
        private readonly IApiMiCorreo correoArgentinoRequestService;

        public MonitorHandler(
            ISellerRepository sellerRepository,
            IApiMiCorreo correoArgentinoRequestService)
        {
            this.sellerRepository = sellerRepository
                ?? throw new ArgumentNullException(nameof(sellerRepository));
            this.correoArgentinoRequestService = correoArgentinoRequestService
                ?? throw new ArgumentNullException(nameof(correoArgentinoRequestService));
        }

        public async Task<Monitor> Handle(MonitorRequest request, CancellationToken cancellationToken)
        {
            var monitor = new Monitor();

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                //var seller = sellerRepository.FindByTNUserIdAsync("1");
                var seller = sellerRepository.FindByTNUserIdForceAsync("1");
                stopwatch.Stop();
                monitor.connection_to_db = stopwatch.ElapsedMilliseconds < 1000
                    ? "OK" : stopwatch.ElapsedMilliseconds < 3000
                    ? "Warning" : "Error";
            }
            catch (Exception ex)
            {
                monitor.connection_to_db = "ERROR: " + ex.Message;
                stopwatch.Stop();
            }

            monitor.connection_to_db += $" [elapsed time = {stopwatch.ElapsedMilliseconds}ms]";

            stopwatch = new Stopwatch();
            stopwatch.Start();

            try
            {
                monitor.api_cotizador = await correoArgentinoRequestService.Monitor("MonitorHandler");
            }
            catch (Exception ex)
            {
                monitor.api_cotizador = new Dictionary<string, string>
                {
                    { "ERROR", ex.Message }
                };
            }

            stopwatch.Stop();

            if (monitor.api_cotizador != null)
                monitor.api_cotizador.Add("Elapsed time", $"{stopwatch.ElapsedMilliseconds}ms");
            else
            {
                monitor.api_cotizador = new Dictionary<string, string>
                {
                    { "ERROR", "can not connect to correoargintecotizador" },
                    { "Elapsed time", $"{stopwatch.ElapsedMilliseconds}ms" }
                };
            }

            return monitor;
        }

    }

}
