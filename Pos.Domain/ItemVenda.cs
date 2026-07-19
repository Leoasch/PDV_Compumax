namespace Pos.Domain;

public class ItemVenda
{
    public int Id { get; set; }
    public int VendaId { get; set; }
    public int? ProdutoId { get; set; }
    public string ProdutoCodigo { get; set; } = string.Empty;
    public string ProdutoDescricao { get; set; } = string.Empty;
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal? PrecoDesconto { get; set; }
    public decimal Subtotal => Quantidade * (PrecoDesconto ?? PrecoUnitario);
}
