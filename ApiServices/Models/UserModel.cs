using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Models;

[Table("User")]
[Index("Email", Name = "Email", IsUnique = true)]
[Index("RoleId", Name = "RoleId")]
[Index("Username", Name = "Username", IsUnique = true)]
public class User {
	[Key] public Guid Id { get; set; } = Guid.NewGuid();
	public Guid RoleId { get; set; }
	public string Email { get; set; }
	public string Username { get; set; }
	public bool Active { get; set; } = true;
	[Column(TypeName = "tinyblob")] public byte[] PasswordHash { get; set; } = [];
	[Column(TypeName = "tinyblob")] public byte[] PasswordSalt { get; set; } = [];
	[Column(TypeName = "timestamp")] public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
	[Column(TypeName = "timestamp")] public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

	[ForeignKey("RoleId")]
	[InverseProperty("Users")]
	public virtual Role Role { get; set; }

	[InverseProperty("User")] public virtual ICollection<Fund> Funds { get; set; } = [];

	public User() { }

	public User(string username, string email, string password, Guid roleId) {
		if (string.IsNullOrEmpty(username.Trim()) ||
			 string.IsNullOrEmpty(email.Trim()) ||
			 string.IsNullOrEmpty(password.Trim()))
			throw new Exception($"Invalid user info: username:{username}, email:{email}, password:{password}");

		Username = username;
		Email = email;
		RoleId = roleId;
		GeneratePasswordHash(password);
	}

	public void GeneratePasswordHash(string password) {
		PasswordSalt = RandomNumberGenerator.GetBytes(16);
		var passwordBytes = Encoding.UTF8.GetBytes(password);
		var combinedBytes = Concatenate(passwordBytes, PasswordSalt);
		PasswordHash = SHA256.HashData(combinedBytes);
	}

	public bool VerifyPassword(string password) {
		var passwordBytes = Encoding.UTF8.GetBytes(password);
		var combinedBytes = Concatenate(passwordBytes, PasswordSalt);
		var calculatedHash = SHA256.HashData(combinedBytes);

		return PasswordHash.SequenceEqual(calculatedHash);
	}

	private static byte[] Concatenate(byte[] a, byte[] b) {
		var combined = new byte[a.Length + b.Length];
		Array.Copy(a, 0, combined, 0, a.Length);
		Array.Copy(b, 0, combined, a.Length, b.Length);

		return combined;
	}
}