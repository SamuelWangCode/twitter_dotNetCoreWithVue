using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using twitter_dotNetCoreWithVue.Models;
using Oracle.ManagedDataAccess.Client;
using System.ComponentModel.DataAnnotations;

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
        /// <returns>JsonResult</returns>
        /// <param name="userInfoForSignUp">注册时需要的信息</param>
        [HttpPost("signUp")]
        public IActionResult SignUp([FromBody]UserInfoForSignUp userInfoForSignUp) {
            //TODO
            return Ok();
        }

        /// <summary>
        /// 此接口用于登录
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="userInfoForSignIn">登录时需要的信息</param>
        [HttpPost("signIn")]
        public IActionResult SignIn([FromBody]UserInfoForSignIn userInfoForSignIn)
        {
            //TODO
            return Ok();
        }

        /// <summary>
        /// 此接口用于编辑个人信息界面
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="userInfoEdit">用户可以被直接修改的信息</param>
        /// <param name="user_id">User id</param>
        [HttpPost("editInfo/{user_id}")]
        public IActionResult EditInfo(int user_id, [FromBody]UserInfoEdit userInfoEdit)
        {
            //TODO
            return Ok();
        }

        /// <summary>
        /// 设置当前使用哪一个头像作为主要头像
        /// </summary>
        /// <returns>success or not</returns>
        /// <param name="avatar_id">用户的头像图片id</param>
        [HttpGet("setAvatar/{user_id}")]
        public IActionResult ChangeAvatar([Required]int avatar_id)
        {
            //TODO
            return Ok();
        }

        //TODO upload avatar
        //TODO query public info

    }
}
