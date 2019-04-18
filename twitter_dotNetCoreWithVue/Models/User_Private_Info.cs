using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class User_Private_Info
    {
        [Display(Name = "用户id")]
        public int user_id { get; set; }

        [Display(Name = "性别")]
        public string user_gender { get; set; }

        [Display(Name = "真实姓名")]
        public string user_realname { get; set; }

        [Display(Name = "邮箱")]
        public string user_email { get; set; }

        [Display(Name = "密码")]
        public string user_password { get; set; }
    }
}
