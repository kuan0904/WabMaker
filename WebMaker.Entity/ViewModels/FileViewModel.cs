using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaker.Entity.Models;

namespace WebMaker.Entity.ViewModels
{
    public class FileViewModel
    {
        public Guid ItemID { get; set; }

        //上傳用input name
        public string IdName { get; set; }

        //結構類型(檔案組成類型)
        //public cms_Structure Structure { get; set; }
        public List<FileType> FileTypes { get; set; }
      
        //包含功能
        public List<FileToolType> FileToolTypes { get; set; }

        //已存在的檔案列表
        public List<cms_ItemFile> FileList { get; set; }

        //是否是多檔
        public bool IsMultiple { get; set; }

        //縮圖尺寸
        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }
    }

    public class FileDetailModel
    {
        public string name { get; set; }
        public cms_ItemFile file { get; set; }
        public List<FileToolType> FileToolTypes { get; set; }
    }
}
