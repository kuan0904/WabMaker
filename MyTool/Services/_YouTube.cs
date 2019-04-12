using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public class _YouTube
    {
        /// <summary>
        /// 取得YoutubeId
        /// </summary>
        /// <param name="urlstr">The urlstr.</param>
        /// <returns></returns>
        public static string GetId(string urlstr)
        {
            string youtubeid = "";

            if (!string.IsNullOrEmpty(urlstr))
            {
                string[] symbols = { "?v=", "&v=", "/v/", "/embed/", "//youtu.be/" };
                int strAt = -1;
                int symLen = 0;

                foreach (var sym in symbols)
                {
                    if (strAt == -1)
                    {
                        strAt = urlstr.IndexOf(sym);
                        symLen = sym.Length;
                    }
                }

                if (strAt > 0)
                {
                    int endAt = urlstr.IndexOf("&", strAt + 1);
                    endAt = endAt > 0 ? endAt : urlstr.Length;
                    youtubeid = urlstr.Substring(strAt + symLen, endAt - strAt - symLen);
                }
            }

            return youtubeid;
        }

        /// <summary>
        /// 取得iframe網址
        /// </summary>
        /// <returns></returns>
        public static string SetIframe(string youtubeid)
        {
            return $"https://www.youtube.com/embed/{youtubeid}?showinfo=0";
        }

        public static string SetThumbnail(string youtubeid)
        {
            return $"http://i.ytimg.com/vi/{youtubeid}/maxresdefault.jpg";
        }

        public static string SetUrl(string youtubeid)
        {
            return $"https://www.youtube.com/watch?v={youtubeid}";
        }

    }
}
