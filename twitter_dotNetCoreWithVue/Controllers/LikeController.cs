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
    /// 点赞api
    /// </summary>
    [Route("api/[controller]")]
    public class LikeController : Controller
    {
        /// <summary>
        /// 为某个推特点赞
        /// 客户端不要等信息返回再更新页面状态。点下去就更新，看起来会效果好一点
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpGet("{message_id}")]
        public IActionResult Like([Required]int message_id)
        {
            //TODO 给某个推特点赞
            // 需要身份验证
            // 返回成功与否
            return new JsonResult(new { });
        }

        /// <summary>
        /// 取消点赞
        /// 建议同上
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpGet("cancel/{message_id}")]
        public IActionResult CancelLike([Required]int message_id)
        {
            //TODO 给某个推特取消点赞
            // 需要身份验证
            // 返回成功与否
            return new JsonResult(new { });
        }

       
    }
}
