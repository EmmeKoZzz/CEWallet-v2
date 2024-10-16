using System.Net;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Helpers.Structs;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services;

public class RoleService(AppDbContext dbContext) {
	public async Task<IEnumerable<Role>> GetAll() => await dbContext.Roles.ToArrayAsync();

	public async Task<ServiceFlag<Role?>> FindById(Guid id) {
		var role = await dbContext.Roles.FindAsync(id);

		return role == null
			? new(HttpStatusCode.NotFound, null)
			: new ServiceFlag<Role?>(HttpStatusCode.OK, role);
	}
}