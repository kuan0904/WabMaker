using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyTool.ViewModels
{
    public class UploadViewModel
    {
        public Guid ID { get; set; }

        // 原始名稱
        public string OriName { get; set; }
        // 檔案路徑
        public string FilePath { get; set; }
        // 縮圖路徑
        public string ThumbnailPath { get; set; }

        // 編輯標題
        public string Subject { get; set; }
        // 大小
        public int Size { get; set; }
        // 副檔名
        public string Extension { get; set; }

        public FileType FileType { get; set; }


        public FileStatus FileStatus { get; set; }
        public HttpPostedFileBase FileUpload { get; set; }
    }

}
