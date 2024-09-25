using System.ComponentModel.DataAnnotations;

namespace Common.DataTransferObjects.ApiResponses;

public class TokenValidationDto(string username, string role)
{
	public string Username { get; init; } = username;
	public string Role { get; init; } = role;
}