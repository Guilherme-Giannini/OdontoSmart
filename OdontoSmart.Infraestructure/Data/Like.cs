namespace OdontoSmart.Infraestructure.Data;

internal static class Like
{
    /// <summary>
    /// Monta um padrão "contém" escapando os curingas do LIKE, para que o termo digitado seja tratado
    /// literalmente (o PostgreSQL usa "\" como caractere de escape padrão).
    /// </summary>
    public static string Contem(string valor) =>
        $"%{valor.Replace(@"\", @"\\").Replace("%", @"\%").Replace("_", @"\_")}%";
}
