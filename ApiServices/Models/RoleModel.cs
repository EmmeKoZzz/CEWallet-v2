using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Models;

[Table("Role"), Index("Name", Name = "Name", IsUnique = true)]
public class Role {
	
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();
	[Column("Role"), StringLength(50)]
	public string Name { get; set; } = null!;
	[InverseProperty("Role")]
	public virtual ICollection<User> Users { get; set; } = [];
	
}