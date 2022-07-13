namespace Blazor.Shared.Models;

public class ClientPrincipal
{
    public string IdentityProvider { get; set; } = string.Empty;

    public string UserId { get; set; } = string.Empty;

    public string UserDetails { get; set; } = string.Empty;

    public IEnumerable<string> UserRoles { get; set; } = new List<string>();
}
