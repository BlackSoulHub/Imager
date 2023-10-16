using Imager.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Imager.Web.Controllers;

[Microsoft.AspNetCore.Components.Route("test")]
public class TestController : BaseController
{
    private readonly ApplicationDbContext _dbContext;

    public TestController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("get-all-users")]
    public async Task<IActionResult> GetAllUserList()
    {
        var usersList = await _dbContext.Users
            .Include(u => u.Images)
            .Include(u => u.Friends)
            .ThenInclude(f => f.List)
            .ToListAsync();
        return Ok(usersList);
    }
}