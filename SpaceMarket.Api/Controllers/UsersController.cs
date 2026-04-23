using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SpaceMarket.Api.Classes;
using SpaceMarket.Api.Context;
using SpaceMarket.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SpaceMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SpaceMarketContext _context;
        public UsersController(SpaceMarketContext context) {_context = context;}

        [HttpPost("Register")]
        public async Task<ActionResult<Users>> Register([FromForm] string Usname, [FromForm] string Password, [FromForm] string Level)
        {
            if (await _context.Users.AnyAsync(u => u.UserName == Usname))
                return BadRequest("Ошибка: Такой юзер уже есть в базе!");

            string PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password);

            var newUs = new Users
            {
                UserName = Usname,
                PwdHash = PasswordHash,
                LevelRoot = Level
            };

            _context.Users.Add(newUs);
            await _context.SaveChangesAsync();
            return Ok(new {message = "Успешно!"});
        }

        [HttpPost("Login")]
        public async Task<ActionResult<Users>> Login([FromForm] string Usname, [FromForm] string Password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == Usname);

            if (user == null)
                return Unauthorized("Ошибка: Неверный логин или пароль!");

            bool Verify = BCrypt.Net.BCrypt.Verify(Password, user.PwdHash);
            if (!Verify)
                return Unauthorized("Ошибка: Неверный логин или пароль!");

            string token = JwtToken.Generate(user);
            return Ok(new { token = token });
        }
    }
}
