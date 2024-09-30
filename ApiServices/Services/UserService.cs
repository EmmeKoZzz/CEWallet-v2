using System.Net;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using ApiServices.Models.DataTransferObjects;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services;

public class UserService(AppDbContext dbContext, RoleService roleService) {
	
	/// Retrieves a list of all users.
	public Task<User[]> GetAll(bool role = false) {
		var query = dbContext.Users.Where(entity => entity.Active);
		if (role) query = query.Include(entity => entity.Role);
		
		return query.ToArrayAsync();
	}
	
	/// Finds a user based on the provided ID, name, or email.
	public async Task<ServiceFlag<User?>> FindBy(Guid? id = default, string? name = default, string? email = default) {
		var query = dbContext.Users.Where(entity => entity.Active).Include(entity => entity.Role);
		
		User? user = default;
		if (id != null) user = await query.SingleOrDefaultAsync(entity => entity.Id == id);
		else if (name != null) user = await query.SingleOrDefaultAsync(entity => entity.Username == name);
		else if (email != null) user = await query.SingleOrDefaultAsync(entity => entity.Email == email);
		
		return user == null
			? new(HttpStatusCode.NotFound)
			: new ServiceFlag<User?>(HttpStatusCode.OK, user);
	}
	
	/// Updates the information of a user.
	public async Task<ServiceFlag<User?>> UpdateUser(RegisterUserDto details) {
		var (role, _, _) = await roleService.FindById(details.RoleId);
		
		if (role != HttpStatusCode.OK) return new(HttpStatusCode.BadRequest, null);
		
		var (status, user, _) = await FindBy(name: details.UserName);
		
		if (status != HttpStatusCode.OK) return new(HttpStatusCode.NotFound);
		
		(user!.Email, user.RoleId, user.Username) = (details.Email, details.RoleId, details.Password);
		await dbContext.SaveChangesAsync();
		
		return new(HttpStatusCode.OK, user);
	}
	
	/// Resets the password for a specified user.
	public async Task<HttpStatusCode> ResetPassword(ResetPasswordDto details) {
		var (status, user, _) = await FindBy(details.UserId);
		
		if (status != HttpStatusCode.OK) return HttpStatusCode.NotFound;
		
		if (!user!.VerifyPassword(details.OldPassword)) return HttpStatusCode.Unauthorized;
		
		if (details.OldPassword == details.Password) return HttpStatusCode.OK;
		
		user.GeneratePasswordHash(details.Password);
		await dbContext.SaveChangesAsync();
		
		return HttpStatusCode.OK;
	}
	
	/// Deletes a user with the specified ID.
	public async Task<ServiceFlag<User?>> Delete(Guid id) {
		var user = await dbContext.Users.FindAsync(id);
		
		if (user is not { Active: true }) return new(HttpStatusCode.NotFound);
		
		user.Active = false;
		
		var funds = dbContext.Funds.Where(entity => entity.UserId == id);
		foreach (var fund in funds) fund.UserId = null;
		
		await dbContext.SaveChangesAsync();
		await dbContext.Entry(user).Reference(entity => entity.Role).LoadAsync();
		
		return new(HttpStatusCode.OK, user);
	}
	
}