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

public class AuthService(
	IConfiguration configuration,
	AppDbContext dbContext,
	RoleService roleService,
	UserService userService
) {
	private static readonly Dictionary<string, string> TokenRegistry = [];
	private static readonly JwtSecurityTokenHandler TokenHandler = new();

	public record Tokens(string SigninToken, string RefreshToken);

	public async Task<ServiceFlag<User>> Authorize(HttpContext http, IEnumerable<UserRole.Type>? rolesRequired = null) {
		var authorizationHeader = http.Request.Headers.Authorization;
		const string message = "Unauthorized (user attempting to register without administrator privileges).";

		if (authorizationHeader.Count == 0) return new ServiceFlag<User>(HttpStatusCode.Unauthorized, Message: message);
		var tokenParts = authorizationHeader[0]!.Split("Bearer ");

		if (tokenParts.Length != 2) return new ServiceFlag<User>(HttpStatusCode.Unauthorized, Message: message);
		var token = tokenParts[1];

		var validationResult = await TokenHandler.ValidateTokenAsync(
			token,
			CreateValidationParams(configuration["JWT:SigningKey"] ?? ""));

		if (!validationResult.IsValid) return new ServiceFlag<User>(HttpStatusCode.Unauthorized, Message: message);

		var claims = validationResult.ClaimsIdentity;
		var role = claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType)!.Value;
		var name = claims.FindFirst(ClaimsIdentity.DefaultNameClaimType)!.Value;

		var user = await userService.FindBy(name: name);

		if (user.Value is null) return new ServiceFlag<User>(HttpStatusCode.Unauthorized, Message: message);

		if (rolesRequired == null || !rolesRequired.Any() || rolesRequired.Contains(UserRole.Value(role)))
			return new ServiceFlag<User>(HttpStatusCode.OK, user.Value);

		return new ServiceFlag<User>(HttpStatusCode.Unauthorized, Message: message);
	}

	public async Task<ServiceFlag<AuthResponseDto>> RefreshTokens(Tokens request) {
		if (!TokenRegistry.TryGetValue(request.RefreshToken, out var storeToken))
			return new ServiceFlag<AuthResponseDto>(HttpStatusCode.Unauthorized, Message: "Invalid token.");

		if (request.SigninToken != storeToken)
			return new ServiceFlag<AuthResponseDto>(HttpStatusCode.Unauthorized, Message: "Invalid token.");

		var refreshTokenValidation = await TokenHandler.ValidateTokenAsync(
			request.RefreshToken,
			CreateValidationParams(configuration["JWT:RefreshKey"] ?? ""));

		if (!refreshTokenValidation.IsValid)
			return new ServiceFlag<AuthResponseDto>(HttpStatusCode.Unauthorized, Message: "Invalid token.");

		var signinTokenValidation = await TokenHandler.ValidateTokenAsync(
			request.SigninToken,
			CreateValidationParams(configuration["JWT:SigningKey"] ?? "", false));

		var userInfo = signinTokenValidation.Claims;
		var username = userInfo[ClaimsIdentity.DefaultNameClaimType] as string;
		var role = userInfo[ClaimsIdentity.DefaultRoleClaimType] as string;

		var user = await dbContext.Users.Where(entity => entity.Username == username).
			Select(entity => entity.Id).
			SingleOrDefaultAsync();

		if (user == null) return new ServiceFlag<AuthResponseDto>(HttpStatusCode.Unauthorized, Message: "Invalid user.");
		var tokens = await GenerateToken(username!, role!);

		TokenRegistry.Remove(request.RefreshToken);

		return new ServiceFlag<AuthResponseDto>(
			HttpStatusCode.OK,
			new AuthResponseDto(user, role!, tokens.SigninToken, tokens.RefreshToken));
	}

	public async Task<ServiceFlag<User?>> RegisterUser(RegisterUserDto userDtoDetails) {
		var (_, role, _) = await roleService.FindById(userDtoDetails.RoleId);

		if (role == null) return new ServiceFlag<User?>(HttpStatusCode.NotFound);

		var user = new User(
			userDtoDetails.UserName,
			userDtoDetails.Email,
			userDtoDetails.Password,
			userDtoDetails.RoleId);

		await dbContext.Users.AddAsync(user);
		await dbContext.SaveChangesAsync();
		user.Role = role;

		FileLogger.Log($"Register: {user.Username} as {role.Name.ToUpper()}.");

		return new ServiceFlag<User?>(HttpStatusCode.OK, user);
	}

	public async Task<ServiceFlag<AuthResponseDto>> LoginUser(LoginUserDto userDtoDetails) {
		var userDb = await dbContext.Users.Include(entity => entity.Role).
			Where(entity => entity.Active).
			SingleOrDefaultAsync(e => e.Username == userDtoDetails.UserName);

		if (userDb == null) return new ServiceFlag<AuthResponseDto>(HttpStatusCode.NotFound);
		if (!userDb.VerifyPassword(userDtoDetails.Password))
			return new ServiceFlag<AuthResponseDto>(HttpStatusCode.Unauthorized);
		var tokens = await GenerateToken(userDb.Username, userDb.Role.Name);

		FileLogger.Log($"Login: {userDb.Username}.");

		return new ServiceFlag<AuthResponseDto>(
			HttpStatusCode.OK,
			new AuthResponseDto(userDb.Id, userDb.Role.Name, tokens.SigninToken, tokens.RefreshToken));
	}

	#region Helpers

	private TokenValidationParameters CreateValidationParams(string key, bool checkTime = true) {
		var validationParameters = new TokenValidationParameters {
			IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
			ValidIssuer = configuration["JWT:Issuer"],
			ValidateIssuerSigningKey = true,
			ValidateIssuer = true,
			ValidateAudience = false
		};

		if (!checkTime) return validationParameters;
		validationParameters.ValidateLifetime = true;
		validationParameters.ClockSkew = TimeSpan.Zero;

		return validationParameters;
	}

	private async Task<Tokens> GenerateToken(string username, string roleName) {
		string? signinKey = configuration["JWT:SigningKey"],
			refreshKey = configuration["JWT:RefreshKey"],
			issuer = configuration["JWT:Issuer"];

		if (string.IsNullOrEmpty(signinKey) || string.IsNullOrEmpty(refreshKey))
			throw new ArgumentException("JWT keys values are missing or empty.");

		var now = DateTime.UtcNow;

		var identity = new ClaimsIdentity(
		[
			new Claim(ClaimsIdentity.DefaultRoleClaimType, roleName),
			new Claim(ClaimsIdentity.DefaultNameClaimType, username)
		]);

		Task<string>[] tasks = [CreateToken(signinKey, 1), CreateToken(refreshKey, 3)];
		await Task.WhenAll(tasks);
		string signinToken = await tasks[0], refreshToken = await tasks[1];

		TokenRegistry.Add(refreshToken, signinToken);

		return new Tokens(signinToken, refreshToken);

		async Task<string> CreateToken(string key, int time) {
			return await Task.Run(
				() => {
					SecurityTokenDescriptor tokenDescriptor = new() {
						Subject = identity,
						SigningCredentials = new SigningCredentials(
							new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
							SecurityAlgorithms.HmacSha256Signature),
						Issuer = issuer,
						Expires = now.AddHours(time)
					};

					return TokenHandler.WriteToken(TokenHandler.CreateToken(tokenDescriptor));
				});
		}
	}

	#endregion
}