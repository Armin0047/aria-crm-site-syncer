using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SiteSyncerAria_DrDoosti.Classes
{
    public static class Utility
    {
        public static string EscapeSql(string value)
        {
            if (value == null) return "";
            return value.Replace("'", "''");
        }
    }
}
