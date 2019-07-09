using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using twitter_dotNetCoreWithVue.Controllers.Utils;
using System.Data;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{
    [Route("api/[controller]")]
    public class TestConnection : Controller
    {

        // GET api/<controller>/5
        /// <summary>
        /// Test DB Connection
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet("/")]
        public IActionResult Get()
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                conn.ConnectionString = ConnStr.getConnStr();
                conn.Open();
                string procedureName = "FUNC_GET_MESSAGE_NUMS";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = 8;

                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("search_result", OracleDbType.RefCursor);
                p3.Direction = ParameterDirection.Output;

                var reader = cmd.ExecuteReader();
                
                return new JsonResult(p1.Value.ToString());
            }
        }

    }
}
