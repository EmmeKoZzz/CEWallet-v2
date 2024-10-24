using System.Net;
using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.Decorators;
using ApiServices.Helpers;
using ApiServices.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Controllers;

[ApiController]
[Route("user")]
public class UserController(UserService userService, AuthService authService, AppDbContext dbContext) : ControllerBase {
	///<summary> Retrieves a list of all user details from the database. </summary>
	[HttpPost]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<IEnumerable<UserDto>>>> GetUsers([FromBody] string[]? keywords = default,
		[FromQuery] bool role = true,
		[FromQuery] int page = 0,
		[FromQuery] int size = 10) {
		try {
			var response = await userService.GetAll(page, size, role, keywords);

			return this.CustomOk(response);
		} catch (Exception e) { return this.HandleErrors(e); }
	}

	///<summary> Retrieves a list of user details based on specified criteria. </summary>
	/// <response code="404"> User not found. </response>
	[HttpGet("find-by")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<UserDto>>> GetUserById([FromQuery] Guid? id,
		[FromQuery] string? name,
		[FromQuery] string? email) {
		try {
			var (status, user, _) = await userService.FindBy(id, name, email);

			return status switch {
				HttpStatusCode.OK => this.CustomOk(
					new UserDto(user!.Id, user.Username, user.Email, user.Role.Name, user.CreatedAt)),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "User not found.")
			};
		} catch (Exception e) { return this.HandleErrors(e); }
	}

	///<summary> Updates the details of a specified user. </summary>
	/// <response code="400"> Invalid request (e.g., invalid data). </response>
	/// <response code="401"> Unauthorized. </response>
	/// <response code="404"> User not found. </response>
	[HttpPut]
	public async Task<ActionResult<BaseDto<UserDto>>> UpdateUser([FromBody] EditUserDto details) {
		var (validation, sessionUser, _) = await authService.Authorize(HttpContext);
		if (validation != HttpStatusCode.OK) return this.CustomUnauthorized();

		if (UserRole.Value(sessionUser!.Role.Name) != UserRole.Type.Administrator) {
			var (_, user, _) = await userService.FindBy(name: details.UserName);

			if (user == null) return this.CustomNotFound(detail: "User not found.");
			if (sessionUser.Username != user.Username) return this.CustomUnauthorized(detail: "You are not authorized.");
		}

		try {
			var (status, _, _) = await userService.UpdateUser(details);
			await dbContext.SaveChangesAsync();

			return status switch {
				HttpStatusCode.OK => Ok(),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "User not found."),
				HttpStatusCode.BadRequest => this.CustomBadRequest(detail: "Invalid role.")
			};
		} catch (Exception e) { return this.HandleErrors(e); }
	}

	///<summary> Resets the password for a specified user. </summary>
	/// <response code="400"> Invalid request (e.g., invalid data). </response>
	/// <response code="401"> Unauthorized (user attempting to access without administrator privileges or invalid old password). </response>
	/// <response code="404"> User not found. </response>
	[HttpPatch("reset-password")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<UserDto>>> ResetPassword([FromBody] ResetPasswordDto details) {
		try {
			var status = await userService.ResetPassword(details);
			await dbContext.SaveChangesAsync();

			return status switch {
				HttpStatusCode.OK => Ok(),
				HttpStatusCode.Unauthorized => this.CustomUnauthorized(detail: "Invalid old password."),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "User not found.")
			};
		} catch (Exception e) { return this.HandleErrors(e); }
	}

	/// <summary> Handles user delete requests. </summary>
	/// <response code="401"> Unauthorized (user attempting to access without administrator privileges). </response>
	/// <response code="404"> User not found. </response>
	[HttpDelete("{id:guid}")]
	[AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<BaseDto<UserDto>>> Delete([FromRoute] Guid id) {
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try {
			var (status, user, _) = await userService.Delete(id);
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
			return status switch {
				HttpStatusCode.BadRequest => this.CustomBadRequest(detail: "User \"admin\" can not be delete."),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "User not found."),
				HttpStatusCode.OK => Ok(new UserDto(user!.Id, user.Username, user.Email, user.Role.Name, user.CreatedAt))
			};
		} catch (Exception e) {
			await trx.RollbackAsync();
			return this.HandleErrors(e);
		}
	}
}