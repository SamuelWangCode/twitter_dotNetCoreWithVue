using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using twitter_dotNetCoreWithVue.Controllers.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    /// <summary>
    /// 定义用于有关话题Topic的api
    /// </summary>
    [Route("api/[controller]")]
    public class TopicController : Controller
    {
        /// <summary>
        /// 查找包含topic_id的最近range范围内的message_id组成的列表
        /// </summary>
        /// <returns>返回包含message_id列表的Json对象</returns>
        /// <param name="topic_id">Topic identifier.</param>
        /// <param name="range">Range.</param>
        [HttpGet("queryMessageIdsContains/{topic_id}")]
        public IActionResult QueryMessageIdsContains([Required]int topic_id, [Required][FromBody]Range range)
        {
            //TODO 无需登录态验证
            //根据range来查找时间最近的几条message_id组成的列表
            //返回Json对象
            return new JsonResult(new { });
        }


        /// <summary>
        /// 查找最热的几条话题
        /// 根据range
        /// </summary>
        /// <returns>Json 包含Topics</returns>
        /// <param name="range">Range.</param>
        [HttpGet("queryTopicsBaseOnHeat")]
        public IActionResult QueryTopicsBaseOnHeat([Required][FromBody]Range range)
        {
            //TODO 无需登录态验证
            //可以直接使用Likes模型
            return new JsonResult(new { });
        }
    }

}
