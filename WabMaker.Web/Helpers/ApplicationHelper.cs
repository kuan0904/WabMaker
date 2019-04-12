using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web;
using WebMaker.BLL.Services;

namespace WabMaker.Web.Helpers
{
    //前台用
    public enum ApplicationName
    {
        //是否是測試環境
        IsLocal,
        //路由Host (判斷是哪個系統)
        HostName,

        //客戶ID
        ClientID,
        //客戶代碼 (製造SystemCode用)
        ClientCode,
        //系統設定
        ClientSettings,
        //系統名稱
        SystemName,
        //網站名 (追蹤碼用)
        SiteName,
        //是否前端使用API
        IsApiHtml,
        //是否使用 SSL 憑證
        IsHttps,
        //預設語系
        DefaultLanguage,
        //管理員登入圖片
        AdminLoginImage,
        //會員專區首頁
        MemberIndex,
        //會員資料Email同頁編輯 (預設false)
        ProfileWithEmail,
        //會員資料Phone同頁編輯 (預設false)
        ProfileWithPhone,
        //會員登入頁樣式
        LoginStyle,


        //網站基本資料設定
        SiteInfo,
        //Api Key
        ApiKey
    }

    public class ApplicationHelper
    {
        public void Init()
        {

            #region client
            var defaultHost = ConfigurationManager.AppSettings["DefaultHost"];
            var host = !string.IsNullOrEmpty(defaultHost) ? defaultHost : RouteHelper.HostName();
            IsLocal = !string.IsNullOrEmpty(defaultHost);

            IsApiHtml = false;
            IsHttps = false;
            DefaultLanguage = LanguageType.Chinese;
            AdminLoginImage = "default";
            MemberIndex = "Index";
            ProfileWithEmail = false;
            ProfileWithPhone = false;
            LoginStyle = LoginStyle.Normal;
            switch (host)
            {
                case "cms.xnet.world"://Demo中
                                      // ClientID = new Guid("00000000-1111-2222-3333-123456789999");
                                      // ClientCode = "000";
                                      // SystemName = "Demo";

                    ClientID = new Guid("a4dde220-30e5-490b-bb5b-3c39c3abbe4f");
                    ClientCode = "003";
                    SystemName = "Demo";

                    break;
                //拙八郎
                case "www.eightgeman.com":
                    ClientID = new Guid("82092779-8fc9-47d3-adb1-7574bcddc00d");
                    //ClientCode = "001";
                    //SystemName = "8GeMan";
                    SiteName = "eightgeman";
                    IsHttps = true;
                    MemberIndex = "Profile";
                    ProfileWithEmail = true;
                    ProfileWithPhone = true;
                    break;
                //滑輪
                case "www.rollersports.org.tw":
                    ClientID = new Guid("524dde74-fdef-481a-b9ed-49bab41f7964");
                    //ClientCode = "002";
                    //SystemName = "RollerSport";
                    SiteName = "rollersports";
                    IsHttps = true;
                    MemberIndex = "Profile";
                    ProfileWithEmail = true;
                    ProfileWithPhone = true;
                    LoginStyle = LoginStyle.Popup;
                    break;
                //迷你足球
                case "soccer.ai-sportthings.com":
                case "www.ai-sportthings.com":
                    ClientID = new Guid("a4dde220-30e5-490b-bb5b-3c39c3abbe4f");
                    //ClientCode = "003";
                    //SystemName = "MiniSoccer";
                    //SiteName = "ai-sportthings";
                    IsHttps = true;
                    AdminLoginImage = "003";
                    MemberIndex = "Profile";
                    break;

                default:
                    break;
            }
            #endregion

            ClientService clientService = new ClientService();
            if (ClientID != Guid.Empty)
            {
                var client = clientService.Get(ClientID);
                ClientCode = client.ClientCode;
                SystemName = client.SystemName;
                ClientSettings = client.ClientSetting.ToContainList<ClientSetting>();
            }
            clientService.Dispose();
            _Log.CreateText("Application init " + SystemName);

            #region SetSystemSetting

            SystemSettingService service = new SystemSettingService();
            service.ClientID = ClientID;

            //SiteInfo
            var resultSiteInfo = service.Get<SiteInfoViewModel>(SystemSettingType.SiteInfo);
            if (resultSiteInfo.IsSuccess)
            {
                // Html Decode 
                resultSiteInfo.Data.Footer = HttpUtility.HtmlDecode(resultSiteInfo.Data.Footer);
                SiteInfo = resultSiteInfo.Data;
            }

            //ApiKey
            var resultApiKey = service.Get<ApiKeyViewModel>(SystemSettingType.ApiKey);
            if (resultApiKey.IsSuccess)
            {
                ApiKey = resultApiKey.Data;
            }

            service.Dispose();
            #endregion

        }

        #region Client
        public static bool IsLocal
        {
            get { return GetBool(ApplicationName.IsLocal); }
            set { Set(ApplicationName.IsLocal, value); }
        }

        public static string HostName
        {
            get { return GetString(ApplicationName.HostName); }
            set { Set(ApplicationName.HostName, value); }
        }

        public static Guid ClientID
        {
            get { return GetGuid(ApplicationName.ClientID); }
            set { Set(ApplicationName.ClientID, value); }
        }

        public static string ClientCode
        {
            get { return GetString(ApplicationName.ClientCode); }
            set { Set(ApplicationName.ClientCode, value); }
        }

        public static List<ClientSetting> ClientSettings
        {
            get { return Get<List<ClientSetting>>(ApplicationName.ClientSettings); }
            set { Set(ApplicationName.ClientSettings, value); }
        }

        public static string SystemName
        {
            get { return GetString(ApplicationName.SystemName); }
            set { Set(ApplicationName.SystemName, value); }
        }

        public static string SiteName
        {
            get { return GetString(ApplicationName.SiteName); }
            set { Set(ApplicationName.SiteName, value); }
        }

        public static bool IsApiHtml
        {
            get { return GetBool(ApplicationName.IsApiHtml); }
            set { Set(ApplicationName.IsApiHtml, value); }
        }

        public static bool IsHttps
        {
            get { return GetBool(ApplicationName.IsHttps); }
            set { Set(ApplicationName.IsHttps, value); }
        }


        public static LanguageType DefaultLanguage
        {
            get { return Get<LanguageType>(ApplicationName.DefaultLanguage); }
            set { Set(ApplicationName.DefaultLanguage, value); }
        }

        public static string AdminLoginImage
        {
            get { return GetString(ApplicationName.AdminLoginImage); }
            set { Set(ApplicationName.AdminLoginImage, value); }
        }

        public static string MemberIndex
        {
            get { return GetString(ApplicationName.MemberIndex); }
            set { Set(ApplicationName.MemberIndex, value); }
        }

        public static bool ProfileWithEmail
        {
            get { return GetBool(ApplicationName.ProfileWithEmail); }
            set { Set(ApplicationName.ProfileWithEmail, value); }
        }

        public static bool ProfileWithPhone
        {
            get { return GetBool(ApplicationName.ProfileWithPhone); }
            set { Set(ApplicationName.ProfileWithPhone, value); }
        }

        public static LoginStyle LoginStyle
        {
            get { return Get<LoginStyle>(ApplicationName.LoginStyle); }
            set { Set(ApplicationName.LoginStyle, value); }
        }

        #endregion

        #region Setting

        public static SiteInfoViewModel SiteInfo
        {
            get { return Get<SiteInfoViewModel>(ApplicationName.SiteInfo); }
            set { Set(ApplicationName.SiteInfo, value); }
        }

        public static ApiKeyViewModel ApiKey
        {
            get { return Get<ApiKeyViewModel>(ApplicationName.ApiKey); }
            set { Set(ApplicationName.ApiKey, value); }
        }

        #endregion

        #region base action
        private static bool GetBool(ApplicationName name)
        {
            var value = Get(name);
            return value == null ? false : Convert.ToBoolean(value);
        }

        private static string GetString(ApplicationName name)
        {
            return Get(name).FieldToString();
        }

        private static Guid GetGuid(ApplicationName name)
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

        private static T Get<T>(ApplicationName name)
        {
            var value = Get(name);
            return value == null ? default(T) : (T)value;
        }

        /// <summary>
        /// 取得Application
        /// </summary>  
        private static object Get(ApplicationName name)
        {
            return HttpContext.Current.Application[name.ToString()];
        }

        /// <summary>
        /// 設置Application
        /// </summary>    
        private static void Set(ApplicationName name, object value)
        {
            HttpContext.Current.Application.Lock();
            HttpContext.Current.Application[name.ToString()] = value;
            HttpContext.Current.Application.UnLock();
        }

        /// <summary>
        /// 清除登入Session
        /// </summary>
        public void RemoveAll()
        {
            HttpContext.Current.Application.Lock();

            foreach (ApplicationName name in Enum.GetValues(typeof(ApplicationName)))
            {
                Set(name, null);
            }

            HttpContext.Current.Application.UnLock();
        }

        #endregion
    }
}