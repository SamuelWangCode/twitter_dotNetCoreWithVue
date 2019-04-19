using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    /// <summary>
    /// 有关评论推特的api
    /// </summary>
    [Route("api/[controller]")]
    public class CommentController : Controller
    {
        //用于评论推特时的模型
        public class CommentForSender
        {
            [Display(Name = "评论内容")]
            [Required]
            [StringLength(280)]
            public int comment_content { set; get; }

        }

        /// <summary>
        /// 给某个推特添加评论时调用
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="message_id">Message identifier.</param>
        /// <param name="comment">Comment.</param>
        [HttpPost("add/{message_id}")]
        public IActionResult Add([Required]int message_id, [Required][FromBody]CommentForSender comment)
        {
            //TODO 需要身份验证
            //返回是否成功
            return new JsonResult(new { });
        }

    }
}
