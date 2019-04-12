using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Web;
using MyTool.Enums;
using MyTool.ViewModels;
using MyTool.Commons;
using MyTool.Services;

namespace MyTool.Tools
{
    public static class UploadTool
    {
        /// <summary>
        /// 檔案上傳根目錄
        /// </summary>
        public const string _RootFolder = "FileUpload";
        public const string _ThumbnailFolder = "Thumbnail";
        public static string GetFileFolder(string SystemName, SourceType folderType)
        {
            return $"~/{_RootFolder}/{SystemName}/{folderType.ToString()}/";
        }

        /// <summary>
        /// 上傳檔案
        /// </summary>     
        /// <param name="fileUpload">file upload</param>
        /// <param name="fileType">檔案類型</param>
        /// <param name="fileFolder">資料夾路徑</param>       
        /// <param name="saveThumb">是否儲存縮圖</param>
        /// <param name="sizeW">縮圖寬</param>
        /// <param name="sizeH">縮圖高</param>
        /// <returns></returns>
        public static CiResult<UploadViewModel> FileUpload(HttpPostedFileBase fileUpload, FileType fileType, string fileFolder,
                                                         bool saveThumb = true, int sizeW = 0, int sizeH = 0)
        {
            var result = new CiResult<UploadViewModel>();

            try
            {
                if (fileUpload == null || fileUpload.ContentLength == 0)
                {
                    result.Message = SystemMessage.FileUploadEmpty;
                    return result;
                }

                #region 資料夾
                //string fileFolder = null;
                string thumbFolder = null;

                // 檔案資料夾
                //fileFolder = _RootFolder + folderType.ToString().AddRightSlash();
                _File.MakeDir(fileFolder);

                // 縮圖資料夾
                if (saveThumb)
                {
                    thumbFolder = fileFolder + $"/{_ThumbnailFolder}/";
                    _File.MakeDir(thumbFolder);
                }

                #endregion

                #region 副檔名類型
                var checkArr = fileType.GetDescription().Split(',');
                var checkString = new List<string>(checkArr);

                // FileName紀錄原始檔名, path儲存只用數字檔名
                string fileExt = Path.GetExtension(fileUpload.FileName).ToLower();
                string newName = $"{DateTime.Now.ToString("yMdHmsfff")}{fileExt}";

                // 檢查副檔名是否為圖片檔
                if (checkString != null && (!string.IsNullOrEmpty(fileExt) && !checkString.Contains(fileExt)))
                {
                    result.Message += $"{SystemMessage.ExtensionError} [{fileUpload.FileName}]";
                    return result;
                }

                #endregion

                #region 儲存檔案
                UploadViewModel model = new UploadViewModel();

                // 原始檔名
                model.OriName = fileUpload.FileName;
                model.Extension = fileExt;
                model.Size = fileUpload.ContentLength;
                model.FilePath = fileFolder + newName;
                model.FileType = fileType;

                // 儲存檔案
                fileUpload.SaveAs(HttpContext.Current.Server.MapPath(model.FilePath));

                // 儲存縮圖
                if (saveThumb && fileType == FileType.Images)
                {
                    string thumbnailPath = thumbFolder + newName;
                    if (sizeW != 0 || sizeH != 0)
                    {
                        var thumbResult = ImageTool.Resize(model.FilePath, thumbnailPath, sizeW, sizeH);
                        if (thumbResult.IsSuccess)
                        {
                            model.ThumbnailPath = thumbnailPath;
                        }
                        else {
                            result.Message = thumbResult.Message;
                            return result;
                        }
                    }
                }
                #endregion

                result.Data = model;
                result.IsSuccess = true;

            }
            catch (Exception e)
            {
                result.Message = SystemMessage.FileUploadError;
                _Log.CreateText("FileUpload:" + e.Message);
            }

            return result;
        }

    }
}
