using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public class _Config
    {   
        public static string GetStr(string field)
        {
            if (ConfigurationManager.AppSettings[field] != null)
            {
                return ConfigurationManager.AppSettings[field];
            }
            else
            {
                _Log.CreateText("config file has no field. (" + field + ")");
                return "";
            }
        }

        public static bool GetBool(string field)
        {
            string data = GetStr(field).ToLower();
            return (data == "true");
        }

        public static int GetInt(string field)
        {
            int value;
            if (Int32.TryParse(GetStr(field), out value))
                return value;
            else
            {
                _Log.CreateText("config file int field wrong. (" + field + ")");
                return 0;
            }
        }

    }
}
