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
    /// 此控制器定义用户之间互相关注的相关api
    /// </summary>
    [Route("api/[controller]")]
    public class RelationController : Controller
    {

        public class Simple_User_Info
        {
            [Required]
            public int user_id { get; set; }
            [Required]
            public string nickName { get; set; }
            [Required]
            public string avatarUrl { get; set; }
        }

        /// <summary>
        /// 关注某人时使用
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="user_id">User identifier.</param>
        [HttpGet("follow/{user_id}")]
        public IActionResult FollowUser([Required]int user_id)
        {
            //TODO getAuthentication and do the CURD
            //Authentication
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
                //FUNC_Add_Relation(follower_id in INTEGER, be_followed_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_ADD_RELATION";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add first parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("follower_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add second parameter be_followed_id
                p3 = cmd.Parameters.Add("be_followed_id", OracleDbType.Int32);
                p3.Value = user_id;
                p3.Direction = ParameterDirection.Input;

                cmd.ExecuteReader();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });

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
            //TODO do the CURD
            //注意需要按时间排序
            //使用range作为限制参数
            //返回Json对象
            return Wrapper.wrap((OracleConnection conn) => 
            {
                //FUNC_QUERY_FOLLOWING_LIST(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_FOLLOWING_LIST";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = user_id;
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
                //Add output parameter search_result
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;
                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                //dt: user_id, nickName, avatarId
                Simple_User_Info[] a = new Simple_User_Info[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    Simple_User_Info info = new Simple_User_Info();
                    info.user_id = int.Parse(dt.Rows[i][0].ToString());
                    info.nickName = dt.Rows[i][1].ToString();
                    info.avatarUrl = "/avatars/" + dt.Rows[i][2].ToString();
                    a[i] = info;
                }

                RestfulResult.RestfulArray<Simple_User_Info> rr = new RestfulResult.RestfulArray<Simple_User_Info>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = a;

                return new JsonResult(rr);
            });


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
                //FUNC_QUERY_FOLLOWERS_LIST(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_FOLLOWERS_LIST";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
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
                p5 = cmd.Parameters.Add("result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                if (int.Parse(p1.ToString()) != 1)
                {
                    throw new Exception("failed");
                }

                //dt: user_id, nickName, avatarId
                Simple_User_Info[] a = new Simple_User_Info[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    Simple_User_Info info = new Simple_User_Info();
                    info.user_id = int.Parse(dt.Rows[i][0].ToString());
                    info.nickName = dt.Rows[i][1].ToString();
                    info.avatarUrl = "/avatars/" + dt.Rows[i][2].ToString();
                    a[i] = info;
                }

                RestfulResult.RestfulArray<Simple_User_Info> rr = new RestfulResult.RestfulArray<Simple_User_Info>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = a;

                return new JsonResult(rr);
            });
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
                //FUNC_REMOVE_RELATION(follower_id in INTEGER, be_followed_id in INTEGER)
                //return INTEGER
                string procudureName = "FUNC_REMOVE_RELATION";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add first parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("follower_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add second parameter be_followed_id
                p3 = cmd.Parameters.Add("be_followed_id", OracleDbType.Int32);
                p3.Value = user_id;
                p3.Direction = ParameterDirection.Input;

                cmd.ExecuteReader();
                Console.WriteLine(p1.Value);

                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

    }
}
