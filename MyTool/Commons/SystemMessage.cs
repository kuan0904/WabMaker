using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Commons
{
    /// <summary>
    /// 系統訊息
    /// </summary>
    public class SystemMessage
    {
        public static string Success = "Success!";
        public static string ErrorAndHelp = "發生錯誤，請通知管理員";
        public static string Error = "發生錯誤";

        // ============CRUD==============  
        public static string CreateSuccess = "新增成功";
        public static string CreateFail = "新增失敗";
        public static string UpdateSuccess = "編輯成功";
        public static string UpdateFail = "編輯失敗";
        public static string DeleteSuccess = "刪除成功";
        public static string DeleteFail = "刪除失敗";
        public static string EnableSuccess = "啟用狀態已變更";
        public static string EnableFail = "狀態變更失敗";
        public static string TagError = "取得Tag失敗";
        public static string SortSuccess = "排序成功";
        public static string SortFail = "排序失敗";

        // ============帳號============== 
        public static string LoginFail = "登入失敗，帳號或密碼錯誤";
        public static string LoginNotEnabled = "登入失敗，帳號未啟用 (請完成Email驗證)";
        public static string LoginError = "登入發生錯誤";
        public static string PasswordWrong = "密碼錯誤";
        public static string AccountExist = "帳號已被註冊:{0}";
        public static string LogoutSuccess = "登出成功";
        public static string CaptchaError = "驗證碼錯誤";

        public static string ValidCodeWrong = "驗證碼格式錯誤";
        public static string ValidCodUrlError = "驗證碼錯誤或已過期，請重寄認證信";
        public static string ValidCodError = "驗證碼錯誤";
        public static string IdCardExist = "身分證或護照已被註冊:{0}";
        public static string EmailExist = "Email已被註冊:{0}";
        public static string PhoneExist = "手機號碼已被註冊:{0}";
        public static string PhoneNotFound = "查無手機號碼，請重新確認";
        public static string PhoneValidSuccess = "手機號碼已完成驗證";
        public static string EmailNotFound = "查無Email，請重新確認";
        public static string EmailValidSuccess = "Email帳號已完成驗證，請登入";
        public static string PasswordValidSuccess = "請重設您的密碼";
        public static string PasswordResetSuccess = "密碼已變更，請重新登入";
        public static string AccoundNotEnabled = "帳號未啟用";
        public static string AccoundToFbLogin = "未建立密碼，請使用Facebook帳號登入";

        // =====Client、SystemSetting======== 
        public static string AdminRouteError = "管理後台錯誤";
        public static string ClientError = "查無Client";
        public static string SystemNumberError = "取得系統編號失敗";
        public static string SettingNone = "查無設定檔";

        // ============欄位檢查============== 
        public static string DataTableSortError = "排序發生錯誤";
        public static string FieldNull = "{0} 不可空白";
        public static string FieldExist = "{0} 已存在";
        //public static string NameExist = "名稱已存在";
        public static string TypeExist = "類型已存在";
        public static string BadCharacters = "包含錯誤字元";
        public static string LanguageError = "語系錯誤";
        public static string FormatError = "{0} 格式錯誤";        

        // ============檔案上傳============== 
        public static string FileUploadError = "上傳失敗";
        public static string FileUploadEmpty = "查無上傳檔案";
        public static string ExtensionError = "不正確的副檔名";
        public static string ImageResizeNull = "縮圖未指定尺寸";
        public static string ImageResizeSmall = "圖片尺寸太小";

        // ============會員============== 
        public static string SignUpSuccess = "註冊完成";
        public static string BindingSuccess = "綁定Email完成";
        public static string FacebookError = "Facebook登入錯誤，請通知管理員";
        public static string MailServerError = "郵件服務錯誤，請通知管理員";
        public static string EmailConfirm = "信箱驗證信已發送至您的信箱：<br> {0}<br>"  //email                                        + ""
                                          + "請點擊信件內的連結網址，即可完成信箱驗證。";
        public static string PasswordReset = "重設密碼連結已發送至您的信箱：<br> {0}<br>" //email
                                           + "請點擊信件內的連結網址，完成密碼重設。";
        public static string SendSuccess = "通知信已寄出";

        public static string SmsServiceError = "簡訊服務錯誤，請通知管理員";
        public static string PhoneConfirm = "驗證碼已發送至您的手機：{0}<br>"  //sms                                        + ""
                                          + "請於下一頁輸入驗證碼，即可完成驗證";
        public static string SendSmsSuccess = "通知簡訊已寄出";

        // ============訂單==============
        public static string NoAuthorize = "無此權限";
        public static string EmailNotEnabled = "Email未驗證";       
        public static string OrderEmpty = "未建立資料";
        public static string OrderStatusError = "狀態為 {0} 不可編輯";
        public static string OrderDeleteError = "{0} 有{1}資料 不可刪除";
        //public static string OrderStatusReject = "不允許 {0}";
        public static string StartTimeError = "尚未開始";
        public static string EndTimeError = "已結束";
        public static string NoStock = "已售完";
        public static string StockNotEnough = "已超出銷售量";
        public static string StockRemain = "{0}: 剩餘{1}組";
        public static string NoRepeatMember = "不可重複報名";
        public static string NoCombineMulti = "不可合併多次";
    }
}
