using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using WebMaker.Entity.Models;

namespace WebMaker.Entity.ViewModels
{
    public class ItemViewModel
    {
        public cms_Item Item { get; set; }
        public cms_ItemLanguage ItemLanguage { get; set; }
        public List<LangNode> LangNodes { get; set; }
        public List<cms_ItemFile> ItemFiles { get; set; }
        public mgt_UserProfile UserProfile { get; set; }
        //麵包屑(RouteName, itemSubject)
        public List<BreadCrumb> BreadCrumbs { get; set; }

        //主parent(麵包屑用)
        public Guid ParentID { get; set; }
        //所有parent
        public List<BreadCrumb> ParentItems { get; set; }
    }

    public class BreadCrumb
    {
        public Guid ID { get; set; }
        public string RouteName { get; set; }
        public string Subject { get; set; }
        public string ItemTypes { get; set; }//from structure
        public Guid StructureID { get; set; }
    }

    public class EditItemViewModel : ItemViewModel
    {
        //從標題自動產生路由
        public bool IsAutoRouteName { get; set; }

        //Category-structure下拉選單、Article-GroupSelectList
        public List<SelectListItem> SelectList { get; set; }
        //Article-分類Checkbox
        public List<TreeViewModel> CategoryTree { get; set; }

        //ckeditor上傳的圖片
        public string ContentImagesJson { get; set; }
        //封面
        public List<UploadViewModel> CoverModel { get; set; }
        //封面縮圖尺寸
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
        //內頁圖
        public List<UploadViewModel> SubImageModel { get; set; }

        //訂單允許身分
        public List<CheckBoxListItem> AllowRoleCheckList { get; set; }
        public List<Guid> AllowRoleIDs { get; set; }
        //訂單完成身分
        public List<CheckBoxListItem> CreateRoleSelectList { get; set; }
        public List<Guid> CreateRoleIDs { get; set; }

        //屬於部門Department-下拉選單
        public List<SelectListItem> DepartmentSelectList { get; set; }

        public EditItemViewModel()
        {
        }
        public EditItemViewModel(ItemViewModel baseData)
        {
            this.Item = baseData.Item;
            this.ItemLanguage = baseData.ItemLanguage;
            this.LangNodes = baseData.LangNodes;
            this.ItemFiles = baseData.ItemFiles;
            this.UserProfile = baseData.UserProfile;
            this.BreadCrumbs = baseData.BreadCrumbs;
            this.ParentID = baseData.ParentID;
            this.ParentItems = baseData.ParentItems;
        }
    }


    public class ItemFileViewMdel
    {
        public Guid ItemID { get; set; }

        public Guid StructureID { get; set; }

        public List<UploadViewModel> FileModel { get; set; }

        //縮圖尺寸
        public int ImageWidth { get; set; }
        public int imageHeight { get; set; }
    }

    public class TagModel
    {
        public string Name { get; set; }
        public int Count { get; set; }
    }

    /// <summary>
    /// Item篩選
    /// </summary>
    public class ItemFilter
    {
        /// <summary>
        /// 語系
        /// </summary>
        public LanguageType LangType { get; set; }

        /// <summary>
        /// 結構 (至少要有一個)
        /// </summary>    
        public List<Guid> StructureIDs { get; set; }

        /// <summary>
        /// 單位
        /// </summary>  
        public List<Guid> DepartmentIDs { get; set; }

        /// <summary>
        /// 類別(ParentID)
        /// </summary>    
        public List<Guid> CategoryIDs { get; set; }

        /// <summary>
        /// 路由名稱
        /// </summary>  
        public string RouteName { get; set; }

        /// <summary>
        /// 排除ID
        /// </summary>
        public Guid? ExceptID { get; set; }

        /// <summary>
        /// 已啟用
        /// </summary>
        public bool SelectEnabled { get; set; }

        /// <summary>
        /// 置頂
        /// </summary>
        public bool SelectTop { get; set; }

        /// <summary>
        /// 搜尋字串 for Subject, Description, Content, Keywords, Author
        /// </summary>
        public string SearchString { get; set; }

        /// <summary>
        /// 搜尋類型
        /// </summary>
        public SearchType SearchType { get; set; }

        /// <summary>
        /// 封面類型
        /// </summary> 
        public CoverType? CoverType { get; set; }

        //todo-----

        /// <summary>
        /// 開始範圍 
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// 結束範圍 
        /// </summary>
        public DateTime EndTime { get; set; }
    }

    /// <summary>
    /// ItemTree篩選
    /// </summary>
    public class ItemTreeFilter
    {
        /// <summary>
        /// 項目類型
        /// </summary>
        public ItemType ItemType { get; set; }

        /// <summary>
        /// 語系
        /// </summary>
        public LanguageType LangType { get; set; }

        //------Edit-------

        /// <summary>
        /// 結構起點(文章編輯:Category Tree)
        /// </summary>  
        public List<Guid> TopStructures { get; set; }

        /// <summary>
        /// 結尾接上指定結構(文章編輯:Category Tree接上的文章結構)
        /// </summary>
        public Guid? EndWithStructureID { get; set; }

        /// <summary>
        /// 選取的項目(文章編輯:Category Tree checked)
        /// </summary>
        public List<Guid> CheckIDs { get; set; }

        /// <summary>
        /// 是否接上結構tree(分類編輯:接上結構名稱)
        /// </summary>
        public bool JoinStructureTree { get; set; }

        //------Web-------

        /// <summary>
        /// 選單版位(web:主選單、底部選單)
        /// </summary>
        public MenuPosition? MenuPosition { get; set; }

        /// <summary>
        /// 已啟用(web:只顯示已啟用的)
        /// </summary>
        public bool SelectEnabled { get; set; }

        /// <summary>
        /// 父層不包含也繼續遞迴子層
        /// </summary>      
        public bool EmptyContinue { get; set; }
    }

    /// <summary>
    /// 商品、銷售量、限制
    /// </summary>
    public class ItemProductModel
    {
        public Guid ID { get; set; }

        public string Subject { get; set; }

        public Guid StructureID { get; set; }

        public string ContentTypes { get; set; }

        //售價
        public decimal SalePrice { get; set; }

        //販售數量
        public int StockCount { get; set; }

        public int OriSaleCount { get; set; }

        //統計出的銷售量
        public int SaleCount { get; set; }


        //販售/報名開始
        public DateTime? SaleStartTime { get; set; }

        //販售/報名結束
        public DateTime? SaleEndTime { get; set; }


        //銷售限制
        public int SaleLimit { get; set; }

        //人數最小限制
        public int PeopleMin { get; set; }

        //人數最大限制
        public int PeopleMax { get; set; }
        
        //日期限制
        public DateTime? DateLimit { get; set; }
        
        //排序用
        public int Sort { get; set; }

        //是否限制銷售量
        public bool IsCheckSaleCount { get; set; }


    }
}
