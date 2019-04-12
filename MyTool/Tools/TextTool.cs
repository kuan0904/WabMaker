using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace MyTool.Tools
{
    /// <summary>
    /// 讀寫文字檔
    /// </summary>
    public static class TextTool
    {
        //讀取文字檔案
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
        //寫入文字檔案
        public static void StreamWriterTextFile(string strContent, string strFilePath)
        {
            StreamWriter objStreamWriter = new StreamWriter(strFilePath);
            objStreamWriter.Write(strContent);
            objStreamWriter.Close();
            objStreamWriter.Dispose();
        }

    }
}
