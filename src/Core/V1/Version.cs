using Core.V1.TiendaNube.AddNewOrders;
using Core.V1.TiendaNube.GetBranchOffices;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Core.V1
{
    public class Version
    {
        public Version() { }

        public string branch { get; } = "6.0";
        public string version { get; } = "v6.0.0";
        public string release { get; } = "2023-02-10 12:00:00";
    }

    public class Monitor
    {
        public Monitor()
        {
            version = new Version();

            DateTime dts = GetBranchOfficesRequestHandler.GetInitializeService();
            initialized_service = dts.ToString();

            provinces_in_memory = Agencies.get_provinces_in_memory();
            branchoffice_in_memory = Agencies.get_branchoffice_in_memory();
            DateTime dt = Agencies.get_last_clear_provinces();
            last_clear_provinces = dt.ToString();

            importorders_stores = AddNewOrdersRequestHandler.get_importorders_stores();
            dt = AddNewOrdersRequestHandler.get_last_clear();
            last_clear_importorders_stores = dt.ToString();

            sellers_updateToken = AddNewOrdersRequestHandler.get_sellers_updateToken();
        }

        public Version version { get; }
        public string initialized_service { get; }
        public string last_clear_provinces { get; }
        public int provinces_in_memory { get; }
        public int branchoffice_in_memory { get; }
        public string last_clear_importorders_stores { get; }
        public int importorders_stores { get; }
        public int sellers_updateToken { get; }
        public string connection_to_db { get; set; }
        public Dictionary<String, String> api_cotizador { get; set; }
    }
}
