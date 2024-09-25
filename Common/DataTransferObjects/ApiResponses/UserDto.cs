namespace Common.DataTransferObjects.ApiResponses;

public class UserDto(Guid id, string username, string role, DateTime createAt)
{
	public Guid Id { get; } = id;
	public string Username { get; } = username;
	public string Role { get; } = role;
	public DateTime CreatedAt { get; } = createAt;
}