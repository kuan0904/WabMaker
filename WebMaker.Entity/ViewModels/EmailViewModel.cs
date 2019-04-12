using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebMaker.Entity.Models;

namespace WebMaker.Entity.ViewModels
{
    public class EmailViewModel
    {
        public cms_Email Email { get; set; }

        public List<cms_EmailSendUser> SendUsers { get; set; }
    }
}
