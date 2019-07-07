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
        [HttpPost("add")]
        public IActionResult Add([Required]int message_id)
        {
            //TODO 需要验证登录态 添加收藏 EZ
            //返回是否成功
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
                //FUNC_ADD_COLLECTION(user_id in INTEGER, message_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_ADD_COLLECTION";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add first parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add second parameter be_followed_id
                p3 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                p3.Value = message_id;
                p3.Direction = ParameterDirection.Input;

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


        /// <summary>
        /// 删除收藏
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="message_id">Message identifier.</param>
        [HttpPost("delete")]
        public IActionResult Delete([Required]int message_id)
        {
            //TODO 需要验证登录态 添加收藏 EZ
            //返回是否成功
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
                //FUNC_DELETE_COLLECTION(user_id in INTEGER, message_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_DELETE_COLLECTION";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add first parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id_input", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add second parameter be_followed_id
                p3 = cmd.Parameters.Add("message_id_input", OracleDbType.Int32);
                p3.Value = message_id;
                p3.Direction = ParameterDirection.Input;

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

        /// <summary>
        /// 查询收藏的列表
        /// 需要Range作为参数限制
        /// </summary>
        /// <returns>包含所有收藏的推特id的Json数据</returns>
        /// <param name="range">Range.</param>
        [HttpPost("query")]
        public IActionResult Query([Required][FromBody]Range range)
        {
            //TODO 需要验证登录态
            //需要range作为参数
            //从数据库取出message_id们 加油
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
                //FUNC_QUERY_COLLECTIONS_OF_MINE(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_COLLECTIONS_OF_MINE";
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
                DataAdapter.Fill(dt);

                //dt: message_id
                int[] message_ids = new int[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    message_ids[i] = int.Parse(dt.Rows[i][0].ToString());
                }

                RestfulResult.RestfulArray<int> rr = new RestfulResult.RestfulArray<int>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = message_ids;

                return new JsonResult(rr);
            });
        }


        public class User_Collect_Message
        {
            [Display(Name = "用户id")]
            public int user_id { get; set; }

            [Display(Name = "推特id")]
            public int message_id { get; set; }
        }

        public class UserCollects
        {
            public bool favor { get; set; }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userLikeMessage"></param>
        /// <returns></returns>
        [HttpPost("checkUserCollectMessage")]
        public IActionResult checkUserCollectMessge([Required]User_Collect_Message userCollectMessage)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
                RestfulResult.RestfulData<UserCollects> rr = new RestfulResult.RestfulData<UserCollects>();
                rr.Code = 200;
                UserCollects l = new UserCollects();
                l.favor = checkUserCollectMessageBool(userCollectMessage.user_id, userCollectMessage.message_id);
                rr.Data = l;
                rr.Message = "success";
                return new JsonResult(rr);
            });

        }

        

        public static bool checkUserCollectMessageBool(int user_id, int message_id)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_QUERY_IF_USER_COLLECTS(user_id in INTEGER, message_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_QUERY_IF_USER_COLLECTS";
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

                cmd.ExecuteReader();
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

        public class CollectionNum
        {
            public int collection_num;
        }

        static public int GetCollectionCount(int user_id)
        {
            string procudureName = "FUNC_GET_COLLECTION_NUM";
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
            p3.Direction = ParameterDirection.Output;

            cmd.ExecuteReader();
            return int.Parse(p3.Value.ToString());
        }

        /// </summary>
        /// 获取收藏个数
        /// <param name="user_id"></param>
        /// <returns></returns>
        /// 
        [HttpPost("getCollectionNum")]
        public IActionResult CollectionCount([Required]int user_id)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
               
                RestfulResult.RestfulData<CollectionNum> rr = new RestfulResult.RestfulData<CollectionNum>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = new CollectionNum();
                rr.Data.collection_num = GetCollectionCount(user_id);
                return new JsonResult(rr);


            });

        }
    }
}
