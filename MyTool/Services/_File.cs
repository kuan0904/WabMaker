using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace MyTool.Services
{
    public class _File
    {
        /// <summary>
        /// 建立一個目錄
        /// </summary>
        /// <param name="ps_dir"></param>
        public static void MakeDir(string ps_dir)
        {
            ps_dir = _Str.RemoveRightSlash(ps_dir);

            //相對路徑變絕對
            if (ps_dir.StartsWith("~/"))
                ps_dir = HttpContext.Current.Server.MapPath(ps_dir);
         
            //若不存才在建立
            if (!Directory.Exists(ps_dir))
                Directory.CreateDirectory(ps_dir);
        }

        /// <summary>
        /// 讀取文字檔案
        /// </summary>
        /// <param name="strFilePath"></param>
        /// <returns></returns>
        public static string StreamReadTextFile(string strFilePath)
        {
            if (File.Exists(strFilePath) == true)
            {
                StreamReader objStreamReader = new StreamReader(strFilePath, Encoding.UTF8);
                string strText = objStreamReader.ReadToEnd();
                objStreamReader.Close();
                objStreamReader.Dispose();
                return strText;
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// 寫入文字檔案
        /// </summary>
        /// <param name="strContent"></param>
        /// <param name="strFilePath"></param>
        public static void StreamWriterTextFile(string strContent, string strFilePath)
        {
            StreamWriter objStreamWriter = new StreamWriter(strFilePath);
            objStreamWriter.Write(strContent);
            objStreamWriter.Close();
            objStreamWriter.Dispose();
        }
    }
}
