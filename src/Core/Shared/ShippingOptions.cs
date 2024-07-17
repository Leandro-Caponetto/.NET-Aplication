using System.Collections.Generic;
using Core.RequestsHTTP.Models.TiendaNube;

namespace Core.Shared
{
    public static class ShippingOptions
    {
        public const string Ship = "ship";
        public const string Pickup = "pickup";

        public static List<ShippingCarrierOptionModel> Options
        {
            get
            {
                return new List<ShippingCarrierOptionModel>
                {
                    new ShippingCarrierOptionModel()
                    {
                        code = "CPD",
                        name = "Correo Argentino Clasico - Envio a domicilio"
                    },
                    new ShippingCarrierOptionModel()
                    {
                        code = "CPS",
                        name = "Correo Argentino Clasico - Envio a sucursal"
                    },
                    new ShippingCarrierOptionModel()
                    {
                        code = "EPD",
                        name = "Correo Argentino Expreso - Envio a domicilio"
                    },
                    new ShippingCarrierOptionModel()
                    {
                        code = "EPS",
                        name = "Correo Argentino Expreso - Envio a sucursal"
                    },
                };
            }
        }
    }
}