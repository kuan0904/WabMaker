using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 帳號類型
    /// </summary>
    public enum AccountType
    {
        /// <summary>
        /// 無
        /// </summary>
        [Display(Name = "無")] //訂閱EPaper
        None = 0,

        /// <summary>
        /// 管理員(可登入前後台)
        /// </summary>
        [Display(Name = "管理員")]
        Admin = 1,

        /// <summary>
        /// 會員(可登入前台)
        /// </summary>
        [Display(Name = "會員")]
        Member = 2,

    }

    /// <summary>
    /// 會員層級
    /// </summary>
    public enum MemberLevel
    {
        [Display(Name = "一般")]
        Normal = 1,

        [Display(Name = "進階")] //報名過活動，必填欄位增加
        Advanced = 2,

        [Display(Name = "最高")]
        Highest = 10
    }

    /// <summary>
    /// 使用者狀態
    /// </summary>
    public enum UserStatus
    {
        /// <summary>
        /// 無
        /// </summary>
        [Display(Name = "無")]
        None = 0,

        /// <summary>
        /// 邀請中
        /// </summary>
        [Display(Name = "邀請中")]
        Invite = 1,

        /// <summary>
        /// 等待驗證中 / 外部登入尚未填入基本個資
        /// </summary>
        [Display(Name = "等待驗證中")]
        OnVerify = 2,

        /// <summary>
        /// 啟用中
        /// </summary>
        [Display(Name = "啟用")]
        Enabled = 10,

        /// <summary>
        /// 停用
        /// </summary>
        [Display(Name = "停用")]
        Stop = 43,

        /// <summary>
        /// 刪除
        /// </summary>
        [Display(Name = "刪除")]
        Delete = 44,
    }

    /// <summary>
    /// 使用者狀態 (篩選用)
    /// </summary>
    public enum SelectUserStatus
    {
        /// <summary>
        /// 無
        /// </summary>
        [Display(Name = "無")]
        None = UserStatus.None,

        /// <summary>
        /// 等待驗證中 / 外部登入尚未填入基本個資
        /// </summary>
        [Display(Name = "等待驗證中")]
        OnVerify = UserStatus.OnVerify,

        /// <summary>
        /// 啟用中
        /// </summary>
        [Display(Name = "啟用")]
        Enabled = UserStatus.Enabled,
    }

    /// <summary>
    /// 登入方式 (多選)
    /// </summary>
    public enum LoginType
    {
        [Display(Name = "")]
        None = 0,

        [Display(Name = "帳號")]
        Account = 1,

        [Display(Name = "Email")]
        Email = 2,

        //Phone = 3, //未啟用

        [Display(Name = "Facebook")]
        Facebook = 10,
    }

    /// <summary>
    /// 會員登入類型 (篩選用)
    /// </summary>
    public enum MemberLoginType
    {
        [Display(Name = "Email")]
        Email = LoginType.Email,

        [Display(Name = "Facebook")]
        Facebook = LoginType.Facebook,
    }

    /// <summary>
    /// 外部登入類型
    /// </summary>
    public enum ExternalType
    {
        Facebook = LoginType.Facebook
    }

    /// <summary>
    /// 驗證碼類型
    /// </summary>
    public enum ValidType
    {
        /// <summary>
        /// Email驗證信
        /// </summary>   
        [Display(Name = "Email驗證信")]
        ConfirmEmail = SystemMailType.ConfirmEmail,

        /// <summary>
        /// 驗證手機
        /// </summary>
        ConfirmPhone = 1001,


        /// <summary>
        /// 忘記密碼驗證信
        [Display(Name = "忘記密碼驗證信")]
        ForgotPassword = SystemMailType.ForgotPassword,
    }

    /// <summary>
    /// 性別
    /// </summary>
    public enum GenderType
    {
        [Display(Name = "男")]
        Male = 1,

        [Display(Name = "女")]
        Female = 2
    }

    /// <summary>
    /// 婚姻狀態
    /// </summary>
    public enum MarriageType
    {
        //[Display(Name = "不提供")]
        //None = 0,

        [Display(Name = "未婚")]
        Single = 1,

        [Display(Name = "已婚")]
        Married = 2
    }

    ///// <summary>
    ///// 學歷類型
    ///// </summary>
    //public enum EducationType
    //{
    //    [Display(Name = "")]
    //    None = 0,

    //    [Display(Name = "國小")]
    //    Elementary = 1,

    //    [Display(Name = "國中")]
    //    JuniorHigh,

    //    [Display(Name = "高中職")]
    //    SeniorHigh,

    //    [Display(Name = "大學/大專")]
    //    University,

    //    [Display(Name = "碩士")]
    //    Master,

    //    [Display(Name = "博士")]
    //    Doctor
    //}

}
