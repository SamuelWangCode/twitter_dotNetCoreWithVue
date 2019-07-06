using System;
using System.Data;
using System.IO;
using System.Data.Common;
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
            public int message_is_transpond { get; set; }

            [Display(Name = "发布人ID")]
            public int message_sender_user_id { get; set; }

            [Display(Name = "推特热度")]
            public int message_heat { get; set; }

            [Display(Name = "转发来源推特ID")]
            public int message_transpond_message_id { get; set; }

            [Display(Name = "推特含图数量")]
            public int message_image_count { get; set; }

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

            [Display(Name = "推特含图数量")]
            public int message_image_count { get; set; }

        }
        //用于转发推特时的模型
        public class MessageForTransponder
        {
            [Display(Name = "推特内容")]
            [Required]
            [StringLength(280)]
            public string message_content { get; set; }

            [Display(Name = "来源推特是否亦为转发")]
            [Required]
            public bool message_source_is_transpond { get; set; }

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
            //获得推特的详细信息
            //无需验证登录态
            //除了基本的推特信息以外，我们需要根据这条推特是否含有图，来把MessageForShow的图片url列表填好

            return Wrapper.wrap((OracleConnection conn) =>
            {
                //function FUNC_SHOW_MESSAGE_BY_ID(message_id in INTEGER, result out sys_refcursor)
                //return INTEGER
                string procedurename = "FUNC_SHOW_MESSAGE_BY_ID";
                OracleCommand cmd = new OracleCommand(procedurename, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                //Add first parameter message_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = message_id;

                //Add second parameter search_result
                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("result", OracleDbType.RefCursor);
                p3.Direction = ParameterDirection.Output;

                //Get the result table
                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }
                MessageForShow infos = new MessageForShow();
                infos.message_id = int.Parse(dt.Rows[0][0].ToString());
                infos.message_content = dt.Rows[0][1].ToString();
                infos.message_create_time = dt.Rows[0][2].ToString();
                infos.message_agree_num = int.Parse(dt.Rows[0][3].ToString());
                infos.message_transpond_num = int.Parse(dt.Rows[0][4].ToString());
                infos.message_comment_num = int.Parse(dt.Rows[0][5].ToString());
                infos.message_view_num = int.Parse(dt.Rows[0][6].ToString());
                infos.message_has_image = int.Parse(dt.Rows[0][7].ToString());
                infos.message_is_transpond = int.Parse(dt.Rows[0][8].ToString());
                infos.message_sender_user_id = int.Parse(dt.Rows[0][9].ToString());
                infos.message_heat = int.Parse(dt.Rows[0][10].ToString());
                infos.message_transpond_message_id = int.Parse(dt.Rows[0][11].ToString());
                infos.message_image_count = int.Parse(dt.Rows[0][12].ToString());

                if(infos.message_has_image==1)
                {
                    for(int i=0;i<infos.message_image_count;i++)
                    {
                      infos.message_image_urls.Append<string>("/Messages/" + infos.message_id.ToString() + "/" + i.ToString());                       
                    }
                }

                RestfulResult.RestfulData<MessageForShow> rr = new RestfulResult.RestfulData<MessageForShow>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = infos;
                return new JsonResult(rr);

            });
            
                    
        }

        /// <summary>
        /// 此api用于首页，需要查找所有的显示在首页的推特时调用
        /// 根据range来返回前几条推荐的信息
        /// 实际上我们返回的是自己的以及关注者的推特
        /// 按照时间排序
        /// </summary>
        /// <returns>The messages for index.</returns>
        /// <param name="range">Range.</param>
        [HttpPost("queryForIndex")]
        public IActionResult QueryForIndex([Required][FromBody]Range range,[Required]string user_id)
        {
            //根据range来吧
            //这个稍微有些复杂，SQL会比较难写，加油。

            return Wrapper.wrap((OracleConnection conn) =>
            {
                //function FUNC_SHOW_HOME_MESSAGE_BY_RANGE(user_id in INTEGER, rangeStart in INTEGER, rangeLimitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procedurename = "FUNC_SHOW_MESSAGE_BY_RANGE";
                OracleCommand cmd = new OracleCommand(procedurename, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                //Add first parameter user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = int.Parse(user_id);

                //Add second parameter rangeStart
                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("rangeStart", OracleDbType.Int32);
                p3.Direction = ParameterDirection.Input;
                p3.Value = range.startFrom;

                //Add third parameter rangeLimitation
                OracleParameter p4 = new OracleParameter();
                p4 = cmd.Parameters.Add("rangeLimitation", OracleDbType.Int32);
                p4.Direction = ParameterDirection.Input;
                p4.Value = range.limitation;

                //Add fourth parameter search_result
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                //Get the result table
                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }
                MessageForShow[] infos = new MessageForShow[dt.Rows.Count];

                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    infos[i] = new MessageForShow();
                    infos[i].message_id = int.Parse(dt.Rows[i][0].ToString());
                    infos[i].message_content = dt.Rows[i][1].ToString();
                    infos[i].message_create_time = dt.Rows[i][2].ToString();
                    infos[i].message_agree_num = int.Parse(dt.Rows[i][3].ToString());
                    infos[i].message_transpond_num = int.Parse(dt.Rows[i][4].ToString());
                    infos[i].message_comment_num = int.Parse(dt.Rows[i][5].ToString());
                    infos[i].message_view_num = int.Parse(dt.Rows[i][6].ToString());
                    infos[i].message_has_image = int.Parse(dt.Rows[i][7].ToString());
                    infos[i].message_is_transpond = int.Parse(dt.Rows[i][8].ToString());
                    infos[i].message_sender_user_id = int.Parse(dt.Rows[i][9].ToString());
                    infos[i].message_heat = int.Parse(dt.Rows[i][10].ToString());
                    infos[i].message_transpond_message_id = int.Parse(dt.Rows[i][11].ToString());
                    infos[i].message_image_count = int.Parse(dt.Rows[0][12].ToString());
                }
                for(int i=0;i<dt.Rows.Count;i++)
                {
                    if (infos[i].message_has_image == 1)
                    {
                        string path = @"wwwroot\Messages\" + infos[i].message_id.ToString();
                        for (int j = 0; ; j++)
                        {
                            if (System.IO.File.Exists(path + @"\" + j.ToString()))
                            {
                                infos[i].message_image_urls.Append<string>("/Messages/" + infos[i].message_id.ToString() + "/" + j.ToString());
                            }
                            else break;
                        }
                        
                    }
                }

                RestfulResult.RestfulArray<MessageForShow> rr = new RestfulResult.RestfulArray<MessageForShow>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = infos;
                return new JsonResult(rr);
            });
            
        }

        /// <summary>
        /// 调用api发送推特
        /// 若推特还含有图片，还需要另外调用图片上传的接口
        /// </summary>
        /// <returns>The message.</returns>
        /// <param name="message">Message.</param>
        [HttpPost("send")]
        public async Task<IActionResult> Send([Required][FromBody]MessageForSender message)
        {
            //TODO 需要验证身份
            //有很多参数都是有初始化的
            //!!!!!与Topic的联动
            //首先要检查message中是否有两个#号括起来的连续无空格字符串
            //若有，则去数据库中检索该Topic是否存在，若不存在则添加，若存在则将其热度提高

            int userId;
            List<string> topics = new List<string>();
            int index1 = message.message_content.IndexOf('#');
            int index2;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userId = int.Parse(HttpContext.User.Claims.First().Value);

                //检查message_content里含有的话题，用两个#包含的内容作为话题。若出现两个连续的#，则忽略之。
                //所有的话题内容会被保存到topics列表内，并在调用第二个函数FUNC_ADD_TOPIC时，逐一对topic的内容进行处理（不存在则创建，存在则热度+1）
                while(index1!=-1)
                {
                    index2 = message.message_content.Substring(index1 + 1).IndexOf('#');
                    if (index2 == -1) break;
                    if (index2 - index1 == 1) continue;
                    topics.Append(message.message_content.Substring(index1 + 1, index2 - index1 - 1));
                    index1 = message.message_content.Substring(index2 + 1).IndexOf('#');
                }
            }
            else
            {
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }

            return Wrapper.wrap(async(OracleConnection conn) =>
            {
                //FUNC_SEND_MESSAGE(message_content in VARCHAR2, message_has_image in INTEGER, user_id in INTEGER, message_image_count in INTEGER, message_id out INTEGER)
                //return INTEGER
                string procedureName = "FUNC_SEND_MESSAGE";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                //Add first parameter message_content
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("message_content", OracleDbType.Varchar2);
                p2.Direction = ParameterDirection.Input;
                p2.Value = message.message_content;

                //Add second parameter message_has_image
                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("message_content", OracleDbType.Int32);
                p3.Direction = ParameterDirection.Input;
                p3.Value = message.message_has_image;

                //Add third parameter user_id
                OracleParameter p4 = new OracleParameter();
                p4 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p4.Direction = ParameterDirection.Input;
                p4.Value = userId;

                //Add fourth parameter message_image_count
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("message_image_count", OracleDbType.Int32);
                p5.Direction = ParameterDirection.Input;
                p5.Value = message.message_image_count;

                //Add fifth parameter message_id
                OracleParameter p6 = new OracleParameter();
                p6 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                p6.Direction = ParameterDirection.Output;

                cmd.ExecuteReader();
                if(int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }

                if(topics.Count!=0)
                {
                    //对于topics列表里的每一个话题，分别作为函数参数来执行一次FUNC_ADD_TOPIC函数
                    //FUNC_ADD_TOPIC(topic_content in VARCHAR2, message_id in INTEGER)
                    //return INTEGER
                    string procedureName2 = "FUNC_ADD_TOPIC";
                    OracleCommand cmd2 = new OracleCommand(procedureName2, conn);
                    cmd2.CommandType = CommandType.StoredProcedure;

                    //Add return value
                    OracleParameter p7 = new OracleParameter();
                    p7 = cmd2.Parameters.Add("state", OracleDbType.Int32);
                    p7.Direction = ParameterDirection.ReturnValue;

                    //Add first parameter topic_content
                    OracleParameter p8 = new OracleParameter();
                    p8 = cmd2.Parameters.Add("topic_content", OracleDbType.Varchar2);
                    p8.Direction = ParameterDirection.Input;
                    p8.Value = topics[0];

                    //Add second parameter message_id
                    OracleParameter p9 = new OracleParameter();
                    p9 = cmd2.Parameters.Add("message_id", OracleDbType.Int32);
                    p9.Direction = ParameterDirection.Input;
                    p9.Value = p6.Value;

                    cmd2.ExecuteReader();
                    for(int i = 1; i < topics.Count; i++)
                    {
                        if (int.Parse(p7.Value.ToString()) != 1)
                        {
                            throw new Exception("failed");
                        }
                        p8.Value = topics[i];
                        cmd2.ExecuteReader();
                    }
                    if (int.Parse(p7.Value.ToString()) != 1)
                    {
                        throw new Exception("failed");
                    }
                }

                //TODO 若推特含图，从POST体内获得图的内容并保存到服务器
                var images = Request.Form.Files;
                int img_num = 0;
                Directory.CreateDirectory(@"wwwroot\Messages\" + p6.Value.ToString());
                foreach(var imgfile in images)
                {
                    if(imgfile.Length>0)
                    {
                        var img_path = @"wwwroot\Messages\" + p6.Value.ToString() + @"\" + img_num.ToString();
                        using (var stream = new FileStream(img_path, FileMode.Create))
                        {
                            await imgfile.CopyToAsync(stream);
                        }
                        img_num++;
                    }
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

        /// <summary>
        /// 转发消息时调用的api
        /// </summary>
        /// <returns>成功与否</returns>
        /// <param name="message_id">Message identifier.</param>
        /// <param name="message">Message.</param>
        [HttpPost("transpond/{message_id}")]
        public IActionResult Transpond([Required][FromBody]MessageForTransponder message)
        {
            //需要验证身份
            //返回是否转发成功
            //同样存在与Topic的联动
            int userId;
            List<string> topics = new List<string>();
            int index1 = message.message_content.IndexOf('#');
            int index2;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userId = int.Parse(HttpContext.User.Claims.First().Value);

                //检查message_content里含有的话题，用两个#包含的内容作为话题。若出现两个连续的#，则忽略之。
                //所有的话题内容会被保存到topics列表内，并在调用第二个函数FUNC_ADD_TOPIC时，逐一对topic的内容进行处理（不存在则创建，存在则热度+1）
                while (index1 != -1)
                {
                    index2 = message.message_content.Substring(index1 + 1).IndexOf('#');
                    if (index2 == -1) break;
                    if (index2 - index1 == 1) continue;
                    topics.Append(message.message_content.Substring(index1 + 1, index2 - index1 - 1));
                    index1 = message.message_content.Substring(index2 + 1).IndexOf('#');
                }
            }
            else
            {
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }

            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_TRANSPOND_MESSAGE(message_content in VARCHAR2, message_source_is_transpond in INTEGER, message_sender_user_id in INTEGER, message_transpond_message_id in INTEGER, message_id out INTEGER)
                //return INTEGER
                string procedureName = "FUNC_TRANSPOND_MESSAGE";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                //Add first parameter message_content
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("message_content", OracleDbType.Varchar2);
                p2.Direction = ParameterDirection.Input;
                p2.Value = message.message_content;

                //Add second parameter message_is_transpond
                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("message_is_transpond", OracleDbType.Int32);
                p3.Direction = ParameterDirection.Input;
                p3.Value = message.message_source_is_transpond;

                //Add third parameter message_sender_user_id
                OracleParameter p4 = new OracleParameter();
                p4 = cmd.Parameters.Add("message_sender_user_id", OracleDbType.Int32);
                p4.Direction = ParameterDirection.Input;
                p4.Value = message.message_sender_user_id;

                //Add fourth parameter message_transpond_message_id
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("message_transpond_message_id", OracleDbType.Int32);
                p5.Direction = ParameterDirection.Input;
                p5.Value = message.message_transpond_message_id;

                //Add fifth parameter message_id
                OracleParameter p6 = new OracleParameter();
                p6 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                p6.Direction = ParameterDirection.Output;

                cmd.ExecuteReader();
                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }

                if (topics.Count != 0)
                {
                    //对于topics列表里的每一个话题，分别作为函数参数来执行一次FUNC_ADD_TOPIC函数
                    //FUNC_ADD_TOPIC(topic_content in VARCHAR2, message_id in INTEGER)
                    //return INTEGER
                    string procedureName2 = "FUNC_ADD_TOPIC";
                    OracleCommand cmd2 = new OracleCommand(procedureName2, conn);
                    cmd2.CommandType = CommandType.StoredProcedure;

                    //Add return value
                    OracleParameter p7 = new OracleParameter();
                    p7 = cmd2.Parameters.Add("state", OracleDbType.Int32);
                    p7.Direction = ParameterDirection.ReturnValue;

                    //Add first parameter topic_content
                    OracleParameter p8 = new OracleParameter();
                    p8 = cmd2.Parameters.Add("topic_content", OracleDbType.Varchar2);
                    p8.Direction = ParameterDirection.Input;
                    p8.Value = topics[0];

                    //Add second parameter message_id
                    OracleParameter p9 = new OracleParameter();
                    p9 = cmd2.Parameters.Add("message_id", OracleDbType.Int32);
                    p9.Direction = ParameterDirection.Input;
                    p9.Value = p6.Value;

                    cmd2.ExecuteReader();
                    for (int i = 1; i < topics.Count; i++)
                    {
                        if (int.Parse(p7.Value.ToString()) != 1)
                        {
                            throw new Exception("failed");
                        }
                        p8.Value = topics[i];
                        cmd2.ExecuteReader();
                    }
                    if (int.Parse(p7.Value.ToString()) != 1)
                    {
                        throw new Exception("failed");
                    }
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

        /// <summary>
        /// 上传图片的接口
        /// 暂时不清楚前端是通过怎样的方式来上传的
        /// 看网上说有用IFormFile
        /// 也有说用Requet.Form.Files来获取的
        /// 具体再议
        /// </summary>
        /// <returns>success or not</returns>

        private IActionResult UploadImgs()
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
            //需要验证登录态
            //返回成功与否

            int userId;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                userId = int.Parse(HttpContext.User.Claims.First().Value);
            }
            else
            {
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }

            return Wrapper.wrap((OracleConnection conn) =>
            {
                //function FUNC_DELETE_MESSAGE(message_id in INTEGER, message_has_image out INTEGER)
                //return INTEGER
                string procedurename = "FUNC_DELETE_MESSAGE";
                OracleCommand cmd = new OracleCommand(procedurename, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                //Add first parameter message_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = message_id;

                //Add second parameter search_result
                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("message_has_image", OracleDbType.RefCursor);
                p3.Direction = ParameterDirection.Output;

                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }                

                //根据返回内容，表示推特是否有图片。如果推特有图片，则把这条推特ID所对应的图片下的文件夹删掉
                if (int.Parse(p3.Value.ToString()) == 1)
                {
                    string path = @"wwwroot\Messages\" + message_id.ToString();
                    if(Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);

            });
        }

    }
}
