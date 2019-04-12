using MyTool.Enums;
using MyTool.Services;
using MyTool.Tools;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;
using WabMaker.Web.MainService;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.Models;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 比賽分組名單
    /// </summary>
    public class OrderTeamController : AuthBaseController
    {
        private OrderService service = new OrderService();
        private StructureService structureService = new StructureService();
        private ItemService itemService = new ItemService();
        private DepartmentService departmentService = new DepartmentService();

        public OrderTeamController()
        {
            if (SessionManager.Client != null)
            {
                service.ClientID = SessionManager.Client.ID;
                service.ClientCode = SessionManager.Client.ClientCode;
                structureService.ClientID = SessionManager.Client.ID;
                structureService.ClientCode = SessionManager.Client.ClientCode;
                itemService.ClientID = SessionManager.Client.ID;
                departmentService.ClientID = SessionManager.Client.ID;
            }
        }

        [HttpGet]
        public ActionResult Index(Guid articleID)
        {
            var data = itemService.GetView(articleID, SystemLanguage);
            var structure = data.Item.cms_Structure;

            //篩選部門
            if (structure.ContentTypes.HasValue((int)ContentType.Department) && !SessionManager.IsSuperManager)
            {
                if (!SessionManager.DepartmentIDs.Contains(data.Item.DepartmentID.Value))
                {
                    return View("Error");
                }
            }

            //狀態統計
            ViewBag.StatusModels = service.GetStatusList(new OrderFilter
            {
                ArticleID = articleID,
                StructureID = structure.ID
            });
            return View(data);
        }

        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, OrderFilter filter,
            OrderTeamSelect Type, ReportStyle ReportStyle, string subject = "", bool IsExport = false)
        {
            filter.LangType = SystemLanguage;
            ViewBag.ReportStyle = ReportStyle;
            string fileName = $"{Type.GetDisplayName()}-{subject}-" + string.Format("{0:MMddHHmmss}", DateTime.Now) + ".xlsx";

            if (Type == OrderTeamSelect.CompetitionUnits)
            {
                var model = itemService.GetCompetitionUnits(filter.ArticleID.Value);

                if (IsExport)
                {
                    #region dataTable
                    List<CompetitionUnitsModelAll> dt = model
                        .SelectMany(x => x.Members.Select(y => new CompetitionUnitsModelAll
                        {
                            Creater = x.Creater,
                            Phone = x.Phone,
                            Email = x.Email,
                            County = x.County,
                            UnitTempNo = x.TempNo,
                            Unit = x.Unit,
                            UnitShort = x.UnitShort,
                            Coach = x.Coach,
                            Leader = x.Leader,
                            Manager = x.Manager,
                            OrderNumber = x.OrderNumber,
                            //TotalPrice = x.TotalPrice,
                            MemberCount = x.MemberCount,
                            MemberTempNo = y.TempNo,
                            MemberName = y.MemberName
                        })).ToList();

                    #endregion
                    var exportResult = ExcelTool.Create(dt);
                    return File(exportResult, "application/vnd.ms-excel", fileName);
                }

                return PartialView("_PageList_Units", model);

            }
            else if (Type == OrderTeamSelect.CompetitionItems)
            {
                var model = itemService.GetCompetitionItems(filter.ArticleID.Value);

                if (IsExport)
                {
                    #region dataTable
                    List<CompetitionItemsModelAll> dt = model
                        .SelectMany(x => x.Teams.SelectMany(y => y.Members.Select(z => new CompetitionItemsModelAll
                        {
                            Subject = x.Subject,
                            Option = x.Option,
                            TeamCount = x.TeamCount,

                            DetailTeamName = y.DetailTeamName,
                            FilePath = y.FilePath,
                            DetailPrice = y.DetailPrice,
                            DiscountPrice = y.DiscountPrice,

                            TempNo = z.TempNo,
                            MemberName = z.MemberName,
                            Unit = z.Unit

                        }))).ToList();
                    #endregion
                    var exportResult = ExcelTool.Create(dt);
                    return File(exportResult, "application/vnd.ms-excel", fileName);
                }

                return PartialView("_PageList_Items", model);
            }
            else if (Type == OrderTeamSelect.CompetitionMembers)
            {
                var model = itemService.GetCompetitionMembers(filter.ArticleID.Value);

                if (IsExport)
                {
                    #region dataTable
                    List<CompetitionMemberItemAll> dt = model
                        .SelectMany(x => x.Items.Select(y => new CompetitionMemberItemAll
                        {
                            TempNo = x.TempNo,
                            MemberName = x.MemberName,
                            IdentityCard = x.IdentityCard,
                            Birthday = x.Birthday,
                            Gender = x.Gender,
                            County = x.County,
                            UnitTempNo = x.UnitTempNo,
                            Unit = x.Unit,
                            Coach = x.Coach,
                            ItemCount = x.ItemCount,
                            Subject = y.Subject,
                            Option = y.Option,
                            DetailPrice = y.DetailPrice,
                            DiscountPrice = y.DiscountPrice
                        })).ToList();

                    #endregion
                    var exportResult = ExcelTool.Create(dt);
                    return File(exportResult, "application/vnd.ms-excel", fileName);
                }

                return PartialView("_PageList_Members", model);

            }
            else
            {
                var structure = structureService.Get(filter.StructureID);
                ViewBag.Structure = structure;

                var model = service.GetTeamList(param, filter);
                return PartialView("_PageList", model);
            }
        }

        /// <summary>
        /// 建立選手、單位編號
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateNumber(Guid articleID)
        {
            var result = itemService.SetCompetitionMembers(articleID);

            return Content(_Json.ModelToJson(result));
        }

        /// <summary>
        /// 比賽銷售和狀態統計
        /// </summary>
        /// <returns></returns>
        public ActionResult GetCompetitionCount(Guid articleID)
        {
            var model = itemService.GetCompetitionCount(articleID);

            var data = itemService.GetView(articleID, SystemLanguage);       
            ViewBag.ParentSubject = data.ItemLanguage.Subject;
            //取訂單結構
            var structure = structureService.Get(data.Item.cms_Structure.ChildStructures.FirstOrDefault(x => x.ItemTypes.HasValue((int)ItemType.Order)).ID);
            ViewBag.Structure = structure;

            return View(model);
        }
    }
}