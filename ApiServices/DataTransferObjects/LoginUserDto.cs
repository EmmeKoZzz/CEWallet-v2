using System.ComponentModel.DataAnnotations;

namespace ApiServices.DataTransferObjects;

public record LoginUserDto([Required, MinLength(2)] string UserName, [Required, MinLength(8)] string Password);