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
    /// 定义用于有关话题Topic的api
    /// </summary>
    [Route("api/[controller]")]
    public class TopicController : Controller
    {
        /// <summary>
        /// 查找包含topic_id的最近range范围内的message_id组成的列表
        /// </summary>
        /// <returns>返回包含message_id列表的Json对象</returns>
        /// <param name="topic_id">Topic identifier.</param>
        /// <param name="range">Range.</param>
        [HttpPost("queryMessageIdsContains/{topic_id}")]
        public IActionResult QueryMessageIdsContains([Required]int topic_id, [Required][FromBody]Range range)
        {
            //TODO 无需登录态验证
            //根据range来查找时间最近的几条message_id组成的列表
            //返回Json对象
            return Wrapper.wrap((OracleConnection conn) => 
            {
                //FUNC_QUERY_MESSAGE_IDS_CONTAINS_CERTAIN_TOPIC_ID(topic_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_MESSAGE_IDS_CONTAINS_CERTAIN_TOPIC_ID";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
                //Add input parameter follower_id
                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("topic_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = topic_id;
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

                if (int.Parse(p1.Value.ToString()) == 0)
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


        /// <summary>
        /// 查找最热的几条话题
        /// 根据range
        /// </summary>
        /// <returns>Json 包含Topics</returns>
        /// <param name="range">Range.</param>
        [HttpPost("queryTopicsBaseOnHeat")]
        public IActionResult QueryTopicsBaseOnHeat([Required][FromBody]Range range)
        {
            //TODO 无需登录态验证
            //可以直接使用Topic模型
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_QUERY_TOPIC_IDS_ORDER_BY_HEAT(startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_TOPIC_IDS_ORDER_BY_HEAT";
                OracleCommand cmd = new OracleCommand(procudureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                //Add return value
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;
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

                //dt: topic_id
                int[] topic_ids = new int[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; ++i)
                {
                    topic_ids[i] = int.Parse(dt.Rows[i][0].ToString());

                }

                RestfulResult.RestfulArray<int> rr = new RestfulResult.RestfulArray<int>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = topic_ids;

                return new JsonResult(rr);
            });


        }

        //推特中包含的话题类
        public class TopicInfos
        {
            public string topicName = "";
            public int topicId = 0;
        }

        /// <summary>
        /// 根据传入的内容，将内容中的Topic全都ADD，并返回内容中所包含的话题以及其ID
        /// </summary>
        /// <returns>TopicInfos Class.</returns>
        /// <param name="content">Twitter Content.</param>
        /// <param name="messageID">Twitter ID.</param>
        static public TopicInfos[] AddTopicsInTwitter(string content,int messageID)
        {
            System.Text.RegularExpressions.Regex topicRegex = new System.Text.RegularExpressions.Regex(@"#\w+#");
            System.Text.RegularExpressions.MatchCollection topicCollection = topicRegex.Matches(content);
            TopicInfos[] topicInfos = new TopicInfos[topicCollection.Count];
            for (int i = 0; i < topicCollection.Count; i++)
            {
                TopicInfos topic = new TopicInfos();
                topic.topicName = topicCollection[i].ToString();
                topicInfos[i] = topic;
            }
            if (topicInfos.Count() == 0) return topicInfos;

            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();

                    foreach (TopicInfos temp_topic in topicInfos)
                    {
                        string topic = temp_topic.topicName.Replace("#", "");
                        //对于topics列表里的每一个话题，分别作为函数参数来执行一次FUNC_ADD_TOPIC和FUNC_SEARCH_TOPICS函数
                        //FUNC_ADD_TOPIC(topic_content in VARCHAR2, message_id in INTEGER)
                        //return INTEGER
                        string procedureName = "FUNC_ADD_TOPIC";
                        OracleCommand cmd = new OracleCommand(procedureName, conn);
                        cmd.CommandType = CommandType.StoredProcedure;

                        //Add return value
                        OracleParameter p1 = new OracleParameter();
                        p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                        p1.Direction = ParameterDirection.ReturnValue;

                        //Add first parameter topic_content
                        OracleParameter p2 = new OracleParameter();
                        p2 = cmd.Parameters.Add("topic_content", OracleDbType.Varchar2);
                        p2.Direction = ParameterDirection.Input;
                        p2.Value = topic;

                        //Add second parameter message_id
                        OracleParameter p3 = new OracleParameter();
                        p3 = cmd.Parameters.Add("message_id", OracleDbType.Int32);
                        p3.Direction = ParameterDirection.Input;
                        p3.Value = messageID;

                        cmd.ExecuteReader();

                        if (int.Parse(p1.Value.ToString()) == 0)
                        {
                            throw new Exception("failed");
                        }

                        //FUNC_GET_TOPIC_ID_BY_NAME(Searchkey in VARCHAR2, Search_Result out INTEGER)
                        //return INTEGER
                        string procedureName2 = "FUNC_GET_TOPIC_ID_BY_NAME";
                        OracleCommand cmd2 = new OracleCommand(procedureName2, conn);
                        cmd2.CommandType = CommandType.StoredProcedure;

                        //Add return value
                        OracleParameter p4 = new OracleParameter();
                        p4 = cmd2.Parameters.Add("state", OracleDbType.Int32);
                        p4.Direction = ParameterDirection.ReturnValue;

                        //Add first parameter Searchkey
                        OracleParameter p5 = new OracleParameter();
                        p5 = cmd2.Parameters.Add("Searchkey", OracleDbType.Varchar2);
                        p5.Direction = ParameterDirection.Input;
                        p5.Value = topic;

                        //Add second parameter Search_Result
                        OracleParameter p6 = new OracleParameter();
                        p6 = cmd2.Parameters.Add("Search_Result", OracleDbType.Int32);
                        p6.Direction = ParameterDirection.Output;

                        cmd2.ExecuteReader();
                        if (int.Parse(p4.Value.ToString()) == 0)
                        {
                            throw new Exception("failed");
                        }
                        temp_topic.topicId = int.Parse(p6.Value.ToString());
                    }
                }
                catch (Exception e)
                {
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData(500, "fail");
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                    return new TopicInfos[] { };
                }
            }

            return topicInfos;
        }

        static public TopicInfos[] SearchTopicsInTwitter(string content)
        {
            System.Text.RegularExpressions.Regex topicRegex = new System.Text.RegularExpressions.Regex(@"#\w+#");
            System.Text.RegularExpressions.MatchCollection topicCollection = topicRegex.Matches(content);
            TopicInfos[] topicInfos = new TopicInfos[topicCollection.Count];
            for (int i = 0; i < topicCollection.Count; i++)
            {
                TopicInfos topic = new TopicInfos();
                topic.topicName = topicCollection[i].ToString();
                topicInfos[i] = topic;
            }
            if (topicInfos.Count() == 0) return topicInfos;

            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();

                    foreach (TopicInfos temp_topic in topicInfos)
                    {
                        string topic = temp_topic.topicName.Replace("#", "");
                       
                        //FUNC_GET_TOPIC_ID_BY_NAME(Searchkey in VARCHAR2, Search_Result out INTEGER)
                        //return INTEGER
                        string procedureName2 = "FUNC_GET_TOPIC_ID_BY_NAME";
                        OracleCommand cmd2 = new OracleCommand(procedureName2, conn);
                        cmd2.CommandType = CommandType.StoredProcedure;

                        //Add return value
                        OracleParameter p4 = new OracleParameter();
                        p4 = cmd2.Parameters.Add("state", OracleDbType.Int32);
                        p4.Direction = ParameterDirection.ReturnValue;

                        //Add first parameter Searchkey
                        OracleParameter p5 = new OracleParameter();
                        p5 = cmd2.Parameters.Add("Searchkey", OracleDbType.Varchar2);
                        p5.Direction = ParameterDirection.Input;
                        p5.Value = topic;

                        //Add second parameter Search_Result
                        OracleParameter p6 = new OracleParameter();
                        p6 = cmd2.Parameters.Add("Search_Result", OracleDbType.Int32);
                        p6.Direction = ParameterDirection.Output;

                        cmd2.ExecuteReader();
                        if (int.Parse(p4.Value.ToString()) == 0)
                        {
                            throw new Exception("failed");
                        }
                        temp_topic.topicId = int.Parse(p6.Value.ToString());
                    }
                }
                catch (Exception e)
                {
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData(500, "fail");
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                    return new TopicInfos[] { };
                }
            }

            return topicInfos;
        }

    }

}
