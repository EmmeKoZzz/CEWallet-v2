namespace ApiServices.DataTransferObjects.ApiResponses;

public record UserDto(Guid Id, string Username, string email, string? Role, DateTime CreateAt);