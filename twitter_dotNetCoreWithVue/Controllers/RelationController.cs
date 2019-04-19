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
    /// 此控制器定义用户之间互相关注的相关api
    /// </summary>
    [Route("api/[controller]")]
    public class RelationController : Controller
    {
        /// <summary>
        /// 关注某人时使用
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="user_id">User identifier.</param>
        [HttpGet("follow/{user_id}")]
        public IActionResult FollowUser([Required]int user_id)
        {
            //TODO getAuthentication and do the CURD
            return Ok();
        }

        /// <summary>
        /// 查询某人的关注列表
        /// 需要user_id和range作为参数
        /// </summary>
        /// <returns>Json</returns>
        /// <param name="user_id">User identifier.</param>
        /// <param name="range">Range</param>
        [HttpGet("queryFollowingFor/{user_id}")]
        public IActionResult QueryFollowingFor([Required]int user_id, [Required][FromBody]Range range)
        {
            //TODO get Authentication and do the CURD
            //注意需要按时间排序
            //使用range作为限制参数
            //返回Json对象
            return new JsonResult(new { });
        }

        /// <summary>
        /// 查询关注自己的用户列表
        /// 需要range作为参数
        /// </summary>
        /// <returns>Json</returns>
        /// <param name="range">Range</param>
        [HttpPost("queryFollowersForMe")]
        public IActionResult QueryFollowersForMe([Required][FromBody]Range range)
        {
            //TODO 查找关注我的人的列表
            //该函数逻辑和上面的相同，只是查找的对象不同
            //可以简化
            //注意需要按时间排序
            //使用range作为限制参数
            //返回Json对象
            return new JsonResult(new { });
        }

        /// <summary>
        /// 取关
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="user_id">User identifier.</param>
        [HttpGet("cancelFollowingTo/{user_id}")]
        public IActionResult CancelFollowingTo([Required]int user_id)
        {
            //TODO 取关
            //需要验证登录态
            return new JsonResult(new { });
        }
    }
}
