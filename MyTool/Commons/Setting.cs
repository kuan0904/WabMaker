using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MyTool.Enums;

namespace MyTool.Commons
{
    /// <summary>
    /// 後台設定
    /// </summary>
    public class Setting
    {
        /*
         * 登入
         *  1.RouteConfig check AdminRoute exist
         *  2.AdminRoute/User/Login
         *  3.add Session: client=routeAdmin      
         */

        /// <summary>
        /// 超級管理員後台路由
        /// </summary>
        public static string SuperAdminManagement = "SuperAdminManagement";

        /// <summary>
        /// 超級管理員帳號
        /// </summary>
        public static Guid SuperManagerID = new Guid("00000000-0000-5555-3333-123456789999");

        /// <summary>
        /// 客戶後台路由(必須和db內一致)
        /// </summary>
        public static string[] AdminRoutes = new string[]
        {
            SuperAdminManagement,
            "DemoAdmin",
            "AnnieAdmin",
            "Q8Admin",
            "RollerAdmin",
            "MiniSoccerAdmin",
        };

        /// <summary>
        /// 客戶前台路由(必須和db內一致) ----> 直接判斷hotst
        /// </summary>
        //public static string[] WebRoutes = new string[]
        //{
        //    SuperAdminManagement,
        //    "DemoCompany",
        //    "AnnieWork",
        //    "Q8"
        //};

        public static string[] Languages = new string[] {
            "zh-tw",
            "en-us"
        };

        public static string IbonCompanyCode = "88";

        /// <summary>
        /// User解密Key
        /// </summary>
        public static string UserCryptoKey = "qo862g4iz";

        /// <summary>
        /// Order解密Key
        /// </summary>
        public static string OrderCryptoKey = "94jDpsugn";
    }

}