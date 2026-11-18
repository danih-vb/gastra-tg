using Gastra.Communication.Responses;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using ComunicacaoEnums = Gastra.Communication.Enums;

namespace Gastra.Application.UseCases.Alocacoes;

// Consulta de quem está em qual praça no turno (salão inteiro)
public interface IObterAlocacaoTurnoUseCase
{
    Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodo);
}

public class ObterAlocacaoTurnoUseCase(
    IRepositorioAlocacao repositorio,
    IRepositorioUsuario repositorioUsuario,
    IRepositorioPraca repositorioPraca,
    IRepositorioIndicadores indicadores,
    IUsuarioLogado usuarioLogado) : IObterAlocacaoTurnoUseCase
{
    public async Task<AlocacaoTurnoResponse> Executar(DateOnly data, ComunicacaoEnums.PeriodoAlocacao periodoRequest)
    {
        var periodo = LeitorDoTurno.Periodo(periodoRequest);
        var turno = await repositorio.ListarDoTurno(data, periodo);

        // O garçom também consulta o salão, mas a faixa de faturamento dos colegas não é dele para ver.
        var explica = usuarioLogado.ObterPapel() is PapelUsuario.Metre or PapelUsuario.Gerente;
        var fatores = explica
            ? await FatoresDoTurno.Calcular(
                data, turno.Select(a => a.GarcomId).ToList(), await repositorioPraca.ListarTodas(), indicadores, repositorio)
            : null;

        return await LeitorDoTurno.Montar(data, periodo, turno, repositorioUsuario, repositorioPraca, fatores);
    }
}
