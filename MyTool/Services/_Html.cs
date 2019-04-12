using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace MyTool.Services
{
    public static class _Html
    {
        /// <summary>
        /// 字串長度限制
        /// </summary>
        /// <param name="str"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string WordSubstring(string str, int length = 0)
        {
            if (length > 0 && !(string.IsNullOrEmpty(str)))
            {
                str = str.Length >= length ? str.Substring(0, length - 1) + "..." : str;
                str = str.Trim();
            }

            return str;
        }

        /// <summary>
        /// 去除html標籤
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="keepBr">是否保留換行字元</param>
        /// <returns>string</returns>
        public static string RemoveHtml(string str, bool keepBr = false)
        {
            if (string.IsNullOrEmpty(str))
            {
                return str;
            }

            string NEWLINE = "@@NEWLINE@@";
            string TAB = "@@TAB@@";

            if (keepBr)
            {
                str = str.Replace("</div>", "</div>" + NEWLINE + NEWLINE)
                         .Replace("</p>", "</p>" + NEWLINE)
                         .Replace("<hr/>", "<hr/>" + NEWLINE)
                         .Replace("<br/>", "<br/>" + NEWLINE);

                str = str.Replace("</tr>", "</tr>" + NEWLINE)
                         .Replace("</table>", "</table>" + NEWLINE)
                         .Replace("</th>", "</th>" + TAB)
                         .Replace("</td>", "</td>" + TAB);
            }

            // 去html標籤
            str = System.Text.RegularExpressions.Regex.Replace(str, "<[^>]*>", "");
            // 去特殊字元
            str = System.Text.RegularExpressions.Regex.Replace(str, "&.*?;", "");
            // 去空白
            if (!keepBr)
            {
                str = System.Text.RegularExpressions.Regex.Replace(str, @"\s+", "");
            }

            if (keepBr)
            {
                str = str.Replace(NEWLINE, "\n")
                         .Replace(TAB, "\t");
            }

            return str;
        }

        /// <summary>
        /// 自訂格式
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string CustomFormat_Link(string str, string css = "", bool isNewLink = false)
        {
            if (string.IsNullOrEmpty(str)) return "";

            UrlHelper url = new UrlHelper(HttpContext.Current.Request.RequestContext);

            //a link
            str = Regex.Replace(str, @"\[([^]]*)\]\(([^\s^\)]*)[\s\)]", @"<a class='" + css + "'href='$2' " + (isNewLink ? "target='_blank'" : "") + ">$1</a>");
            str = str.Replace("href='~/", "href='" + url.Content("~/"));

            //break line
            str = str.Replace("\n", "<br>").Replace("\t", "<br>");

            return str;
        }

        /// <summary>
        /// 換行
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string BreakLine(this string str)
        {
            if (string.IsNullOrEmpty(str)) return "";

            //break line
            str = str.Replace("\n", "<br>").Replace("\t", "<br>");

            return str;
        }
    }
}
