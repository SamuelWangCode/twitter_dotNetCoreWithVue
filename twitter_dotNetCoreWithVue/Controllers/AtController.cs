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
                //FUNC_QUERY_MESSAGE_IDS_THAT_AT_USER(user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_MESSAGE_IDS_THAT_AT_USER";
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
                p5 = cmd.Parameters.Add("search_result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                if (int.Parse(p1.Value.ToString()) != 1)
                {
                    throw new Exception("failed");
                }

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

        
    }
}
