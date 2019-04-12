using MyTool.Commons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyTool.Services
{
    /// <summary>
    /// 設格式驗證
    /// </summary>
    public class _Check
    {

        /// 驗證Email格式
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns>bool.</returns>
        public static bool IsEmail(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            Regex reg = new Regex(@"\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*");
            Match m = reg.Match(str);
            return m.Success;
        }

        /// <summary>
        /// 檢查密碼格式
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsPassword(string str, int type = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            bool result = false;

            switch (type)
            {
                case 0: // 英數字、長度6~12
                    Regex reg0 = new Regex(@"^.[A-Za-z0-9]+$");
                    result = reg0.Match(str).Success;
                    result &= str.Length >= 6 && str.Length <= 12;
                    break;

            }

            return result;
        }

        /// <summary>
        /// 是否是數字
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            Regex reg = new Regex(@"^[0-9]*$");
            Match m = reg.Match(str);
            return m.Success;
        }

        /// <summary>
        /// 檢查電話格式
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static bool IsPhone(string str, int type = 0)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            bool result = false;

            switch (type)
            {
                case 0: // 純數字
                    Regex reg0 = new Regex(@"^[0-9]{7,10}$");// {n,m} 字數n~m個  
                    result = reg0.Match(str).Success;
                    break;

                case 1: // - + () #
                    Regex reg1 = new Regex(@"^[0-9\-\+\(\)\#]{7,18}$");
                    result = reg1.Match(str).Success;
                    break;
            }

            return result;
        }

        /// <summary>
        /// 驗證身份證字號
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool IsIDcard(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            Regex reg = new Regex(@"^[A-Z]{1}[1-2]{1}[0-9]{8}$");
            Match m = reg.Match(str);
            if (m.Success == false)
            {
                return false;
            };

            // 開頭英文字母=N1N2  A=10,B=12  (ascii A=65)
            // 九個數字= N3~N11
            // N1 + N2*9 + N3*8 + N4*7 + N5*6 + N6*5 + N7*4 + N8*3 + N9*2 + N10 + N11 = 10的倍數
            int result = 0;
            int count = 8;

            // 第一個符號                    
            string alpha = str.Substring(0, 1);
            int sym = 0;
            #region "字母轉數字"
            switch (alpha)
            {
                case "A": sym = 10; break;
                case "B": sym = 11; break;
                case "C": sym = 12; break;
                case "D": sym = 13; break;
                case "E": sym = 14; break;
                case "F": sym = 15; break;
                case "G": sym = 16; break;
                case "H": sym = 17; break;
                case "I": sym = 34; break;//
                case "J": sym = 18; break;
                case "K": sym = 19; break;
                case "L": sym = 20; break;
                case "M": sym = 21; break;
                case "N": sym = 22; break;
                case "O": sym = 35; break;//
                case "P": sym = 23; break;
                case "Q": sym = 24; break;
                case "R": sym = 25; break;
                case "S": sym = 26; break;
                case "T": sym = 27; break;
                case "U": sym = 28; break;
                case "V": sym = 29; break;
                case "W": sym = 32; break;//
                case "X": sym = 30; break;
                case "Y": sym = 31; break;
                case "Z": sym = 33; break; //
            }
            #endregion
            if (sym == 0)
            {
                return false;
            }
            result = (sym / 10) + (sym % 10) * 9;

            for (int i = 1; i <= 7; i++)
            {
                int x = int.Parse(str.Substring(i, 1));
                result += x * count;
                count--;
            }

            result = result + int.Parse(str.Substring(8, 1)) + int.Parse(str.Substring(9, 1));

            if (result % 10 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 驗證中文姓名
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        //public static bool IsChineseName(string str)
        //{
        //    if (string.IsNullOrEmpty(str))
        //    {
        //        return false;
        //    }

        //    Regex reg = new Regex(@"^[\u4e00-\u9fa5]{2,4}$");
        //    Match m = reg.Match(str);
        //    return m.Success;
        //}

        /// <summary>
        /// 驗證統編
        /// </summary>
        /// <param name="str">The string.</param>
        /// <returns></returns>
        public static bool IsCompanyNumber(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            Regex reg = new Regex(@"^[0-9]{8}$");
            Match m = reg.Match(str);

            return m.Success;
        }

        public static bool IsBirthday(DateTime date) {
            /*
             * SqlServer的datetime有效範圍是1753~9999，如果超出這個範圍，EF就會把datetime轉換為datetime2
             * 出現錯誤: 將 datetime2 資料類型轉換成 datetime 資料類型時，產生超出範圍的值。
             */
            return date.Year >= 1800;
        }
        /// <summary>
        /// url 驗証 //todo 確認可用
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public static bool IsUrl(string url)
        {
            return Regex.IsMatch(url, @"http(s)?://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?");
        }


        /// <summary>
        /// 路由保留字
        /// </summary>
        private static List<string> RouteReservedWords = new List<string> { "admins", "home", "index", "member", "search", "q", "api", "apihelp",
                                                                            "item", "menu", "get", "preview", "getpagelist", "partial" };

        /// <summary>
        /// Bad 字元
        /// </summary>
        private static List<string> BadCharacters = new List<string> { "~", "`", "#", "$", "%", "^", "&", "*", "<", "=", "+", "{", "}", "[", "]", @"\", "/", "|", ",", ".", "?", ":", ";", "@", "--", "\"" };
        private static List<string> BadCharacters2 = new List<string> { " ", "「", "」", "『", "』", "《", "》", "〈", "〉", "【", "】", "(", ")", "，", "！", "!", "？", "：", "…", "／", "、" };


        /// <summary>
        /// check路由字元正確(不含保留字)
        /// </summary>
        /// <param name="name">The name.</param>
        public static bool IsRouteWords(string str)
        {
            return !RouteReservedWords.Contains(str.ToLower())
                && !Setting.AdminRoutes.Contains(str, StringComparer.OrdinalIgnoreCase); //不分大小寫
        }

        /// <summary>
        /// check 無 bad字元
        /// </summary>
        public static bool NobadCharacters(string str)
        {
            return !BadCharacters.Any(s => str.Contains(s));
        }

        /// <summary>
        /// 取代所有bad字元
        /// </summary>
        /// <param name="str">The string.</param>
        public static string RelpaceBadCharacters(string str, string symbol = "")
        {
            foreach (var c in BadCharacters)
            {
                str = str.Replace(c, symbol);
            }

            foreach (var c in BadCharacters2)
            {
                str = str.Replace(c, symbol);
            }

            if (!string.IsNullOrEmpty(symbol))
            {
                str = str.Replace(symbol + symbol, symbol);
            }

            return str;
        }
    }
}
