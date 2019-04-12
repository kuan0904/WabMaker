using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WabMaker.Web.Authorize;

namespace WabMaker.Web.Areas.Admins.Controllers
{
    //[Authorize]
    [AutoRolesAuthorize]
    public class AuthBaseController : Controller
    {
        public LanguageType SystemLanguage = LanguageType.Chinese;

        /// <summary>
        /// 顯示alert訊息
        /// </summary>
        protected void SetAlertMessage(string message , AlertType type) {
            TempData["AlertMessage"] = message;
            TempData["AlertType"] = type.ToString();
        }
        /// <summary>
        /// 驗證失敗錯誤訊息
        /// </summary>
        //protected void SetModelStateError()
        //{
        //    IEnumerable<ModelError> errors = ModelState.Keys.SelectMany(key => ModelState[key].Errors);

        //    foreach (ModelError error in errors)
        //    {
        //        TempData["alert"] += error.ErrorMessage + "<br/>";
        //    }
        //}
    }
}