using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using twitter_dotNetCoreWithVue.Controllers.Utils;
using static twitter_dotNetCoreWithVue.Controllers.SearchController;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    [Route("api/[controller]")]
    public class RecommendController : Controller
    {

        public class UserResult
        {
            public int user_id { get; set; }
            public string user_nickname { get; set; }
            public int followers_num { get; set; }
            public string avatar_url { get; set; }
        }

        /// <summary>
        /// 获得推荐的前五个用户，用于推荐栏
        /// </summary>
        /// <returns>The get.</returns>
        [HttpGet("getRecommendUsers")]
        public async Task<IActionResult> getRecommendedUsers()
        {
            //FUNC_RECOMMEND_USER(search_result out sys_refcursor)
            //return INTEGER
            return await Wrapper.wrap(async (OracleConnection conn) =>
            {
                string procudureName = "FUNC_RECOMMEND_USER";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter search_result
                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("search_result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                await Task.FromResult(DataAdapter.Fill(dt));

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                //dt: user\_id,user\_nickname,user\_avatar\_image\_id
                UserResult[] receivedUsers = new UserResult[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    receivedUsers[i] = new UserResult();
                    receivedUsers[i].user_id = int.Parse(dt.Rows[i][0].ToString());
                    receivedUsers[i].user_nickname = dt.Rows[i][1].ToString();
                    receivedUsers[i].followers_num = int.Parse(dt.Rows[i][2].ToString());
                    string avatarUrl = await UserController.getAvatarUrl(receivedUsers[i].user_id);
                    receivedUsers[i].avatar_url = avatarUrl;
                }
                RestfulResult.RestfulArray<UserResult> rr = new RestfulResult.RestfulArray<UserResult>();
                rr.Code = 200;
                rr.Data = receivedUsers;
                rr.Message = "success";
                return new JsonResult(rr);
            });
            
        }

    }

}

