using Bolao.Domain.Entities;
using Bolao.Infrastructure.Persistence;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;

    public AuthController(AppDbContext context)
    {
        _context = context;
    }

    [HttpPost("google")]
    public async Task<IActionResult> GoogleLogin()
    {
        try
        {
            var authHeader = Request.Headers.Authorization.ToString();

            if (string.IsNullOrWhiteSpace(authHeader))
                return Unauthorized("Token não enviado.");

            if (!authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                return Unauthorized("Formato do token inválido.");

            var token = authHeader["Bearer ".Length..].Trim();

            if (string.IsNullOrWhiteSpace(token))
                return Unauthorized("Token vazio.");

            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            var uid = decodedToken.Uid;

            var email = decodedToken.Claims.TryGetValue("email", out var emailObj)
                ? emailObj?.ToString()
                : null;

            var name = decodedToken.Claims.TryGetValue("name", out var nameObj)
                ? nameObj?.ToString()
                : null;

            var picture = decodedToken.Claims.TryGetValue("picture", out var pictureObj)
                ? pictureObj?.ToString()
                : null;

            if (string.IsNullOrWhiteSpace(email))
                return BadRequest("Conta sem e-mail válido.");

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var normalizedName = string.IsNullOrWhiteSpace(name)
                ? normalizedEmail
                : name.Trim();

            var user = await _context.Users
                .FirstOrDefaultAsync(x => x.FirebaseUid == uid);

            if (user is null)
            {
                user = new User(
                    uid,
                    normalizedName,
                    normalizedEmail,
                    picture
                );

                _context.Users.Add(user);
            }
            else
            {
                if (!user.IsActive)
                    return Unauthorized("Usuário inativo.");

                user.UpdateProfile(normalizedName, picture);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                user.Id,
                user.FirebaseUid,
                user.Name,
                user.Email,
                user.PhotoUrl,
                user.IsActive
            });
        }
        catch (FirebaseAuthException)
        {
            return Unauthorized("Token Firebase inválido.");
        }
        catch
        {
            return StatusCode(500, "Erro interno ao autenticar usuário.");
        }
    }
}