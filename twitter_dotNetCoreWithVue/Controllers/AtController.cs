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
        [HttpPost("query")]
        public async Task<IActionResult> Query([Required][FromBody]Range range)
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

            return await Wrapper.wrap(async (OracleConnection conn) =>
            {
                //FUNC_QUERY_MESSAGE_AT_USER(in_user_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_MESSAGE_AT_USER";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("in_user_id", OracleDbType.Int32);
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
                await Task.FromResult(DataAdapter.Fill(dt));

                //dt: message_id
                MessageController.MessageForShow[] messages = new MessageController.MessageForShow[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    int message_id = int.Parse(dt.Rows[i][0].ToString());
                    messages[i] = await MessageController.InnerQuery(message_id);
                }


                RestfulResult.RestfulArray<MessageController.MessageForShow> rr = new RestfulResult.RestfulArray<MessageController.MessageForShow>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = messages;
                return new JsonResult(rr);
            });
        }

        /// <summary>
        /// 返回对自己的未读艾特数
        /// </summary>
        /// <returns>未读艾特数，int值</returns>
        /// <param name="range">Range.</param>
        [HttpPost("queryUnreadAt")]
        public async Task<IActionResult> QueryUnreadAt()
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

            return await Wrapper.wrap(async (OracleConnection conn) =>
            {
                //FUNC_QUERY_UNREAD_AT(userid in INTEGER, unread_count out INTEGER)
                //return INTEGER
                string procudureName = "FUNC_QUERY_UNREAD_AT";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("userid", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = my_user_id;
                OracleParameter p3 = new OracleParameter();
                //Add input parameter be_followed_id
                p3 = cmd.Parameters.Add("unread_count", OracleDbType.Int32);
                p3.Direction = ParameterDirection.Output;

                await cmd.ExecuteReaderAsync();
                
                RestfulResult.RestfulData<int> rr = new RestfulResult.RestfulData<int>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = int.Parse(p3.Value.ToString());
                return new JsonResult(rr);
            });
        }

        //推特中包含的艾特信息的类
        public class AtInfos
        {
            public string atName = "";
            public int atIds = 0;
        }

        /// <summary>
        /// 根据传入的内容，将内容中的At全都ADD
        /// </summary>
        /// <returns>AtInfos的类.</returns>
        /// <param name="content">Twitter Content.</param>
        /// <param name="messageID">Twitter ID.</param>
        /// <param name="source_user_id">Source User ID.</param>
        static public async Task<AtInfos[]> AddAtsInTwitter(string content, int messageID, int source_user_id)
        {
            System.Text.RegularExpressions.Regex atRegex = new System.Text.RegularExpressions.Regex(@"@\w+");
            System.Text.RegularExpressions.MatchCollection atCollection = atRegex.Matches(content);
            AtInfos[] atInfos = new AtInfos[atCollection.Count];
            for (int i = 0; i < atCollection.Count; i++)
            {
                AtInfos at = new AtInfos();
                at.atName = atCollection[i].ToString();
                atInfos[i] = at;
            }
            if (atInfos.Count() == 0) return atInfos;

            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();

                    foreach (AtInfos temp_at in atInfos)
                    {
                        string at = temp_at.atName.Replace("@", "");
                        //对于ats列表里的每一个话题，分别作为函数参数来执行一次FUNC_AT_USER函数
                        //FUNC_AT_USER(at_nickname in VARCHAR2, atmessage_id in INTEGER, source_user_id in INTEGER)
                        //return INTEGER
                        string procedureName = "FUNC_ADD_AT_USER";
                        OracleCommand cmd = new OracleCommand(procedureName, conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        //Add return value
                        OracleParameter p1 = new OracleParameter();
                        p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                        p1.Direction = ParameterDirection.ReturnValue;

                        //Add first parameter topic_content
                        OracleParameter p2 = new OracleParameter();
                        p2 = cmd.Parameters.Add("at_nickname", OracleDbType.Varchar2);
                        p2.Direction = ParameterDirection.Input;
                        p2.Value = at;

                        //Add second parameter message_id
                        OracleParameter p3 = new OracleParameter();
                        p3 = cmd.Parameters.Add("atmessage_id", OracleDbType.Int32);
                        p3.Direction = ParameterDirection.Input;
                        p3.Value = messageID;

                        //Add third parameter source_user_id
                        OracleParameter p4 = new OracleParameter();
                        p4 = cmd.Parameters.Add("source_user_id", OracleDbType.Int32);
                        p4.Direction = ParameterDirection.Input;
                        p4.Value = source_user_id;

                        await cmd.ExecuteReaderAsync();
                        if (int.Parse(p1.Value.ToString()) == 0)
                        {
                            throw new Exception("failed");
                        }

                        //FUNC_GET_USER_ID_BY_NAME(Searchkey in VARCHAR2, Search_Result out INTEGER)
                        //return INTEGER
                        string procedureName2 = "FUNC_GET_USER_ID_BY_NAME";
                        OracleCommand cmd2 = new OracleCommand(procedureName2, conn);
                        cmd2.CommandType = CommandType.StoredProcedure;

                        //Add return value
                        OracleParameter p5 = new OracleParameter();
                        p5 = cmd2.Parameters.Add("state", OracleDbType.Int32);
                        p5.Direction = ParameterDirection.ReturnValue;

                        //Add first parameter Searchkey
                        OracleParameter p6 = new OracleParameter();
                        p6 = cmd2.Parameters.Add("Searchkey", OracleDbType.Varchar2);
                        p6.Direction = ParameterDirection.Input;
                        p6.Value = at;

                        //Add second parameter Search_Result
                        OracleParameter p7 = new OracleParameter();
                        p7 = cmd2.Parameters.Add("Search_Result", OracleDbType.Int32);
                        p7.Direction = ParameterDirection.Output;

                        await cmd2.ExecuteReaderAsync();
                        if (int.Parse(p5.Value.ToString()) == 0)
                        {
                            throw new Exception("failed");
                        }
                        temp_at.atIds = int.Parse(p7.Value.ToString());
                    }
                    
                }
                catch (Exception e)
                {
                    conn.Close();
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData(500, "fail");
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                    return new AtInfos[] { };
                }
                conn.Close();
            }
            
            return atInfos;
        }

        static public async Task<AtInfos[]> SearchAtsInTwitter(string content)
        {
            System.Text.RegularExpressions.Regex atRegex = new System.Text.RegularExpressions.Regex(@"@\w+");
            System.Text.RegularExpressions.MatchCollection atCollection = atRegex.Matches(content);
            AtInfos[] atInfos = new AtInfos[atCollection.Count];
            for (int i = 0; i < atCollection.Count; i++)
            {
                AtInfos at = new AtInfos();
                at.atName = atCollection[i].ToString();
                atInfos[i] = at;
            }
            if (atInfos.Count() == 0) return atInfos;

            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();

                    foreach (AtInfos temp_at in atInfos)
                    {
                        string at = temp_at.atName.Replace("@", "");
                        //对于ats列表里的每一个话题，分别作为函数参数来执行一次FUNC_AT_USER函数

                        //FUNC_GET_USER_ID_BY_NAME(Searchkey in VARCHAR2, Search_Result out INTEGER)
                        //return INTEGER
                        string procedureName2 = "FUNC_GET_USER_ID_BY_NAME";
                        OracleCommand cmd2 = new OracleCommand(procedureName2, conn);
                        cmd2.CommandType = CommandType.StoredProcedure;

                        //Add return value
                        OracleParameter p5 = new OracleParameter();
                        p5 = cmd2.Parameters.Add("state", OracleDbType.Int32);
                        p5.Direction = ParameterDirection.ReturnValue;

                        //Add first parameter Searchkey
                        OracleParameter p6 = new OracleParameter();
                        p6 = cmd2.Parameters.Add("Searchkey", OracleDbType.Varchar2);
                        p6.Direction = ParameterDirection.Input;
                        p6.Value = at;

                        //Add second parameter Search_Result
                        OracleParameter p7 = new OracleParameter();
                        p7 = cmd2.Parameters.Add("Search_Result", OracleDbType.Int32);
                        p7.Direction = ParameterDirection.Output;

                        await cmd2.ExecuteReaderAsync();
                        if (int.Parse(p5.Value.ToString()) == 0)
                        {
                            throw new Exception("failed");
                        }
                        temp_at.atIds = int.Parse(p7.Value.ToString());
                    }
                }
                catch (Exception e)
                {
                    conn.Close();
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData(500, "fail");
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                    return new AtInfos[] { };
                }
                conn.Close();
            }

            return atInfos;
        }
    }
}
