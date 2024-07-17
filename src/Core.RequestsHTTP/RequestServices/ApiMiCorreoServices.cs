using Core.Data.RequestServices;
using Core.Entities;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.RequestsHTTP.RequestServices
{
    public class ApiMiCorreoServices : IApiMiCorreo
    {
        protected static readonly string customerGroup = "TIENDANUBE";

        private readonly IHttpClientSender httpClient;
        private readonly string urlService;
        private readonly string username;
        private readonly string password;

        public ApiMiCorreoServices(
            IHttpClientSender httpClient,
            string urlService,
            string username,
            string password)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            this.urlService = urlService ?? throw new ArgumentNullException(nameof(urlService));
            this.username = username ?? throw new ArgumentNullException(nameof(username));
            this.password = password ?? throw new ArgumentNullException(nameof(password));
        }

        public async Task<IEnumerable<AgencyModel>> Agencies(string processName, GetAgencyModel agency)
        {
            agency.customerGroup = customerGroup;

            var url = urlService + "agencies";

            var response = await httpClient.SendGetAsync<IEnumerable<AgencyModel>>(processName, url, agency, null, null, this.username, this.password);

            return response;
            
        }

        
        public async Task<LoginResponseModel> Login(string processName, LoginModel loginModel)
        {
            loginModel.customerGroup = customerGroup;

            var url = urlService + "users/validate";

            var response = await httpClient.SendPostAsync<LoginResponseModel>(processName, url, loginModel, null, null, this.username, this.password);

            return response;
        }


        public async Task<LoginResponseModel> Register(string processName, NewUserModel newUserModel)
        {
            newUserModel.customerGroup = customerGroup;

            var urlpost = urlService + "register";

            var response = await httpClient.SendPostAsync<LoginResponseModel>(processName, urlpost, newUserModel, null, null, this.username, this.password);

            return response;
        }


        public async Task<ShipPriceResponseModel> Rates(string processName, ShipPriceModel shipPriceModel)
        {
            shipPriceModel.customerGroup = customerGroup;

            var url = urlService + "rates";

            var response = await httpClient.SendPostAsync<ShipPriceResponseModel>(processName, url, shipPriceModel, null, null, this.username, this.password);

            return response;
        }


        public async Task<DimensionLimits> GetDimensionsLimits(string processName)
        {
            var url = urlService + "shippingDimensionsLimits/" + customerGroup;

            var response = await httpClient.SendGetAsync<DimensionLimits>(processName, url, null, null, null, this.username, this.password);

            return response;
        }


        public async Task<RegisterSendResponseModel> importShipping(string processName, RegisterSendModel newSendModel)
        {
            newSendModel.customerGroup = customerGroup;

            var url = urlService + "shipping/import";
            
            var response = await httpClient.SendPostAsync<RegisterSendResponseModel>(processName, url, newSendModel, null, null, this.username, this.password);

            return response;
        }


        public async Task<Dictionary<String, String>> Monitor(string processName)
        {

            var url = urlService + "monitor";

            var response = await httpClient.SendGetAsync<Dictionary<String, String>>(processName, url, null, null, null, this.username, this.password);

            return response;
        }

    }
}