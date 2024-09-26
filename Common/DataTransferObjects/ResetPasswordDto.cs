using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record ResetPasswordDto(
	[Required] Guid UserId,
	[Required, MinLength(8)] string OldPassword,
	[Required, MinLength(8)] string Password,
	[property: Compare("Password")] string Confirmation);