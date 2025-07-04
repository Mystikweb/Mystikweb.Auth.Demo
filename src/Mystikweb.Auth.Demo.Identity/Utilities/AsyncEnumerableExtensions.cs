﻿namespace Mystikweb.Auth.Demo.Identity.Utilities;

public static class AsyncEnumerableExtensions
{
	public static Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> source)
	{
		ArgumentNullException.ThrowIfNull(source);

		return ExecuteAsync();

		async Task<List<T>> ExecuteAsync()
		{
			var list = new List<T>();

			await foreach (var element in source)
			{
				list.Add(element);
			}

			return list;
		}
	}
}
