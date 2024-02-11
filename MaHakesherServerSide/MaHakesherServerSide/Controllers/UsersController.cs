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
        private readonly MaHakesherServerSideContext _context;
        private readonly MySqlConnection _connection;

        public UsersController(MaHakesherServerSideContext context, MySqlConnection connection)
        {
            _context = context;
            _connection = connection;
        }

        private async Task<bool> checkIfUserExistsAsync(string userName)
        {
            // An example for how to use MySQL server
/*            try
            {
                await _connection.OpenAsync();

                using var command = new MySqlCommand("SELECT * FROM mahakesher.book;", _connection);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var value = reader.GetValue(0);
                    // do something with 'value'
                }
            }
            catch(Exception ex)
            {
                Console.Write(ex.Message);
            }*/

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
    }
}
