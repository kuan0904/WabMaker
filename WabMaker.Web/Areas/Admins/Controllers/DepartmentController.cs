using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 組織圖(部門)
    /// </summary>   
    public class DepartmentController : AuthBaseController
    {
        private DepartmentService service = new DepartmentService();

        public DepartmentController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {    
            //demo department
            #region
            var demotree = new List<TreeViewModel> {
                new TreeViewModel { Name ="總部", Type = TreeType.Department, ID = Guid.NewGuid(),
                    Nodes =  new List<TreeViewModel> {
                       //new TreeViewModel { Name ="第七屆理事長", Type = TreeType.User, ID = Guid.NewGuid() },
                     
                       new TreeViewModel { Name ="行政組", Type = TreeType.Department, ID = Guid.NewGuid(),
                            Nodes =  new List<TreeViewModel> {
                                new TreeViewModel { Name ="台北市", Type = TreeType.Department, ID = new Guid("da9a626f-9a35-4cd7-b77f-13326e16cf01")
                                    ,Nodes=new List<TreeViewModel> {
                                          new TreeViewModel { Name ="理事長", Type = TreeType.User, ID = Guid.NewGuid() },
                                          new TreeViewModel { Name ="總幹事", Type = TreeType.User, ID = Guid.NewGuid() },
                                    }
                                },
                                new TreeViewModel { Name ="基隆市", Type = TreeType.Department, ID = new Guid("4dbd106a-27eb-4ba8-8a64-56b963c84d80")
                                 ,Nodes=new List<TreeViewModel> {
                                          new TreeViewModel { Name ="主任委員", Type = TreeType.User, ID = Guid.NewGuid() },
                                          new TreeViewModel { Name ="總幹事", Type = TreeType.User, ID = Guid.NewGuid() },
                                    }
                                },
                                new TreeViewModel { Name ="新竹市", Type = TreeType.Department, ID = Guid.NewGuid()  ,Nodes=new List<TreeViewModel> {
                                          new TreeViewModel { Name ="主任委員", Type = TreeType.User, ID = Guid.NewGuid() },
                                          new TreeViewModel { Name ="總幹事", Type = TreeType.User, ID = Guid.NewGuid() },
                                    } },
                                new TreeViewModel { Name ="宜蘭縣", Type = TreeType.Department, ID = Guid.NewGuid()   ,Nodes=new List<TreeViewModel> {
                                          new TreeViewModel { Name ="主任委員", Type = TreeType.User, ID = Guid.NewGuid() },
                                          new TreeViewModel { Name ="總幹事", Type = TreeType.User, ID = Guid.NewGuid() },
                                    }},
                                new TreeViewModel { Name ="嘉義市", Type = TreeType.Department, ID = Guid.NewGuid()   ,Nodes=new List<TreeViewModel> {
                                          new TreeViewModel { Name ="主任委員", Type = TreeType.User, ID = Guid.NewGuid() },
                                          new TreeViewModel { Name ="總幹事", Type = TreeType.User, ID = Guid.NewGuid() },
                                    }},
                                new TreeViewModel { Name ="苗栗縣", Type = TreeType.Department, ID = Guid.NewGuid()  ,Nodes=new List<TreeViewModel> {
                                          new TreeViewModel { Name ="理事長", Type = TreeType.User, ID = Guid.NewGuid() },
                                          new TreeViewModel { Name ="總幹事", Type = TreeType.User, ID = Guid.NewGuid() },
                                    }},
                            }
                       },
                       new TreeViewModel { Name ="教練組", Type = TreeType.Department, ID = Guid.NewGuid(),
                         Nodes =  new List<TreeViewModel> {
                                new TreeViewModel { Name ="106花式A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106花式B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106花式C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106競速A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106競速B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106競速C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106溜冰曲棍球A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106溜冰曲棍球B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106溜冰曲棍球C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106自由式A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106自由式B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106自由式C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106滑板A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106滑板B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106滑板C級", Type = TreeType.User, ID = Guid.NewGuid() },
                              
                         } },
                       new TreeViewModel { Name ="裁判組", Type = TreeType.Department, ID = Guid.NewGuid(),
                         Nodes =  new List<TreeViewModel> {
                                new TreeViewModel { Name ="106花式A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106花式B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106花式C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106競速A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106競速B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106競速C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106溜冰曲棍球A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106溜冰曲棍球B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106溜冰曲棍球C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106自由式A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106自由式B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106自由式C級", Type = TreeType.User, ID = Guid.NewGuid() },

                                new TreeViewModel { Name ="106滑板A級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106滑板B級", Type = TreeType.User, ID = Guid.NewGuid() },
                                new TreeViewModel { Name ="106滑板C級", Type = TreeType.User, ID = Guid.NewGuid() },
                         } },
                    }
                },
                new TreeViewModel { Name ="理事", Type = TreeType.Department, ID = Guid.NewGuid()
                  ,Nodes=new List<TreeViewModel> {
                        new TreeViewModel { Name ="理事長", Type = TreeType.User, ID = Guid.NewGuid() },
                        new TreeViewModel { Name ="常務理事 ", Type = TreeType.User, ID = Guid.NewGuid() },
                        new TreeViewModel { Name ="理事 ", Type = TreeType.User, ID = Guid.NewGuid() },
                  }},
                new TreeViewModel { Name ="監事", Type = TreeType.Department, ID = Guid.NewGuid()
                 ,Nodes=new List<TreeViewModel> {
                        new TreeViewModel { Name ="監事長", Type = TreeType.User, ID = Guid.NewGuid() },
                        new TreeViewModel { Name ="常務監事 ", Type = TreeType.User, ID = Guid.NewGuid() },
                        new TreeViewModel { Name ="監事 ", Type = TreeType.User, ID = Guid.NewGuid() },
                  }},              
            };
            #endregion

            return View();
        }

        [PartialCheck]
        public ActionResult _Tree()
        {
            var tree = service.GetTrees(null, getUser: false);
            return PartialView(tree);
        }

        [PartialCheck]
        public ActionResult GetNode(Guid? id = null)
        {
            var node = new TreeViewModel();
            if (id != null)
            {
                var item = service.Get(id.Value);
                node = service.ItemToTreeNode(item);
            }

            return PartialView("_TreeNode", node);
        }
        
        [PartialCheck]
        public ActionResult Create()
        {
            var model = new mgt_Department();
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(mgt_Department model)
        {
            var result = service.Create(model);
            return Json(result);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.Get(id);
            return PartialView("_Edit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Update(mgt_Department model)
        {
            var result = service.Update(model);
            return Json(result);
        }

        [PartialCheck]
        [HttpPost]
        public ActionResult Delete(Guid id)
        {
            var result = service.Delete(id);
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [PartialCheck]
        public ActionResult Sort()
        {
            var tree = service.GetTrees(null);
            return PartialView("_TreeSort", tree);
        }

        [HttpPost]
        [PartialCheck]
        public ActionResult Sort(List<Guid> IDs)
        {
            var result = service.Sort(IDs);
            return Json(result);
        }
    }
}