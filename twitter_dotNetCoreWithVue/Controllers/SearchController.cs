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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    [Route("api/[controller]")]
    public class SearchController : Controller
    {
        //用于展示推特时的模型
        public class TwitterResult
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

        public class UserResult
        {
            [Display(Name = "用户ID")]
            public int user_id { get; set; }

            [Display(Name = "用户昵称")]
            public string user_nickname { get; set; }

            [Display(Name = "用户头像url")]
            public string user_avatar_url { get; set; }
        }

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
            public TwitterResult[] twitters { get; set; }

            [Display(Name = "用户信息集合")]
            public UserResult[] users { get; set; }

            [Display(Name = "话题信息集合")]
            public TopicResult[] topics { get; set; }
        }

        /// <summary>
        /// 搜索接口
        /// </summary>
        /// <returns>搜索的内容，分三个部分，推特/用户/话题</returns>
        /// <param name="searchKey">Identifier.</param>
        [HttpPost("{searchKey}")]
        public IActionResult getSearchResult(string searchKey, [FromBody]Range range)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //TwitterResult[] twitterResults = GetTwitterResults(conn, searchKey, range);
                UserResult[] userResults = GetUserResults(conn, searchKey, range);
                //TopicResult[] topicResults = GetTopicResults(conn, searchKey, range);
                ResultSet resultSet = new ResultSet();
                //resultSet.twitters = twitterResults;
                resultSet.users = userResults;
                //resultSet.topics = topicResults;
                RestfulResult.RestfulData<ResultSet> rr = new RestfulResult.RestfulData<ResultSet>();
                rr.Code = 200;
                rr.Data = resultSet;
                rr.Message = "success";
                return new JsonResult(rr);
            });
        }



        private TwitterResult[] GetTwitterResults(OracleConnection conn, string searchKey, Range range)
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

            if (int.Parse(p1.Value.ToString()) != 1)
            {
                throw new Exception("failed");
            }

            //dt: sender_user_id, private_letter_id, content, timestamp
            TwitterResult[] receivedTwitters = new TwitterResult[dt.Rows.Count];
            for (int i = 0; i < dt.Rows.Count; ++i)
            {
                receivedTwitters[i] = new TwitterResult();
                receivedTwitters[i].message_id = int.Parse(dt.Rows[i][0].ToString());
                receivedTwitters[i].message_content = dt.Rows[i][1].ToString();
                receivedTwitters[i].message_create_time = dt.Rows[i][2].ToString();
                receivedTwitters[i].message_agree_num = int.Parse(dt.Rows[i][3].ToString());
                receivedTwitters[i].message_transpond_num = int.Parse(dt.Rows[i][4].ToString());
                receivedTwitters[i].message_comment_num = int.Parse(dt.Rows[i][5].ToString());
                receivedTwitters[i].message_view_num = int.Parse(dt.Rows[i][6].ToString());
                receivedTwitters[i].message_has_image = int.Parse(dt.Rows[i][7].ToString());
                receivedTwitters[i].message_is_transpond = int.Parse(dt.Rows[i][8].ToString());
                receivedTwitters[i].message_sender_user_id = int.Parse(dt.Rows[i][9].ToString());
                receivedTwitters[i].message_heat = int.Parse(dt.Rows[i][10].ToString());
                receivedTwitters[i].message_transpond_message_id = int.Parse(dt.Rows[i][11].ToString());
                receivedTwitters[i].message_image_count = int.Parse(dt.Rows[i][12].ToString());

                if (receivedTwitters[i].message_has_image == 1)
                {
                    //TODO 返回图片url格式需要修改
                    string path = @"wwwroot\Messages\" + receivedTwitters[i].message_id.ToString();
                    for (int j = 0; j < receivedTwitters[j].message_image_count; j++)
                    {
                        receivedTwitters[i].message_image_urls.Append<string>("/Messages/" + receivedTwitters[i].message_id.ToString() + "/" + j.ToString());
                    }
                }
            }
            return receivedTwitters;
        }

        private UserResult[] GetUserResults(OracleConnection conn, string searchKey, Range range)
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

            if (int.Parse(p1.Value.ToString()) != 1)
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

                string avatarUrl = UserController.getAvatarUrl(receivedUsers[i].user_id);
                receivedUsers[i].user_avatar_url = avatarUrl;

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

            if (int.Parse(p1.Value.ToString()) != 1)
            {
                throw new Exception("failed");
            }

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
