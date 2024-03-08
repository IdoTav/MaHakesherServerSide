using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using MaHakesherServerSide.Data;
using MaHakesherServerSide.Models;
using MaHakesherServerSide.JsonClasses;
using MySqlConnector;
using System.Linq.Expressions;

namespace MaHakesherServerSide.Controllers
{ 
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class UsersController : Controller
    {
        private readonly string PASSWORD = "123456";
        private readonly bool USERS_ALREADY_GENERATED = false;


        private readonly MaHakesherServerSideContext _context;
        private readonly MySqlConnection _connection;

        public UsersController(MaHakesherServerSideContext context, MySqlConnection connection)
        {
            _context = context;
            _connection = connection;
        }

        private async Task<bool> checkIfUserExistsAsync(string userName)
        {
            var existsUserName = _context.User.Where(m => m.UserName == userName);
            if (existsUserName.Any())
            {
                return true;
            }
            return false;

        }

        [HttpPost]
        [ActionName("login")]
        public async Task<IActionResult> loginAsync([Bind("UserName, Password")] UserJson user)
        {
            if (!USERS_ALREADY_GENERATED)
            {
                try {
                    await GenerateUsers();
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                
            }
            if(! await checkIfUserExistsAsync(user.UserName))
            {
                return NotFound();
            }
            var isUserNameAndPasswordCorrect = _context.User.Where(m => m.UserName == user.UserName && m.Password == user.Password);
            if(isUserNameAndPasswordCorrect.Any())
            {
                return Ok();
            }
            return BadRequest();
        }
        
        [HttpGet]
        [ActionName("history")]
        public async Task<IActionResult> getHistory([FromQuery] string userName)
        {
            var history = _context.Relations.Find(userName);
            if (history != null)
            {
                return Ok(history.History);
            }
            return NotFound();
        }

        [HttpPost]
        [ActionName("history")]
        public async Task<IActionResult> history([Bind("UserName, History")] Relations relation)
        {
            var history = _context.Relations.Find(relation.UserName);
            if(history != null)
            {
                history.History = history.History + "$$" + relation.History;
                _context.SaveChanges();
                return Ok(200);
            }
            Relations relations = new Relations(relation.UserName, relation.History);
            _context.Add(relations);
            _context.SaveChanges();
            return Ok(200);
        }
        
  

        [HttpPost]
        [ActionName("register")]
        public async Task<IActionResult> registerAsync([Bind("UserName, Password")] UserJson user)
        {
            if(await checkIfUserExistsAsync(user.UserName))
            {
                return BadRequest();
            }
            User newUser = new User(user.UserName, user.Password);
            _context.Add(newUser);
            _context.SaveChanges();
            return Ok();

        }

        private async Task GenerateUsers()
        {
            List<string> names = new List<string>
            {
                "Avigail", "Ariel", "Batya", "Daniel", "Hannah", "Itamar", "Maya", "Noam", "Rachel", "Shimon",
                "Yael", "Eitan", "Tamar", "Oren", "Ruth", "Yitzhak", "Sara", "Aviv", "Leah", "Eli",
                "Shira", "Yosef", "Miri", "Adam", "Dina", "Amir", "Rivka", "Yonatan", "Orli", "Ariella",
                "Moshe", "Tali", "Yehuda", "Yifat", "Uri", "Gila", "Ezra", "Nava", "Ephraim", "Yona",
                "Tzipporah", "Matan", "Yochanan", "Yaeli", "Netanel", "Penina", "Eliezer", "Zahava", "Nathan", "Tova",
                "Yair", "Esther", "Oded", "Avital", "Boaz", "Ilana", "Elad", "Chava", "Ido", "Shoshana",
                "Yarden", "Dov", "Nechama", "Yaniv", "Aviva", "Gideon", "Rina", "Yoav", "Eden", "Chaim",
                "Orit", "Ephrat", "Mordechai", "Talia", "Haim", "Linoy", "Shlomo", "Vered", "Lior", "Ziva",
                "Erez", "Adina", "Zvi", "Nina", "Yotam", "Yaelle", "Nimrod", "Ayelet", "Israel", "Atara",
                "Yehudit", "Meir", "Tzipora", "Shai", "Yiska", "Hadar", "Ilan", "Tal", "Eliran", "Tirza"
            };
            names.ForEach((name) =>
            {
                User newUser = new User(name, PASSWORD);
                try {
                    _context.Add(newUser);
                }
                catch (Exception ex)
                {
                    int a = 5;
                }

            });
            await _context.SaveChangesAsync();
            return;
        }
    }
}
