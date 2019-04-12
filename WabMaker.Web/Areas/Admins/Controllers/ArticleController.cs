using MyTool.Commons;
using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// Item-文章管理(網站內容)
    /// </summary>
    public class ArticleController : AuthBaseController
    {
        #region service
        private ItemService service = new ItemService();
        private StructureService structureService = new StructureService();
        private RoleService roleService = new RoleService();
        private DepartmentService departmentService = new DepartmentService();

        public ArticleController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;

                structureService.ClientID = SessionManager.Client.ID;
                structureService.ClientCode = SessionManager.Client.ClientCode;

                roleService.ClientID = SessionManager.Client.ID;

                departmentService.ClientID = SessionManager.Client.ID;
                departmentService.IsSuperManager = SessionManager.IsSuperManager;
            }
        }
        #endregion

        #region CRUD
        /// <summary>
        /// Index
        /// </summary>
        /// <param name="type">StructureID</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResult Index(Guid type)
        {
            var structure = structureService.Get(type);
            return View(structure);
        }

        /// <summary>
        /// Article列表 指定StructureID、Lang
        /// </summary>       
        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, ItemFilter filter)
        {
            var structure = structureService.Get(filter.StructureIDs.FirstOrDefault());
            if (structure == null)
                throw new Exception();
            ViewBag.Structure = structure;

            //sort           
            if (structure.RequiredTypes.HasValue((int)ContentType.Date))
            {
                param.SortColumn = SortColumn.Date;
            }
            else if (structure.RequiredTypes.HasValue((int)ContentType.Sort))
            {
                param.SortColumn = SortColumn.Sort;
            }
            else
            {
                param.SortColumn = SortColumn.CreateTime;
            }

            //篩選部門
            if (structure.ContentTypes.HasValue((int)ContentType.Department) && !SessionManager.IsSuperManager)
            {
                filter.DepartmentIDs = SessionManager.DepartmentIDs;
            }

            var result = service.GetListView(param, filter);
            return PartialView("_PageList", result);
        }

        [PartialCheck]
        public ActionResult Create(Guid structureIDs, Guid? parentID = null, string viewName = "_Edit")
        {
            cms_Structure structure = structureService.Get(structureIDs);
            List<Guid> topStructures = new List<Guid>();
            cms_Item parentItem = null;

            if (viewName == "_Edit") //一般
                topStructures = structureService.GetTopParents(structureIDs);
            else //項目
                parentItem = service.Get(parentID.Value);


            var model = new EditItemViewModel
            {
                Item = new cms_Item
                {
                    StructureID = structure.ID,
                    cms_Structure = structure
                },
                ItemLanguage = new cms_ItemLanguage { LanguageType = structure.DefaultLanguage },
                CategoryTree = service.GetTrees(parentItem, new ItemTreeFilter
                {
                    ItemType = ItemType.Category,
                    TopStructures = topStructures,
                    EndWithStructureID = structure.ID,
                    LangType = (LanguageType)structure.DefaultLanguage,
                }),
                SelectList = service.GetArticleGroup(structure, (LanguageType)structure.DefaultLanguage),
                AllowRoleCheckList = service.GetOrderRoleCheckList(null, ItemOrderRoleType.OrderAllowRole),
                CreateRoleSelectList = service.GetOrderRoleCheckList(null, ItemOrderRoleType.OrderCreateRole),
                DepartmentSelectList = departmentService.GetSelectList(SessionManager.DepartmentID)
            };

            // parent
            if (parentID != null)
            {
                model.ParentID = parentID.Value;
            }

            // routename random (no edit)
            if (!structure.ContentTypes.HasValue((int)ContentType.RouteName))
            {
                model.Item.RouteName = Guid.NewGuid().ToString();
            }

            // get sort
            if (structure.ContentTypes.HasValue((int)ContentType.Sort))
            {
                model.Item.Sort = service.GetSort(structureIDs) + 1;

                if (viewName == "_OptionEdit")
                    model.Item.Sort *= -1; //新的在最下方
            }

            // people range defalut:1
            if (structure.ContentTypes.HasValue((int)ContentType.PeopleRange))
            {
                model.Item.PeopleMin = 1;
                model.Item.PeopleMax = 1;
            }

            // Html Decode       
            model.Item.cms_Structure.Template = HttpUtility.HtmlDecode(model.Item.cms_Structure.Template);

            return PartialView(viewName, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Create(EditItemViewModel model)
        {
            var proResult = Process(model);
            if (!proResult.IsSuccess)
            {
                return Json(proResult);
            }

            var result = service.Create(proResult.Data);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id, LanguageType langType, string viewName = "_Edit")
        {
            var dataModel = service.GetView(id, langType);
            var model = new EditItemViewModel(dataModel);
            List<Guid> topStructures = new List<Guid>();
            cms_Item parentItem = null;

            if (viewName == "_Edit") //一般
                topStructures = structureService.GetTopParents(model.Item.StructureID);
            else //項目
                parentItem = service.Get(model.ParentID);

            model.CategoryTree = service.GetTrees(parentItem, new ItemTreeFilter
            {
                ItemType = ItemType.Category,
                TopStructures = topStructures,
                EndWithStructureID = model.Item.StructureID,
                CheckIDs = model.ParentItems?.Select(x => x.ID).ToList(),
                LangType = (LanguageType)model.Item.cms_Structure.DefaultLanguage
            });
            model.SelectList = service.GetArticleGroup(model.Item.cms_Structure, (LanguageType)model.Item.cms_Structure.DefaultLanguage, model.ParentID);

            model.AllowRoleCheckList = service.GetOrderRoleCheckList(id, ItemOrderRoleType.OrderAllowRole);
            model.CreateRoleSelectList = service.GetOrderRoleCheckList(id, ItemOrderRoleType.OrderCreateRole);
            model.DepartmentSelectList = departmentService.GetSelectList(SessionManager.DepartmentID, selectedID: model.Item.DepartmentID);

            //語系可null
            if (model.ItemLanguage == null)
            {
                model.ItemLanguage = new cms_ItemLanguage { LanguageType = (int)langType };
            }
            else
            {
                // Html Decode
                model.ItemLanguage.Content = HttpUtility.HtmlDecode(model.ItemLanguage.Content);
                model.ItemLanguage.TemplateText = HttpUtility.HtmlDecode(model.ItemLanguage.TemplateText);
            }
            // Html Decode       
            model.Item.cms_Structure.Template = HttpUtility.HtmlDecode(model.Item.cms_Structure.Template);

            return PartialView(viewName, model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult Update(EditItemViewModel model)
        {
            var proResult = Process(model);
            if (!proResult.IsSuccess)
            {
                return Json(proResult);
            }

            var result = service.Update(proResult.Data);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result);
        }

        /// <summary>
        /// 新增修改前處理
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns></returns>
        private CiResult<EditItemViewModel> Process(EditItemViewModel model)
        {
            var result = new CiResult<EditItemViewModel>();

            model.Item.UpdateUser = SessionManager.UserID;

            //ckeditor
            model.ItemFiles = SetEditorFile(model.ContentImagesJson);

            //fileupload cover       
            var coverResult = SetItemFile(model.CoverModel, SourceType.ItemConver, model.ImageWidth, model.ImageHeight);
            result.IsSuccess = coverResult.IsSuccess;
            result.Message = coverResult.Message;
            if (!coverResult.IsSuccess)
            {
                return result;
            }
            model.ItemFiles.AddRange(coverResult.Data);

            //fileupload subimage
            var subResult = SetItemFile(model.SubImageModel, SourceType.ItemSubImage, 0, 0);
            result.IsSuccess = subResult.IsSuccess;
            result.Message = subResult.Message;
            if (!subResult.IsSuccess)
            {
                return result;
            }
            model.ItemFiles.AddRange(subResult.Data);

            model.AllowRoleIDs = _Model.ToCheckedGuid(model.AllowRoleCheckList);
            model.CreateRoleIDs = _Model.ToCheckedGuid(model.CreateRoleSelectList);

            result.Data = model;
            return result;
        }

        [PartialCheck]
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var result = service.Delete(id, SessionManager.UserID);
            if (result.IsSuccess)
            {
                //remove all cache
                var cacheHelper = new CacheHelper();
                cacheHelper.RemoveAll();
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [PartialCheck]
        public ActionResult Sort(ItemFilter filter)
        {
            var structure = structureService.Get(filter.StructureIDs.FirstOrDefault());
            if (structure == null)
                throw new Exception();
            ViewBag.Structure = structure;

            var result = service.GetListView(new PageParameter
            {
                SortColumn = SortColumn.Sort,
                IsPaged = false,

            }, filter);

            return PartialView("_Sort", result);
        }

        [HttpPost]
        [PartialCheck]
        public ActionResult Sort(List<Guid> IDs)
        {
            var result = service.Sort(IDs);
            return Json(result);
        }

        [AllowAnonymous]
        public ActionResult GetAllTags(TagType type)
        {
            var result = service.GetAllTags(type);
            var data = result.Data?.Select(x => x.Name).ToList();

            return Json(data, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// CKEditorUpload
        /// </summary>
        /// <param name="upload"></param>
        /// <param name="CKEditorFuncNum"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult CKEditorUpload(HttpPostedFileBase upload, string CKEditorFuncNum)
        {
            string result = "";

            // 上傳圖片
            var uploadResult = ItemFileUpload(upload, FileType.Images, SourceType.Editor);
            if (!uploadResult.IsSuccess)
            {
                return View("Error");
            }

            /*將model傳回js, 儲存Item時再一並新增
              Ckeditor上傳的圖片，只做紀錄用，顯示時直接用path不經過db*/
            string jsonStr = JsonConvert.SerializeObject(uploadResult.Data);

            //var saveResult = service.CreateFile(model);
            //if (!saveResult.IsSuccess)
            //{
            //    return View("Error");
            //}

            //回傳圖片路徑顯示畫面上
            var imageUrl = Url.Content(uploadResult.Data.FilePath);
            var vMessage = string.Empty;
            result = @"<html><body><script>"
                     + "window.parent.CKEDITOR.tools.callFunction(" + CKEditorFuncNum + ", \"" + imageUrl + "\", \"" + vMessage + "\");"
                     + "window.parent.site.ckeditor.setImage(" + jsonStr + ");"
                     + "</script></body></html>";


            return Content(result);
        }
        #endregion

        #region ItemFile、UserProfile

        [PartialCheck]
        public ActionResult ItemFile(Guid itemID, Guid fileStructureID)
        {
            var structure = structureService.Get(fileStructureID);
            var itemFiles = service.GetItemFiles(itemID, fileStructureID);
            ViewBag.Structure = structure;

            var FileTypes = structure.FileTypes.ToContainList<FileType>();
            var FileToolTypes = structure.FileToolTypes.ToContainList<FileToolType>();
            var model = new FileViewModel
            {
                ItemID = itemID,
                IdName = "FileModel",
                FileTypes = FileTypes,
                FileToolTypes = FileToolTypes,
                FileList = itemFiles,
                IsMultiple = true,
                ImageWidth = structure.ImageWidth,
                ImageHeight = structure.ImageHeight
            };
            return PartialView("_ItemFile", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult ItemFile(ItemFileViewMdel model)
        {
            var fileResult = SetItemFile(model.FileModel, SourceType.ItemFile, model.ImageWidth, model.imageHeight, structureID: model.StructureID);
            if (!fileResult.IsSuccess)
            {
                return Json(fileResult);
            }

            var result = service.UpdateItemFile(model.ItemID, fileResult.Data);

            return Json(result);
        }

        /// <summary>
        /// 人物檔案(只有8geman使用)
        /// </summary>
        /// <param name="itemID">The item identifier.</param>
        /// <param name="subject">subject</param>
        /// <returns></returns>
        [PartialCheck]
        public ActionResult ItemUserProfile(Guid itemID, string subject)
        {
            ViewBag.Subject = subject;

            var model = service.GetUserProfileByItem(itemID);
            if (model == null)
            {
                model = new mgt_UserProfile
                {
                    ItemID = itemID
                };
            }

            return PartialView("_UserProfile", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PartialCheck]
        public ActionResult ItemUserProfile(mgt_UserProfile model)
        {
            var result = service.UpdateItemUserProfile(model);

            return Json(result);
        }


        #endregion

        #region 訂單項目
        /// <summary>
        /// 項目
        /// </summary>
        /// <param name="itemID">The item identifier.</param>
        /// <param name="subject">subject</param>
        /// <returns></returns>
        [PartialCheck]
        public ActionResult OptionList(Guid itemID, Guid optionStructureID, string subject)
        {
            ViewBag.Subject = subject;
            ViewBag.ItemID = itemID;

            var structure = structureService.Get(optionStructureID);
            if (structure == null)
                throw new Exception();
            ViewBag.Structure = structure;

            var model = service.GetSubOrderList(optionStructureID, itemID, (LanguageType)structure.DefaultLanguage);
            // var subModel = service.GetSubOrderList(optionStructureID, itemID, (LanguageType)structure.DefaultLanguage);


            return PartialView("_OptionList", model);
        }


        [PartialCheck]
        public ActionResult OptionGet(Guid itemID, LanguageType langType)
        {
            return Update(itemID, langType, "_OptionGet");
        }

        [PartialCheck]
        public ActionResult OptionCreate(Guid structureIDs, Guid itemID)
        {
            return Create(structureIDs, itemID, "_OptionEdit");
        }

        [PartialCheck]
        public ActionResult OptionUpdate(Guid itemID, LanguageType langType)
        {
            return Update(itemID, langType, "_OptionEdit");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult OptionEdit(EditItemViewModel model, bool isCreate)
        {
            var proResult = Process(model);
            if (!proResult.IsSuccess)
            {
                return Json(proResult);
            }

            var result = isCreate ?
                service.Create(proResult.Data) : service.Update(proResult.Data);

            var dataResult = new CiResult<string>();
            dataResult.IsSuccess = result.IsSuccess;
            dataResult.Message = result.Message;
            dataResult.Data = $"?itemID={model.Item.ID}&langType={model.ItemLanguage.LanguageType}";
            dataResult.ID = model.Item.ID;

            return Json(dataResult);
        }

        [PartialCheck]
        public ActionResult OptionDelete(Guid itemID)
        {
            return Delete(itemID);
        }
        #endregion

        #region 圖片處理private

        /// <summary>
        /// ckeditor圖片處理 (SourceType: Editor)
        /// </summary>
        /// <param name="ContentImagesJson">The structure images json.</param>
        /// <returns></returns>
        private List<cms_ItemFile> SetEditorFile(string ContentImagesJson)
        {
            var itemFileList = new List<cms_ItemFile>();

            if (!string.IsNullOrEmpty(ContentImagesJson))
            {
                var models = JsonConvert.DeserializeObject<List<cms_ItemFile>>(ContentImagesJson);
                itemFileList.AddRange(models);
            }

            return itemFileList;
        }


        /// <summary>
        /// fileupload圖片處理 (SourceType: ItemConver、ItemFile)
        /// </summary>    
        /// <param name="fileModels">圖片List</param>
        /// <param name="sourceType">來源類型</param>
        /// <param name="imageWidth">縮圖尺寸width</param>
        /// <param name="imageHeight">縮圖尺寸height</param>
        /// <param name="structureID">組成類型</param>
        /// <returns></returns>
        private CiResult<List<cms_ItemFile>> SetItemFile(List<UploadViewModel> fileModels, SourceType sourceType, int imageWidth, int imageHeight, Guid? structureID = null)
        {
            var result = new CiResult<List<cms_ItemFile>>();
            var itemFileList = new List<cms_ItemFile>();

            //image file
            int sort = 0;
            if (fileModels != null)
            {
                foreach (var fileModel in fileModels)
                {
                    sort++;
                    switch (fileModel.FileStatus)
                    {
                        case FileStatus.New:
                            if (fileModel.FileType == FileType.YouTube)
                            {
                                //youtube
                                var youteube = YoutubeToItemFile(fileModel.FilePath, sourceType, sort, fileModel.Subject);
                                if (structureID != null)
                                {
                                    youteube.StructureID = structureID.Value;
                                }
                                itemFileList.Add(youteube);
                            }
                            else
                            {
                                //upload         
                                var uploadResult = ItemFileUpload(fileModel.FileUpload, fileModel.FileType, sourceType, imageWidth, imageHeight, sort, fileModel.Subject);
                                if (uploadResult.IsSuccess)
                                {
                                    if (structureID != null)
                                    {
                                        uploadResult.Data.StructureID = structureID.Value;
                                    }
                                    itemFileList.Add(uploadResult.Data);
                                }
                                else
                                {
                                    //---break---
                                    result.Message = uploadResult.Message;
                                    return result;
                                }
                            }

                            break;

                        case FileStatus.Delete:
                        case FileStatus.Normal:
                            if (fileModel.ID != Guid.Empty)//未上傳的刪除 直接不加入
                            {
                                var data = ToIitemFile(fileModel, sourceType, sort);
                                itemFileList.Add(data);
                            }
                            break;

                    }//end switch

                }//end foreach

            }//end if

            result.IsSuccess = true;
            result.Data = itemFileList;

            return result;
        }

        /// <summary>
        /// Item圖片上傳
        /// </summary>
        /// <param name="upload">The upload.</param>
        /// <param name="filetype">The filetype.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="sort">The sort.</param>
        /// <returns></returns>
        private CiResult<cms_ItemFile> ItemFileUpload(HttpPostedFileBase upload, FileType filetype, SourceType sourceType, int imageWidth = 0, int imageHeight = 0, int sort = 0, string subject = "")
        {
            var result = new CiResult<cms_ItemFile>();

            var fileFolder = UploadTool.GetFileFolder(SessionManager.Client.SystemName, sourceType);
            var uploadResult = UploadTool.FileUpload(upload, filetype, fileFolder, true, imageWidth, imageHeight);
            if (!uploadResult.IsSuccess)
            {
                result.Message = uploadResult.Message;
            }
            else
            {
                uploadResult.Data.Subject = subject;
                result.Data = ToIitemFile(uploadResult.Data, sourceType, sort);
                result.IsSuccess = true;
            }

            return result;
        }

        /// <summary>
        /// ViewModel轉cms_ItemFile
        /// </summary>
        /// <param name="viewModel">The view model.</param>
        /// <param name="filetype">The filetype.</param>
        /// <param name="sourceType">Type of the source.</param>
        /// <param name="sort">The sort.</param>
        /// <returns></returns>
        private cms_ItemFile ToIitemFile(UploadViewModel viewModel, SourceType sourceType, int sort)
        {
            return new cms_ItemFile()
            {
                ID = viewModel.ID,
                OriName = viewModel.OriName,
                FilePath = viewModel.FilePath,
                ThumbnailPath = viewModel.ThumbnailPath,
                Subject = viewModel.Subject,
                Size = viewModel.Size,
                Extension = viewModel.Extension,
                FileType = (int)viewModel.FileType,
                SourceType = (int)sourceType,
                Sort = sort,
                IsDelete = viewModel.FileStatus == FileStatus.Delete,
                UpdateUser = SessionManager.UserID,
            };
        }

        private cms_ItemFile YoutubeToItemFile(string youtubeid, SourceType sourceType, int sort, string subject = "")
        {
            return new cms_ItemFile
            {
                OriName = FileType.YouTube.ToString(),
                FilePath = youtubeid,
                Subject = subject,
                FileType = (int)FileType.YouTube,
                SourceType = (int)sourceType,
                Sort = sort,
                UpdateUser = SessionManager.UserID,
            };
        }
        #endregion
    }
}