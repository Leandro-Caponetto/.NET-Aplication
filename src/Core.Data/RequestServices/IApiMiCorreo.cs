
using Core.Entities;
using Core.RequestsHTTP.Models.ApiMiCorreo;
using Core.RequestsHTTP.Models.ApiMiCorreo.RegisterSendModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core.Data.RequestServices
{
    public interface IApiMiCorreo
    {
        Task<LoginResponseModel> Login(string processName, LoginModel loginModel);
        Task<LoginResponseModel> Register(string processName, NewUserModel newUserModel);
        Task<IEnumerable<AgencyModel>> Agencies(string processName, GetAgencyModel getSucursalesModel);
        Task<DimensionLimits> GetDimensionsLimits(string processName);
        Task<ShipPriceResponseModel> Rates(string processName, ShipPriceModel shipPriceModel);
        Task<RegisterSendResponseModel> importShipping(string processName, RegisterSendModel newSend);
        Task<Dictionary<String, String>> Monitor(string processName);
    }
}

