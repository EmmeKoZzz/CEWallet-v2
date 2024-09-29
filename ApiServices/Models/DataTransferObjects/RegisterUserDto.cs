using System.ComponentModel.DataAnnotations;

namespace ApiServices.Models.DataTransferObjects;

public record RegisterUserDto {
	
	[Required]
	public Guid RoleId { get; init; }
	[Required, MinLength(2)]
	public string UserName { get; init; }
	[Required, EmailAddress]
	public string Email { get; init; }
	[Required, MinLength(8)]
	public string Password { get; init; }
	[Compare("Password")]
	public string PasswordConfirmation { get; init; }
	
}