using System.Runtime.CompilerServices;

namespace Mystikweb.Auth.Demo;

public static class EntityHelpers
{
    	/// <summary>
	/// Internal navigation property loader for Entity Framework entities
	/// </summary>
	/// <typeparam name="TRelated"></typeparam>
	/// <param name="loader"></param>
	/// <param name="entity"></param>
	/// <param name="navigationField"></param>
	/// <param name="navigationName"></param>
	/// <returns></returns>
	public static TRelated Load<TRelated>(this Action<object, string> loader,
		object entity,
		ref TRelated navigationField,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
		[CallerMemberName] string navigationName = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	where TRelated : class
	{
		loader?.Invoke(entity, navigationName);

		return navigationField;
	}

	/// <summary>
    /// Internal nullable navigation property loader for Entity Framework entities
	/// </summary>
	/// <typeparam name="TRelated"></typeparam>
	/// <param name="loader"></param>
	/// <param name="entity"></param>
	/// <param name="navigationField"></param>
	/// <param name="navigationName"></param>
	/// <returns></returns>
	public static TRelated? LoadNullable<TRelated>(this Action<object, string> loader,
	 object entity,
	 ref TRelated? navigationField,
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
	   [CallerMemberName] string navigationName = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
	where TRelated : class
	{
		loader?.Invoke(entity, navigationName);
		return navigationField;
	}
}
