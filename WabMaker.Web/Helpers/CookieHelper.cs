using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyTool.Services;

namespace WabMaker.Web.Helpers
{
    public class CookieHelper
    {
        public enum CookieName
        {
          
        }

        public static void Set<T>(CookieName name, T data, int day = 30)
        {
            HttpCookie cookie = new HttpCookie(name.ToString());

            var json = _Json.ModelToJson(data);
            cookie.Value = HttpContext.Current.Server.UrlEncode(json);
            cookie.Expires = DateTime.Now.AddDays(day);

            HttpContext.Current.Request.Cookies.Add(cookie);
        }


        public static T Get<T>(CookieName name) 
        {
            var cookie = HttpContext.Current.Request.Cookies[name.ToString()];
            var value = HttpContext.Current.Server.UrlDecode(cookie?.Value);

            if (!string.IsNullOrEmpty(value))
            {
                var data = _Json.JsonToModel<T>(value);
                return data;
            }
            return default(T);
        }


        public static void Remove(CookieName name)
        {
            HttpCookie cookie = new HttpCookie(name.ToString());
            cookie.Expires = DateTime.Now.AddDays(-1);
        }
    }
}