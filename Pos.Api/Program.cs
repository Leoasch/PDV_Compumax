using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Pos.Api.Middlewares;
using Pos.Api.Servicos;
using Pos.Domain;
using Pos.Infrastructure.Dados;
using Pos.Infrastructure.Dados.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers()
    .AddJsonOptions(opcoes => opcoes.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorGlobalDeExcecoes>();

const string politicaCorsPadrao = "PadraoPos";
var origensPermitidas = builder.Configuration.GetSection("Cors:OrigensPermitidas").Get<string[]>() ?? [];

builder.Services.AddCors(opcoes =>
{
    opcoes.AddPolicy(politicaCorsPadrao, politica =>
    {
        politica.WithOrigins(origensPermitidas)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

const string politicaLoginRigorosa = "LoginRigoroso";

builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.AddPolicy(politicaLoginRigorosa, contexto =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    opcoes.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(contexto =>
    {
        var chave = contexto.User.Identity?.IsAuthenticated == true
            ? contexto.User.FindFirstValue(ClaimTypes.NameIdentifier)!
            : contexto.Connection.RemoteIpAddress?.ToString() ?? "desconhecido";

        return RateLimitPartition.GetFixedWindowLimiter(chave, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    });

    opcoes.OnRejected = async (contexto, cancellationToken) =>
    {
        contexto.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

        var problemDetailsService = contexto.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = contexto.HttpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Title = "Muitas tentativas.",
                Detail = "Aguarde um pouco antes de tentar novamente."
            }
        });
    };
});

var cadeiaDeConexao = builder.Configuration.GetConnectionString("BancoDados")
    ?? throw new InvalidOperationException("A conexão 'BancoDados' não foi configurada.");

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "dados"));

builder.Services.AdicionarPersistenciaSqlite(cadeiaDeConexao);

builder.Services.AddSingleton<ServicoToken>();
builder.Services.AddSingleton<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

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

app.UseExceptionHandler();

using (var escopo = app.Services.CreateScope())
{
    var contextoBancoDados = escopo.ServiceProvider.GetRequiredService<ContextoBancoDados>();
    await contextoBancoDados.Database.MigrateAsync();
    await ProdutoSeeder.SemearAsync(contextoBancoDados);
    await UsuarioSeeder.SemearAsync(contextoBancoDados, escopo.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>());
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseCors(politicaCorsPadrao);

app.UseAuthentication();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
