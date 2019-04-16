using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Likes
    {
        [Display(Name = "点赞ID")]
        public int likes_id { get; set; }

        [Display(Name = "点赞用户ID")]
        public int likes_user_id { get; set; }

        [Display(Name = "被点赞推特的ID")]
        public int likes_message_id { get; set; }

        [Display(Name = "点赞时间")]
        [Required]
        public string likes_time { get; set; }
    }
}
