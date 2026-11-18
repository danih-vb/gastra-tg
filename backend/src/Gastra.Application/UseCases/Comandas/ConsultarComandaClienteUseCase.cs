using Gastra.Communication.Requests;
using Gastra.Communication.Responses;
using Gastra.Domain.Entidades;
using Gastra.Domain.Enums;
using Gastra.Domain.Repositorios;
using Gastra.Domain.Seguranca;
using Gastra.Exceptions;

namespace Gastra.Application.UseCases.Comandas;

// UC20 — Consulta da comanda pelo cliente, por QR code, sem login (RF13)
public interface IConsultarComandaClienteUseCase
{
    Task<ComandaClienteResponse> Executar(string codigoAcesso);
}

public class ConsultarComandaClienteUseCase(
    IRepositorioComanda repositorio,
    IRepositorioMesa repositorioMesa,
    IRepositorioItemCardapio repositorioCardapio) : IConsultarComandaClienteUseCase
{
    public async Task<ComandaClienteResponse> Executar(string codigoAcesso)
    {
        var comanda = await repositorio.ObterPorCodigoAcesso(codigoAcesso)
                      ?? throw new NaoEncontradoException(MensagensErro.ComandaNaoEncontrada);

        var mesa = await repositorioMesa.ObterPorId(comanda.MesaId);
        var nomes = await LeitorDeNomesDoCardapio.Obter(repositorioCardapio, comanda);

        return MapeadorComanda.MontarParaCliente(comanda, mesa?.Numero ?? string.Empty, nomes);
    }
}
