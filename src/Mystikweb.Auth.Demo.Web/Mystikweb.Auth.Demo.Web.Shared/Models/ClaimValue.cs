namespace Mystikweb.Auth.Demo.Web.Shared.Models;

public sealed class ClaimValue(string type, string value)
{
    public string Type => type ?? throw new ArgumentNullException(nameof(type));

    public string Value => value ?? throw new ArgumentNullException(nameof(value));
}
