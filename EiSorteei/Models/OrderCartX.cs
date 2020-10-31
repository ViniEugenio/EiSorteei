using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EiSorteei.Models
{
    public class OrderCartX
    {
        public string id { get; set; }
        public string shop_id { get; set; }
        public string name { get; set; }
        public string subtotal_price { get; set; }
        public string subtotal_price_set { get; set; }
        public string email { get; set; }
        public string price { get; set; }
        public string title { get; set; }       
        public string status_id { get; set; }
        public ProductsCartX[] line_items { get; set; }
        public OrderShippingsCartX orders_shippings { get; set; }
        public string payment_status { get; set; }
    }
}