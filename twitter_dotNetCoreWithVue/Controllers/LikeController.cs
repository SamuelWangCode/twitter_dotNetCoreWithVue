using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using twitter_dotNetCoreWithVue.Controllers.Utils;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    public class Message_Id
    {
        [Required]
        public int[] like_message_id { get; set; }
    }
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
        public async Task<IActionResult> Like([Required]int message_id)
        {
            //TODO 给某个推特点赞
            // 需要身份验证
            // 返回成功与否
            int my_user_id = -1;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                my_user_id = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            }
            else
            {
                //进入到这部分意味着用户登录态已经失效，需要返回给客户端信息，即需要登录。
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }

            return await Wrapper.wrap(async (OracleConnection conn) => {
                //FUNC_ADD_LIKE(user_id in INTEGER, like_message_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_ADD_LIKE";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add input parameter message_id
                p3 = cmd.Parameters.Add("like_message_id", OracleDbType.Int32);
                p3.Value = message_id;
                p3.Direction = ParameterDirection.Input;

                await cmd.ExecuteReaderAsync();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

        /// <summary>
        /// 取消点赞
        /// 建议同上
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpGet("cancel/{message_id}")]
        public async Task<IActionResult> CancelLike([Required]int message_id)
        {
            //TODO 给某个推特取消点赞
            // 需要身份验证
            // 返回成功与否
            int my_user_id = -1;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                my_user_id = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            }
            else
            {
                //进入到这部分意味着用户登录态已经失效，需要返回给客户端信息，即需要登录。
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }

            return await Wrapper.wrap(async (OracleConnection conn) => {
                //FUNC_DELETE_LIKE(user_id in INTEGER, like_message_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_DELETE_LIKE";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add input parameter message_id
                p3 = cmd.Parameters.Add("like_message_id", OracleDbType.Int32);
                p3.Value = message_id;
                p3.Direction = ParameterDirection.Input;

                await cmd.ExecuteReaderAsync();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="user_id"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        [HttpGet("query/{user_id}")]
        public async Task<IActionResult> QueryUserLikes([Required]int user_id, [Required]Range range)
        { 

            return await Wrapper.wrap(async (OracleConnection conn) =>
            {
                //FUNC_QUERY_MESSAGE_IDS_LIKES(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_MESSAGE_IDS_LIKES";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = user_id;
                OracleParameter p3 = new OracleParameter();
                //Add input parameter startFrom
                p3 = cmd.Parameters.Add("startFrom", OracleDbType.Int32);
                p3.Value = range.startFrom;
                p3.Direction = ParameterDirection.Input;
                OracleParameter p4 = new OracleParameter();
                //Add input parameter limitation
                p4 = cmd.Parameters.Add("limitation", OracleDbType.Int32);
                p4.Value = range.limitation;
                p4.Direction = ParameterDirection.Input;
                //Add output parameter result
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("search_result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                await Task.FromResult(DataAdapter.Fill(dt));

                
                //dt: message_id
                int[] message_ids = new int[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    message_ids[i] = int.Parse(dt.Rows[i][0].ToString());
                }

                RestfulResult.RestfulData<Message_Id> rr = new RestfulResult.RestfulData<Message_Id>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = new Message_Id();
                rr.Data.like_message_id = message_ids;

                return new JsonResult(rr);
            });
        }

        public class User_Like_Message
        {
            [Display(Name = "用户id")]
            public int user_id { get; set; }

            [Display(Name = "推特id")]
            public int message_id { get; set; }
        }

        public class UserLikes
        {
            public bool like { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLikeMessage"></param>
        /// <returns></returns>
        [HttpPost("checkUserLikesMessage")]
        public async Task<IActionResult> checkUserLikesMessge([Required]User_Like_Message userLikeMessage)
        {
            return await Wrapper.wrap(async (OracleConnection conn) =>
            {
                RestfulResult.RestfulData<UserLikes> rr = new RestfulResult.RestfulData<UserLikes>();
                rr.Code = 200;
                UserLikes l = new UserLikes();
                l.like = await checkUserLikesMessageBool(userLikeMessage.user_id, userLikeMessage.message_id);
                rr.Data = l;
                rr.Message = "success";
                return new JsonResult(rr);
            });

        }

        public static async Task<bool> checkUserLikesMessageBool(int user_id, int message_id)
        {
            return await Wrapper.wrap(async (OracleConnection conn) =>
            {
                //FUNC_QUERY_USER_LIKES_MESSAGE(user_id in INTEGER, message_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_QUERY_USER_LIKES_MESSAGE";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = user_id;
                OracleParameter p3 = new OracleParameter();
                //Add input parameter message_id
                p3 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                p3.Value = message_id;
                p3.Direction = ParameterDirection.Input;

                await cmd.ExecuteReaderAsync();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    return false;
                }
                else
                {
                    return true;
                }
                
            });
        }
    }
}
