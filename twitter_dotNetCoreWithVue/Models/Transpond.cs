using System;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Transpond
    {
        [Display(Name = "转发者消息id")]
        [Required]
        public int message_id { get; set; }

        [Display(Name = "被转发消息id")]
        public int transponded_message_id { get; set; }
    }
}
