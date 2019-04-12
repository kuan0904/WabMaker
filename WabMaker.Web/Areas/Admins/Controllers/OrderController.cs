using MyTool.Enums;
using MyTool.Services;
using MyTool.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WabMaker.Web.MainService;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;
using static MyTool.Tools.MailTool;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 訂單紀錄(活動報名)
    /// </summary>
    public class OrderController : AuthBaseController
    {
        private OrderService service = new OrderService();
        private StructureService structureService = new StructureService();
        private RoleService roleService = new RoleService();
        private UserService userService = new UserService();

        public OrderController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;
                structureService.ClientID = SessionManager.Client.ID;
                structureService.ClientCode = SessionManager.Client.ClientCode;
                roleService.ClientID = SessionManager.Client.ID;
                userService.ClientID = SessionManager.Client.ID;
            }
        }

        [HttpGet]
        public ActionResult Index(Guid type, Guid? articleID = null)
        {
            var structure = structureService.Get(type);

            //文章下拉選單
            ItemService itemService = new ItemService { ClientID = SessionManager.Client.ID };
            var param = new PageParameter
            {
                IsPaged = false
            };
            if (structure.RequiredTypes.HasValue((int)ContentType.Date))
            {
                param.SortColumn = SortColumn.Date;
            }
            else
            {
                param.SortColumn = SortColumn.Sort;
            }

            var filter = new ItemFilter();
            //篩選部門
            if (structure.ContentTypes.HasValue((int)ContentType.Department) && !SessionManager.IsSuperManager)
            {
                filter.DepartmentIDs = SessionManager.DepartmentIDs;
            }

            //篩選文章
            filter.StructureIDs = structure.ID.ToListObject();
            filter.LangType = SystemLanguage;
            var data = itemService.GetListView(param, filter);

            ViewBag.SelectItem = data.Data.ToList();

            //篩選文章群組
            var parentStructures = structure.ParentStructure.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.ArticleGroup));
            if (parentStructures!=null)            {
                filter.StructureIDs = parentStructures.ID.ToListObject();
                filter.LangType = SystemLanguage;
                var dataGroup = itemService.GetListView(param, filter);

                ViewBag.SelectGroupItem = dataGroup.Data.Select(
                    x => new SelectListItem
                    {
                        Value = x.Item.ID.ToString(),
                        Text = x.ItemLanguage.Subject,
                        Selected = x.Item.ID == articleID
                    }).ToList();
            }

            return View(structure);
        }

        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, OrderFilter filter)
        {
            var structure = structureService.Get(filter.StructureID);
            if (structure == null)
                throw new Exception();
            ViewBag.Structure = structure;

            //篩選部門
            if (structure.ContentTypes.HasValue((int)ContentType.Department) && !SessionManager.IsSuperManager)
            {
                filter.DepartmentIDs = SessionManager.DepartmentIDs;
            }

            filter.LangType = SystemLanguage;
            filter.IsAdmin = true;
            var result = service.GetList(param, filter);
            return PartialView("_PageList", result);
        }

        [PartialCheck]
        public ActionResult View(Guid id)
        {
            var model = service.GetView(id);
            ViewBag.structure = structureService.Get(model.Order.StructureID);
            ViewBag.IsEdit = false;

            return PartialView("_View", model);
        }

        [PartialCheck]
        public ActionResult Update(Guid id)
        {
            var model = service.GetView(id);
            ViewBag.structure = structureService.Get(model.Order.StructureID);
            //與view共用
            ViewBag.IsEdit = true;
            //身分選擇
            ViewBag.RoleSelectList = roleService.GetSelectList(AccountType.Member, model.NewRoleRelation?.RoleID);

            return PartialView("_View", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Update(OrderViewModel model)
        {
            if (model.User == null) model.User = new mgt_User();
            model.User.ID = SessionManager.UserID;

            var result = service.ChangeStatus(model, AccountType.Admin, true);

            //通知信
            if (result.IsSuccess)
            {
                var mailService = new MailService(SessionManager.Client.ID);

                //存在emailTemplate才寄信
                if (mailService.IsExistTemplate((SystemMailType)model.Order.OrderStatus, model.Order.StructureID))
                {
                    var user = userService.Get(model.Order.CreateUser);
                    var dataOrder = service.GetView(model.Order.ID, SystemLanguage, DataLevel.Simple);
                    var mailCoutent = new ReplaceMailContent
                    {
                        UserName = user.Name,
                        UserEmail = user.Email,
                        OrderContent = mailService.CreateOrderContent(dataOrder),
                        OrderATMInfo = mailService.CreateOrderATMInfo(dataOrder),
                        OrderAdminNote = dataOrder.Order.PublicNote
                    };
                    var mailResult = await mailService.SendEmail(user.ID, mailCoutent, (SystemMailType)model.Order.OrderStatus, structureID: model.Order.StructureID);
                    result.Message += "<br>" + mailResult.Message;
                }
            }

            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateDetail(OrderViewModel model)
        {
            if (model.User == null) model.User = new mgt_User();
            model.User.ID = SessionManager.UserID;

            var result = service.ChangeDetailStatusAll(model, AccountType.Admin);
            
            return Json(result);
        }

        [PartialCheck]
        [HttpPost]
        public ActionResult Revert(Guid id)
        {
            var result = service.Revert(id, SessionManager.UserID);

            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}