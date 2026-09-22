using Gastra.Domain.Entidades;

namespace Gastra.Domain.Repositorios;

public interface IRepositorioComanda
{
    Task Adicionar(Comanda comanda);

    /// <summary>Traz a comanda com os itens e as restrições, que é como as regras precisam dela.</summary>
    Task<Comanda?> ObterPorId(int id);

    /// <summary>Consulta do cliente por QR code (UC20), sem login.</summary>
    Task<Comanda?> ObterPorCodigoAcesso(string codigoAcesso);

    /// <summary>
    /// A mesma comanda da consulta, mas rastreada e com a avaliação carregada: é a única escrita que o
    /// código de acesso permite (RF25).
    /// </summary>
    Task<Comanda?> ObterParaAvaliacao(string codigoAcesso);

    Task<List<Comanda>> ListarAbertas();

    Task<bool> ExisteAbertaNaMesa(int mesaId);
}
