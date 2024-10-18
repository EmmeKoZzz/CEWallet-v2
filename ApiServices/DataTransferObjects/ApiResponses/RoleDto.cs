using ApiServices.Models;

namespace ApiServices.DataTransferObjects.ApiResponses;

public class RoleDto(Guid id, string role, IEnumerable<User>? users = default) {
	public Guid Id { get; } = id;
	public string Role { get; } = role;

	public IEnumerable<UserDto>? Users { get; set; } =
		users?.Select(u => new UserDto(u.Id, u.Username, u.Email, role, u.CreatedAt));
}