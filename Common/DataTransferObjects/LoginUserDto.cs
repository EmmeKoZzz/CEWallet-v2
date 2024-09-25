using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects;

public record LoginUserDto(
	[property: Required, MinLength(2)] string UserName,
	[property: Required, MinLength(8)] string Password);