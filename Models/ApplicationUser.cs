using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyFakexiecheng.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string Address { get; set; }
        // ShoppingCart
        public ShoppingCart ShoppingCart { get; set; }

        // Order
        //一定要与父类一致
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; }//用户角色，多对多关系11-9 02：50
        //public virtual ICollection<IdentityUserClaim<string>> Claims { get; set; }//与用户绑定的claim
        //public virtual ICollection<IdentityUserLogin<string>> Logins { get; set; }//三方用户登录
        //public virtual ICollection<IdentityUserToken<string>> Tokens { get; set; }//记录用户session，已经有jwt了，所以不需要

    }
}
