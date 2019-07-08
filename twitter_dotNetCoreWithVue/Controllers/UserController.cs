using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using twitter_dotNetCoreWithVue.Models;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using twitter_dotNetCoreWithVue.Controllers.Utils;
using System.Data;
using System.IO;
using Microsoft.AspNetCore.Cors;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{

    /// <summary>
    /// User controller.
    /// 此控制器定义注册，登录，个人信息的增删改查等操作api接口
    /// </summary>
    [EnableCors("Admin")]
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        //用于注册时的数据模型
        public class UserInfoForSignUp
        {
            public string email { get; set; }
            public string password { get; set; }
            public string nickname { get; set; }
        }

        //用于登录时的数据模型
        public class UserInfoForSignIn
        {
            public string email { get; set; }
            public string password { get; set; }
        }

        //该模型用于在个人界面修改资料时使用
        public class UserInfoEdit
        {
            [Required]
            public string nickname { get; set; }
            public string password { get; set; }
            public string realname { get; set; }
            public string gender { get; set; }
            public string self_introduction { get; set; }
            //public int mode { get; set; }
        }

        public class UserId
        {
            public int user_id { get; set; }
        }

        public class UserPublicInfo
        {
            public int user_id { get; set; }
            public string nickname { get; set; }
            public string register_time { get; set; }
            public string self_introction { get; set; }
            public int followers_num { get; set; }
            public int follows_num { get; set; }
            public string avatar_url { get; set; }
            public int messages_num { get; set; }

            public int collection_num { get; set; }
        }

        bool CheckUserEamil(string email, OracleConnection conn)
        {
            string procedureName = "FUNC_CHECK_USER_EMAIL_EXIST";
            OracleCommand cmd = new OracleCommand(procedureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            OracleParameter p1 = new OracleParameter();
            p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
            p1.Direction = ParameterDirection.ReturnValue;

            OracleParameter p2 = new OracleParameter();
            p2 = cmd.Parameters.Add("email", OracleDbType.Varchar2);
            p2.Direction = ParameterDirection.Input;
            p2.Value = email;

            cmd.ExecuteReader();
            return int.Parse(p1.Value.ToString()) == 1;
        }

        [HttpPost("checkEmail")]
        public IActionResult CheckEmail([Required]string email)
        {
            //FUNC_CHECK_USER_EMAIL_EXIST(email in VARCHAR)
            //return INGETER
            return Wrapper.wrap((OracleConnection conn) =>
            {

                if (CheckUserEamil(email, conn))
                {
                    return new JsonResult(new RestfulResult.RestfulData(200, "The email is used"));
                }
                else
                {
                    return new JsonResult(new RestfulResult.RestfulData(200, "The email don't be used"));
                }
            });
        }

        /// <summary>
        /// 此接口在注册时使用。
        /// 使用POST方法，传递邮箱，密码，昵称即可，其他用户信息在个人界面处修改和添加。
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="userInfoForSignUp">注册时需要的信息</param>
        [HttpPost("signUp")]
        public IActionResult SignUp([FromBody]UserInfoForSignUp userInfoForSignUp)
        {
            //TODO 注册啦
            //返回是否注册成功


            return Wrapper.wrap((OracleConnection conn) =>
            {
                if (!(ParameterChecker.CheckPara(userInfoForSignUp.email, ParameterChecker.ParaTpye.Email)
                && ParameterChecker.CheckPara(userInfoForSignUp.password, ParameterChecker.ParaTpye.Password)))
                {
                    return new JsonResult(new RestfulResult.RestfulData(200, "Invalid Email or Password"));
                }
                if (CheckUserEamil(userInfoForSignUp.email, conn))
                {
                    return new JsonResult(new RestfulResult.RestfulData(200, "The email is used"));
                }
                //FUNC_USER_SIGN_UP(email in VARCHAR, nickname in VARCHAR, password in VARCHAR)
                //return INGETER
                string procedureName = "FUNC_USER_SIGN_UP";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("email", OracleDbType.Varchar2);
                p2.Direction = ParameterDirection.Input;
                p2.Value = userInfoForSignUp.email;

                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("nickname", OracleDbType.Varchar2);
                p3.Direction = ParameterDirection.Input;
                p3.Value = userInfoForSignUp.nickname;

                OracleParameter p4 = new OracleParameter();
                p4 = cmd.Parameters.Add("password", OracleDbType.Varchar2);
                p4.Direction = ParameterDirection.Input;
                p4.Value = userInfoForSignUp.password;

                cmd.ExecuteReader();

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });

        }

        /// <summary>
        /// 此接口用于登录
        /// !!!!!!!!!!!!!!!!!!!此接口十分重要
        /// 
        /// </summary>
        /// <returns>返回用户的user_id</returns>
        /// <param name="userInfoForSignIn">登录时需要的信息</param>
        [HttpPost("signIn")]
        public IActionResult SignIn([FromBody]UserInfoForSignIn userInfoForSignIn)
        {
            //下面的变量claims是Claim类型的数组，Claim是string类型的键值对，所以claims数组中可以存储任意个和用户有关的信息，
            //不过要注意这些信息都是加密后存储在客户端浏览器cookie中的，所以最好不要存储太多特别敏感的信息
            //表示当前登录的用户是谁
            //TODO 从数据库依照email获取user_id password
            //TODO 然后再和用户输入的进行核对，若password核对成功
            //TODO 则将一下信息存入cookies
            int userId = -1;

            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //若已经登录，直接返回
                RestfulResult.RestfulData<UserId> rr = new RestfulResult.RestfulData<UserId>();
                userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                rr.Code = 200;
                rr.Message = "Aready Sign In";
                rr.Data = new UserId();
                rr.Data.user_id = userId;
                return new JsonResult(rr);
            }

            else
            {
                return Wrapper.wrap((OracleConnection conn) =>
                {
                    //FUNC_USER_SIGN_IN_BY_EMAIL(email in VARCHAR, password in VARCHAR, user_id out INTEGER)
                    //return INTEGER
                    string procedureName = "FUNC_USER_SIGN_IN_BY_EMAIL";
                    OracleCommand cmd = new OracleCommand(procedureName, conn);
                    cmd.CommandType = CommandType.StoredProcedure;
                    OracleParameter p1 = new OracleParameter();
                    p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                    p1.Direction = ParameterDirection.ReturnValue;

                    OracleParameter p2 = new OracleParameter();
                    p2 = cmd.Parameters.Add("email", OracleDbType.Varchar2);
                    p2.Direction = ParameterDirection.Input;
                    p2.Value = userInfoForSignIn.email;

                    OracleParameter p3 = new OracleParameter();
                    p3 = cmd.Parameters.Add("password", OracleDbType.Varchar2);
                    p3.Direction = ParameterDirection.Input;
                    p3.Value = userInfoForSignIn.password;

                    OracleParameter p4 = new OracleParameter();
                    p4 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                    p4.Direction = ParameterDirection.Output;

                    cmd.ExecuteReader();

                    RestfulResult.RestfulData<UserId> rr = new RestfulResult.RestfulData<UserId>();
                    rr.Code = 200;
                    if (int.Parse(p1.Value.ToString()) == 0)
                    {
                        rr.Data = null;
                        rr.Message = "E-mail or Password Wrong";
                        return new JsonResult(rr);
                    }
                    else
                    {
                        userId = int.Parse(p4.Value.ToString());
                        rr.Data = new UserId();
                        rr.Data.user_id = userId;
                        rr.Message = "Sign in success";
                    }

                    var claims = new[] {
                new Claim("user_id", userId.ToString()),
                new Claim("email", userInfoForSignIn.email),
                new Claim("password", userInfoForSignIn.password)
            };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    ClaimsPrincipal user = new ClaimsPrincipal(claimsIdentity);

                    //signin 在内部实际上是在设置cookies
                    //HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user).Wait();

                    //可以使用HttpContext.SignInAsync方法的重载来定义持久化cookie存储用户认证信息，例如下面的代码就定义了用户登录后60分钟内cookie都会保留在客户端计算机硬盘上，
                    //即便用户关闭了浏览器，60分钟内再次访问站点仍然是处于登录状态，除非调用Logout方法注销登录。
                    HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    user, new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = DateTimeOffset.Now.AddMinutes(60) }).Wait();
                    //TODO 我们需要做的将用户的id返回给客户端
                    return new JsonResult(rr);
                });
            }
        }
        [HttpGet("check_login")]
        public IActionResult CheckLogin()
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
                //claims键值对都读出来，比如我们刚才定义的UserName的值Wangdacui就在这里读取出来了
                RestfulResult.RestfulData<UserId> rr = new RestfulResult.RestfulData<UserId>();
                rr.Code = 200;
                rr.Data = new UserId();
                rr.Data.user_id= int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                rr.Message = "Aready login";
                return new JsonResult(rr);
            }
            else
            {
                //TODO
                //进入到这部分意味着用户登录态已经失效，需要返回给客户端信息，即需要登录。
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }
        }
        /// <summary>
        /// 此接口用于编辑个人信息界面
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="userInfoEdit">用户可以被直接修改的信息</param>
        [HttpPost("editInfo")]
        public IActionResult EditInfo([FromBody]UserInfoEdit userInfoEdit)
        {
            //如果HttpContext.User.Identity.IsAuthenticated为true，
            //或者HttpContext.User.Claims.Count()大于0表示用户已经登录
            //TODO 编辑个人信息
            int userId = -1;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
                //claims键值对都读出来，比如我们刚才定义的UserName的值Wangdacui就在这里读取出来了
                userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            }
            else
            {
                //TODO
                //进入到这部分意味着用户登录态已经失效，需要返回给客户端信息，即需要登录。
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }

            return Wrapper.wrap((OracleConnection conn) =>
            {
                if (!(ParameterChecker.CheckPara(userInfoEdit.password, ParameterChecker.ParaTpye.Password)
                || userInfoEdit.password == ""))
                {
                    return new JsonResult(new RestfulResult.RestfulData(200, "Invalid Password"));
                }
                if (!(ParameterChecker.CheckPara(userInfoEdit.gender, ParameterChecker.ParaTpye.Gender)
                || userInfoEdit.gender == ""))
                {
                    return new JsonResult(new RestfulResult.RestfulData(200, "Invalid Gender"));
                }

                //FUNC_SET_USER_INFO
                //(nickname in VARCHAR, self_introduction in VARCHAR, password in VARCHAR, realname in VARCHAR, gender in VARCHAR,id in INTEGER, mode in INTEGER)
                //return INTEGER
                int mode = 0;
                string procedureName = "FUNC_SET_USER_INFO";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("nickname", OracleDbType.Varchar2);
                p2.Direction = ParameterDirection.Input;
                p2.Value = userInfoEdit.nickname;
                if (userInfoEdit.nickname != "")
                {
                    mode |= 1 << 0;
                }

                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("introduction", OracleDbType.Varchar2);
                p3.Direction = ParameterDirection.Input;
                p3.Value = userInfoEdit.self_introduction;
                if (userInfoEdit.self_introduction != "")
                {
                    mode |= 1 << 1;
                }


                OracleParameter p4 = new OracleParameter();
                p4 = cmd.Parameters.Add("password", OracleDbType.Varchar2);
                p4.Direction = ParameterDirection.Input;
                p4.Value = userInfoEdit.password;
                if (userInfoEdit.password != "")
                {
                    mode |= 1 << 2;
                }

                OracleParameter p5 = new OracleParameter();
                p5 = cmd.Parameters.Add("realname", OracleDbType.Varchar2);
                p5.Direction = ParameterDirection.Input;
                p5.Value = userInfoEdit.realname;
                if (userInfoEdit.realname != "")
                {
                    mode |= 1 << 3;
                }




                OracleParameter p6 = new OracleParameter();
                p6 = cmd.Parameters.Add("gender", OracleDbType.Varchar2);
                p6.Direction = ParameterDirection.Input;
                p6.Value = userInfoEdit.gender;
                if (userInfoEdit.gender != "")
                {
                    mode |= 1 << 4;
                }



                OracleParameter p7 = new OracleParameter();
                p7 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p7.Direction = ParameterDirection.Input;
                p7.Value = userId;

                OracleParameter p8 = new OracleParameter();
                p8 = cmd.Parameters.Add("mode", OracleDbType.Int32);
                p8.Direction = ParameterDirection.Input;
                p8.Value = mode;

                cmd.ExecuteReader();

                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

        /// <summary>
        /// 设置当前使用哪一个头像作为主要头像
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="avatar_id">用户的头像图片id</param>
        [HttpGet("setAvatar")]
        public IActionResult ChangeAvatar([Required]int avatar_id)
        {
            string userId;
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
                //claims键值对都读出来
                userId = HttpContext.User.Claims.First().Value;
            }
            else
            {
                RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                rr.Code = 200;
                rr.Message = "Need Authentication";
                return new JsonResult(rr);
            }
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_SET_MAIN_AVATAR(user_id in INTEGER, avatar_id in INTEGER)
                //return INGETER
                string procedureName = "FUNC_SET_MAIN_AVATAR";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;

                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = userId;

                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("avatar_id", OracleDbType.Int32);
                p3.Direction = ParameterDirection.Input;
                p3.Value = avatar_id;

                cmd.ExecuteReader();
                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }

                RestfulResult.RestfulData rr = new RestfulResult.RestfulData(200, "success");
                return new JsonResult(rr);
            });
        }

        /// <summary>
        /// 获得用户正在使用的头像图片url
        /// <returns>用户的头像url</returns>
        /// </summary>
        [HttpGet("getAvatarImageSrc/{user_id}")]
        public IActionResult GetAvatar([Required]int user_id)
        {
            //TODO 无需验证身份
            //从数据库获得此人的正在使用头像
            //返回头像的url
            return Wrapper.wrap((OracleConnection conn) =>
            {
                string avatarUrl = getAvatarUrl(user_id);
                RestfulResult.RestfulData<string> rr = new RestfulResult.RestfulData<string>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = avatarUrl;
                return new JsonResult(rr);
            });
        }


        public static string getAvatarUrl(int user_id)
        {
            //FUNC_GET_USER_AVATAR(in_user_id in INTEGER, avatar_id out INTEGER)
            //return INTEGER
            OracleConnection conn = new OracleConnection(ConnStr.getConnStr());
            conn.ConnectionString = ConnStr.getConnStr();
            conn.Open();
            string procedureName = "FUNC_GET_USER_AVATAR";
            OracleCommand cmd = new OracleCommand(procedureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            OracleParameter p1 = new OracleParameter();
            p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
            p1.Direction = ParameterDirection.ReturnValue;

            OracleParameter p2 = new OracleParameter();
            p2 = cmd.Parameters.Add("in_user_id", OracleDbType.Int32);
            p2.Direction = ParameterDirection.Input;
            p2.Value = user_id;

            OracleParameter p3 = new OracleParameter();
            p3 = cmd.Parameters.Add("avatar_id", OracleDbType.Int32);
            p3.Direction = ParameterDirection.Output;

            cmd.ExecuteReader();
            if (int.Parse(p1.Value.ToString()) == 0)
            {
                return "/avatars/0.jpg";
            }
            return "/avatars/" + int.Parse(p3.Value.ToString()).ToString() + ".jpg";
        }


        public static UserPublicInfo getUserPublicInfo(int user_id)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //FUNC_GET_USER_PUBLIC_INFO(user_id in INTEGER, info out sys_refcursor)
                //return INGETER
                string procedureName = "FUNC_GET_USER_PUBLIC_INFO";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = user_id;

                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("info", OracleDbType.RefCursor);
                p3.Direction = ParameterDirection.Output;

                var reader = cmd.ExecuteReader();
                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }
                else
                {
                    if (reader.Read())
                    {
                        RestfulResult.RestfulData<UserPublicInfo> rr = new RestfulResult.RestfulData<UserPublicInfo>();
                        string[] temp = new string[reader.FieldCount];
                        for (int i = 0; i < reader.FieldCount; ++i)
                        {
                            temp[i] = reader.GetValue(i).ToString();
                        }
                        rr.Data = new UserPublicInfo();
                        rr.Data.user_id = int.Parse(reader["USER_ID"].ToString());
                        rr.Data.nickname = reader["USER_NICKNAME"].ToString();
                        rr.Data.self_introction = reader["USER_SELF_INTRODUCTION"].ToString();
                        rr.Data.register_time = reader["USER_REGISTER_TIME"].ToString();
                        rr.Data.followers_num = int.Parse(reader["USER_FOLLOWERS_NUM"].ToString());
                        rr.Data.follows_num = int.Parse(reader["USER_FOLLOWS_NUM"].ToString());
                        rr.Data.messages_num = getUserMessageNum(rr.Data.user_id);
                        rr.Data.avatar_url = getAvatarUrl(user_id);
                        rr.Data.collection_num = CollectionController.GetCollectionCount(user_id,conn);
                        return rr.Data;
                    }
                    else
                    {
                        throw new Exception("failed");
                    }
                }
            });
        }

        /// <summary>
        /// 用户注销时调用的api
        /// </summary>
        /// <returns>success</returns>
        [HttpGet("logOut")]
        public IActionResult LogOut()
        {
            //注销登录的用户，意味着删除客户端的cookies
            HttpContext.SignOutAsync().Wait();
            //TODO
            return new JsonResult(new RestfulResult.RestfulData(200, "success"));
        }

        /// <summary>
        /// 查看某个人的可公开信息
        /// </summary>
        /// <returns>User_Public_Info的实例</returns>
        /// <param name="user_id">User identifier.</param>
        
        [HttpGet("getUserPublicInfo/{user_id}")]
        public IActionResult QueryUser([Required]int user_id)
        {
            //TODO 查询可公开信息
            //返回含有列表的Json对象
            return Wrapper.wrap((OracleConnection conn) =>
            {
                RestfulResult.RestfulData<UserPublicInfo> rr = new RestfulResult.RestfulData<UserPublicInfo>();
                rr.Code = 200;
                rr.Message = "success";
                rr.Data = getUserPublicInfo(user_id);
                return new JsonResult(rr);
            });

        }


        public static int getUserMessageNum(int user_id)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
            //FUNC_GET_MESSAGE_NUMS(user_id in INTEGER, info out sys_refcursor)
            //return INGETER
            string procedureName = "FUNC_GET_MESSAGE_NUMS";
            OracleCommand cmd = new OracleCommand(procedureName, conn);
            cmd.CommandType = CommandType.StoredProcedure;
            OracleParameter p1 = new OracleParameter();
            p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
            p1.Direction = ParameterDirection.ReturnValue;

            OracleParameter p2 = new OracleParameter();
            p2 = cmd.Parameters.Add("user_id", OracleDbType.Int32);
            p2.Direction = ParameterDirection.Input;
            p2.Value = user_id;

            OracleParameter p3 = new OracleParameter();
            p3 = cmd.Parameters.Add("search_result", OracleDbType.RefCursor);
            p3.Direction = ParameterDirection.Output;

            var reader = cmd.ExecuteReader();
            if (int.Parse(p1.Value.ToString()) == 0)
            {
                throw new Exception("failed");
            }
            else
            {
                if (reader.Read())
                {
                    RestfulResult.RestfulData<UserPublicInfo> rr = new RestfulResult.RestfulData<UserPublicInfo>();
                    string[] temp = new string[reader.FieldCount];
                    for (int i = 0; i < reader.FieldCount; ++i)
                    {
                        temp[i] = reader.GetValue(i).ToString();
                    }
                    return int.Parse(reader["message_num"].ToString());
                    }
                    else
                    {
                        throw new Exception("failed");
                    }
                }

            });
        }


        /// <summary>
        /// 上传头像图片的api, 目前不知道前端如何上传，待议
        /// </summary>
        /// <returns>返回是否成功</returns>
        [HttpPost("uploadAvatar")]
        public async Task<IActionResult> UploadAvatar([Required][FromForm]int user_id)
        {
            ////TODO 需要验证登录态
            ////返回成功与否
            //int userId = -1;
            //if (HttpContext.User.Identity.IsAuthenticated)
            //{
            //    //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
            //    //claims键值对都读出来，比如我们刚才定义的UserName的值Wangdacui就在这里读取出来了
            //    userId = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
            //}
            //else
            //{
            //    //TODO
            //    //进入到这部分意味着用户登录态已经失效，需要返回给客户端信息，即需要登录。
            //    RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
            //    rr.Code = 200;
            //    rr.Message = "Need Authentication";
            //    return new JsonResult(rr);
            //}

            using (OracleConnection conn = new OracleConnection(ConnStr.getConnStr()))
            {
                try
                {
                    conn.ConnectionString = ConnStr.getConnStr();
                    conn.Open();
                    RestfulResult.RestfulData rr = new RestfulResult.RestfulData();
                    rr.Code = 200;
                    rr.Message = "success";

                    var imgfile = Request.Form.Files[0];
                    int avatar_id = addAvatarAndGetAvatarID(user_id);
                    Directory.CreateDirectory(@"wwwroot\avatars\" + user_id.ToString());
                    
                    if (imgfile.Length > 0)
                    {
                        var img_path = @"wwwroot\avatars\" + user_id.ToString() + @"\" + avatar_id.ToString() + ".jpg";
                        using (var stream = new FileStream(img_path, FileMode.Create))
                        {
                            await imgfile.CopyToAsync(stream);
                        }
                    }
                    

                    return new JsonResult(rr);
                }
                catch (Exception e)
                {
                    return null;
                }
            }
        }


        public static int addAvatarAndGetAvatarID(int user_id)
        {
            return Wrapper.wrap((OracleConnection conn) =>
            {
                //ADD_AVATAR(userid in INTEGER, avatarid out INTEGER)
                //return INGETER
                string procedureName = "ADD_AVATAR";
                OracleCommand cmd = new OracleCommand(procedureName, conn);
                cmd.CommandType = CommandType.StoredProcedure;
                OracleParameter p1 = new OracleParameter();
                p1 = cmd.Parameters.Add("state", OracleDbType.Int32);
                p1.Direction = ParameterDirection.ReturnValue;

                OracleParameter p2 = new OracleParameter();
                p2 = cmd.Parameters.Add("userid", OracleDbType.Int32);
                p2.Direction = ParameterDirection.Input;
                p2.Value = user_id;

                OracleParameter p3 = new OracleParameter();
                p3 = cmd.Parameters.Add("avatarid", OracleDbType.Int32);
                p3.Direction = ParameterDirection.Output;

                var reader = cmd.ExecuteReader();
                if (int.Parse(p1.Value.ToString()) == 0)
                {
                    throw new Exception("failed");
                }
                else
                {
                    return int.Parse(p3.Value.ToString());
                }

            });
        }

    }
}
