using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 系統郵件、簡訊
    /// </summary>
    public enum SystemMailType
    {
        [Display(Name = "自訂")]
        None = 0,

        //[Display(Name = "註冊通知信")]
        //UserCreate,

        [Display(Name = "驗證")] //email or 簡訊
        ConfirmEmail = 1, // 點此連結完成信箱認證 

        [Display(Name = "忘記密碼")]
        ForgotPassword = 2, //密碼已重設請重新登入

        //訂閱電子報


        //---訂單:每種structure各建一個---  10 ~ 100
        [Display(Name = "待確認")] //訂單成立
        OrderNew = 11,

        [Display(Name = "待付款")]
        OrderNonPayment = 20,

        [Display(Name = "處理中")] //已付款
        OrderProcessing = 30,

        [Display(Name = "已出貨")]
        OrderShipment = 31,

        //--------會員---------
        [Display(Name = "未完成")]
        OrderTeamEdit = 50,

        [Display(Name = "退回")]
        OrderRefuse = 70,

        [Display(Name = "取消")]
        OrderCancel = 99,

        [Display(Name = "完成")]
        OrderDone = 100,


        //---選手指派:---  200~
        [Display(Name = "選手指派")] //to教練
        AssignCreate = 200,

        [Display(Name = "選手指派取消")] //to教練
        AssignCancel = 205,

        [Display(Name = "選手參賽")] //to家長
        MemberEnter = 210,

        [Display(Name = "選手參賽取消")] //to家長
        MemberOut = 215,

        [Display(Name = "選手退賽")] //to教練
        MemberQuit = 220,
    }

    /// <summary>
    /// 郵件狀態
    /// </summary>
    public enum MailStatus
    {
        [Display(Name = "編輯")]
        Edit = 0,

        [Display(Name = "寄送中")]
        Sending = 1,

        [Display(Name = "完成")]
        Done = 2,

        [Display(Name = "刪除")]
        Delete = 99
    }

    /// <summary>
    /// 郵件標籤
    /// </summary>
    public enum MailTag
    {
        [Display(Name = "網站名稱")]
        WebsiteName,

        [Display(Name = "網址")]
        WebsiteUrl,

        [Display(Name = "會員郵件")]
        UserEmail,

        [Display(Name = "會員名稱")]
        UserName,

        [Display(Name = "驗證連結")]
        ConfirmUrl,

        [Display(Name = "驗證碼")]//for SMS
        ValidCode,


        [Display(Name = "訂單內容")]
        OrderContent,

        [Display(Name = "訂單匯款帳號")]
        OrderATMInfo,
                
        [Display(Name = "訂單管理員備註")]
        OrderAdminNote,


        [Display(Name = "指派選手姓名")]
        MemberName,

        [Display(Name = "指派比賽名稱")]
        CompetitionCame
    }
    
    /// <summary>
    /// 寄送簡訊結果
    /// </summary>
    public enum SmsResultType
    {
        [Display(Name = ("未寄送"))]
        Undo = 0,

        [Display(Name = ("寄送中"))]
        Progress = 1,

        [Display(Name = ("完成"))]
        Done = 2,

        [Display(Name = ("失敗"))]
        Fail = 3
    }
}
