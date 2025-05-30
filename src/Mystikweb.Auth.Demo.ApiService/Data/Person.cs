using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mystikweb.Auth.Demo.ApiService.Data;

public sealed class Person : AddressBookBaseEntity
{
    private ICollection<Address>? _addresses;

    public Person()
    { }

    public Person(Action<object, string> lazyLoader)
        : base(lazyLoader)
    { }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public DateTime? BirthDate { get; set; }
    public string? Email { get; set; }

    public ICollection<Address> Addresses
    {
        get
        {
            LazyLoader?.LoadNullable(this, ref _addresses);
            return _addresses ??= [];
        }
        set => _addresses = value;
    }
}

public sealed class PersonEntityConfiguration : AddressBookEntityConfiguration<Person>
{
    public override void ConfigureEntity(EntityTypeBuilder<Person> builder)
    {
        builder
            .ToTable("person", "AddressBook");

        builder
            .HasKey(k => k.Id)
            .HasName("pk_person_id");

        builder
            .Property(p => p.FirstName)
            .HasColumnName("first_name")
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(p => p.LastName)
            .HasColumnName("last_name")
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(p => p.BirthDate)
            .HasColumnName("birth_date");

        builder
            .Property(p => p.Email);


        builder
            .HasMany(p => p.Addresses)
            .WithOne(a => a.Person)
            .HasForeignKey(a => a.PersonId)
            .OnDelete(DeleteBehavior.Cascade)
            .HasConstraintName("fk_person_address");
    }
}
