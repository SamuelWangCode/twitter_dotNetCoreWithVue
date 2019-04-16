using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Models
{
    public class Topic
    {
        [Display(Name ="话题ID")]
        public int topic_id { get; set; }

        [Display(Name ="话题热度")]
        public float topic_heat { get; set; }

        [Display(Name = "话题内容")]
        [Required]
        [StringLength(50)]
        public string topic_content { get; set; }

    }
}
