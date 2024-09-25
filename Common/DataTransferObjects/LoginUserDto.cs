using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public class LoginUserDto
{
	[Required] [MinLength(2)] public string UserName { get; set; }
	[Required] [MinLength(8)] public string Password { get; set; }
}