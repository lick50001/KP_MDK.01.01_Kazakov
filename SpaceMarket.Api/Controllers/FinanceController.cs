using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceMarket.Api.Context;
using SpaceMarket.Api.Models;

namespace SpaceMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FinanceController : ControllerBase
    {
        private readonly SpaceMarketContext _context;
        public FinanceController(SpaceMarketContext context) { _context = context; }

        [HttpGet("Get")]
        public async Task<ActionResult> GetMyFinance([FromQuery] string token)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var fins = await _context.Finance
                .Where(i => i.UserId == usId)
                .Select(i => new {
                    Finance_Id = i.Finance_Id,
                    FinanceType = i.FinanceType,
                    Message = i.Message,
                    EventTime = i.EventTime,
                    UserId = i.UserId
                })
                .ToListAsync();

            return Ok(fins);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> AddMyFinance([FromQuery] string token, [FromForm] string finType, [FromForm] string message, [FromForm] DateTime eventTime)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var newFin = new Finance
            {
                FinanceType = finType,
                Message = message,
                EventTime = eventTime,
                UserId = usId,
            };
            _context.Finance.Add(newFin);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [HttpPut("Edit")]
        public async Task<ActionResult> EditMyFinance([FromQuery] string token, [FromForm] int finid, [FromForm] string finType, [FromForm] string message, [FromForm] DateTime eventTime)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var bdFins = await _context.Finance.FirstOrDefaultAsync(x => x.Finance_Id == finid && x.UserId == usId);

            if (bdFins == null)
                return NotFound("Лог не найден");

            bdFins.FinanceType = finType;
            bdFins.Message = message;
            bdFins.EventTime = eventTime;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteMyFinance([FromQuery] string token, [FromForm] int finid)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var dbFinance = await _context.Finance.FirstOrDefaultAsync(x => x.Finance_Id == finid && x.UserId == usId);

            if (dbFinance == null)
                return NotFound("Предмет не найден");

            _context.Remove(dbFinance);
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
