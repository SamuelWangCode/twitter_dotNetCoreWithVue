using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using twitter_dotNetCoreWithVue.Controllers.Utils;
using twitter_dotNetCoreWithVue.Controllers;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {

        public class TopicResult
        {
            [Display(Name = "话题ID")]
            public int topic_id { get; set; }

            [Display(Name = "话题热度")]
            public int topic_heat { get; set; }

            [Display(Name = "话题内容")]
            public string topic_content { get; set; }
        }

        public class ResultSet
        {
            [Display(Name = "推特信息集合")]
            public MessageController.MessageForShow[] twitters { get; set; }

            [Display(Name = "用户信息集合")]
            public UserController.UserPublicInfo[] users { get; set; }

            [Display(Name = "话题信息集合")]
            public TopicResult[] topics { get; set; }
        }

        /// <summary>
        /// 搜索接口
        /// </summary>
        /// <returns>搜索的内容，分三个部分，推特/用户/话题</returns>
        /// <param name="searchKey">Identifier.</param>
        [HttpGet("{searchKey}")]
        public IActionResult getSearchResult(string searchKey, [FromBody]Range range)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
                MessageController.MessageForShow[] twitterResults = GetTwitterResults(conn, searchKey, range);
                UserController.UserPublicInfo[] userResults = GetUserResults(conn, searchKey, range);
                TopicResult[] topicResults = GetTopicResults(conn, searchKey, range);
                ResultSet resultSet = new ResultSet();
                resultSet.twitters = twitterResults;
                resultSet.users = userResults;
                resultSet.topics = topicResults;
                RestfulResult.RestfulData<ResultSet> rr = new RestfulResult.RestfulData<ResultSet>();
                rr.Code = 200;
                rr.Data = resultSet;
                rr.Message = "success";
                return new JsonResult(rr);
            });
        }



        private MessageController.MessageForShow[] GetTwitterResults(OracleConnection conn, string searchKey, Range range)
        {
            //FUNC_SEARCH_MESSAGE(searchKey in VARCHAR2(50), startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
            //return INTEGER
            string procudureName = "FUNC_SEARCH_MESSAGE";
            OracleCommand cmd = new OracleCommand(procudureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //Add return value
            OracleParameter p1 = new OracleParameter();
            p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
            p1.Direction = ParameterDirection.ReturnValue;
            //Add input parameter user_id
            OracleParameter p2 = new OracleParameter();
            p2 = cmd.Parameters.Add("searchKey", OracleDbType.Varchar2);
            p2.Value = searchKey;
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
            
            //dt: sender_user_id, private_letter_id, content, timestamp
            MessageController.MessageForShow[] receivedTwitters = new MessageController.MessageForShow[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                
                int message_id = int.Parse(dt.Rows[i][0].ToString());
                receivedTwitters[i] = MessageController.InnerQuery(message_id);
            }
            return receivedTwitters;
        }

        private UserController.UserPublicInfo[] GetUserResults(OracleConnection conn, string searchKey, Range range)
        {
            //FUNC_SEARCH_USER(searchKey in VARCHAR2(50), startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
            //return INTEGER
            string procudureName = "FUNC_SEARCH_USER";
            OracleCommand cmd = new OracleCommand(procudureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //Add return value
            OracleParameter p1 = new OracleParameter();
            p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
            p1.Direction = ParameterDirection.ReturnValue;
            //Add input parameter user_id
            OracleParameter p2 = new OracleParameter();
            p2 = cmd.Parameters.Add("searchKey", OracleDbType.Varchar2);
            p2.Value = searchKey;
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


            //dt: user\_id,user\_nickname,user\_avatar\_image\_id
            UserController.UserPublicInfo[] receivedUsers = new UserController.UserPublicInfo[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                int user_id = int.Parse(dt.Rows[i][0].ToString());
                receivedUsers[i] = UserController.getUserPublicInfo(user_id);
            }
            return receivedUsers;
        }

        private TopicResult[] GetTopicResults(OracleConnection conn, string searchKey, Range range)
        {
            //FUNC_SEARCH_TOPICS(searchKey in VARCHAR2(50), startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
            //return INTEGER
            string procudureName = "FUNC_SEARCH_TOPICS";
            OracleCommand cmd = new OracleCommand(procudureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            //Add return value
            OracleParameter p1 = new OracleParameter();
            p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
            p1.Direction = ParameterDirection.ReturnValue;
            //Add input parameter user_id
            OracleParameter p2 = new OracleParameter();
            p2 = cmd.Parameters.Add("searchKey", OracleDbType.Varchar2);
            p2.Value = searchKey;
            p2.Direction = ParameterDirection.Input;
            //Add input parameter follower_idss
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


            //dt: topics_id, topic_content
            TopicResult[] receivedTopics = new TopicResult[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                receivedTopics[i] = new TopicResult();
                receivedTopics[i].topic_id = int.Parse(dt.Rows[i][0].ToString());
                receivedTopics[i].topic_heat = int.Parse(dt.Rows[i][1].ToString());
                receivedTopics[i].topic_content = dt.Rows[i][2].ToString();
            }
            return receivedTopics;
        }
    }
}
