using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class At_User
    {
        [Display(Name = "艾特id")]
        public int at_user_id { set; get; }

        [Display(Name = "艾特属于的推特id")]
        public int message_id { set; get; }

        [Display(Name = "艾特的用户id")]
        public int user_id { set; get; }

        [Display(Name = "艾特的时间")]
        [Required]
        public string at_time { set; get; }
    }
}
