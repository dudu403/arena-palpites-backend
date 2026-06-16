using System.Security.Claims;
using System.Text.Encodings.Web;
using FirebaseAdmin.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Bolao.Api.Authentication;

public class FirebaseAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public FirebaseAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authorizationHeader = Request.Headers.Authorization.ToString();

        if (string.IsNullOrWhiteSpace(authorizationHeader))
            return AuthenticateResult.NoResult();

        if (!authorizationHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Formato do token inválido.");

        var token = authorizationHeader["Bearer ".Length..].Trim();

        if (string.IsNullOrWhiteSpace(token))
            return AuthenticateResult.Fail("Token vazio.");

        try
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, decodedToken.Uid),
                new("firebase_uid", decodedToken.Uid)
            };

            if (decodedToken.Claims.TryGetValue("email", out var email) && email is not null)
                claims.Add(new Claim(ClaimTypes.Email, email.ToString()!));

            if (decodedToken.Claims.TryGetValue("name", out var name) && name is not null)
                claims.Add(new Claim(ClaimTypes.Name, name.ToString()!));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
        catch (FirebaseAuthException ex)
        {
            Logger.LogWarning(ex, "Falha ao validar token Firebase.");
            return AuthenticateResult.Fail("Token Firebase inválido.");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Erro inesperado ao autenticar usuário.");
            return AuthenticateResult.Fail("Erro de autenticação.");
        }
    }
}