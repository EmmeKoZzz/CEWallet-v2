using System.ComponentModel.DataAnnotations;
using Common.Constants;

namespace Common.DataTransferObjects;

public record RegisterUserDto(
	[property: Required, MinLength(2)] string UserName,
	[property: Required, EmailAddress] string Email,
	[property: Required, MinLength(8)] string Password,
	[property: Compare("Password")] string PasswordConfirmation,
	[property: Required] Guid RoleId);