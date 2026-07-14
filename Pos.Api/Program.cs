using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pos.Api.Servicos;
using Pos.Infrastructure.Dados;
using Pos.Infrastructure.Dados.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(opcoes => opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

var cadeiaDeConexao = builder.Configuration.GetConnectionString("BancoDados")
    ?? throw new InvalidOperationException("A conexão 'BancoDados' não foi configurada.");

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "dados"));

builder.Services.AdicionarPersistenciaSqlite(cadeiaDeConexao);

builder.Services.AddSingleton<ServicoToken>();

var chaveSecreta = builder.Configuration["Jwt:ChaveSecreta"]
    ?? throw new InvalidOperationException("A chave 'Jwt:ChaveSecreta' não foi configurada.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opcoes =>
    {
        opcoes.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Emissor"],
            ValidAudience = builder.Configuration["Jwt:Audiencia"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(chaveSecreta))
        };

        opcoes.Events = new JwtBearerEvents
        {
            OnTokenValidated = async contexto =>
            {
                var idClaim = contexto.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
                if (idClaim is null || !int.TryParse(idClaim, out var usuarioId))
                {
                    contexto.Fail("Token inválido.");
                    return;
                }

                var contextoBancoDados = contexto.HttpContext.RequestServices.GetRequiredService<ContextoBancoDados>();
                var usuario = await contextoBancoDados.Usuarios.FindAsync(usuarioId);

                if (usuario is null || !usuario.Ativo)
                {
                    contexto.Fail("Usuário inativo ou removido.");
                    return;
                }

                var identity = (ClaimsIdentity)contexto.Principal!.Identity!;
                var claimPapelAntiga = identity.FindFirst(ClaimTypes.Role);
                if (claimPapelAntiga is not null)
                {
                    identity.RemoveClaim(claimPapelAntiga);
                }

                identity.AddClaim(new Claim(ClaimTypes.Role, usuario.Papel.ToString()));
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var contextoBancoDados = escopo.ServiceProvider.GetRequiredService<ContextoBancoDados>();
    await contextoBancoDados.Database.MigrateAsync();
    await ProdutoSeeder.SemearAsync(contextoBancoDados);
    await UsuarioSeeder.SemearAsync(contextoBancoDados);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
