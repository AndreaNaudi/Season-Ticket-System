using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace WebAssignment.Models
{
    public class PayPalModel
    {
        public string cmd { get; set; }
        public string business { get; set; }
        public string no_shipping { get; set; }
        public string @return { get; set; }
        public string cancel_return { get; set; }
        public string notify_url { get; set; }
        public string currency_code { get; set; }
        public string item_name { get; set; }
        public string item_quantity { get; set; }
        public string amount { get; set; }
        public string actionURL { get; set; }
        public string Tgs { get; set; }
        public string id { get; set; }

        public PayPalModel()
        {
            Random random = new Random();
            int length = random.Next(12, 20);
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            id =  new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}