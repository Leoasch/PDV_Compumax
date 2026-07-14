using Microsoft.EntityFrameworkCore;
using Pos.Infrastructure.Dados;
using Pos.Infrastructure.Dados.Seed;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();

var cadeiaDeConexao = builder.Configuration.GetConnectionString("BancoDados")
    ?? throw new InvalidOperationException("A conexão 'BancoDados' não foi configurada.");

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "dados"));

builder.Services.AdicionarPersistenciaSqlite(cadeiaDeConexao);

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var contextoBancoDados = escopo.ServiceProvider.GetRequiredService<ContextoBancoDados>();
    await contextoBancoDados.Database.MigrateAsync();
    await ProdutoSeeder.SemearAsync(contextoBancoDados);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
