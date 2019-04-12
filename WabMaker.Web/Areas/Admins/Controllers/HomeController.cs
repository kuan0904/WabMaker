using MyTool.Enums;
using MyTool.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using WebMaker.BLL.Helpers;
using WebMaker.BLL.Services;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    public class HomeController : AuthBaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        [AllowAnonymous]
        public ActionResult Test()
        {
            // 非同步取回資料
            string validCode = Guid.NewGuid().ToString();

            var length = 4;

            int min = (int)Math.Pow(10, length - 1);//1000
            int max = (int)Math.Pow(10, length) - 1;//9999
            validCode = new Random().Next(min, max).ToString();

            return Content(validCode);
        }


        [AllowAnonymous]
        public ActionResult ErrorMsg()
        {
            return View("Error");
        }

        /// <summary>
        /// 程式清單
        /// </summary>
        /// <returns></returns>
        public ActionResult CodeList()
        {
            if (!SessionManager.IsSuperManager)
            {
                throw new HttpException(404, "Product not found");
            }

            return View();
        }

        /// <summary>
        /// 移轉滑輪訂單.
        /// </summary>
        /// <returns></returns>
        public ActionResult TransOrder()
        {
            if (!SessionManager.IsSuperManager)
            {
                throw new HttpException(404, "Product not found");
            }

            var service = new OrderService();
            service.ClientID = SessionManager.Client.ID;

            //service.TransOrder();

            return Content("done");
        }

        //public ActionResult TestVirtualAccount()
        //{
        //    if (!SessionManager.IsSuperManager)
        //    {
        //        throw new HttpException(404, "Product not found");
        //    }

        //    var service = new OrderService();
        //    service.ClientID = SessionManager.Client.ID;

        //    var PayDeadline = DateTime.Today.AddDays(5).AddSeconds(-1);
        //    var result = service.CreateVirtualAccount(PayDeadline, 1);

        //    return Content(result.Message + " " + result.Data);
        //}

    }
}