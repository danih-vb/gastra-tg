using System.Net.Mail;
using System.Text;
using Gastra.Communication.Enums;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Usuarios;

/// <summary>
/// Valida o formato dos dados da conta. Regras que dependem do banco (e-mail repetido) ou de quem está
/// logado (não mexer na própria conta) ficam nos casos de uso.
/// </summary>
public static class ValidadorUsuario
{
    public const int TamanhoMinimoSenha = 8;

    // O BCrypt só considera os primeiros 72 bytes da senha: o que passasse disso seria ignorado sem aviso.
    private const int BytesMaximosSenha = 72;

    public static void ValidarDados(string nome, string email, PapelUsuario papel) =>
        LancarSeHouverErros(ErrosDosDados(nome, email, papel));

    public static void ValidarCadastro(string nome, string email, string senha, PapelUsuario papel)
    {
        var erros = ErrosDosDados(nome, email, papel);

        if (string.IsNullOrWhiteSpace(senha))
            erros.Add(MensagensErro.SenhaObrigatoria);
        else if (senha.Length < TamanhoMinimoSenha)
            erros.Add(MensagensErro.SenhaCurta);
        else if (Encoding.UTF8.GetByteCount(senha) > BytesMaximosSenha)
            erros.Add(MensagensErro.SenhaLonga);

        LancarSeHouverErros(erros);
    }

    public static void ValidarSenha(string senha)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(senha))
            erros.Add(MensagensErro.SenhaObrigatoria);
        else if (senha.Length < TamanhoMinimoSenha)
            erros.Add(MensagensErro.SenhaCurta);
        else if (Encoding.UTF8.GetByteCount(senha) > BytesMaximosSenha)
            erros.Add(MensagensErro.SenhaLonga);

        LancarSeHouverErros(erros);
    }

    private static List<string> ErrosDosDados(string nome, string email, PapelUsuario papel)
    {
        var erros = new List<string>();

        if (string.IsNullOrWhiteSpace(nome))
            erros.Add(MensagensErro.NomeObrigatorio);
        else if (nome.Trim().Length > 100)
            erros.Add(MensagensErro.NomeMuitoLongo);

        if (string.IsNullOrWhiteSpace(email))
            erros.Add(MensagensErro.EmailObrigatorio);
        else if (email.Trim().Length > 150)
            erros.Add(MensagensErro.EmailMuitoLongo);
        else if (!EmailValido(email.Trim()))
            erros.Add(MensagensErro.EmailInvalido);

        if (!Enum.IsDefined(papel))
            erros.Add(MensagensErro.PapelInvalido);

        return erros;
    }

    private static bool EmailValido(string email) =>
        MailAddress.TryCreate(email, out var endereco) && endereco.Address == email && email.Contains('.');

    private static void LancarSeHouverErros(List<string> erros)
    {
        if (erros.Count > 0)
            throw new ErroValidacaoException(erros);
    }
}
