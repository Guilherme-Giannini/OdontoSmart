namespace OdontoSmart.Application.Orcamentos;

/// <summary>
/// Lançada pela persistência quando a chave estrangeira do paciente é violada
/// (por exemplo, o paciente foi excluído enquanto o orçamento era salvo).
/// </summary>
public class PacienteInexistenteException(Exception? innerException = null)
    : Exception("O paciente informado não existe.", innerException);
