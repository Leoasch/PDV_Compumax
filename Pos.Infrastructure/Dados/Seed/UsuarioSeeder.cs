using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Pos.Domain;

namespace Pos.Infrastructure.Dados.Seed;

/// <summary>
/// Garante a existência de um usuário administrador padrão.
/// </summary>
public static class UsuarioSeeder
{
    public const string NomeUsuarioAdminPadrao = "admin";
    public const string SenhaAdminPadrao = "admin123";

    public static async Task SemearAsync(ContextoBancoDados contexto, IPasswordHasher<Usuario> hasher)
    {
        if (await contexto.Usuarios.AnyAsync())
        {
            return;
        }

        var admin = new Usuario
        {
            NomeUsuario = NomeUsuarioAdminPadrao,
            NomeCompleto = "Administrador",
            Papel = PapelUsuario.Admin
        };

        admin.SenhaHash = hasher.HashPassword(admin, SenhaAdminPadrao);

        contexto.Usuarios.Add(admin);
        await contexto.SaveChangesAsync();
    }
}
