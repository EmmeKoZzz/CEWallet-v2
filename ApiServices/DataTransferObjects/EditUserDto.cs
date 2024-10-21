using System.ComponentModel.DataAnnotations;

namespace ApiServices.DataTransferObjects;

public class EditUserDto {
	[Required] public Guid Id { get; init; }
	[Required] [MinLength(2)] public string UserName { get; init; }
	[Required] [EmailAddress] public string Email { get; init; }
}