using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.RequestsHTTP
{
    public class HttpClientSender : IHttpClientSender
    {
        private readonly ILogger logger;

        public HttpClientSender(ILogger logger)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region Public Methods IHttpClientSender

        public async Task<T> SendPostAsync<T>(
            string processName,
            string urlService,
            object requestContent,
            string token = null,
            string userAgent = null,
            string username = null,
            string password = null)
        {
            LogHttpOperation(processName, "Post", urlService, token, userAgent, username, password);
            var byteContent = GetJsonByteArrayContent(processName, requestContent);
            var httpClient = new HttpClient();
            
           if (!string.IsNullOrEmpty(token))
               httpClient.DefaultRequestHeaders.Add("Authentication", "bearer " + token);

           if (!string.IsNullOrEmpty(userAgent))
               httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

           if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
           {
               var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
               httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
           }
           
            var response = await httpClient.PostAsync(urlService, byteContent);

            return await DeserializeJsonResponse<T>(processName, response);
        }

        public async Task<T> SendPutAsync<T>(
           string processName,
           string urlService,
           object requestContent,
           string token = null,
           string userAgent = null,
           string username = null,
           string password = null)
        {
            LogHttpOperation(processName, "Put", urlService, token, userAgent, username, password);
            var byteContent = GetJsonByteArrayContent(processName, requestContent);
            var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Add("Authentication", "bearer " + token);

            if (!string.IsNullOrEmpty(userAgent))
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            if (!string.IsNullOrEmpty(username))
                httpClient.DefaultRequestHeaders.Add("username", username);

            if (!string.IsNullOrEmpty(password))
                httpClient.DefaultRequestHeaders.Add("password", password);

            var response = await httpClient.PutAsync(urlService, byteContent);

            return await DeserializeJsonResponse<T>(processName, response);
        }

        public async Task<T> SendGetAsync<T>(
            string processName,
            string urlService,
            object requestContent,
            string token = null,
            string userAgent = null,
            string username = null,
            string password = null)
        {
            LogHttpOperation(processName, "Get", urlService, token, userAgent, username, password);
            var queryString = GetQuertString(processName, requestContent);
            var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Add("Authentication", "bearer " + token);

            if (!string.IsNullOrEmpty(userAgent))
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }

            var response = await httpClient.GetAsync(urlService + queryString);

            return await DeserializeJsonResponse<T>(processName, response);
        }


        public async Task<T> SendGetWithBodyAsync<T>(
            string processName,
            string urlService,
            object requestContent,
            string token = null,
            string userAgent = null,
            string username = null,
            string password = null)
        {
            LogHttpOperation(processName, "Get B", urlService, token, userAgent, username, password);
            var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Add("Authentication", "bearer " + token);

            if (!string.IsNullOrEmpty(userAgent))
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);


            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                var byteArray = Encoding.ASCII.GetBytes(username + ":" + password);
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }


            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(urlService),
                Content = new StringContent(JsonConvert.SerializeObject(requestContent), Encoding.UTF8, "application/json")
            };


            var response = await httpClient.SendAsync(request);

            return await DeserializeJsonResponse<T>(processName, response);
        }


        public async Task<T> SendDeleteAsync<T>(
            string processName,
            string urlService,
            string token = null,
            string userAgent = null,
            string username = null,
            string password = null)
        {
            LogHttpOperation(processName, "Delete", urlService, token, userAgent, username, password);
            var httpClient = new HttpClient();

            if (!string.IsNullOrEmpty(token))
                httpClient.DefaultRequestHeaders.Add("Authentication", "bearer " + token);

            if (!string.IsNullOrEmpty(userAgent))
                httpClient.DefaultRequestHeaders.Add("User-Agent", userAgent);

            if (!string.IsNullOrEmpty(username))
                httpClient.DefaultRequestHeaders.Add("username", username);

            if (!string.IsNullOrEmpty(password))
                httpClient.DefaultRequestHeaders.Add("password", password);

            var response = await httpClient.DeleteAsync(urlService);

            return await DeserializeJsonResponse<T>(processName, response);
        }

        #endregion

        #region Private Methods

        private void LogHttpOperation(string processName, string operationType, string urlService, string token = null, string userAgent = null, string username = null, string password = null)
        {
            var msg = $"{processName} - Tipo operacion: {operationType} - Url: {urlService} \n";
            if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
            {
                msg += !string.IsNullOrEmpty(token) ? $"Header: Authentication  Token: bearer {token} \n" : "";
                msg += !string.IsNullOrEmpty(userAgent) ? $"Header: User-Agent - Value: {userAgent} \n" : "";
                msg += string.IsNullOrEmpty(username) ? $"Header: Authorization - Username: {username} - Password: {password} \n" : "";
                logger.Debug(msg);
            }
            else
                logger.Information(msg);
        }

        private ByteArrayContent GetJsonByteArrayContent(string processName, object requestContent)
        {
            var content = JsonConvert.SerializeObject(requestContent);
            logger.Information($"{processName} - Body Content: {content}");
            var buffer = System.Text.Encoding.UTF8.GetBytes(content);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return byteContent;
        }

        private async Task<T> DeserializeJsonResponse<T>(string processName, HttpResponseMessage response)
        {
            var responseString = await response.Content.ReadAsStringAsync();
            if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                logger.Debug($"{processName} - Response Content: {responseString}");
            else
            {
                if (responseString.Length < 160)
                    logger.Information($"{processName} - Response Content: {responseString}");
                else
                    logger.Information($"{processName} - Response Content: {responseString.Substring(0, 160)}...");
            }
            T responseModel = JsonConvert.DeserializeObject<T>(responseString);

            return responseModel;
        }

        private string GetQuertString(string processName, object requestContent)
        {
            string queryString = "";

            if (requestContent == null)
                return queryString;

            bool isFirst = true;
            Type myType = requestContent.GetType();
            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties());

            foreach (PropertyInfo prop in props)
            {
                object propValue = prop.GetValue(requestContent, null);

                if (isFirst)
                {
                    isFirst = false;
                    queryString += $"?{prop.Name}={propValue.ToString()}";
                }
                else
                    queryString += $"&{prop.Name}={propValue.ToString()}";
            }

            if (logger.IsEnabled(Serilog.Events.LogEventLevel.Debug))
                logger.Debug($"{processName} - Query String: {queryString}");
            else
            {
                if (queryString.Length < 160)
                    logger.Information($"{processName} - Query String: {queryString}");
                else
                    logger.Information($"{processName} - Query String: {queryString.Substring(0, 160)}...");
            }
            return queryString;
        }

        #endregion
    }
}
