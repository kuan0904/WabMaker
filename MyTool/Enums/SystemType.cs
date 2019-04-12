using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 選單類型
    /// </summary>
    public enum MenuType
    {
        [Display(Name = "後台")]
        Admin = 0,

        [Display(Name = "會員專區")]
        Mmeber = 1
    }

    /// <summary>
    /// 選單板位
    /// </summary>
    public enum MenuPosition
    {
        [Display(Name = "主選單")]
        Top = 1,

        [Display(Name = "底部選單")]
        Bottom = 2,

        [Display(Name = "Banner")]
        Banner = 10,
    }

    /// <summary>
    /// 系統功能
    /// </summary>
    public enum ControllerType
    {
        ///// <summary>
        ///// 無
        ///// </summary>   
        //[Display(Name = "Controller")]
        //None = 0,

        /// <summary>
        /// 分類管理
        /// </summary>
        [Display(Name = "分類管理")]
        Category = 1,

        /// <summary>
        /// 文章管理
        /// </summary>
        [Display(Name = "文章管理")]
        Article = 2,

        ///// <summary>
        ///// 商品
        ///// </summary>
        //[Display(Name = "商品")]
        //Product = 10,

        /// <summary>
        /// 訂單
        /// </summary>
        [Display(Name = "訂單")]
        Order = 11,

        /// <summary>
        /// 比賽分組名單
        /// </summary>
        [Display(Name = "比賽分組名單")]
        OrderTeam = 15,

        ///// <summary>
        ///// 行銷
        ///// </summary>
        //[Display(Name= "行銷")]
        //Marketing = 12,

        ///// <summary>
        ///// 活動
        ///// </summary>
        //[Display(Name = "活動")]
        //Activity = 20,

        /// <summary>
        /// 會員
        /// </summary>
        [Display(Name = "會員管理")]
        Member = 50,

        [Display(Name = "選手管理")]
        MemberChildren = 52,

        [Display(Name = "入帳明細")]
        PayMessage = 60,

        ///// <summary>
        ///// 外部登入
        ///// </summary>
        //[Display(Name= "外部登入")]
        //ExternalLoginSetting = 51,

        [Display(Name = "通知信管理")]
        EmailTemplate = 95,

        /// <summary>
        /// 郵件服務
        /// </summary>
        [Display(Name = "郵件服務")]
        MailBox = 96,

        /// <summary>
        /// 組織圖
        /// </summary>
        [Display(Name = "組織圖")]
        Department = 97,

        /// <summary>
        /// 角色管理
        /// </summary>
        [Display(Name = "權限管理")]
        Role = 98,

        /// <summary>
        /// 帳號管理
        /// </summary>
        [Display(Name = "帳號管理")]
        User = 99,

        /// <summary>
        /// 系統設定
        /// </summary>
        [Display(Name = "系統設定")]
        SystemSetting = 100,

    }

    /// <summary>
    /// 操作權限(多選)
    /// </summary>
    public enum ActionType
    {
        /// <summary>
        /// 瀏覽
        /// </summary>        
        [Display(Name = "Index")]
        Index = 2,

        /// <summary>
        /// 分頁
        /// </summary>
        [Display(Name = "getPageList")]
        getPageList = 3,

        /// <summary>
        /// 樹
        /// </summary>
        [Display(Name = "_Tree")]
        _Tree = 4,

        /// <summary>
        /// 樹節點
        /// </summary>
        [Display(Name = "GetNode")]
        GetNode = 5,


        /// <summary>
        /// 新增
        /// </summary>       
        [Display(Name = "Create")]
        Create = 10,

        /// <summary>
        /// 編輯
        /// </summary>      
        [Display(Name = "Update")]
        Update = 11,

        /// <summary>
        /// 刪除
        /// </summary>     
        [Display(Name = "Delete")]
        Delete = 12,

        /// <summary>
        /// 瀏覽
        /// </summary>     
        [Display(Name = "View")]
        View = 13,

        /// <summary>
        /// CKEditor (Article only)
        /// </summary>
        [Display(Name = "CKEditor")]
        CKEditorUpload = 20,

        /// <summary>
        /// 檔案列表 (Article only)
        /// </summary>
        [Display(Name = "ItemFile")]
        ItemFile = 21,

        /// <summary>
        /// 排序
        /// </summary>
        [Display(Name = "Sort")]
        Sort = 22,

        /// <summary>
        /// 人物檔案(只有8geman使用)
        /// </summary>
        [Display(Name = "ItemUserProfile")]
        ItemUserProfile = 50,

        /// <summary>
        /// 基本資料
        /// </summary>
        [Display(Name = "基本資料")]
        Profile = 100,

        /// <summary>
        /// 修改密碼
        /// </summary>
        [Display(Name = "修改密碼")]
        Password = 101
    }

    /// <summary>
    /// Log動作紀錄
    /// </summary>
    public enum ActionLogType
    {
        Login = 0,

        Create = 10,
        Update = 11,
        Delete = 12,

        //other
    }
}
