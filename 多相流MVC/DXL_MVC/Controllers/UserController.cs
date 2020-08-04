using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DXL_MVC.Models;
using DXL_MVC.SQL;
using Microsoft.AspNetCore.Mvc;

namespace DXL_MVC.Controllers
{
    public class UserController : Controller
    {
        [HttpGet]
        public IActionResult Login()
        {

            ViewData["login_info"] = "";


            return View();
        }

        [HttpPost]
        public IActionResult Login(string usrname,string usrpw)
        {
            string sql = "select * from userlist where userid='"+usrname+"' and password2='"+ usrpw + "'";

            User usr = OracleHelper.ReturnFirstObject<User>(sql);






            if(usrname=="abc")
            {

                ViewData["login_info"] = "登陆成功";
            }
            else
                ViewData["login_info"] = "登陆失败";


            return View();
        }
    }
}
