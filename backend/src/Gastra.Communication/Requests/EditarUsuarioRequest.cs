using Gastra.Communication.Enums;

namespace Gastra.Communication.Requests;

public class EditarUsuarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public PapelUsuario Papel { get; set; }
}
