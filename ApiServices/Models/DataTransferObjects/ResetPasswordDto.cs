using System.ComponentModel.DataAnnotations;


namespace ApiServices.Models.DataTransferObjects;

public record ResetPasswordDto {
	[Required]
	public Guid UserId { get; init; }
	
	[Required, MinLength(8)] 
	public string OldPassword;
	
	[Required, MinLength(8)] 
	public string Password;
	
	[Compare("Password")]
	public string Confirmation { get; init; }
}