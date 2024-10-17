using System.Net;
using ApiServices.Configuration;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.Helpers.Structs;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services;

public class UserService(AppDbContext dbContext, RoleService roleService) {
	public async Task<PaginationDto<UserDto>> GetAll(int page, int size, bool role = false) {
		var query = dbContext.Users.Where(entity => entity.Active);
		if (role) query = query.Include(entity => entity.Role);

		// aqui el cache del server guarda la union de la tabla de roles en los usuarios que ya se les haya hecho esa union
		var data = await query.Skip(page * size).Take(size).ToListAsync();
		return new PaginationDto<UserDto>(
			data.Select(user => new UserDto(user.Id, user.Username, role ? user.Role.Name : null, user.CreatedAt)),
			page,
			size,
			await query.CountAsync());
	}

	public async Task<ServiceFlag<User>> FindBy(Guid? id = default, string? name = default, string? email = default) {
		var query = dbContext.Users.Where(entity => entity.Active).Include(entity => entity.Role);

		User? user = default;
		if (id != null) user = await query.SingleOrDefaultAsync(entity => entity.Id == id);
		else if (name != null) user = await query.SingleOrDefaultAsync(entity => entity.Username == name);
		else if (email != null) user = await query.SingleOrDefaultAsync(entity => entity.Email == email);

		return user == null
			? new ServiceFlag<User>(HttpStatusCode.NotFound)
			: new ServiceFlag<User>(HttpStatusCode.OK, user);
	}

	public async Task<ServiceFlag<User?>> UpdateUser(RegisterUserDto details) {
		var (role, _, _) = await roleService.FindById(details.RoleId);
		if (role != HttpStatusCode.OK) return new ServiceFlag<User?>(HttpStatusCode.BadRequest);

		var (status, user, _) = await FindBy(name: details.UserName);
		if (status != HttpStatusCode.OK) return new ServiceFlag<User?>(HttpStatusCode.NotFound);

		(user!.Email, user.RoleId, user.Username) = (details.Email, details.RoleId, details.Password);
		await dbContext.SaveChangesAsync();

		return new ServiceFlag<User?>(HttpStatusCode.OK, user);
	}

	public async Task<HttpStatusCode> ResetPassword(ResetPasswordDto details) {
		var (status, user, _) = await FindBy(details.UserId);

		if (status != HttpStatusCode.OK) return HttpStatusCode.NotFound;

		if (!user!.VerifyPassword(details.OldPassword)) return HttpStatusCode.Unauthorized;

		if (details.OldPassword == details.Password) return HttpStatusCode.OK;

		user.GeneratePasswordHash(details.Password);
		await dbContext.SaveChangesAsync();

		return HttpStatusCode.OK;
	}

	public async Task<ServiceFlag<User?>> Delete(Guid id) {
		var user = await dbContext.Users.FindAsync(id);

		if (user is not {
			Active: true
		}) return new ServiceFlag<User?>(HttpStatusCode.NotFound);

		user.Active = false;

		var funds = dbContext.Funds.Where(entity => entity.UserId == id);
		foreach (var fund in funds) fund.UserId = null;

		await dbContext.SaveChangesAsync();
		await dbContext.Entry(user).Reference(entity => entity.Role).LoadAsync();

		return new ServiceFlag<User?>(HttpStatusCode.OK, user);
	}
}