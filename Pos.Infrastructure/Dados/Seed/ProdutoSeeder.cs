using Microsoft.EntityFrameworkCore;
using Pos.Domain;

namespace Pos.Infrastructure.Dados.Seed;

/// <summary>
/// Popula a tabela de produtos com itens comuns de mercado, caso ela esteja vazia.
/// </summary>
public static class ProdutoSeeder
{
    public static async Task SemearAsync(ContextoBancoDados contexto)
    {
        if (await contexto.Produtos.AnyAsync())
        {
            return;
        }

        contexto.Produtos.AddRange(
            new Produto { Codigo = "0001", Descricao = "Arroz Branco 5kg", PrecoCusto = 18.50m, PrecoVenda = 24.90m, QuantidadeEstoque = 80 },
            new Produto { Codigo = "0002", Descricao = "Feijão Carioca 1kg", PrecoCusto = 6.80m, PrecoVenda = 8.99m, QuantidadeEstoque = 100 },
            new Produto { Codigo = "0003", Descricao = "Açúcar Cristal 1kg", PrecoCusto = 3.20m, PrecoVenda = 4.49m, QuantidadeEstoque = 120 },
            new Produto { Codigo = "0004", Descricao = "Óleo de Soja 900ml", PrecoCusto = 5.90m, PrecoVenda = 7.99m, QuantidadeEstoque = 90 },
            new Produto { Codigo = "0005", Descricao = "Café Torrado e Moído 500g", PrecoCusto = 12.00m, PrecoVenda = 16.90m, QuantidadeEstoque = 60 },
            new Produto { Codigo = "0006", Descricao = "Leite Integral 1L", PrecoCusto = 4.20m, PrecoVenda = 5.49m, QuantidadeEstoque = 150 },
            new Produto { Codigo = "0007", Descricao = "Macarrão Espaguete 500g", PrecoCusto = 3.50m, PrecoVenda = 4.99m, QuantidadeEstoque = 100 },
            new Produto { Codigo = "0008", Descricao = "Molho de Tomate 340g", PrecoCusto = 2.80m, PrecoVenda = 3.99m, QuantidadeEstoque = 110 },
            new Produto { Codigo = "0009", Descricao = "Sal Refinado 1kg", PrecoCusto = 1.50m, PrecoVenda = 2.29m, QuantidadeEstoque = 130 },
            new Produto { Codigo = "0010", Descricao = "Farinha de Trigo 1kg", PrecoCusto = 4.00m, PrecoVenda = 5.49m, QuantidadeEstoque = 95 },
            new Produto { Codigo = "0011", Descricao = "Manteiga 200g", PrecoCusto = 8.50m, PrecoVenda = 11.90m, QuantidadeEstoque = 50 },
            new Produto { Codigo = "0012", Descricao = "Papel Higiênico 4 rolos", PrecoCusto = 6.90m, PrecoVenda = 9.49m, QuantidadeEstoque = 70 },
            new Produto { Codigo = "0013", Descricao = "Detergente Líquido 500ml", PrecoCusto = 1.90m, PrecoVenda = 2.79m, QuantidadeEstoque = 140 },
            new Produto { Codigo = "0014", Descricao = "Sabão em Pó 1kg", PrecoCusto = 9.50m, PrecoVenda = 13.90m, QuantidadeEstoque = 65 },
            new Produto { Codigo = "0015", Descricao = "Refrigerante Cola 2L", PrecoCusto = 6.00m, PrecoVenda = 8.49m, QuantidadeEstoque = 85 },
            new Produto { Codigo = "0016", Descricao = "Cerveja Pilsen Lata 350ml", PrecoCusto = 2.80m, PrecoVenda = 4.29m, QuantidadeEstoque = 200 },
            new Produto { Codigo = "0017", Descricao = "Água Mineral 1,5L", PrecoCusto = 1.80m, PrecoVenda = 2.99m, QuantidadeEstoque = 160 },
            new Produto { Codigo = "0018", Descricao = "Biscoito Recheado 130g", PrecoCusto = 2.20m, PrecoVenda = 3.29m, QuantidadeEstoque = 120 },
            new Produto { Codigo = "0019", Descricao = "Pão de Forma 500g", PrecoCusto = 5.50m, PrecoVenda = 7.99m, QuantidadeEstoque = 40 },
            new Produto { Codigo = "0020", Descricao = "Ovos Brancos Dúzia", PrecoCusto = 9.00m, PrecoVenda = 12.90m, QuantidadeEstoque = 55 }
        );

        await contexto.SaveChangesAsync();
    }
}
