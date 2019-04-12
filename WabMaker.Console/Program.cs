using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WabMaker.Console
{
    using MyTool.Services;
    using System.Configuration;
    using WebMaker.BLL.Services;
    using Console = System.Console;

    class Program
    {
        protected static bool Enable_QueryPayMessage = Convert.ToBoolean(ConfigurationManager.AppSettings["Enable_QueryPayMessage"]);
        protected static bool Enable_SendEmail = Convert.ToBoolean(ConfigurationManager.AppSettings["Enable_SendEmail"]);

        static void Main(string[] args)
        {
            //國泰對帳
            if (Enable_QueryPayMessage)
            {
                var orderService = new OrderService();
                Console.WriteLine("QueryPayMessage");
                orderService.QueryPayMessage();
            }

            //Email檢查
            if (Enable_SendEmail)
            {
                var emailBoxServie = new EmailLogService();
                Console.WriteLine("SendEmail");
                emailBoxServie.SendEmailDelay();
            }

            Console.WriteLine("--------Done--------");
            Console.ReadKey();
        }

        public static void ShowMessage(string msg)
        {
            Console.WriteLine(msg);
            _Log.CreateText(msg);
        }

    }
}
