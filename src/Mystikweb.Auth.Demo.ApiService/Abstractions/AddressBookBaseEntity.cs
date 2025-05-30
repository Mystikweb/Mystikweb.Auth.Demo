using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mystikweb.Auth.Demo.ApiService.Abstractions;

public abstract class AddressBookBaseEntity : BaseEntity<Guid>
{
    protected AddressBookBaseEntity() { }

    protected AddressBookBaseEntity(Action<object, string> lazyLoader)
        : base(lazyLoader) { }

    public required string InsertBy { get; set; }
    public required DateTimeOffset InsertAt { get; set; }
    public string? UpdateBy { get; set; }
    public DateTimeOffset? UpdateAt { get; set; }
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
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder
            .Property(p => p.InsertBy)
            .HasColumnName("insert_by")
            .HasMaxLength(64)
            .IsRequired();

        builder
            .Property(p => p.InsertAt)
            .HasColumnName("insert_at")
            .IsRequired();

        builder
            .Property(p => p.UpdateBy)
            .HasColumnName("update_by")
            .HasMaxLength(64);

        builder
            .Property(p => p.UpdateAt)
            .HasColumnName("update_at");

        builder
            .Property(p => p.Timestamp)
            .HasColumnName("timestamp")
            .IsRowVersion()
            .IsConcurrencyToken();
    }
}
