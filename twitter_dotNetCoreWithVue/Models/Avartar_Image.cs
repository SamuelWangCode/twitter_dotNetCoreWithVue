using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace twitter_dotNetCoreWithVue.Models
{
    public class Avartar_Image
    {
        //直接根据id获取头像图片
        [Display(Name = "头像图片id")]
        public int avatar_image_id { get; set; }

        [Display(Name = "头像是否正在被使用")]
        public bool avatar_image_in_use { get; set; }
    }
}
