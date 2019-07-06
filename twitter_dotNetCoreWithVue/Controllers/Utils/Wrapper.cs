using System;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Threading.Tasks;

namespace twitter_dotNetCoreWithVue.Controllers.Utils
{
    public class Wrapper
    {
        static public IActionResult wrap(Func<OracleConnection, IActionResult> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    return func(conn);
                }
                catch (Exception e)
                {
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData(500, "fail");
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                    return new JsonResult(rr);
                }
            }
        }

        static public IActionResult wrap(Func<OracleConnection, Task<IActionResult>> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    return func(conn).Result;
                }
                catch (Exception e)
                {
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData(500, "fail");
                    Console.Write(e.Message);
                    Console.Write(e.StackTrace);
                    return new JsonResult(rr);
                }
            }
        }


    }
}
