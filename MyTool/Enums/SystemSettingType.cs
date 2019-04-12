using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 系統設定類型 (對應ViewModel)
    /// </summary>
    public enum SystemSettingType
    {
        /// <summary>
        /// 客戶基本資料
        /// </summary>
        ClientInfo,

        /// <summary>
        /// 網站基本資料
        /// </summary>
        SiteInfo,

        /// <summary>
        /// The API key
        /// </summary>
        ApiKey,

        /// <summary>
        /// 寄件服務設定
        /// </summary>
        SmtpServer,

        /// <summary>
        /// 簡訊服務設定
        /// </summary>
        SmsService,
    }

    /// <summary>
    /// 功能開關
    /// </summary>
    public enum ClientSetting
    {
        MemberLog = 1,

        //組織圖
        Department = 2,


        //編輯角色
        RoleEdit = 11,

        //角色有編號和時間限制
        RolTimeLimit = 12,


        //簡訊服務
        SMS = 50,

        //------console------

        //國泰自動對帳
        CathayPayAuto = 100,

        //延遲寄信
        SendEmailDelay = 150,
    }

    /// <summary>
    /// 登入頁樣式
    /// </summary>
    public enum LoginStyle
    {
        Normal = 0,

        //跳窗
        Popup = 1
    }
}
