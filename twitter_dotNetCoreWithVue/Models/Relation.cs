using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Relation
    {
        [Display(Name = "关注id")]
        public int relation_id { set; get; }

        [Display(Name = "关注创建时间")]
        [Required]
        public string relation_create_time { set; get; }

        [Display(Name = "关注者id")]
        public int relation_user_follower_id { set; get; }

        [Display(Name = "被关注者id")]
        public int relation_user_be_followed_id { set; get; }

    }
}
