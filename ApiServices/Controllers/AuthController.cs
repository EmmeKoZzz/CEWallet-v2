using System.Net;
using ApiServices.Decorators;
using ApiServices.Helpers;
using ApiServices.Models.Constants;
using ApiServices.Models.DataTransferObjects;
using ApiServices.Models.DataTransferObjects.ApiResponses;
using ApiServices.Services;
using Microsoft.AspNetCore.Mvc;


namespace ApiServices.Controllers;

[ApiController, Route("/auth")]
public class AuthController(AuthService authService) : ControllerBase {
	/// <summary> Validates a given token and returns the user and role information if successful. </summary>
	/// <response code="401"> Invalid or expired token. </response>
	/// <response code="200"> Token is valid, and user information is returned. </response>
	[HttpGet]
	public async Task<ActionResult<Response<TokenValidationDto>>> ValidateToken() {
		try {
			var (status, response, _) = await authService.Authorize(HttpContext);
			return status switch {
				HttpStatusCode.Unauthorized => this.CustomUnauthorized(detail: "Invalid or expired token."),
				HttpStatusCode.OK => this.CustomOk(response)
			};
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary> Logs in a user. </summary>
	/// <param name="credentials">The user's login credentials.</param>
	/// <response code="200"> Login successful. </response>
	/// <response code="401"> Incorrect password. </response>
	/// <response code="404"> User not found. </response>
	[HttpPost("login")]
	public async Task<ActionResult<Response<LoginResponseDto>>> LoginUser([FromBody] LoginUserDto credentials) {
		try {
			var (status, user, _) = await authService.LoginUser(credentials);
			return status switch {
				HttpStatusCode.Unauthorized => this.CustomUnauthorized(detail: "Incorrect password."),
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "User not found."),
				HttpStatusCode.OK => Ok(new Response<LoginResponseDto>(status,
					user))
			};
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
	
	/// <summary> Registers a new user. </summary>
	/// <param name="userDtoDetails">The details of the user to be registered, including their role.</param>
	/// <response code="400"> Invalid request (e.g., missing data). </response>
	/// <response code="401"> Unauthorized (user attempting to register without administrator privileges). </response>
	/// <response code="404"> Invalid role specified (role doesn't exist). </response>
	/// <response code="200"> User registered successfully. </response>
	[HttpPost("register"), AuthorizeRole(UserRole.Type.Administrator)]
	public async Task<ActionResult<Response<UserDto>>?> RegisterUser([FromBody] RegisterUserDto userDtoDetails) {
		try {
			var (status, user, _) = await authService.RegisterUser(userDtoDetails);
			return status switch {
				HttpStatusCode.NotFound => this.CustomNotFound(detail: "Invalid role specified (role doesn't exist)."),
				HttpStatusCode.OK => this.CustomOk(new UserDto(user!.Id,
					user.Username,
					user.Role.Name,
					user.CreatedAt))
			};
		} catch (Exception e) { return this.InternalError(e.Message); }
	}
}