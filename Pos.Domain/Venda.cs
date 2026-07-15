namespace Pos.Domain;

public class Venda
{
    public int Id { get; set; }
    public int? UsuarioId { get; set; }
    public string UsuarioNomeCompleto { get; set; } = string.Empty;
    public DateTime DataHora { get; set; }
    public decimal Total { get; set; }
    public FormaPagamento FormaPagamento { get; set; }
    public bool Cancelada { get; set; }
    public DateTime? DataCancelamento { get; set; }
    public List<ItemVenda> Itens { get; set; } = [];
}
