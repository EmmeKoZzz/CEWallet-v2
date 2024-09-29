using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Text;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using ApiServices.Models.Constants;
using ApiServices.Models.DataTransferObjects;
using ApiServices.Models.DataTransferObjects.ApiResponses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace ApiServices.Services;

public class AuthService(
	IConfiguration configuration,
	AppDbContext dbContext,
	RoleService roleService,
	UserService userService
) {
	
	// TODO: queda hacer un esquema para guardar las sessions con el token de refresco
	/// Generates a JSON Web Token (JWT) based on the provided configuration and identity claims.
	private string GenerateToken(ClaimsIdentity identity) {
		var jwtSecret = configuration["JWT_SECRET"];
		var issuer = configuration["JWT_ISSUER"];
		var audience = configuration["JWT_AUDIENCE"];
		
		if (string.IsNullOrEmpty(jwtSecret) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience)) {
			throw new ArgumentException(
				"JWT configuration values (JWT_SECRET, JWT_ISSUER, JWT_AUDIENCE) are missing or empty."
			);
		}
		
		var tokenExpirationTime = DateTime.UtcNow.AddMinutes(60); // Adjust based on your security requirements
		
		var tokenHandler = new JwtSecurityTokenHandler();
		var secureKey =
			new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)); // Use UTF-8 encoding for broader compatibility
		
		var tokenDescriptor = new SecurityTokenDescriptor {
			Subject = identity,
			Issuer = issuer,
			Audience = audience,
			SigningCredentials = new(secureKey, SecurityAlgorithms.HmacSha256Signature),
			Expires = tokenExpirationTime
		};
		
		var token = tokenHandler.CreateToken(tokenDescriptor);
		
		return tokenHandler.WriteToken(token);
	}
	
	/// Authorizes a request based on the provided JWT token.
	public async Task<ServiceFlag<TokenValidationDto?>> Authorize(
		HttpContext httpContext,
		IEnumerable<UserRole.Type>? rolesRequired = null
	) {
		var authorizationHeader = httpContext.Request.Headers.Authorization;
		if (authorizationHeader.Count == 0) { return new(HttpStatusCode.Unauthorized); }
		
		var tokenParts = authorizationHeader[0]!.Split("Bearer ");
		if (tokenParts.Length != 2) { return new(HttpStatusCode.Unauthorized); }
		
		var token = tokenParts[1];
		
		var key = configuration["JWT_SECRET"];
		var issuer = configuration["JWT_ISSUER"];
		var audience = configuration["JWT_AUDIENCE"];
		
		if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience)) {
			throw new ArgumentException("Authorization: Configuration values cannot be null or empty.");
		}
		
		var tokenHandler = new JwtSecurityTokenHandler();
		var secureKey = Encoding.ASCII.GetBytes(key);
		var tokenValidationParameters = new TokenValidationParameters {
			ValidateIssuerSigningKey = true,
			IssuerSigningKey = new SymmetricSecurityKey(secureKey),
			ValidateIssuer = true,
			ValidIssuer = issuer,
			ValidateAudience = true,
			ValidAudience = audience,
			ValidateLifetime = true,
			ClockSkew = TimeSpan.Zero
		};
		
		var validationResult = await tokenHandler.ValidateTokenAsync(token, tokenValidationParameters);
		
		if (!validationResult.IsValid) return new(HttpStatusCode.Unauthorized);
		
		var claims = validationResult.ClaimsIdentity;
		var role = claims.FindFirst(ClaimsIdentity.DefaultRoleClaimType)!.Value;
		var name = claims.FindFirst(ClaimsIdentity.DefaultNameClaimType)!.Value;
		
		var user = await userService.FindBy(name: name);
		
		if (user.Value is not { }) return new(HttpStatusCode.Unauthorized);
		
		if (rolesRequired == null || !rolesRequired.Any() || rolesRequired.Contains(UserRole.Value(role))) {
			return new(HttpStatusCode.OK, new(user.Value, role));
		}
		
		return new(HttpStatusCode.Unauthorized);
	}
	
	/// Registers a new user in the database.
	public async Task<ServiceFlag<User?>> RegisterUser(RegisterUserDto userDtoDetails) {
		var (_, role, _) = await roleService.FindById(userDtoDetails.RoleId);
		if (role == null) { return new(HttpStatusCode.NotFound); }
		
		var user = new User(
			userDtoDetails.UserName,
			userDtoDetails.Email,
			userDtoDetails.Password,
			userDtoDetails.RoleId
		);
		
		await dbContext.Users.AddAsync(user);
		await dbContext.SaveChangesAsync();
		user.Role = role;
		
		FileLogger.Log($"Register: {user.Username} as {role.Name.ToUpper()}.");
		
		return new(HttpStatusCode.OK, user);
	}
	
	/// Signs in a user using their username and password.
	public async Task<ServiceFlag<LoginResponseDto?>> LoginUser(LoginUserDto userDtoDetails) {
		var userDb = await dbContext.Users.Include(entity => entity.Role).
			Where(entity => entity.Active).
			SingleOrDefaultAsync(e => e.Username == userDtoDetails.UserName);
		
		if (userDb == null) { return new(HttpStatusCode.NotFound); }
		
		if (!userDb.VerifyPassword(userDtoDetails.Password)) { return new(HttpStatusCode.Unauthorized); }
		
		var token = GenerateToken(
			new(
				[
					new(ClaimsIdentity.DefaultRoleClaimType, userDb.Role.Name),
					new(ClaimsIdentity.DefaultNameClaimType, userDb.Username)
				]
			)
		);
		
		FileLogger.Log($"Login: {userDb.Username}.");
		
		return new(HttpStatusCode.OK, new(userDb.Id, userDb.Role.Name, token));
	}
	
}