using System.Web;
using System.Web.Optimization;

namespace WabMaker.Web
{
    public class BundleConfig
    {
        // 如需「搭配」的詳細資訊，請瀏覽 http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery-migrate-{version}.js")); //for高版本jquery兼容

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // 使用開發版本的 Modernizr 進行開發並學習。然後，當您
            // 準備好實際執行時，請使用 http://modernizr.com 上的建置工具，只選擇您需要的測試。
            //bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
            //            "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));



            // 後台---------------------------

            bundles.Add(new ScriptBundle("~/bundles/backjs").Include(
                      "~/Scripts/jquery-{version}.js",
                      "~/Scripts/jquery-ui-{version}.js",
                      "~/Scripts/bootstrap.min.js",
                      "~/Scripts/bootstrap-select.min.js",
                      "~/Scripts/bootstrap-datepicker.min.js",
                      "~/Scripts/moment.min.js",
                      "~/Scripts/bootstrap-datetimepicker.min.js",
                      "~/Scripts/bootstrap-tagsinput.min.js",
                      "~/Scripts/typeahead.bundle.min.js",//for tag
                      "~/Scripts/respond.js",
                      "~/Scripts/toastr.min.js",
                      "~/Scripts/jquery.validate*",
                      "~/Scripts/assets/ace.min.js",
                      //"~/Scripts/assets/ace-extra.min.js",
                      //"~/Scripts/assets/ace-elements.min.js",
                      "~/Scripts/custom/floatingFormLabels.js"               
                      ));

            bundles.Add(new StyleBundle("~/Content/backcss").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/bootstrap-select.min.css",
                      "~/Contents/bootstrap-datepicker.min.css",
                      "~/Content/bootstrap-datetimepicker.min.css",
                      "~/Content/bootstrap-tagsinput.css",
                      "~/Content/font-awesome.min.css",
                      "~/Content/assets/ace.min.css",
                      "~/Content/custom/site.css",
                      //"~/Content/jsTree/themes/default/style.css",    
                       "~/Content/toastr.css"
                      ));

        }
    }
}
