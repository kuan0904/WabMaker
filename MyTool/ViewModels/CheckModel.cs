using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ViewModels
{
    public class CheckBoxListItem
    {
        public Guid ID { get; set; }
        public string Text { get; set; }
        public bool IsChecked { get; set; }
    }

    public class KeyValueModel
    {
        public Guid Key { get; set; }
        public int Value { get; set; }
    }
}
