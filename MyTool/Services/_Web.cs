using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyTool.Services
{
    public class _Web
    {
        /// <summary>
        /// 取得IP
        /// </summary>
        /// <returns></returns>
        public static string GetIp()
        {
            return HttpContext.Current.Request.UserHostAddress;
        }

        /// <summary>
        /// 檢查目前裝置是否為手機
        /// </summary>
        public static bool IsMobile()
        {
            return HttpContext.Current.Request.Browser.IsMobileDevice;           
        }

        /// <summary>
        /// 讀取網址內容Get
        /// </summary>
        /// <returns></returns>
        public static string RequestUrlGet(string url)
        {
            string result = string.Empty;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                StreamReader reader = new StreamReader(response.GetResponseStream());
                result = reader.ReadToEnd();
                reader.Close();
            }
            return result;
        }
    }
}
