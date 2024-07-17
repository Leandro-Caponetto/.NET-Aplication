using System.Threading.Tasks;

namespace Core.RequestsHTTP
{
    public interface IHttpClientSender
    {
        Task<T> SendPostAsync<T>(string processName, string urlService, object requestContent, string token = null, string userAgent = null, string username = null, string password = null);
        Task<T> SendGetAsync<T>(string processName, string urlService, object requestContent, string token = null, string userAgent = null, string username = null, string password = null);
        Task<T> SendGetWithBodyAsync<T>(string processName, string urlService, object requestContent, string token = null, string userAgent = null, string username = null, string password = null);
        Task<T> SendPutAsync<T>(string processName, string urlService, object requestContent, string token = null, string userAgent = null, string username = null, string password = null);
        Task<T> SendDeleteAsync<T>(string processName, string urlService, string token = null, string userAgent = null, string username = null, string password = null);
    }
}
