using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Message
    {
        [Display(Name ="推特ID")]
        public int message_id { get; set; }

        [Display(Name ="推特内容")]
        [Required]
        [StringLength(280)]
        public string message_content { get; set; }

        [Display(Name = "推特发布时间")]
        [Required]
        public string message_create_time { get; set; }

        [Display(Name = "点赞量")]
        public int message_agree_num { get; set; }

        [Display(Name = "转发量")]
        public int message_transpond_num { get; set; }

        [Display(Name = "评论量")]
        public int message_comment_num { get; set; }

        [Display(Name = "浏览量")]
        public int message_view_num { get; set; }

        [Display(Name = "推特是否带图")]
        [Required]
        public int message_has_image { get; set; }

        [Display(Name = "发布人ID")]
        public int message_sender_user_id { get; set; }

        [Display(Name = "推特热度")]
        public int message_heat { get; set; }

    }
}
