using System.Net;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;


namespace ApiServices.Services;

public class RoleService(AppDbContext dbContext) {
	/// Retrieves a list of all available user roles.
	public async Task<IEnumerable<Role>> GetAll() => await dbContext.Roles.ToArrayAsync();
	
	/// Retrieves a specific role by its unique identifier.
	public async Task<ServiceFlag<Role?>> FindById(Guid id) {
		var role = await dbContext.Roles.FindAsync(id);
		return role == null
			? new(HttpStatusCode.NotFound,
				null)
			: new ServiceFlag<Role?>(HttpStatusCode.OK,
				role);
	}
}