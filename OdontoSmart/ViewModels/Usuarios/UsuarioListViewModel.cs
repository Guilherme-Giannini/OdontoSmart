using System.Globalization;
using OdontoSmart.Application.Common;
using OdontoSmart.Application.Usuarios;
using OdontoSmart.Domain.Enums;

namespace OdontoSmart.Web.ViewModels.Usuarios;

public class UsuarioListViewModel
{
    public IReadOnlyList<UsuarioListItemViewModel> Usuarios { get; init; } = [];

    // Filtros aplicados.
    public string? Busca { get; init; }
    public FiltroSituacaoUsuario Situacao { get; init; }

    public int PaginaAtual { get; init; }
    public int TotalPaginas { get; init; }
    public int TotalRegistros { get; init; }

    public bool PossuiFiltros => !string.IsNullOrEmpty(Busca) || Situacao != FiltroSituacaoUsuario.Ativos;

    /// <summary>Parâmetros de rota que preservam os filtros ao navegar entre páginas.</summary>
    public Dictionary<string, string> RotaDaPagina(int pagina)
    {
        var rota = new Dictionary<string, string>
        {
            ["pagina"] = pagina.ToString(CultureInfo.InvariantCulture),
            ["situacao"] = Situacao.ToString()
        };

        if (!string.IsNullOrEmpty(Busca))
            rota["busca"] = Busca;

        return rota;
    }

    public static UsuarioListViewModel De(
        PaginaResultado<UsuarioResumoDto> pagina, FiltroUsuarios filtro, Guid? usuarioAtualId) => new()
    {
        Usuarios = pagina.Itens.Select(u => UsuarioListItemViewModel.De(u, usuarioAtualId)).ToList(),
        Busca = filtro.Busca?.Trim(),
        Situacao = filtro.Situacao,
        PaginaAtual = pagina.Pagina,
        TotalPaginas = pagina.TotalPaginas,
        TotalRegistros = pagina.TotalRegistros
    };
}

public class UsuarioListItemViewModel
{
    public Guid Id { get; init; }
    public string NomeCompleto { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public PerfilUsuario Perfil { get; init; }
    public bool Ativo { get; init; }
    public bool Bloqueado { get; init; }
    public string UltimoAcesso { get; init; } = string.Empty;

    /// <summary>O usuário não pode desativar nem redefinir a senha de si mesmo por esta tela.</summary>
    public bool EhUsuarioAtual { get; init; }

    public static UsuarioListItemViewModel De(UsuarioResumoDto usuario, Guid? usuarioAtualId) => new()
    {
        Id = usuario.Id,
        NomeCompleto = usuario.NomeCompleto,
        Email = usuario.Email,
        Perfil = usuario.Perfil,
        Ativo = usuario.Ativo,
        Bloqueado = usuario.Bloqueado,
        UltimoAcesso = UsuarioFormatacao.UltimoAcesso(usuario.UltimoAcesso),
        EhUsuarioAtual = usuario.Id == usuarioAtualId
    };
}
