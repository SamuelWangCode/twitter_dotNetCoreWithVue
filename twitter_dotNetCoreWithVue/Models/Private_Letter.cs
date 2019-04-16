using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Private_Letter
    {
        [Display(Name = "私信id")]
        [Required]
        public int private_letter_id { get; set; }

        [Display(Name = "私信内容")]
        [Required]
        public string private_letter_content { get; set; }

        [Display(Name = "私信是否被阅读")]
        [Required]
        public int private_letter_is_read { get; set; }

        [Display(Name = "私信创建时间")]
        public string private_letter_create_time { get; set; }

        [Display(Name = "发送者id")]
        public int private_letter_sender_id { get; set; }

        [Display(Name = "接受者id")]
        public int private_letter_reveiver_id { get; set; }

    }
}
