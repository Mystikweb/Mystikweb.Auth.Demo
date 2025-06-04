using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mystikweb.Auth.Demo.ApiService.Abstractions;

public abstract class AddressBookBaseEntity : BaseEntity<Guid>
{
    protected AddressBookBaseEntity() { }

    protected AddressBookBaseEntity(Action<object, string> lazyLoader)
        : base(lazyLoader) { }

    public required string InsertBy { get; set; }
    public required Instant InsertAt { get; set; }
    public string? UpdateBy { get; set; }
    public Instant? UpdateAt { get; set; }
}


public abstract class AddressBookEntityConfiguration<T> : IEntityTypeConfiguration<T>
    where T : AddressBookBaseEntity
{
    public abstract void ConfigureEntity(EntityTypeBuilder<T> builder);

    public void Configure(EntityTypeBuilder<T> builder)
    {
        ConfigureEntity(builder);

        builder
            .Property(p => p.Id)
            .HasColumnType("uuid");

        builder
            .Property(p => p.InsertBy)
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(p => p.InsertAt)
            .IsRequired();

        builder
            .Property(p => p.UpdateBy)
            .HasMaxLength(64);

        builder
            .Property(p => p.UpdateAt);

        builder
            .Property(p => p.Timestamp)
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
