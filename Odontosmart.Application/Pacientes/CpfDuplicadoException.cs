namespace OdontoSmart.Application.Pacientes;

/// <summary>
/// Lançada pela persistência quando a restrição de unicidade do CPF é violada
/// (por exemplo, dois cadastros simultâneos com o mesmo CPF).
/// </summary>
public class CpfDuplicadoException(Exception? innerException = null)
    : Exception("Já existe um paciente cadastrado com este CPF.", innerException);
