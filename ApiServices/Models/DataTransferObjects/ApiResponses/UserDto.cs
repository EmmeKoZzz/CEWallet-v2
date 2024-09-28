namespace ApiServices.Models.DataTransferObjects.ApiResponses;

public record UserDto(Guid Id,
	string Username,
	string Role,
	DateTime CreateAt);