using MyTool.Commons;
using MyTool.Enums;
using MyTool.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Mvc;
using WabMaker.Web.Helpers;
using WebMaker.BLL.Services;

namespace WabMaker.Web.Areas.WebApi.Controllers
{
    /// <summary>
    /// 系統設定
    /// </summary>
    /// <seealso cref="System.Web.Mvc.Controller" />
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    [System.Web.Http.RoutePrefix("api")]
    public class SystemController : BaseApiController
    {
        private SystemSettingService service = new SystemSettingService();

        public SystemController()
        {
            service.ClientID = ApplicationHelper.ClientID;
        }

        /// <summary>
        /// 網站基本資料
        /// </summary>
        /// <returns></returns>
        [System.Web.Http.Route("SiteInfo")]
        [System.Web.Http.HttpGet]
        public CiResult<SiteInfoViewModel> SiteInfo()
        {
            var resultSiteInfo = service.Get<SiteInfoViewModel>(SystemSettingType.SiteInfo);
            if (resultSiteInfo.IsSuccess)
            {
                // Html Decode 
                resultSiteInfo.Data.Footer = HttpUtility.HtmlDecode(resultSiteInfo.Data.Footer);

            }

            return resultSiteInfo;
        }
    }
}