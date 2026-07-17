using Pos.Shared;

namespace Pos.Client.Servicos;

public static class FormaPagamentoExtensoes
{
    public static string ObterLabel(this FormaPagamento formaPagamento) => formaPagamento switch
    {
        FormaPagamento.Dinheiro => "Dinheiro",
        FormaPagamento.CartaoCredito => "Cartão de crédito",
        FormaPagamento.CartaoDebito => "Cartão de débito",
        FormaPagamento.Pix => "Pix",
        _ => formaPagamento.ToString()
    };
}
