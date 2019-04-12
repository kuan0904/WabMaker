using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using WabMaker.Web.Areas.WebApi.Controllers;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Services;
using WebMaker.Entity.ViewModels;

namespace WabMaker.Web.Controllers
{ 
    public class TestController : BaseApiController
    {
        [System.Web.Http.HttpGet]
        public Guid Index()
        {
            return ApplicationHelper.ClientID;
        }
    }
}