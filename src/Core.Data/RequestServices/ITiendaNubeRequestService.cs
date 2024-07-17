using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.RequestsHTTP.Models.TiendaNube;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.RequestServices
{
    public interface ITiendaNubeRequestService
    {
        Task<AuthorizeResponseModel> GetTiendaNubeAuthorization(string processName, string storeCode);
        Task<StoreReponseModel> GetTiendaNubeStoreData(string processName, string token, string storeId);
        Task<List<ShippingCarrierResponseModel>> GetTiendaNubeShippingCarriers(string processName, string token, string storeId);
        Task<ShippingCarrierResponseModel> PostTiendaNubeShippingCarrier(string processName, string token, string storeId);
        Task<ShippingCarrierOptionResponseModel> SetShippingCarrierOptions(string processName, string token, string storeId, string shippingId, ShippingCarrierOptionModel model);
        Task<bool> DeleteShippingCarrier(string processName, string token, string storeId, string shippingId);
        Task<OrderResponseModel> GetOrderById(string processName, string token, string storeCode, string id);
        Task<IEnumerable<OrderResponseModel>> GetOrdersById(string processName, string token, string storeCode, IEnumerable<string> ids);
        Task<IEnumerable<ProductNameModel>> GetProductNamesByIds(string processName, IEnumerable<string> ids, string token, string storeId);
        //void UpdateTrackingNumber(string processName, string storeId, string orderId, UpdateOrderModel updateOrderModel, string token);

        Task<OrderResponseModel> UpdateTrackingNumber(string processName, string storeId, string orderId, UpdateOrderModel updateOrderModel, string token);
    }
}
