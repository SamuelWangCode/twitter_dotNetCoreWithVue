using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using twitter_dotNetCoreWithVue.Controllers.Utils;

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
                var cmd = conn.CreateCommand();
                cmd.CommandText = "Select * from User_Public_Info";
                OracleDataReader reader = cmd.ExecuteReader();
                List<int> ids = new List<int>();
                while (reader.Read())
                {
                    ids.Add(reader.GetInt32(0));
                }
                return new JsonResult(ids);
            }
        }
        
    }
}
