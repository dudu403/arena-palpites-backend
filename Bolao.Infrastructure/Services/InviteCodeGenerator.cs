using System.Security.Cryptography;
using Bolao.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Bolao.Infrastructure.Services;

public sealed class InviteCodeGenerator : IInviteCodeGenerator
{
    private readonly IApplicationDbContext _context;

    public InviteCodeGenerator(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default)
    {
        for (var attempt = 0; attempt < 10; attempt++)
        {
            var code = GenerateCode();

            var exists = await _context.Boloes
                .AsNoTracking()
                .AnyAsync(x => x.InviteCode == code, cancellationToken);

            if (!exists)
                return code;
        }

        throw new InvalidOperationException("Não foi possível gerar um código único para o bolão.");
    }

    private static string GenerateCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";

        Span<char> code = stackalloc char[6];

        for (var i = 0; i < code.Length; i++)
        {
            var index = RandomNumberGenerator.GetInt32(chars.Length);
            code[i] = chars[index];
        }

        return new string(code);
    }
}