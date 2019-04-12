using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace MyTool.Tools
{
    public class UserTool : ClaimsPrincipal
    {
        public static string _SuperManager = "00000000-1111-2222-1111-000000000000";
        public ClaimsPrincipal principal;

        public UserTool(ClaimsPrincipal principal) : base(principal)
        {
            this.principal = principal;
        }

        // 取得使用者id
        public Guid Id => new Guid(FindFirst(ClaimTypes.NameIdentifier).Value);

        // 取得使用者帳號
        public string Name => FindFirst(ClaimTypes.Name).Value;

        // 取得使用者Menu
        public List<Guid> UserMenu
        {
            get
            {
                try
                {
                    return FindAll(ClaimTypes.UserData).Select(x => Guid.Parse(x.Value)).ToList();
                }
                catch (Exception)
                {
                    return null;
                }
            }

        }

        // 使用者權限
        public IEnumerable<string> UserRole => FindAll(ClaimTypes.Role).Select(x => x.Value);


        // 是否是超級管理員
        public bool IsSuperManager
        {
            get
            {
                try
                {
                    return FindFirst(ClaimTypes.NameIdentifier).Value == _SuperManager;
                }
                catch (Exception)
                {

                    return false;
                }
            }

        }


    }
}
