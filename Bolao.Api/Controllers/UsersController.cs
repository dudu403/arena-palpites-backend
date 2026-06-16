using Bolao.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var firebaseUid = User.FindFirst("firebase_uid")?.Value;

        if (string.IsNullOrWhiteSpace(firebaseUid))
            return Unauthorized();

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid);

        if (user is null)
            return NotFound("Usuário não encontrado.");

        if (!user.IsActive)
            return Unauthorized("Usuário inativo.");

        return Ok(new
        {
            user.Id,
            user.FirebaseUid,
            user.Name,
            user.Email,
            user.PhotoUrl,
            user.IsActive,
            user.CreatedAt,
            user.UpdatedAt
        });
    }
}