using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SpaceMarket.Api.Classes;
using SpaceMarket.Api.Context;
using SpaceMarket.Api.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace SpaceMarket.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ItemsController : ControllerBase
    {
        private readonly SpaceMarketContext _context;
        public ItemsController(SpaceMarketContext context) { _context = context; }

        [HttpGet("Get")]
        public async Task<ActionResult<IEnumerable<Items>>> GetMyItems([FromQuery] string token)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var items = await _context.Items
                .Where(i => i.UserId == usId && i.IsActive == true)
                .Select(i => new {
                    Item_Id = i.Item_Id,
                    ItemName = i.ItemName,
                    MaxBuyPrice = i.MaxBuyPrice,
                    IsActive = i.IsActive
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpPost("Add")]
        public async Task<ActionResult> AddMyItem([FromQuery] string token, [FromForm] string itemname, [FromForm] int maxBuyprice, [FromForm] bool isactive)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var newItem = new Items
            {
                ItemName = itemname,
                MaxBuyPrice = maxBuyprice,
                IsActive = isactive,
                UserId = usId,
            };
            _context.Items.Add(newItem);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [HttpPut("Edit")]
        public async Task<ActionResult> EditMyItem([FromQuery] string token, [FromForm] int itemId, [FromForm] string itemname, [FromForm] int maxBuyprice, [FromForm] bool isactive)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var bditem = await _context.Items.FirstOrDefaultAsync(x => x.Item_Id == itemId && x.UserId == usId);

            if (bditem == null)
                return NotFound("Предмет не найден");

            bditem.ItemName = itemname;
            bditem.MaxBuyPrice = maxBuyprice;
            bditem.IsActive = isactive;

            await _context.SaveChangesAsync();
            return Ok(new { message = "Успешно!" });
        }

        [HttpDelete("Delete")]
        public async Task<ActionResult> DeleteMyItem([FromQuery] string token, [FromForm] int itemId)
        {
            var userIdStr = VerifyToken(token);
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized("Невалидный токен");
            int usId = int.Parse(userIdStr);

            var bditem = await _context.Items.FirstOrDefaultAsync(x => x.Item_Id == itemId && x.UserId == usId);

            if (bditem == null)
                return NotFound("Предмет не найден");

            _context.Remove(bditem);
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
