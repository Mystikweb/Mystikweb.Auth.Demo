namespace Mystikweb.Auth.Demo.Models;

public sealed record PersonAddressItem
{
    public required Guid Id { get; init; }
    public required string Line1 { get; init; }
    public string? Line2 { get; init; }
    public required string City { get; init; }
    public required string State { get; init; }
    public required string PostalCode { get; init; }
    public string? Country { get; init; }
    public required string InsertBy { get; init; }
    public required DateTimeOffset InsertAt { get; init; }
    public string? UpdateBy { get; init; }
    public DateTimeOffset? UpdateAt { get; init; }
}
