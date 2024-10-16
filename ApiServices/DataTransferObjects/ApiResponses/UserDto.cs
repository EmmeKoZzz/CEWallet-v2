namespace ApiServices.DataTransferObjects.ApiResponses;

public record UserDto(Guid Id, string Username, string? Role, DateTime CreateAt);