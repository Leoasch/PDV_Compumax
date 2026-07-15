using DominioFormaPagamento = Pos.Domain.FormaPagamento;
using CompartilhadoFormaPagamento = Pos.Shared.FormaPagamento;

namespace Pos.Api.Servicos;

public static class FormaPagamentoMapeador
{
    public static DominioFormaPagamento ParaDominio(this CompartilhadoFormaPagamento formaPagamento) => formaPagamento switch
    {
        CompartilhadoFormaPagamento.Dinheiro => DominioFormaPagamento.Dinheiro,
        CompartilhadoFormaPagamento.CartaoCredito => DominioFormaPagamento.CartaoCredito,
        CompartilhadoFormaPagamento.CartaoDebito => DominioFormaPagamento.CartaoDebito,
        CompartilhadoFormaPagamento.Pix => DominioFormaPagamento.Pix,
        _ => throw new NotSupportedException($"Forma de pagamento '{formaPagamento}' não possui mapeamento para o domínio.")
    };

    public static CompartilhadoFormaPagamento ParaCompartilhado(this DominioFormaPagamento formaPagamento) => formaPagamento switch
    {
        DominioFormaPagamento.Dinheiro => CompartilhadoFormaPagamento.Dinheiro,
        DominioFormaPagamento.CartaoCredito => CompartilhadoFormaPagamento.CartaoCredito,
        DominioFormaPagamento.CartaoDebito => CompartilhadoFormaPagamento.CartaoDebito,
        DominioFormaPagamento.Pix => CompartilhadoFormaPagamento.Pix,
        _ => throw new NotSupportedException($"Forma de pagamento '{formaPagamento}' não possui mapeamento para o contrato compartilhado.")
    };
}
