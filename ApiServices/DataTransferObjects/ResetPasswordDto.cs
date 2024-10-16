using System.ComponentModel.DataAnnotations;

namespace ApiServices.DataTransferObjects;

public record ResetPasswordDto {
	[Required] public Guid UserId { get; init; }

	[Required] [MinLength(8)] public string OldPassword { get; init; }

	[Required] [MinLength(8)] public string Password { get; init; }

	[Compare("Password")] public string Confirmation { get; init; }
}