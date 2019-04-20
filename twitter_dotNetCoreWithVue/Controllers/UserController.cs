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

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace twitter_dotNetCoreWithVue.Controllers
{

    /// <summary>
    /// User controller.
    /// 此控制器定义注册，登录，个人信息的增删改查等操作api接口
    /// </summary>
    [Route("api/[controller]")]
    public class UserController : Controller
    {
        //用于注册时的数据模型
        public class UserInfoForSignUp {
            public string email { get; set; }
            public string password { get; set; }
            public string nickname { get; set; }
        }

        //用于登录时的数据模型
        public class UserInfoForSignIn { 
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
        }
        /// <summary>
        /// 此接口在注册时使用。
        /// 使用POST方法，传递邮箱，密码，昵称即可，其他用户信息在个人界面处修改和添加。
        /// </summary>
        /// <returns>是否成功</returns>
        /// <param name="userInfoForSignUp">注册时需要的信息</param>
        [HttpPost("signUp")]
        public IActionResult SignUp([FromBody]UserInfoForSignUp userInfoForSignUp) {
            //TODO 注册啦
            //返回是否注册成功
            return new JsonResult(new { });
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
            int user_id = 1; //假装自己get到了
            var claims = new[] {
                new Claim("user_id", user_id.ToString()),
                new Claim("email", userInfoForSignIn.email),
                new Claim("password", userInfoForSignIn.password)
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal user = new ClaimsPrincipal(claimsIdentity);
            //signin 在内部实际上是在设置cookies
            HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, user).Wait();
            //可以使用HttpContext.SignInAsync方法的重载来定义持久化cookie存储用户认证信息，例如下面的代码就定义了用户登录后60分钟内cookie都会保留在客户端计算机硬盘上，
            //即便用户关闭了浏览器，60分钟内再次访问站点仍然是处于登录状态，除非调用Logout方法注销登录。
            HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            user, new AuthenticationProperties() { IsPersistent = true, ExpiresUtc = DateTimeOffset.Now.AddMinutes(60) }).Wait();
            //TODO 我们需要做的将用户的id返回给客户端
            return new JsonResult(new { });
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
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
                //claims键值对都读出来，比如我们刚才定义的UserName的值Wangdacui就在这里读取出来了
                var id = int.Parse(HttpContext.User.Claims.ElementAt(0).Value);
                var email = HttpContext.User.Claims.ElementAt(1).Value;
                var password = HttpContext.User.Claims.ElementAt(2).Value;
                return new JsonResult(new { });
            }
            else
            {
                //TODO
                //进入到这部分意味着用户登录态已经失效，需要返回给客户端信息，即需要登录。
                return new JsonResult(new { });
            }
        }

        /// <summary>
        /// 设置当前使用哪一个头像作为主要头像
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="avatar_id">用户的头像图片id</param>
        [HttpGet("setAvatar")]
        public IActionResult ChangeAvatar([Required]int avatar_id)
        {
            if (HttpContext.User.Identity.IsAuthenticated)
            {
                //这里通过 HttpContext.User.Claims 可以将我们在Login这个Action中存储到cookie中的所有
                //claims键值对都读出来
                var userName = HttpContext.User.Claims.First().Value;
                return new JsonResult(new { });
            }
            //TODO 
            return new JsonResult(new { });
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
            return new JsonResult(new { src = "avatarUrl" });
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
            return new JsonResult(new { });
        }

        /// <summary>
        /// 查看某个人的可公开信息
        /// </summary>
        /// <returns>User_Public_Info的实例</returns>
        /// <param name="user_id">User identifier.</param>
        [HttpGet("query/{user_id}")]
        public IActionResult QueryUser([Required]int user_id)
        {
            //TODO 查询可公开信息
            //返回含有列表的Json对象
            return new JsonResult(new { });
        }


        /// <summary>
        /// 上传头像图片的api, 目前不知道前端如何上传，待议
        /// </summary>
        /// <returns>返回是否成功</returns>
        [HttpPost("uploadAvatar")]
        public IActionResult UploadAvatar()
        {
            //TODO 需要验证登录态
            //返回成功与否
            return new JsonResult(new { });
        }

    }
}
