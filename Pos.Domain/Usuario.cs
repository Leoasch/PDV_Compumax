namespace Pos.Domain;

public class Usuario
{
    public int Id { get; set; }
    public string NomeUsuario { get; set; } = string.Empty;
    public string NomeCompleto { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public PapelUsuario Papel { get; set; }
    public bool Ativo { get; set; } = true;
}
