using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class User_Public_Info
    {
        [Display(Name = "用户id")]
        public int user_id { get; set; }

        [Display(Name ="用户昵称")]
        [Required]
        public string user_nickname { get; set; }

        [Display(Name = "用户注册时间")]
        [Required]
        public string user_register_time { get; set; }

        
        [Display(Name = "头像表id")]
        [Required]
        public int user_avatar_image_id { get; set; }

        [Display(Name = "用户自我介绍")]
        public string user_self_introduction { get; set; }

        [Display(Name = "关注者数量")]
        public int user_followers_num { get; set; }

        [Display(Name = "关注别人的数量")]
        public int user_follows_num { get; set; }
    }
}
