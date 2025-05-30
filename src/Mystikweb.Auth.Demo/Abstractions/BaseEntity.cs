using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mystikweb.Auth.Demo;

public abstract class BaseEntity<T>
{
    protected Action<object, string>? LazyLoader { get; set; }

    protected BaseEntity() { }

    protected BaseEntity(Action<object, string> lazyLoader)
    {
        LazyLoader = lazyLoader;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public T? Id { get; set; }

    [Timestamp]
    public byte[]? Timestamp { get; set; }
}
