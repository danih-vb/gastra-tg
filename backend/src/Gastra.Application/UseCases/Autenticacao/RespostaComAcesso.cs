using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Seguranca;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Autenticacao;

internal static class RespostaComAcesso
{
    public static LoginResponse Criar(Usuario usuario, IGeradorToken geradorToken) => new()
    {
        TokenAcesso = geradorToken.GerarTokenAcesso(usuario),
        Nome = usuario.Nome,
        Papel = usuario.Papel.Adapt<ComunicacaoEnums.PapelUsuario>(),
    };
}
