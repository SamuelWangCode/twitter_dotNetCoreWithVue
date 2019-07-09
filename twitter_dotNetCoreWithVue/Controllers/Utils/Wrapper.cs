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
                    var re= func(conn);
                    conn.Close();
                    return re;
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
                    conn.Close();
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

        static public int wrap(Func<OracleConnection, int> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    var re = func(conn);
                    conn.Close();
                    return re;
                }
                catch (Exception e)
                {
                    return -1;
                }
            }
        }

        static public async Task<int> wrap(Func<OracleConnection, Task<int>> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    var re = await func(conn);
                    conn.Close();
                    return re;
                }
                catch (Exception e)
                {
                    return -1;
                }
            }
        }

        static public async Task<Boolean> wrap(Func<OracleConnection, Task<Boolean>> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    var re = await func(conn);
                    conn.Close();
                    return re;
                }
                catch (Exception e)
                {
                    return false;
                }
            }
        }



        static public async Task<MessageController.MessageForShow> wrap(Func<OracleConnection, Task<MessageController.MessageForShow>> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    var re = await func(conn);
                    conn.Close();
                    return re;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }
        static public async Task<UserController.UserPublicInfo> wrap(Func<OracleConnection, Task<UserController.UserPublicInfo>> func)
        {
            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    var re = await func(conn);
                    conn.Close();
                    return re;
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }

    }
}
