using System;
using System.Collections.Generic;

namespace Core.RequestsHTTP.Models.TiendaNube
{
    public class OrderResponseModel
    {
        public int code { get; set; }
        public string message { get; set; }
        public string description { get; set; }
        public int id { get; set; }
        public string number { get; set; }
        public string status { get; set; }
        public DateTime created_at { get; set; }
        public DateTime updated_at { get; set; }
        public string shipping_option { get; set; }
        public string shipping_pickup_type { get; set; }
        public string shipping_option_reference { get; set; }
        public string shipping_store_branch_name { get; set; }
        public IEnumerable<ProductsModel> products { get; set; }
        public ShippingAddresModel shipping_address { get; set; }
        public string shipping_option_code { get; set; }
        public ShippingPickupDetail shipping_pickup_details {get; set;}
        // public CustomerModel customer { get; set; }

        public string contact_name { get; set; }
        public string contact_email { get; set; }
        public string contact_phone { get; set; }
        public string contact_identification { get; set; }
    }

    public class ProductsModel
    {
        public string depth { get; set; }
        public string height { get; set; }
        public string price { get; set; }
        public int product_id { get; set; }
        public int quantity { get; set; }
        public bool free_shipping { get; set; }
        public int variant_id { get; set; }
        public string weight { get; set; }
        public string width { get; set; }
    }

    public class ShippingAddresModel
    {
        public string name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string address { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public DateTimeOffset created_at { get; set; }
        public string floor { get; set; }
        public int id { get; set; }
        public string locality { get; set; }
        public string number { get; set; }
        public string phone { get; set; }
        public string province { get; set; }
        public string updated_at { get; set; }
        public string zipcode { get; set; }
    }

    public class CustomerModel
    {
        public string created_at { get; set; }
        public string email { get; set; }
        public int id { get; set; }
        public int last_order_id { get; set; }
        public string name { get; set; }
        public string total_spent { get; set; }
        public string total_spent_currency { get; set; }
        public string updated_at { get; set; }
        public string phone { get; set; }
        public string billing_phone { get; set; }
        public ClientDefaultAdressModel default_address { get; set; }
    }


    public class ClientDefaultAdressModel
    {
        public string address { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public DateTimeOffset created_at { get; set; }
        public string floor { get; set; }
        public int id { get; set; }
        public string locality { get; set; }
        public string number { get; set; }
        public string phone { get; set; }
        public string province { get; set; }
        public DateTimeOffset updated_at { get; set; }
        public string zipcode { get; set; }
    }

    public class ShippingPickupDetail
    {
        public string name { get; set; }
        public ShippingAddresModel address { get; set; }

    }

}


