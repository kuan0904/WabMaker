using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WabMaker.Web.Helpers;

namespace WabMaker.Web.Areas.WebApi.Controllers
{
    public class BaseApiController : ApiController
    {
        public BaseApiController()
        {
            if (ApplicationHelper.ClientID == Guid.Empty)
            {
                //設定網站基本參數
                var applicationHelper = new ApplicationHelper();
                applicationHelper.Init();
            }
        }
    }
}