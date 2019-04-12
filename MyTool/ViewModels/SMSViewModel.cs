using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ViewModels
{
    public class SMSViewModel
    {      
        public string Index { get; set; }

        public string MsgId { get; set; }

        public string StatusCode { get; set; }

        public SmsResultType ResultType { get; set; }
    }
}
