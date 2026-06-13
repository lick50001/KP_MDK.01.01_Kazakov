using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceMarket.Api.Classes;
using SpaceMarket.Api.Context;
using SpaceMarket.Api.Models;
using System.Security.Claims;

namespace SpaceMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly SpaceMarketContext _context;
        public UsersController(SpaceMarketContext context) { _context = context; }

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
                LevelRoot = Level,
                IsBanned = false
            };

            _context.Users.Add(newUs);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
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

            if (user.IsBanned)
                return Unauthorized("Ошибка: Ваш аккаунт заблокирован!");

            string token = JwtToken.Generate(user);
            return Ok(new
            {
                token = token,
                userId = user.UserId,
                userName = user.UserName,
                levelRoot = user.LevelRoot
            });
        }

        [HttpGet("GetCurrent")]
        public async Task<ActionResult<Users>> GetCurrentUser([FromQuery] string token)
        {
            var principal = JwtToken.ValidateToken(token);
            if (principal == null)
                return Unauthorized("Неверный токен");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest("Не удалось определить пользователя");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound("Пользователь не найден");

            user.PwdHash = null;
            return Ok(user);
        }

        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll([FromQuery] string token)
        {
            var principal = JwtToken.ValidateToken(token);
            if (principal == null) return Unauthorized("Неверный токен");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out int currentUserId)) return BadRequest();

            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null || currentUser.LevelRoot?.ToLower() != "admin")
                return Forbid();

            var users = await _context.Users
                .Where(u => u.UserId != currentUserId)
                .Select(u => new {
                    u.UserId,
                    u.UserName,
                    u.LevelRoot,
                    u.IsBanned
                })
                .ToListAsync();

            return Ok(users);
        }

        [HttpPut("SetBan")]
        public async Task<IActionResult> SetBan([FromQuery] string token, [FromForm] int userId, [FromForm] bool isBanned)
        {
            var principal = JwtToken.ValidateToken(token);
            if (principal == null) return Unauthorized("Неверный токен");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out int currentUserId)) return BadRequest();

            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null || currentUser.LevelRoot?.ToLower() != "admin")
                return Forbid();

            var target = await _context.Users.FindAsync(userId);
            if (target == null) return NotFound("Пользователь не найден");
            if (target.LevelRoot?.ToLower() == "admin") return BadRequest("Нельзя банить администратора");

            target.IsBanned = isBanned;
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("Delete")]
        public async Task<IActionResult> DeleteUser([FromQuery] string token, [FromForm] int userId)
        {
            var principal = JwtToken.ValidateToken(token);
            if (principal == null) return Unauthorized("Неверный токен");

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim?.Value, out int currentUserId)) return BadRequest();

            var currentUser = await _context.Users.FindAsync(currentUserId);
            if (currentUser == null || currentUser.LevelRoot?.ToLower() != "admin")
                return Forbid();

            var target = await _context.Users.FindAsync(userId);
            if (target == null) return NotFound("Пользователь не найден");
            if (target.LevelRoot?.ToLower() == "admin") return BadRequest("Нельзя удалить администратора");

            _context.Users.Remove(target);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}