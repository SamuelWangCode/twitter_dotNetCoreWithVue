using System;
using System.ComponentModel.DataAnnotations;

namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    public class Range
    {
        [Required]
        [Display(Name = "从第几条开始")]
        public int startFrom { get; set; }

        [Required]
        [Display(Name = "长度限制")]
        public int limitation { get; set; }
    }
}
