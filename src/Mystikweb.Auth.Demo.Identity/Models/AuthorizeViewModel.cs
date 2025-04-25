using System.ComponentModel.DataAnnotations;

namespace Mystikweb.Auth.Demo.Identity.Models;

public sealed class AuthorizeViewModel
{
	[Display(Name = "Application")]
	public required string ApplicationName { get; set; }

	[Display(Name = "Scope")]
	public required string Scope { get; set; }
}
