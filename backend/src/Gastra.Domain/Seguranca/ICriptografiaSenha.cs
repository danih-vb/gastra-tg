namespace Gastra.Domain.Seguranca;

/// <summary>RN06: a senha só é guardada como hash e verificada por comparação de hash.</summary>
public interface ICriptografiaSenha
{
    string GerarHash(string senha);
    bool Verificar(string senha, string hash);
}
