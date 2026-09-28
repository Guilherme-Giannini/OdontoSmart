namespace OdontoSmart.Application.Common;

/// <param name="Campo">Nome do campo que originou o erro (vazio para erros gerais).</param>
public sealed record ErroValidacao(string Campo, string Mensagem);
