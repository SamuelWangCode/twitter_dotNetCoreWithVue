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
    /// 用于艾特用户相关信息的api
    /// </summary>
    [Route("api/[controller]")]
    public class AtController : Controller
    {
        /// <summary>
        /// 根据Range查找最近几条At自己的message_id列表
        /// </summary>
        /// <returns>包含 message_id组成的列表 的Json对象</returns>
        /// <param name="range">Range.</param>
        [HttpGet("query")]
        public IActionResult Query([Required][FromBody]Range range)
        {
            //TODO 需要身份验证
            //查找At自己的在range范围内的message_id
            //按照时间排序
            //返回包含这些id的Json对象
            return new JsonResult(new { });
        }

        
    }
}
