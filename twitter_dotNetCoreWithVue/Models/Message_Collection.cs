using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Message_Collection
    {
        [Display(Name = "收藏id")]
        public int collection_id { get; set; }

        [Display(Name = "收藏的用户id")]
        public int user_id { get; set; }

        [Display(Name = "收藏的推特id")]
        public int message_id { get; set; }
    }
}
