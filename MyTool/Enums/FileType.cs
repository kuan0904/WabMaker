using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Enums
{
    /// <summary>
    /// 檔案組成類型
    /// </summary>
    public enum FileType
    {
        [Display(Name = "圖片")]
        [Description(".jpg,.jpeg,.gif,.png,.bmp")]
        Images = 1,

        //[Display(Name = "檔案")]
        //Document = 2,

        [Display(Name = "PDF")]
        [Description(".pdf")]
        PDF = 10,

        [Display(Name = "Word")]
        [Description(".doc,.docx")]
        Word = 11,

        [Display(Name = "Excel")]
        [Description(".xls,.xlsx,.csv")]
        Excel = 12,

        [Display(Name = "PowerPoint")]
        [Description(".ppt,.pptx")]
        PowerPoint = 13,

        [Display(Name = "Youtube")]
        [Description("")]
        YouTube = 30,

        [Display(Name = "聲音")]
        [Description(".mp3,.wav,.wma,.mid,.midi")]
        Audio = 50,

    }

    /// <summary>
    /// 封面類型(篩選用)
    /// </summary>
    public enum CoverType
    {
        Images = FileType.Images,
        YouTube = FileType.YouTube
    }

    /// <summary>
    /// 檔案包含功能
    /// </summary>
    public enum FileToolType
    {
        //[Display(Name = "圖片編輯")]
        //ImageEdit = 1,

        [Display(Name = "編輯標題")]
        Subject = 5
    }

    /// <summary>
    /// 檔案來源(資料夾名稱)
    /// </summary>
    public enum SourceType
    {
        //文章封面
        ItemConver = 0,

        //文章檔案
        ItemFile = 1,

        //編輯器
        Editor = 2,

        //系統
        System = 3,

        //文章內頁圖
        ItemSubImage = 4,


        //會員訂單
        MemberOrder = 50,
    };

    /// <summary>
    /// 檔案狀態
    /// </summary>
    public enum FileStatus
    {
        New = 0,
        Delete = 1,
        Normal = 2
    }
}
