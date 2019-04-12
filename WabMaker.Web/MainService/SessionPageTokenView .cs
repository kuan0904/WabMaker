using MyTool.Services;
using System;
using System.Web;
using WebMaker.BLL.Helpers;

namespace WabMaker.Web.MainService
{
    public class SessionPageTokenView
    {
        public static readonly string HiddenTokenName = "hiddenToken";
        public static readonly string SessionMyToken = "Token";

        #region PageTokenViewBase

        /// <summary>
        /// 生成的頁面標記
        /// </summary>
        /// <returns></returns>
        public string GeneratePageToken()
        {
            if (HttpContext.Current.Session[SessionMyToken] != null)
            {
                return HttpContext.Current.Session[SessionMyToken].ToString();
            }
            else
            {
                var token = GenerateHashToken();
                HttpContext.Current.Session[SessionMyToken] = token;
                return token;
            }
        }


        /// <summary>
        /// 取得最後一頁的標記形式
        /// </summary>
        public string GetLastPageToken
        {
            get
            {
                return HttpContext.Current.Request.Params[HiddenTokenName];
            }
        }

        /// <summary>
        /// 獲取一個值，是否匹配標記
        /// </summary>
        /// <value>
        ///   <c>true</c> if [tokens match]; otherwise, <c>false</c>.
        /// </value>
        public bool TokensMatch
        {
            get
            {
                string formToken = GetLastPageToken;
                if (formToken != null)
                {
                    if (formToken.Equals(GeneratePageToken()))
                    {
                        //Refresh token
                        HttpContext.Current.Session[SessionMyToken] = GenerateHashToken();
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion 
              
        /// <summary>
        /// Generates the hash token.
        /// </summary>
        /// <returns></returns>
        public string GenerateHashToken()
        {
            return _Crypto.HashMD5(
                HttpContext.Current.Session.SessionID + DateTime.Now.Ticks.ToString());
        }
      
    }
}
