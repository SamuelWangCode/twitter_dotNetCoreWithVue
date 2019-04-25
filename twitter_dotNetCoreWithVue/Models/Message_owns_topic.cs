using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Message_owns_topic
    {

        [Display(Name = "推特ID")]
        public int message_id { get; set; }

        [Display(Name = "话题ID")]
        public int topic_id { get; set; }
    }
}
