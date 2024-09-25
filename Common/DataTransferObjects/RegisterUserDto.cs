using System.ComponentModel.DataAnnotations;
using Common.Constants;

namespace Common.DataTransferObjects;

public class RegisterUserDto
{
	[Required] [MinLength(2)] public string UserName { get; set; }
	[EmailAddress] [Required] public string Email { get; set; }
	[Required] [MinLength(8)] public string Password { get; set; }
	[Compare("Password")] public string PasswordConfirmation { get; set; }
	[Required] public Guid RoleId { get; set; }
}