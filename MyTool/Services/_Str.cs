using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public static class _Str
    {
        /// <summary>
        /// 字串null也去空白
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToTrim(this string value)
        {
            return value == null ? string.Empty : value.Trim();
        }

        /// <summary>
        /// 空白預設文字
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToDefault(this string value, string def)
        {
            return string.IsNullOrEmpty(value) ? def : value;
        }

        /// <summary>
        /// 數字顯示為金額
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToPrice(this decimal value)
        {
            if (value < 0)
            {
                return "-$" + (value * -1).ToString("0");
            }
            else
            {
                return "$" + value.ToString("0");
            }
        }

        /// <summary>
        /// 在某個目錄字串變數後面加上反斜線, 如果已經存在則不加
        /// </summary>
        /// <param name="dir">目錄字串變數</param>
        /// <returns>新字串</returns>
        public static string AddRightSlash(this string dir)
        {
            if (dir.Substring(dir.Length - 1, 1) != "/" && dir.Substring(dir.Length - 1, 1) != "\\")
                dir = dir + "/";

            return dir;
        }

        public static string RemoveRightSlash(this string dir)
        {
            if (dir.Substring(dir.Length - 1, 1) == "/" || dir.Substring(dir.Length - 1, 1) == "\\")
                dir = dir.Substring(0, dir.Length - 1);

            return dir;
        }

        /// <summary>
        /// 轉 UTF8 編碼
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string BIG5toUTF8(string value)
        {
            byte[] bytes = Encoding.GetEncoding("BIG5").GetBytes(value);
            byte[] strbig5 = Encoding.Convert(Encoding.GetEncoding("BIG5"), Encoding.Unicode, bytes);
            return Encoding.Default.GetString(bytes);
        }

        /// <summary>
        /// 產生檢查碼
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string CreateCheckCode(string str, int codeLength)
        {
            if (!_Check.IsNumber(str)) return "";

            int sum = 0;
            foreach (char c in str)
            {
                sum += int.Parse(c.ToString());
            }

            return sum.ToString().Substring(0, codeLength) + str;
        }

        /// <summary>
        /// 用換行字元分割成陣列
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static string[] SplitByNewLine(this string str)
        {
            return str.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        }

        /// <summary>
        /// 隱藏部分字串 ex:個資
        /// </summary>
        /// <returns></returns>
        public static string HidePartWords(this string str, string symbol = "*")
        {
            #region sample
            /*
 1
 *

 12
 **

 123
 1*3

 1234
 1**4

 12345
 1***5

 123456
 12**56

 1234567
 12***67

 12345678
 12****78

 123456789
 123***789

 12345678910
 123*****910                 
             */
            #endregion

            if (string.IsNullOrEmpty(str)) return "";

            int startindex = str.Length <= 1 ? 0 : str.Length / 3; //hide 中間 1/3
            int endindex = str.Length - startindex;
            string symbolstr = "";

            for (int i = 0; i < endindex - startindex; i++)
            {
                symbolstr += symbol;
            }

            return str.Substring(0, startindex) + symbolstr + str.Substring(endindex);
        }

        ///// <summary>
        ///// 若圖檔不存在用預設圖片
        ///// </summary>
        ///// <param name="value">The value.</param>
        ///// <returns></returns>
        //public static string ToDefaultPath(this string value)
        //{
        //   return File.Exists(HttpContext.Current.Server.MapPath(value)) ? value : Keys.DefaultImagePath;
        //}

    }
}
