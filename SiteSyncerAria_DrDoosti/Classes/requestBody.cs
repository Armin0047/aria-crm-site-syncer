using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti.Classes
{
    public class requestBody
    {
        public string external_id { get; set; }
    }


    public class ListProduct : requestBody
    {
        public string description { get; set; }
        public decimal rate { get; set; }
        public string unit { get; set; }

    }

    public class Invoice : requestBody
    {
        public string phonenumber { get; set; }
        public string company { get; set; }
        public string date { get; set; }
        public List<item> items { get; set; } = new List<item>();
    }

    public class item :ListProduct
    {
        public int qty { get; set; }
    }
}
