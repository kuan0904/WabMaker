using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 使用者動作
    /// </summary>
    public enum UserLogType
    {
        [Display(Name = "註冊")]
        SignUp = 1,

        [Display(Name = "修改個資")]
        UpdateInfo = 2,

        [Display(Name = "修改密碼")]
        UpdatePassword = 3,

        [Display(Name = "修改Email")]
        UpdateEmail = 4,

        [Display(Name = "修改手機")]
        UpdatePhone = 4,


        [Display(Name = "新增身分")]
        CreateRole = 10,

        [Display(Name = "編輯身分")]
        UpdateRole = 11,

        [Display(Name = "刪除身分")]
        DeleteRole = 12,


        [Display(Name = "寄送驗證信")]
        SendValidCode = 20,

        [Display(Name = "寄送Email驗證信")]
        SendConfirmEmail = 21,
        
        [Display(Name = "Email通過驗證")]
        EnabledEmail = 22,

        [Display(Name = "寄送忘記密碼驗證信")]
        SendForgotPassword = 23,


        [Display(Name = "寄送手機驗證簡訊")]
        SendConfirmPhone = 26,

        [Display(Name = "手機通過驗證")]
        EnabledPhone = 27,


        [Display(Name = "訂閱電子報")]
        SubEpaper = 30,
    

        [Display(Name = "啟用帳號登入")]
        AccountLogin = 100,

        [Display(Name = "啟用Email登入")]
        EmailLogin = 101,

        [Display(Name = "啟用Facebook登入")]
        FacebookLogin = 102,
    }
}
