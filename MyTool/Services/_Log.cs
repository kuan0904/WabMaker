using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Services
{
    public class _Log
    {
        /// <summary>
        /// 寫入一行文字到 log 檔案
        /// </summary>
        /// <param name="message">The message.</param>
        public static void CreateText(string message)
        {
            string fileLogPath = AppDomain.CurrentDomain.BaseDirectory + "log";
            string fileName = DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            _File.MakeDir(fileLogPath);

            try
            {
                System.IO.File.AppendAllText(//auto create file and append text
                    fileLogPath + "\\" + fileName, //path
                    DateTime.Now.ToString("HH:mm:ss") + " " + message 
                    + System.Environment.NewLine + System.Environment.NewLine); //text
            }
            catch (Exception)
            {
            }

        }


    }
}
