namespace ApiServices.DataTransferObjects.ApiResponses;

public class RoleDto(Guid id, string role) {
	public Guid Id { get; } = id;
	public string Role { get; } = role;
}