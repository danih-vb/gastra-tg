using Gastra.Communication.Enums;

namespace Gastra.Communication.Responses;

/// <summary>Dados da conta para a tela de gestão. Nunca inclui hash de senha nem segredo do autenticador.</summary>
public class UsuarioResponse
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PapelUsuario Papel { get; set; }
    public bool Ativo { get; set; }
    public bool SegundoFatorConfigurado { get; set; }
}
