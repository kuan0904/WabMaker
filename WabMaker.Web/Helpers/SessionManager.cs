using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WebMaker.BLL.Helpers
{
    public class SessionManager
    {
        /// <summary>
        /// 登入中使用者(管理員/會員)
        /// </summary>   
        public static Guid UserID
        {
            get { return GetGuid(SessionName.UserID); }
            set { Set(SessionName.UserID, value); }
        }

        public static string UserName
        {
            get { return GetString(SessionName.UserName); }
            set { Set(SessionName.UserName, value); }
        }

        public static AccountType AccountType
        {
            get { return Get<AccountType>(SessionName.AccountType); }
            set { Set(SessionName.AccountType, value); }
        }

        public static MemberLevel? MemberLevel
        {
            get { return Get<MemberLevel?>(SessionName.MemberLevel); }
            set { Set(SessionName.MemberLevel, value); }
        }

        /// <summary>
        /// 所屬部門
        /// </summary>
        public static Guid? DepartmentID
        {
            get { return Get<Guid?>(SessionName.DepartmentID); }
            set { Set(SessionName.DepartmentID, value); }
        }

        /// <summary>
        /// 所屬以下部門
        /// </summary>
        public static List<Guid> DepartmentIDs
        {
            get { return Get<List<Guid>>(SessionName.DepartmentIDs); }
            set { Set(SessionName.DepartmentIDs, value); }
        }

        public static bool IsSuperManager
        {
            get { return GetBool(SessionName.IsSuperManager); }
            set { Set(SessionName.IsSuperManager, value); }
        }

        /// <summary>
        /// 使用者權限 (Controller,包含Action)
        /// </summary>
        public static List<RolePermissionModel> RolePermissions
        {
            get { return Get<List<RolePermissionModel>>(SessionName.RolePermissions); }
            set { Set(SessionName.RolePermissions, value); }
        }

        /// <summary>
        /// 使用者選單
        /// </summary>   
        public static List<TreeViewModel> MenuTree
        {
            get { return Get<List<TreeViewModel>>(SessionName.MenuTree); }
            set { Set(SessionName.MenuTree, value); }
        }

        /// <summary>
        /// 客戶資訊(後台用) 前台in Application
        /// </summary>   
        /// FrontLanguageTypes==0 不設定語系
        public static mgt_Client Client
        {
            get { return Get<mgt_Client>(SessionName.Client); }
            set { Set(SessionName.Client, value); }
        }

        /// <summary>
        /// 圖形驗證碼
        /// </summary>
        public static string Captcha
        {
            get { return GetString(SessionName.Captcha); }
            set { Set(SessionName.Captcha, value); }
        }

        /// <summary>
        /// 傳遞FB callback
        /// </summary>
        public static string FBstate
        {
            get { return GetString(SessionName.FBstate); }
            set { Set(SessionName.FBstate, value); }
        }

        public static ExternalLoginViewModel ExternalLogin
        {
            get { return Get<ExternalLoginViewModel>(SessionName.ExternalLogin); }
            set { Set(SessionName.ExternalLogin, value); }
        }
             
        /// <summary>
        /// return url (fb login 用)
        /// </summary>
        public static string ReturnUrl
        {
            get { return GetString(SessionName.ReturnUrl); }
            set { Set(SessionName.ReturnUrl, value); }
        }

        /// <summary>
        /// 暫存新增訂單id
        /// </summary>
        public static Guid TempOrderID
        {
            get { return GetGuid(SessionName.TempOrderID); }
            set { Set(SessionName.TempOrderID, value); }
        }

        /// <summary>
        /// 隨機碼(防止重送)
        /// </summary>
        public static string RandomCode
        {
            get { return GetString(SessionName.RandomCode); }
            set { Set(SessionName.RandomCode, value); }
        }

        #region base action

        private static bool GetBool(SessionName name)
        {
            var value = Get(name);
            return value == null ? false : Convert.ToBoolean(value);
        }

        private static string GetString(SessionName name)
        {
            return Get(name).FieldToString();
        }

        private static Guid GetGuid(SessionName name)
        {
            var value = Get(name);
            try
            {
                return new Guid(value.ToString());
            }
            catch (Exception)
            {
                return default(Guid);
            }
        }

        private static T Get<T>(SessionName name)
        {
            var value = Get(name);
            return value == null ? default(T) : (T)value;
        }

        /// <summary>
        /// 取得Session
        /// </summary>  
        private static object Get(SessionName name)
        {
            return HttpContext.Current.Session[name.ToString()];
        }

        /// <summary>
        /// 設置Session
        /// </summary>    
        private static void Set(SessionName name, object value)
        {
            HttpContext.Current.Session[name.ToString()] = value;
        }

        #endregion

        /// <summary>
        /// 清除登入Session
        /// </summary>
        public static void RemoveAll()
        {
            foreach (SessionName name in Enum.GetValues(typeof(SessionName)))
            {
                Set(name, null);
            }
        }
    }

    public enum SessionName
    {
        UserID,
        UserName,
        AccountType,
        MemberLevel,
        DepartmentID,
        DepartmentIDs,
        IsSuperManager,
        Client,
        RolePermissions,
        MenuTree,
        Captcha,
        FBstate,
        ExternalLogin,       
        ReturnUrl,
        TempOrderID,
        RandomCode
    }
}
