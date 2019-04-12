using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ViewModels
{
    public class BaseSetting { } //for T 約束條件

    /// <summary>
    /// 客戶基本資料設定
    /// </summary>
    public class ClientInfoViewModel : BaseSetting
    {
        /// <summary>
        /// 客戶名稱
        /// </summary>
        [Display(Name = "公司名稱")]
        public string ClientName { get; set; }

        /// <summary>
        /// 公司統編
        /// </summary>
        [Display(Name = "公司統編")]
        public string CompanyNo { get; set; }

        /// <summary>
        /// 聯絡電話
        /// </summary>
        [Display(Name = "聯絡電話")]
        public string Phone { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        [Display(Name = "公司地址")]
        public string Address { get; set; }

        /// <summary>
        /// Logo圖示
        /// </summary>
        [Display(Name = "Logo圖示")]
        public string Logo { get; set; }
    }

    /// <summary>
    /// 網站基本資料設定
    /// </summary>
    public class SiteInfoViewModel : BaseSetting
    {
        /// <summary>
        /// 網站名稱
        /// </summary>    
        [Display(Name = "網站名稱")]
        public string SiteName { get; set; }

        /// <summary>
        /// 網站描述
        /// </summary>
        [Display(Name = "description")]
        public string MetaTagDescription { get; set; }

        /// <summary>
        /// 網站關鍵字
        /// </summary>
        [Display(Name = "keywords")]
        public string MetaTagKeyword { get; set; }

        /// <summary>
        /// 網站作者
        /// </summary>
        [Display(Name = "author")]
        public string MetaTagAuthor { get; set; }

        /// <summary>
        /// FB分享圖片 (首頁用)
        /// </summary>
        [Display(Name = "og:image")]
        public string MetaTagOgImage { get; set; }

        /// <summary>
        /// Footer
        /// </summary>
        [Display(Name = "網頁Footer資訊")]
        public string Footer { get; set; }

        /// <summary>
        /// Footer
        /// </summary>
        [Display(Name = "網頁Footer資訊")]
        public string Footer2 { get; set; }

        /// <summary>
        /// Copyright
        /// </summary>    
        [Display(Name = "Copyright")]
        public string Copyright { get; set; }

        /// <summary>
        /// 聯絡信箱
        /// </summary>
        [Display(Name = "聯絡信箱")]
        public string Email { get; set; }

        /// <summary>
        /// FB網址
        /// </summary>
        [Display(Name = "Facebook網址")]
        public string Facebook { get; set; }

        /// <summary>
        /// YouTube網址
        /// </summary>
        [Display(Name = "YouTube網址")]
        public string YouTube { get; set; }

        /// <summary>
        /// Instagram
        /// </summary>
        [Display(Name = "Instagram網址")]
        public string Instagram { get; set; }
    }

    /// <summary>
    /// Api Key 設定
    /// </summary>
    public class ApiKeyViewModel : BaseSetting
    {
        /// <summary>
        /// GoogleAnalyticsId
        /// </summary>
        [Display(Name = "Google Analytic ID")]
        public string GoogleAnalyticsId { get; set; }


        /// <summary>
        /// FacebookAppID (外部登入 app.UseFacebookAuthentication)
        /// </summary>
        [Display(Name = "Facebook AppId")]
        public string FacebookAppId { get; set; }

        /// <summary>
        /// FacebookAppSecret
        /// </summary>
        [Display(Name = "Facebook AppSecret")]
        public string FacebookAppSecret { get; set; }

        /* 
        /// <summary>
        /// GoogleClientId (外部登入 app.UseGoogleAuthentication)
        /// </summary>
        [Display(Name = "Google ClientId")]
        public string GoogleClientId { get; set; }

        /// <summary>
        /// GoogleClientSecret
        /// </summary>
        [Display(Name = "Google ClientSecret")]
        public string GoogleClientSecret { get; set; }
        */

        /// <summary>
        /// GoogleApi金鑰
        /// </summary>
        //[Display(Name = "Google Api金鑰")]
        //public string GoogleAPIKey { get; set; }

    }

    /// <summary>
    /// 寄件服務設定
    /// </summary>
    public class SmtpServerViewModel : BaseSetting
    {
        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 寄件伺服器
        /// </summary>
        [Display(Name = "MailServer")]
        public string MailServer { get; set; }

        /// <summary>
        /// 寄件連接埠
        /// </summary>
        [Display(Name = "Port")]
        public int Port { get; set; }

        /// <summary>
        /// 寄件是否使用SSL
        /// </summary>
        [Display(Name = "是否使用SSL")]
        public bool IsSsl { get; set; }

        /// <summary>
        /// 寄件者名稱
        /// </summary>
        [Display(Name = "寄件者名稱")]
        public string DisplayName { get; set; }

        /// <summary>
        /// 寄件信箱
        /// </summary>
        [Display(Name = "寄件信箱")]
        public string MailAddress { get; set; }

        /// <summary>
        /// 寄件者帳號
        /// </summary>
        [Display(Name = "帳號")]
        public string Account { get; set; }

        /// <summary>
        /// 寄件者密碼
        /// </summary>
        [Display(Name = "密碼")]
        public string Password { get; set; }


    }

    /// <summary>
    /// 簡訊服務設定
    /// </summary>
    public class SmsServiceViewModel : BaseSetting
    {
        /// <summary>
        /// 是否啟用
        /// </summary>
        [Display(Name = "是否啟用")]
        public bool IsEnabled { get; set; }

        /// <summary>
        /// 帳號
        /// </summary>
        [Display(Name = "Username")]
        public string Username { get; set; }

        /// <summary>
        /// 密碼
        /// </summary>
        [Display(Name = "Password")]
        public string Password { get; set; }
        
    }
}
