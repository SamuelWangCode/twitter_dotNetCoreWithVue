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
    /// 该控制器定义有关私信的api
    /// </summary>
    [Route("api/[controller]")]
    public class PrivateLetterController : Controller
    {
        public class SendingPrivateLetter
        { 

            [Display(Name = "私信内容")]
            [Required]
            public string private_letter_content { get; set; }
        }


        /// <summary>
        /// 查询发送给自己的私信列表
        /// 需要长度的参数
        /// </summary>
        /// <returns>私信列表</returns>
        [HttpPost("queryForMe")]
        public IActionResult QueryForMe([Required]Range range)
        {
            //TODO 需要验证登录态
            //使用range限制获得信息的长度
            //注意 是按时间排序
            //返回含有列表的Json对象
            //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            //注意 !!! 在查询的过程中同时需要修改
            //因为查询即代表阅读过这条私信，我们需要把private_letter_is_read设置为true
            return new JsonResult(new { });
        }

        /// <summary>
        /// 给某人发送私信
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="user_id">接收私信用户的id</param>
        /// <param name="letterInfo">私信内容</param>
        [HttpPost("send/{user_id}")]
        public IActionResult Send([Required]int user_id, [Required][FromBody]SendingPrivateLetter letterInfo)
        {
            //TODO 需要验证登录态
            //返回成功与否
            return new JsonResult(new { });
        }


        //就不弄删除私信了，没啥用
        
    }
}
