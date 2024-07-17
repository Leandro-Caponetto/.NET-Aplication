using MediatR;
using Serilog;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Data.EF.Behaviors
{
    public class TransactionalBehavior<TRequest, TResponse> :
        IPipelineBehavior<TRequest, TResponse> where TRequest : MediatR.IRequest<TResponse>
    {
        private readonly DataContext dataContext;
        private readonly ILogger logger;

        public TransactionalBehavior(DataContext dataContext, ILogger logger)
        {
            this.dataContext = dataContext ?? throw new ArgumentNullException(nameof(dataContext));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            try
            {
                if (!typeof(IRequest).IsAssignableFrom(request.GetType()))
                    return await next();

                var response = await next();
                await dataContext.SaveChangesAsync();
                return response;
            }
            catch (Exception ex)
            {
                logger.Error(ex, ex.Message);
                throw;
            }
        }
    }
}
