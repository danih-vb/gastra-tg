using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Exceptions;
using Mapster;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Alocacoes;

/// <summary>Monta a resposta do turno com nome do garçom e código da praça, e valida o que é comum aos casos de uso.</summary>
internal static class LeitorDoTurno
{
    public static PeriodoAlocacao Periodo(ComunicacaoEnums.PeriodoAlocacao periodo) =>
        Enum.IsDefined(periodo)
            ? periodo.Adapt<PeriodoAlocacao>()
            : throw new ErroValidacaoException([MensagensErro.PeriodoInvalido]);

    public static void GarantirNaoConfirmado(IEnumerable<Alocacao> turno)
    {
        if (turno.Any(a => a.Confirmada))
            throw new RegraDeNegocioException(MensagensErro.AlocacaoJaConfirmada);
    }

    public static async Task<AlocacaoTurnoResponse> Montar(
        DateOnly data,
        PeriodoAlocacao periodo,
        IReadOnlyCollection<Alocacao> turno,
        IRepositorioUsuario repositorioUsuario,
        IRepositorioPraca repositorioPraca,
        bool? servicoDisponivel = null)
    {
        var garcons = (await repositorioUsuario.ListarPorIds(turno.Select(a => a.GarcomId))).ToDictionary(u => u.Id, u => u.Nome);
        var pracas = (await repositorioPraca.ListarTodas()).ToDictionary(p => p.Id, p => p.Codigo);

        return new AlocacaoTurnoResponse
        {
            Data = data,
            Periodo = periodo.Adapt<ComunicacaoEnums.PeriodoAlocacao>(),
            Confirmada = turno.Count > 0 && turno.All(a => a.Confirmada),
            ServicoDisponivel = servicoDisponivel,
            Designacoes = turno
                .OrderBy(a => pracas.GetValueOrDefault(a.PracaId)).ThenBy(a => garcons.GetValueOrDefault(a.GarcomId))
                .Select(a => new DesignacaoResponse
                {
                    GarcomId = a.GarcomId,
                    GarcomNome = garcons.GetValueOrDefault(a.GarcomId, string.Empty),
                    PracaId = a.PracaId,
                    PracaCodigo = pracas.GetValueOrDefault(a.PracaId, string.Empty),
                })
                .ToList(),
        };
    }
}
