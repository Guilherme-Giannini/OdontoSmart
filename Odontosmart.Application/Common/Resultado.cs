namespace OdontoSmart.Application.Common;

public class Resultado
{
    protected Resultado(bool naoEncontrado, IReadOnlyList<ErroValidacao> erros)
    {
        NaoEncontrado = naoEncontrado;
        Erros = erros;
    }

    public bool NaoEncontrado { get; }
    public IReadOnlyList<ErroValidacao> Erros { get; }
    public bool Sucesso => !NaoEncontrado && Erros.Count == 0;

    public static Resultado Ok() => new(false, []);
    public static Resultado Falha(IReadOnlyList<ErroValidacao> erros) => new(false, erros);
    public static Resultado RecursoNaoEncontrado() => new(true, []);
}

public sealed class Resultado<T> : Resultado
{
    private Resultado(T? valor, IReadOnlyList<ErroValidacao> erros)
        : base(false, erros)
    {
        Valor = valor;
    }

    public T? Valor { get; }

    public static Resultado<T> Ok(T valor) => new(valor, []);
    public static new Resultado<T> Falha(IReadOnlyList<ErroValidacao> erros) => new(default, erros);
}
