using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record ResetPasswordDto(
	[property: Required] Guid UserId,
	[property: Required, MinLength(8)] string OldPassword,
	[property: Required, MinLength(8)] string Password,
	[property: Compare("Password")] string Confirmation);