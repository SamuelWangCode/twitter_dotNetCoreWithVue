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

        static public async Task<IActionResult> wrap(Func<OracleConnection, Task<IActionResult>> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    IActionResult t;
                    t = await func(conn);
                    return t;
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

        static public bool wrap(Func<OracleConnection, bool> func)
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
                    return false;
                }
            }
        }
        static public MessageController.MessageForShow wrap(Func<OracleConnection, MessageController.MessageForShow> func)
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
                    return null;
                }
            }
        }
    }
}
