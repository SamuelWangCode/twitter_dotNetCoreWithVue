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
        [HttpGet("queryMessageIdsContains/{topic_id}")]
        public IActionResult QueryMessageIdsContains([Required]int topic_id, [Required][FromBody]Range range)
        {
            //TODO 无需登录态验证
            //根据range来查找时间最近的几条message_id组成的列表
            //返回Json对象
            return Wrapper.wrap((OracleConnection conn) => 
            {
                //FUNC_QUERY_MESSAGE_IDS_CONTAINS_CERTAIN_TOPIC_ID(topic_id in INTEGER, startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
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

                if (int.Parse(p1.ToString()) != 1)
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
        [HttpGet("queryTopicsBaseOnHeat")]
        public IActionResult QueryTopicsBaseOnHeat([Required][FromBody]Range range)
        {
            //TODO 无需登录态验证
            //可以直接使用Topic模型
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_QUERY_MESSAGE_IDS_CONTAINS_CERTAIN_TOPIC_ID(startFrom in INTEGER, limitation in INTEGER, search_result out sys_refcursor)
                //return INTEGER
                string procudureName = "FUNC_QUERY_TOPICS_ORDER_BY_HEAT";
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
                p5 = cmd.Parameters.Add("result", OracleDbType.RefCursor);
                p5.Direction = ParameterDirection.Output;

                OracleDataAdapter DataAdapter = new OracleDataAdapter(cmd);
                DataTable dt = new DataTable();
                DataAdapter.Fill(dt);

                if (int.Parse(p1.ToString()) != 1)
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
