using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mystikweb.Auth.Demo.ApiService.Data;

public sealed class Address : AddressBookBaseEntity
{
    private Person? _person;

    public Address()
    { }

    public Address(Action<object, string> lazyLoader)
        : base(lazyLoader)
    { }

    public required string Line1 { get; set; }
    public string? Line2 { get; set; }
    public required string City { get; set; }
    public required string State { get; set; }
    public required string PostalCode { get; set; }
    public string? Country { get; set; }

    public required Guid PersonId { get; set; }
    public Person? Person
    {
        get => LazyLoader != null ? LazyLoader.LoadNullable(this, ref _person) : _person;
        set => _person = value;
    }
}

public sealed class AddressEntityConfiguration : AddressBookEntityConfiguration<Address>
{
    public override void ConfigureEntity(EntityTypeBuilder<Address> builder)
    {
        builder
            .ToTable("address", "AddressBook");

        builder
            .HasKey(k => k.Id)
            .HasName("pk_address_id");

        builder
            .Property(p => p.Line1)
            .HasColumnName("line_1")
            .HasMaxLength(128)
            .IsRequired();

        builder
            .Property(p => p.Line2)
            .HasColumnName("line_2")
            .HasMaxLength(128);

        builder
            .Property(p => p.City)
            .HasColumnName("city")
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(p => p.State)
            .HasColumnName("state")
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(p => p.PostalCode)
            .HasColumnName("postal_code")
            .HasMaxLength(16)
            .IsRequired();

        builder
            .Property(p => p.Country)
            .HasColumnName("country")
            .HasMaxLength(64);

        builder
            .Property(p => p.PersonId)
            .HasColumnName("person_id")
            .IsRequired();
    }
}
