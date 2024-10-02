using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.Helpers;
using ApiServices.Helpers.Structs;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiServices.Services;

public class AuthService(IConfiguration configuration, AppDbContext dbContext, RoleService roleService, UserService userService) {
	
	static readonly Dictionary<string, string> TokenRegistry = [];
	static readonly JwtSecurityTokenHandler TokenHandler = new();
	
	public record Tokens(string SigninToken, string RefreshToken);
	
	/// <summary>Creates and configures TokenValidationParameters for JWT validation.</summary>
	/// <param name="key">The secret key used for validating the token signature.</param>
	/// <param name="checkTime">A boolean flag indicating whether to validate the token's lifetime. 
	/// Default is true.</param>
	/// <returns> A TokenValidationParameters object configured with the specified settings.
	/// If checkTime is true, it includes lifetime validation with zero clock skew.</returns>
	TokenValidationParameters CreateValidationParams(string key, bool checkTime = true) {
		var validationParameters = new TokenValidationParameters {
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
			ValidIssuer = configuration["JWT:Issuer"],
			ValidateIssuerSigningKey = true,
			ValidateIssuer = true,
			ValidateAudience = false,
		};
		
		if (!checkTime) return validationParameters;
		validationParameters.ValidateLifetime = true;
		validationParameters.ClockSkew = TimeSpan.Zero;
		
		return validationParameters;
	}
	
	/// <summary>Generates a JSON Web Token (JWT) based on the provided user information.</summary>
	/// <param name="username">Username value</param>
	/// <param name="roleName">User Role</param>
	/// <returns>A LoginResponseDto containing the generated tokens and user information.</returns>
	async Task<Tokens> GenerateToken(string username, string roleName) {
		string? signinKey = configuration["JWT:SigningKey"],
			refreshKey = configuration["JWT:RefreshKey"],
			issuer = configuration["JWT:Issuer"];
		
		if (string.IsNullOrEmpty(signinKey) || string.IsNullOrEmpty(refreshKey))
			throw new ArgumentException("JWT keys values are missing or empty.");
		
		var now = DateTime.UtcNow;
		
		var identity = new ClaimsIdentity(
			[new(ClaimsIdentity.DefaultRoleClaimType, roleName), new(ClaimsIdentity.DefaultNameClaimType, username)]
		);
		
		Task<string>[] tasks = [CreateToken(signinKey, 1), CreateToken(refreshKey, 3)];
		await Task.WhenAll(tasks);
		string signinToken = await tasks[0], refreshToken = await tasks[1];
		
		TokenRegistry.Add(refreshToken, signinToken);
		
		return new(signinToken, refreshToken);
		
		async Task<string> CreateToken(string key, int time) => await Task.Run(
			() => {
				SecurityTokenDescriptor tokenDescriptor = new() {
					Subject = identity,
					SigningCredentials = new(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256Signature),
					Issuer = issuer,
					Expires = now.AddHours(time)
				};
				
				return TokenHandler.WriteToken(TokenHandler.CreateToken(tokenDescriptor));
			}
		);
		
	}
	
	/// <summary>Authorizes a request based on the provided JWT token in the HTTP context.</summary>
	/// <param name="http">The HttpContext containing the authorization header.</param>
	/// <param name="rolesRequired">Optional. The roles required for authorization.</param>
	/// <returns>A ServiceFlag containing the authorization result and user information if successful.</returns>
	public async Task<ServiceFlag<TokenValidationDto?>> Authorize(HttpContext http, IEnumerable<UserRole.Type>? rolesRequired = null) {
		var authorizationHeader = http.Request.Headers.Authorization;
		
		if (authorizationHeader.Count == 0) return new(HttpStatusCode.Unauthorized);
		var tokenParts = authorizationHeader[0]!.Split("Bearer ");
		
		if (tokenParts.Length != 2) return new(HttpStatusCode.Unauthorized);
		var token = tokenParts[1];
		
		var validationResult = await TokenHandler.ValidateTokenAsync(token, CreateValidationParams(configuration["JWT:SigningKey"] ?? ""));
		
		if (!validationResult.IsValid) return new(HttpStatusCode.Unauthorized);
		
		var claims = validationResult.ClaimsIdentity;
		var role = claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType)!.Value;
		var name = claims.FindFirst(ClaimsIdentity.DefaultNameClaimType)!.Value;
		
		var user = await userService.FindBy(name: name);
		
		if (user.Value is not { }) return new(HttpStatusCode.Unauthorized);
		
		if (rolesRequired == null || !rolesRequired.Any() || rolesRequired.Contains(UserRole.Value(role)))
			return new(HttpStatusCode.OK, new(user.Value, role));
		
		return new(HttpStatusCode.Unauthorized);
	}
	
	/// <summary>Refreshes the authentication tokens for a user.</summary>
	/// <param name="request">The Tokens object containing the current signin and refresh tokens.</param>
	/// <returns> A ServiceFlag containing an AuthResponseDto with new tokens if successful, or an error status if unsuccessful.
	/// The AuthResponseDto includes the user ID, role, and new signin and refresh tokens.</returns>
	/// <remarks>This method validates both the refresh token and the signin token, extracts user information,
	/// and generates new tokens if the validation is successful.</remarks>
	public async Task<ServiceFlag<AuthResponseDto>> RefreshTokens(Tokens request) {
		
		if (!TokenRegistry.TryGetValue(request.RefreshToken, out var storeToken))
			return new(HttpStatusCode.Unauthorized, Message: "Invalid token.");
		
		if (request.SigninToken != storeToken) {
			TokenRegistry.Remove(request.RefreshToken);
			
			return new(HttpStatusCode.Unauthorized, Message: "Invalid token.");
		}
		
		var refreshTokenValidation = await TokenHandler.ValidateTokenAsync(
			request.RefreshToken,
			CreateValidationParams(configuration["JWT:RefreshKey"] ?? "")
		);
		
		if (!refreshTokenValidation.IsValid) return new(HttpStatusCode.Unauthorized, Message: "Invalid token.");
		
		var signinTokenValidation = await TokenHandler.ValidateTokenAsync(
			request.SigninToken,
			CreateValidationParams(configuration["JWT:SigningKey"] ?? "", false)
		);
		
		var userInfo = signinTokenValidation.Claims;
		var username = userInfo[ClaimsIdentity.DefaultNameClaimType] as string;
		var role = userInfo[ClaimsIdentity.DefaultRoleClaimType] as string;
		
		var user = await dbContext.Users.Where(entity => entity.Username == username).Select(entity => entity.Id).SingleOrDefaultAsync();
		
		if (user == null) return new(HttpStatusCode.Unauthorized, Message: "Invalid user.");
		var tokens = await GenerateToken(username!, role!);
		
		return new(HttpStatusCode.OK, new AuthResponseDto(user, role!, tokens.SigninToken, tokens.RefreshToken));
	}
	
	/// <summary> Registers a new user in the database.</summary>
	/// <param name="userDtoDetails">The details of the user to be registered.</param>
	/// <returns>A ServiceFlag containing the registered user if successful.</returns>
	public async Task<ServiceFlag<User?>> RegisterUser(RegisterUserDto userDtoDetails) {
		var (_, role, _) = await roleService.FindById(userDtoDetails.RoleId);
		
		if (role == null) return new(HttpStatusCode.NotFound);
		
		var user = new User(userDtoDetails.UserName, userDtoDetails.Email, userDtoDetails.Password, userDtoDetails.RoleId);
		
		await dbContext.Users.AddAsync(user);
		await dbContext.SaveChangesAsync();
		user.Role = role;
		
		FileLogger.Log($"Register: {user.Username} as {role.Name.ToUpper()}.");
		
		return new(HttpStatusCode.OK, user);
	}
	
	/// <summary>Authenticates a user using their username and password.</summary>
	/// <param name="userDtoDetails">The login details of the user.</param>
	/// <returns>A ServiceFlag containing the login response with tokens if successful.</returns>
	public async Task<ServiceFlag<AuthResponseDto>> LoginUser(LoginUserDto userDtoDetails) {
		var userDb = await dbContext.Users.Include(entity => entity.Role).
			Where(entity => entity.Active).
			SingleOrDefaultAsync(e => e.Username == userDtoDetails.UserName);
		
		if (userDb == null) return new(HttpStatusCode.NotFound);
		if (!userDb.VerifyPassword(userDtoDetails.Password)) return new(HttpStatusCode.Unauthorized);
		var tokens = await GenerateToken(userDb.Username, userDb.Role.Name);
		
		FileLogger.Log($"Login: {userDb.Username}.");
		
		return new(HttpStatusCode.OK, new(userDb.Id, userDb.Role.Name, tokens.SigninToken, tokens.RefreshToken));
	}
	
}