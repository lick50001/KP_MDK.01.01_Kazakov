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
        public ItemsController(SpaceMarketContext context) {_context = context;}

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Items>>> GetMyItems([FromQuery] string token)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(token);

            var userId = jwt.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier || x.Type == "nameid")?.Value;
            if (userId == null)
                return Unauthorized("Невалидный токен");

            int usId = int.Parse(userId);

            var items = await _context.Items
                .Where(i => i.UserId == usId && i.IsActive == true)
                .Select(i => new {
                    Id = i.Item_Id,
                    Название = i.ItemName,
                    Цена = i.MaxBuyPrice,
                    Активен = i.IsActive
                })
                .ToListAsync();

            return Ok(items);
        } 
    }
}
