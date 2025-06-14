using System.ComponentModel.DataAnnotations;

namespace Mystikweb.Auth.Demo.Identity.Models
{
	public class ErrorViewModel
	{
		public string? RequestId { get; set; }

		public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

		[Display(Name = "Error")]
		public string? Error { get; set; }

		[Display(Name = "Description")]
		public string? ErrorDescription { get; set; }
	}
}
