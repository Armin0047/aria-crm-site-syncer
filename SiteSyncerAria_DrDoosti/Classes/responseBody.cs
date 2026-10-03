using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti.Classes
{
    public class responseBody
    {
        public bool success { get; set; }
        public string message { get; set; }
        public Dto dtos { get; set; }
    }

    public class Dto
    {
        public bool ok { get; set; }
        public int? count { get; set; }
        public string error { get; set; }
        public Record records { get; set; }

    }

    public class Record
    {
        public int inbound_id { get; set; }
        public string type { get; set; }
        public string status { get; set; }
        public int? crm_id { get; set; }
        public bool? duplicate { get; set; }
        public string external_id { get; set; }
        public bool? auto { get; set; }
        public string error { get; set; }
    }
}
