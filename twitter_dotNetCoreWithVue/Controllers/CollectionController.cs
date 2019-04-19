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
    /// 这个控制器定义用户收藏推特的相关api
    /// </summary>
    [Route("api/[controller]")]
    public class CollectionController : Controller
    {
        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpGet("add/{message_id}")]
        public IActionResult Add([Required]int message_id)
        {
            //TODO 需要验证登录态 添加收藏 EZ
            //返回是否成功
            return new JsonResult(new { });
        }

        /// <summary>
        /// 查询收藏的列表
        /// 需要Range作为参数限制
        /// </summary>
        /// <returns>包含所有收藏的推特id的Json数据</returns>
        /// <param name="range">Range.</param>
        [HttpGet("query")]
        public IActionResult Query([Required][FromBody]Range range)
        {
            //TODO 需要验证登录态
            //需要range作为参数
            //从数据库取出message_id们 加油
            return new JsonResult(new { });
        }
    }
}
