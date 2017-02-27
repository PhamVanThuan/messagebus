using System.Web;
using System.Web.Optimization;

namespace YmatouMQAdmin.WebApp2
{
    public class BundleConfig
    {
        // 有关 Bundling 的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkId=254725
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js",
                        "~/Scripts/jquery.unobtrusive-ajax.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"
                        ));

            // 使用要用于开发和学习的 Modernizr 的开发版本。然后，当你做好
            // 生产准备时，请使用 http://modernizr.com 上的生成工具来仅选择所需的测试。
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            //bootstrap-dialog
            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-dialog").Include(
                        "~/Content/bootstrap-dialog/js/bootstrap-dialog.min.js",
                        "~/Content/bootstrap-dialog/js/bootstrap-dialog-extension.js"
                        ));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-dialog").Include(
                       "~/Content/bootstrap-dialog/css/bootstrap-dialog.min.css"
                       ));

            //bootstrap-datetimepicker
            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-datetimepicker").Include(
                        "~/Content/bootstrap-datetimepicker/js/bootstrap-datetimepicker.min.js",
                        "~/Content/bootstrap-datetimepicker/js/locales/bootstrap-datetimepicker.zh-CN.js",
                        "~/Content/bootstrap-datetimepicker/js/locales/bootstrap-datetimepicker-extension.js"
                        ));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-datetimepicker").Include(
                       "~/Content/bootstrap-datetimepicker/css/bootstrap-datetimepicker.min.css"
                       ));


            //bootstrap-table
            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-table").Include(
                        "~/Content/bootstrap-table/bootstrap-table.js",
                         "~/Content/bootstrap-table/locale/bootstrap-table-zh-CN.js",
                         "~/Content/bootstrap-table/extensions/accent-neutralise/bootstrap-table-accent-neutralise.js",
                         "~/Content/bootstrap-table/extensions/angular/bootstrap-table-angular.js",
                         "~/Content/bootstrap-table/extensions/cookie/bootstrap-table-cookie.js",
                         "~/Content/bootstrap-table/extensions/editable/bootstrap-table-editable.js",
                         "~/Content/bootstrap-table/extensions/export/bootstrap-table-export.js",
                         "~/Content/bootstrap-table/extensions/filter/bootstrap-table-filter.js",
                         "~/Content/bootstrap-table/extensions/filter-control/bootstrap-table-filter-control.js",
                         "~/Content/bootstrap-table/extensions/flat-json/bootstrap-table-flat-json.js",
                         "~/Content/bootstrap-table/extensions/group-by/bootstrap-table-group-by.js",
                         "~/Content/bootstrap-table/extensions/key-events/bootstrap-table-key-events.js",
                         "~/Content/bootstrap-table/extensions/mobile/bootstrap-table-mobile.js",
                         "~/Content/bootstrap-table/extensions/multiple-search/bootstrap-table-multiple-search.js",
                         "~/Content/bootstrap-table/extensions/multiple-sort/bootstrap-table-multiple-sort.js",
                         "~/Content/bootstrap-table/extensions/natural-sorting/bootstrap-table-natural-sorting.js",
                         "~/Content/bootstrap-table/extensions/reorder-columns/bootstrap-table-reorder-columns.js",
                         "~/Content/bootstrap-table/extensions/reorder-rows/bootstrap-table-reorder-rows.js",
                         "~/Content/bootstrap-table/extensions/resizable/bootstrap-table-resizable.js",
                         "~/Content/bootstrap-table/extensions/toolbar/bootstrap-table-toolbar.js"
                        ));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-table").Include(
                       "~/Content/bootstrap-table/bootstrap-table.css",
                       "~/Content/bootstrap-table/extensions/group-by/bootstrap-table-group-by.css",
                        "~/Content/bootstrap-table/extensions/reorder-rows/bootstrap-table-reorder-rows.css"
                       ));


            //bootstrap-submenu
            bundles.Add(new ScriptBundle("~/bundles/js/bootstrap-submenu").Include(
                       "~/Content/bootstrap-submenu/js/bootstrap-submenu.min.js"
                       ));

            bundles.Add(new StyleBundle("~/bundles/css/bootstrap-submenu").Include(
                       "~/Content/bootstrap-submenu/css/bootstrap-submenu.min.css"
                       ));


        }
    }
}