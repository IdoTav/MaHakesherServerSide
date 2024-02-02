using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MaHakesherServerSide.Data;
using MaHakesherServerSide.Models;
using MaHakesherServerSide.JsonClasses;

namespace MaHakesherServerSide.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UsersController : Controller
    {
        private readonly MaHakesherServerSideContext _context;

        public UsersController(MaHakesherServerSideContext context)
        {
            _context = context;
        }


        [HttpPost]
        [ActionName("login")]
        public IActionResult login([Bind("UserName, Password")] UserJson user)
        {
            int x = 7;
            return Ok();
        }

        private bool checkIfUserExists(string userName)
        {
            var existsUserName =  _context.User.Where(m => m.UserName == userName);
            if(existsUserName.Any())
            {
                return true;
            }
            return false;

        }

        [HttpPost]
        [ActionName("register")]
        public IActionResult register([Bind("UserName, Password")] UserJson user)
        {
            if(checkIfUserExists(user.UserName))
            {
                return BadRequest();
            }
            User newUser = new User(user.UserName, user.Password);
            _context.Add(newUser);
            _context.SaveChanges();
            return Ok();

        }
    }
}
