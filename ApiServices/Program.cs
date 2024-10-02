using System.Reflection;
using ApiServices.Configuration;
using ApiServices.Decorators;
using ApiServices.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);
var services = builder.Services; // Access the service collection for dependency injection

services.AddControllers(); // Add support for controllers to handle API requests

// Services
services.AddScoped<AuthorizeFilter>();
services.AddScoped<AuthService>();
services.AddScoped<ActivityLogService>();
services.AddScoped<RoleService>();
services.AddScoped<UserService>();
services.AddScoped<CurrencyService>();
services.AddScoped<FundService>();

// Add Endpoints API Explorer to visualize APIs
services.AddEndpointsApiExplorer();

// Add Swagger for API documentation generation
services.AddSwaggerGen(
	c => {
		// Include XML comments for API documentation
		var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
		c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
		
		// Add security definitions
		c.AddSecurityDefinition(
			"Bearer",
			new() { Description = "JWT Bearer Token", In = ParameterLocation.Header, Type = SecuritySchemeType.Http, Scheme = "bearer" }
		);
		
		// Add security requirements
		c.AddSecurityRequirement(new() { { new() { Reference = new() { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, [] } });
	}
);

// Add DbContext for database access

var dbConnection = builder.Configuration.GetConnectionString(
	builder.Environment.IsDevelopment()
		? "Development"
		: "Production"
)!;

services.AddDbContext<AppDbContext>(o => o.UseMySql(dbConnection, ServerVersion.AutoDetect(dbConnection)));

// Configure CORS (Cross-Origin Resource Sharing) based on the environment
var origin = builder.Configuration["AllowedHosts"];
services.AddCors(
	options => { options.AddPolicy("AllowedOrigins", policy => policy.WithOrigins(origin ?? "*").AllowAnyHeader().AllowAnyMethod()); }
);

// Build the WebApplication instance from the configured builder
var app = builder.Build();

// Enable Swagger
app.UseSwagger();
app.UseSwaggerUI(
	options => {
		options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
		options.RoutePrefix = string.Empty;
	}
);

// Redirect HTTP requests to HTTPS for improved security (consider exceptions if needed)
app.UseHttpsRedirection();

// Enable authorization if applicable to your API endpoints
app.UseAuthorization();

// Apply CORS policy with the name "AllowedOrigins" defined earlier
app.UseCors("AllowedOrigins");

// Map controllers to their corresponding API routes
app.MapControllers();

// Start running the web application
app.Run();