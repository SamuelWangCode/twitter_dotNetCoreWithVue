using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Oracle.ManagedDataAccess.Client;
using twitter_dotNetCoreWithVue.Controllers.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    /// <summary>
    /// 定义有关推特信息的api
    /// 包括发送，转发，删除，查询
    /// 以及点赞，评论。
    /// 以及根据话题Topic来查询
    /// </summary>
    [Route("api/[controller]")]
    public class MessageController : Controller
    {
        //用于展示推特时的模型
        public class MessageForShow
        {
            [Display(Name = "推特ID")]
            public int message_id { get; set; }

            [Display(Name = "推特内容")]
            [Required]
            [StringLength(280)]
            public string message_content { get; set; }

            [Display(Name = "推特发布时间")]
            [Required]
            public string message_create_time { get; set; }

            [Display(Name = "点赞量")]
            public int message_agree_num { get; set; }

            [Display(Name = "转发量")]
            public int message_transpond_num { get; set; }

            [Display(Name = "评论量")]
            public int message_comment_num { get; set; }

            [Display(Name = "浏览量")]
            public int message_view_num { get; set; }

            [Display(Name = "推特是否带图")]
            [Required]
            public int message_has_image { get; set; }

            [Display(Name = "推特是否为转发")]
            [Required]
            public bool message_is_transpond { get; set; }

            [Display(Name = "发布人ID")]
            public int message_sender_user_id { get; set; }

            [Display(Name = "推特热度")]
            public int message_heat { get; set; }

            [Display(Name = "转发来源推特ID")]
            public int message_transpond_message_id { get; set; }

            [Display(Name = "图片url列表")]
            public string[] message_image_urls { get; set; }

        }
        //用于发送推特时的模型
        public class MessageForSender
        {

            [Display(Name = "推特内容")]
            [Required]
            [StringLength(280)]
            public string message_content { get; set; }

            [Display(Name = "推特是否带图")]
            [Required]
            public int message_has_image { get; set; }

            [Display(Name = "发布人ID")]
            public int message_sender_user_id { get; set; }

        }
        //用于转发推特时的模型
        public class MessageForTransponder
        {
            [Display(Name = "推特内容")]
            [Required]
            [StringLength(280)]
            public string message_content { get; set; }

            [Display(Name = "推特是否为转发")]
            [Required]
            public bool message_is_transpond { get; set; }

            [Display(Name = "发布人ID")]
            public int message_sender_user_id { get; set; }

            [Display(Name = "转发来源推特ID")]
            public int message_transpond_message_id { get; set; }

        }


        /// <summary>
        /// 查看推特详情时调用的api
        /// 前端需要根据是否转发来改变视图的具体类型
        /// 为减轻前端工作量，我们把推特含有的图片列表同时传送过去
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpGet("query/{message_id}")]
        public IActionResult Query([Required]int message_id)
        {
            //TODO 获得推特的详细信息
            //无需验证登录态
            //除了基本的推特信息以外，我们需要根据这条推特是否含有图，来把MessageForShow的图片url列表填好
            return new JsonResult(new MessageForShow());
        }

        /// <summary>
        /// 此api用于首页，需要查找所有的显示在首页的推特时调用
        /// 根据range来返回前几条推荐的信息
        /// 实际上我们返回的是自己的以及关注者的推特
        /// 按照时间排序
        /// </summary>
        /// <returns>The messages for index.</returns>
        /// <param name="range">Range.</param>
        [HttpGet("queryForIndex")]
        public IActionResult QueryForIndex([Required][FromBody]Range range)
        {
            //TODO 根据range来吧
            //这个稍微有些复杂，SQL会比较难写，加油。
            MessageForShow[] infos = new[] { new MessageForShow() };
            return new JsonResult(infos);
        }

        /// <summary>
        /// 调用api发送推特
        /// 若推特还含有图片，还需要另外调用图片上传的接口
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="message">Message.</param>
        [HttpPost("send")]
        public IActionResult Send([Required][FromBody]MessageForSender message)
        {
            //TODO 需要验证身份
            //有很多参数都是有初始化的
            //!!!!!与Topic的联动
            //首先要检查message中是否有两个#号括起来的连续无空格字符串
            //若有，则去数据库中检索该Topic是否存在，若不存在则添加，若存在则将其热度提高
            return new JsonResult(new { });
        }

        /// <summary>
        /// 转发消息时调用的api
        /// </summary>
        /// <returns>成功与否</returns>
        /// <param name="message_id">Message identifier.</param>
        /// <param name="message">Message.</param>
        [HttpPost("transpond/{message_id}")]
        public IActionResult Transpond([Required]int message_id, [Required][FromBody]MessageForTransponder message)
        {
            //TODO 需要验证身份
            //返回是否转发成功
            //同样存在与Topic的联动
            return new JsonResult(new { });
        }

        /// <summary>
        /// 上传图片的接口
        /// 暂时不清楚前端是通过怎样的方式来上传的
        /// 看网上说有用IFormFile
        /// 也有说用Requet.Form.Files来获取的
        /// 具体再议
        /// </summary>
        /// <returns>success or not</returns>
        [HttpPost("uploadImgs")]
        public IActionResult UploadImgs()
        {
            //TODO 需要验证登录态
            //返回成功与否
            return new JsonResult(new { });
        }


        /// <summary>
        /// 删除推特时使用
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpGet("delete/{message_id}")]
        public IActionResult Delete([Required]int message_id)
        {
            //TODO 需要验证登录态
            //返回成功与否
            return new JsonResult(new { });
        }

    }
}
