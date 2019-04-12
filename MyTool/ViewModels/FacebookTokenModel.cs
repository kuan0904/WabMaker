using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ViewModels
{
    public class FacebookTokenModel
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class FacebookUserModel
    {
        public string id { get; set; }
        public string name { get; set; }
        public string email { get; set; }
    }
}
