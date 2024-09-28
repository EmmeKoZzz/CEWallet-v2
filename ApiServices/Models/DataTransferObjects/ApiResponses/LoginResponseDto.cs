namespace ApiServices.Models.DataTransferObjects.ApiResponses;

public class LoginResponseDto(Guid id, string role, string token)
{
	public Guid Id { get; init; } = id;
	public string Role { get; init; } = role;
	public string Token { get; init; } = token;
}