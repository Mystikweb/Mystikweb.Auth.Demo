namespace Mystikweb.Auth.Demo.Models;

public sealed record PersonItem
{
    public required Guid Id { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DateTime? BirthDate { get; init; }
    public string? Email { get; init; }
    public required string InsertBy { get; init; }
    public required DateTimeOffset InsertAt { get; init; }
    public string? UpdateBy { get; init; }
    public DateTimeOffset? UpdateAt { get; init; }

    public required List<PersonAddressItem> AddressItems { get; init; }
}
