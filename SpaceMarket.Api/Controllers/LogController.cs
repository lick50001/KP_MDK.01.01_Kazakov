using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceMarket.Api.Context;
using SpaceMarket.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpaceMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly SpaceMarketContext _context;
        public LogController(SpaceMarketContext context) { _context = context; }

        [HttpGet("Get")]
        public async Task<ActionResult> GetMyLogs([FromQuery] string token)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var logs = await _context.Logs
                .Where(i => i.UserId == usId)
                .Select(i => new {
                    Id = i.Log_Id,
                    Название = i.LogType,
                    Цена = i.Message,
                    Дата = i.EventTime
                })
                .ToListAsync();

            return Ok(logs);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> AddMyLogs([FromQuery] string token, [FromForm] string logType, [FromForm] string message, [FromForm] DateTime eventTime)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var newLog = new Logs
            {
                LogType = logType,
                Message = message,
                EventTime = eventTime,
                UserId = usId,
            };
            _context.Logs.Add(newLog);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [HttpPut("Edit")]
        public async Task<ActionResult> EditMyLogs([FromQuery] string token, [FromForm] int logid, [FromForm] string logType, [FromForm] string message, [FromForm] DateTime eventTime)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var bdLogs = await _context.Logs.FirstOrDefaultAsync(x => x.Log_Id == logid && x.UserId == usId);

            if (bdLogs == null)
                return NotFound("Лог не найден");

            bdLogs.LogType = logType;
            bdLogs.Message = message;
            bdLogs.EventTime = eventTime;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteMyLogs([FromQuery] string token, [FromForm] int logid)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var dbLogs = await _context.Logs.FirstOrDefaultAsync(x => x.Log_Id == logid && x.UserId == usId);

            if (dbLogs == null)
                return NotFound("Предмет не найден");

            _context.Remove(dbLogs);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [NonAction]
        public string VerifyToken(string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userId = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier || x.Type == "nameid")?.Value;
            return userId;
        }
    }
}
