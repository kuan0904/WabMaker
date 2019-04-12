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

namespace WabMaker.Web.Areas.Admins.Controllers
{
    /// <summary>
    /// 對帳功能
    /// </summary>
    public class PayMessageController : AuthBaseController
    {
        private OrderService service = new OrderService();
       
        public PayMessageController()
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
            return View();
        }
     
        [PartialCheck]
        public ActionResult GetPageList(PageParameter param, OrderFilter filter)
        {   
            filter.LangType = SystemLanguage;
            var result = service.GetPayMessage(param, filter);

            return PartialView("_PageList", result);
        }

    }
}