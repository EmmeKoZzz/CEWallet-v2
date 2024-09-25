using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public class ResetPasswordDto
{
	[Required] public Guid UserId { get; set; }
	[Required] [MinLength(8)] public string OldPassword { get; set; }
	[Required] [MinLength(8)] public string Password { get; set; }
	[Compare("Password")] public string Confirmation { get; set; }
}