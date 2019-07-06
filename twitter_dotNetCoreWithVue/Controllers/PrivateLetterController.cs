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

        public class ReceivedPrivateLetter
        {
            [Display(Name = "私信内容")]
            [Required]
            public string private_letter_content { get; set; }

            [Required]
            [Display(Name = "私信id")]
            public int private_letter_id { get; set; }

            [Required]
            [Display(Name = "发送者id")]
            public int sender_user_id { get; set; }

            [Required]
            public string timeStamp { get; set; }
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

            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_QUERY_PRIVATE_LETTERS_SEND_TO_USER(user_id in INTEGER ,startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_PRIVATE_LETTERS_SEND_TO_USER";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Value = my_user_id;
                p2.Direction = ParameterDirection.Input;
                //Add input parameter follower_id
                OracleParameter p3 = new OracleParameter();
                //Add input parameter be_followed_id
                p3 = cmd.Parameters.Add("startFrom", OracleDbType.Int32);
                p3.Value = range.startFrom;
                p3.Direction = ParameterDirection.Input;
                OracleParameter p4 = new OracleParameter();
                //Add input parameter be_followed_id
                p4 = cmd.Parameters.Add("limitation", OracleDbType.Int32);
                p4.Value = range.limitation;
                p4.Direction = ParameterDirection.Input;
                //Add input parameter search_result
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("search_result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                //dt: sender_user_id, private_letter_id, content, timestamp
                ReceivedPrivateLetter[] receiveds = new ReceivedPrivateLetter[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    receiveds[i].sender_user_id = int.Parse(dt.Rows[i][0].ToString());
                    receiveds[i].private_letter_id = int.Parse(dt.Rows[i][1].ToString());
                    receiveds[i].private_letter_content = dt.Rows[i][1].ToString();
                    receiveds[i].timeStamp = dt.Rows[i][2].ToString();
                }

                RestfulResult.RestfulArray<ReceivedPrivateLetter> rr = new RestfulResult.RestfulArray<ReceivedPrivateLetter>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = receiveds;

                return new JsonResult(rr);
            });
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

            return Wrapper.wrap((OracleConnection conn) => {
                //FUNC_ADD_PRIVATE_LETTER(sender_user_id in INTEGER, receiver_user_id in INTEGER, content in VARCHAR2(255))
                //return INTEGER
                string procudureName = "FUNC_ADD_PRIVATE_LETTER";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter sender_user_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("sender_user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add input parameter receiver_user_id
                p3 = cmd.Parameters.Add("receiver_user_id", OracleDbType.Int32);
                p3.Value = user_id;
                p3.Direction = ParameterDirection.Input;
                OracleParameter p4 = new OracleParameter();
                //Add input parameter content
                p4 = cmd.Parameters.Add("content", OracleDbType.Varchar2);
                p4.Value = letterInfo.private_letter_content;
                p4.Direction = ParameterDirection.Input;

                cmd.ExecuteReader();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

        [HttpGet("delete/{private_letter_id}")]
        public IActionResult Delete([Required]int private_letter_id)
        {
            //TODO 需要验证登录态
            //返回成功与否
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

            return Wrapper.wrap((OracleConnection conn) => {
                //FUNC_DELETE_PRIVATE_LETTER(private_letter_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_DELETE_PRIVATE_LETTER";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add first parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("private_letter_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = private_letter_id;

                cmd.ExecuteReader();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }
        
    }
}
