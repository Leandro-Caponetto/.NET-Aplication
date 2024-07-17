using Core.Data.RequestServices;
using Core.RequestsHTTP.Models.TiendaNube;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Core.RequestsHTTP.Models.ApiMiCorreo;

namespace Core.RequestsHTTP.RequestServices
{
    
    public class TiendaNubeRequestService : ITiendaNubeRequestService
    {
        #region Endpoints TiendaNube
        private readonly string authorizeEndpoint = "/apps/authorize/token";
        private readonly string shippingCarrierEndpoint = "/v1/store_id/";
        #endregion

        private readonly IHttpClientSender httpClient;
        private readonly string urlService;
        private readonly string client_id; //CLIENT ID TAMBIEN ES STORE ID PARA TIENDA NUBE
        private readonly string client_secret;
        private readonly string grant_type;
        private readonly string user_agent;
        private readonly string apiUrlService;
        private readonly string shippingName;
        private readonly string shippingUrl;
        private readonly string shippingType;


        public TiendaNubeRequestService(
            IHttpClientSender httpClient,
            string urlService,
            string client_id,
            string client_secret,
            string grant_type,
            string user_agent,
            string apiUrlService,
            string shippingName,
            string shippingUrl,
            string shippingType)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.urlService = urlService ?? throw new ArgumentNullException(nameof(urlService));
            this.client_id = client_id ?? throw new ArgumentNullException(nameof(client_id));
            this.client_secret = client_secret ?? throw new ArgumentNullException(nameof(client_secret));
            this.grant_type = grant_type ?? throw new ArgumentNullException(nameof(grant_type));
            this.user_agent = user_agent ?? throw new ArgumentNullException(nameof(user_agent));
            this.apiUrlService = apiUrlService ?? throw new ArgumentNullException(nameof(apiUrlService));
            this.shippingName = shippingName ?? throw new ArgumentNullException(nameof(shippingName));
            this.shippingUrl = shippingUrl ?? throw new ArgumentNullException(nameof(shippingUrl));
            this.shippingType = shippingType ?? throw new ArgumentNullException(nameof(shippingType));
        }

        public async Task<OrderResponseModel> GetOrderById(string processName, string token, string storeCode, string id)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeCode);

            var urlRequest = apiUrlService + newUrl + "orders/" + id;

            var autorizeResponse = await httpClient.SendGetAsync<OrderResponseModel>(processName, urlRequest, new Object(), token, this.user_agent);

            return autorizeResponse;
        }

        public async Task<IEnumerable<OrderResponseModel>> GetOrdersById(string processName, string token, string storeCode, IEnumerable<string> ids)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeCode);

            ids = ids.OrderBy(x => x);

            //var urlRequest = apiUrlService + newUrl + "orders?since_id=" + ids.Aggregate((current, next) => current + "&since_id=" + next);

            var urlRequest = apiUrlService + newUrl + "orders?since_id=" + (int.Parse(ids.ToList()[0]) - 1).ToString();

            var autorizeResponse = await httpClient.SendGetAsync<IEnumerable<OrderResponseModel>>(processName, urlRequest, new Object(), token, this.user_agent);

            return autorizeResponse;
        }

        public async Task<AuthorizeResponseModel> GetTiendaNubeAuthorization(string processName, string storeCode)
        {
            var authorize = new AuthorizeModel()
            {
                client_id = this.client_id,
                client_secret = this.client_secret,
                grant_type = this.grant_type,
                code = storeCode
            };

            var urlRequest = urlService + authorizeEndpoint;

            var autorizeResponse = await httpClient.SendPostAsync<AuthorizeResponseModel>(processName, urlRequest, authorize);

            return autorizeResponse;
        }


        public async Task<List<ShippingCarrierResponseModel>> GetTiendaNubeShippingCarriers(string processName, string token, string storeId)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "shipping_carriers";

            List<ShippingCarrierResponseModel> rlscarriers = null;
            List<ShippingCarrierResponseModel> shippingCarriers = await httpClient.SendGetAsync<List<ShippingCarrierResponseModel>>(processName, urlRequest, null, token, this.user_agent);
            if (shippingCarriers != null)
            {
                foreach (ShippingCarrierResponseModel scarrier in shippingCarriers)
                {
                    if (scarrier.name == this.shippingName)
                    {
                        if (rlscarriers == null)
                            rlscarriers = new List<ShippingCarrierResponseModel>();
                        rlscarriers.Add(scarrier);
                    }
                }
            }
            return rlscarriers;
        }


        public async Task<ShippingCarrierResponseModel> PostTiendaNubeShippingCarrier(string processName, string token, string storeId)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var shippingCarrier = new ShippingCarrierModel()
            {
                name = this.shippingName,
                callback_url = this.shippingUrl,
                types = this.shippingType
            };

            var urlRequest = apiUrlService + newUrl + "shipping_carriers";

            var autorizeResponse = await httpClient.SendPostAsync<ShippingCarrierResponseModel>(processName, urlRequest, shippingCarrier, token, this.user_agent);

            return autorizeResponse;
        }

        public async Task<ShippingCarrierOptionResponseModel> SetShippingCarrierOptions(string processName, string token, string storeId, string shippingId ,ShippingCarrierOptionModel model)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "shipping_carriers/" + shippingId + "/options";

            var autorizeResponse = await httpClient.SendPostAsync<ShippingCarrierOptionResponseModel>(processName, urlRequest, model, token, this.user_agent);

            return autorizeResponse;

        }


        public async Task<bool> DeleteShippingCarrier(string processName, string token, string storeId, string shippingId)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "shipping_carriers/" + shippingId;

            bool ok = await httpClient.SendDeleteAsync<bool>(processName, urlRequest, token, this.user_agent);

            return ok;

        }


        public async Task<StoreReponseModel> GetTiendaNubeStoreData(string processName, string token, string storeId)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "store";

            var autorizeResponse = await httpClient.SendGetAsync<StoreReponseModel>(processName, urlRequest, new Object(), token, this.user_agent);

            return autorizeResponse;
        }


        public async Task<IEnumerable<ProductNameModel>> GetProductNamesByIds(string processName, IEnumerable<string> ids, string token, string storeId)
        {
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "products";

            //var urlRequest = apiUrlService + defaultEndpoint + "products/" + ids.Aggregate((current, next) => current + "," + next) ;

            var autorizeResponse = await httpClient.SendGetAsync<IEnumerable<ProductNameModel>>(processName, urlRequest, new Object(), token, this.user_agent);

            return autorizeResponse;
        }

        /*
        public async void UpdateTrackingNumber(string processName, string storeId, string orderId, UpdateOrderModel updateOrderModel, string token)
        {
        
            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "orders/" + orderId + "/fulfill";

            var response = await httpClient.SendPostAsync<ProductNameModel>(processName, urlRequest, updateOrderModel, token, this.user_agent);

            var asd = response;
            
        }
        */

        public async Task<OrderResponseModel> UpdateTrackingNumber(string processName, string storeId, string orderId, UpdateOrderModel updateOrderModel, string token)
        {

            var newUrl = shippingCarrierEndpoint.Replace("store_id", storeId);

            var urlRequest = apiUrlService + newUrl + "orders/" + orderId + "/fulfill";

            var autorizeResponse = await httpClient.SendPostAsync<OrderResponseModel>(processName, urlRequest, updateOrderModel, token, this.user_agent);

            return autorizeResponse;

        }


    }
}
