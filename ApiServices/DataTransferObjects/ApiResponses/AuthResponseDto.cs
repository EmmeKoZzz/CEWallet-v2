namespace ApiServices.DataTransferObjects.ApiResponses;

public class AuthResponseDto(Guid id, string role, string signinToken, string refreshToken) {
	
	public Guid Id { get; init; } = id;
	public string Role { get; init; } = role;
	public string SigninToken { get; init; } = signinToken;
	public string RefreshToken { get; init; } = refreshToken;
	
}