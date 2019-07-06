using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Comment
    {
        [Display(Name = "评论id")]
        public int comment_id { set; get; }

        [Display(Name = "评论内容")]
        [Required]
        [StringLength(280)]
        public string comment_content { set; get; }

        [Display(Name = "评论是否被发送者阅读")]
        public int comment_is_read { set; get; }

        [Display(Name = "发送者id")]
        public int comment_sender_id { set; get; }

        [Display(Name = "评论的推特id")]
        public int comment_message_id { set; get; }

        [Display(Name = "评论时间")]
        [Required]
        public string comment_create_time { set; get; }

    }
}
