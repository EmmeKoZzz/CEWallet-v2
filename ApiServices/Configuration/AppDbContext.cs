using ApiServices.Constants;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Configuration;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
	public virtual DbSet<ActivityLog> ActivityLogs { get; set; }
	public virtual DbSet<Currency> Currencies { get; set; }
	public virtual DbSet<Fund> Funds { get; set; }
	public virtual DbSet<FundCurrency> FundCurrencies { get; set; }
	public virtual DbSet<Role> Roles { get; set; }
	public virtual DbSet<User> Users { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder) {
		modelBuilder.UseCollation("utf8mb4_bin").HasCharSet("utf8mb4");

		modelBuilder.Entity<ActivityLog>(
			entity => {
				entity.HasKey(e => e.Id).HasName("PRIMARY");
				entity.Property(e => e.CreatedAt).HasDefaultValueSql("current_timestamp()");
			});

		modelBuilder.Entity<Currency>(entity => { entity.HasKey(e => e.Id).HasName("PRIMARY"); });

		modelBuilder.Entity<Fund>(
			entity => {
				entity.HasKey(e => e.Id).HasName("PRIMARY");
				entity.Property(e => e.CreatedAt).HasDefaultValueSql("current_timestamp()");
			});

		modelBuilder.Entity<FundCurrency>(
			entity => {
				entity.HasKey(e => new { e.FundId, e.CurrencyId }).
					HasName("PRIMARY").
					HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

				entity.HasOne(d => d.Currency).WithMany(p => p.FundCurrencies).HasConstraintName("Fund_Currency_ibfk_2");
				entity.HasOne(d => d.Fund).WithMany(p => p.FundCurrencies).HasConstraintName("Fund_Currency_ibfk_1");
			});

		var adminRoleId = Guid.NewGuid();

		modelBuilder.Entity<Role>(
			entity => {
				entity.HasKey(e => e.Id).HasName("PRIMARY");
				entity.HasData(
					new Role { Id = Guid.NewGuid(), Name = UserRole.Value(UserRole.Type.Assessor) },
					new Role { Id = Guid.NewGuid(), Name = UserRole.Value(UserRole.Type.Supervisor) },
					new Role { Id = adminRoleId, Name = UserRole.Value(UserRole.Type.Administrator) });
			});

		modelBuilder.Entity<User>(
			entity => {
				entity.HasKey(e => e.Id).HasName("PRIMARY");
				entity.Property(e => e.CreatedAt).HasDefaultValueSql("current_timestamp()");
				entity.Property(e => e.UpdatedAt).ValueGeneratedOnAddOrUpdate().HasDefaultValueSql("current_timestamp()");
				entity.HasOne(d => d.Role).WithMany(p => p.Users).HasConstraintName("User_ibfk_1");
				entity.HasData(new User("admin", "admin@cewallet.org", "rootcewallet", adminRoleId));
			});
	}
}