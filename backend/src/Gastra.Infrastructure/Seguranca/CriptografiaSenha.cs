using Gastra.Domain.Seguranca;

namespace Gastra.Infrastructure.Seguranca;

/// <summary>
/// Hash de senha com BCrypt: gera um salt diferente para cada senha e é propositalmente lento
/// (fator de custo 12), o que encarece ataques de força bruta.
/// </summary>
public class CriptografiaSenha : ICriptografiaSenha
{
    private const int FatorDeCusto = 12;

    public string GerarHash(string senha) => BCrypt.Net.BCrypt.HashPassword(senha, FatorDeCusto);

    public bool Verificar(string senha, string hash) => BCrypt.Net.BCrypt.Verify(senha, hash);
}
