using MyTool.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.ViewModels
{
    public class TreeViewModel
    {
        public Guid ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TreeType Type { get; set; }

        public string Controller { get; set; }
        public string Action { get; set; }
        public string Url { get; set; }

        public bool IsChecked { get; set; }
        public bool IsEnabled { get; set; }
        public int Sort { get; set; }

        //選單包含權限
        public string MenuActions { get; set; }
        //角色包含權限
        public string RoleActions { get; set; }

        public List<TreeViewModel> Nodes { get; set; }
        public List<LangNode> TreeLangs { get; set; }
    }

    //語系狀態
    public class LangNode
    {
        public LanguageType Lang { get; set; }
        public LanguageStatus Status { get; set; }
    }

    public enum TreeType
    {
        None,
        User,
        Department,
        Action,
        Menu,
        Item,
        Structure
    }

    public class CheckTreeModel
    {
        public List<TreeViewModel> tree { get; set; }      
        public ItemRelationType relationType { get; set; }
    }
}
