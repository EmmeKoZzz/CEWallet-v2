namespace ApiServices.Models.DataTransferObjects.ApiResponses;

public class TokenValidationDto(User user, string role) {
	
	public User User { get; init; } = user;
	public string Role { get; init; } = role;
	
}