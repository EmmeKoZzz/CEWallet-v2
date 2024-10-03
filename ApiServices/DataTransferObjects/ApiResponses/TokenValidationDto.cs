using ApiServices.Models;

namespace ApiServices.DataTransferObjects.ApiResponses;

public class TokenValidationDto(string user, string role) {
	
	public string User { get; init; } = user;
	public string Role { get; init; } = role;
	
}