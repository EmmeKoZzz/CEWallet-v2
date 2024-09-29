using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Models;

[Table("Fund"), Index("UserId", Name = "UserId")]
public class Fund {
	
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid? UserId { get; set; }
	public bool Active { get; set; } = true;
	[ForeignKey("UserId"), InverseProperty("Funds")]
	public User? User { get; set; }
	[StringLength(255)]
	public string Name { get; set; } = null!;
	[StringLength(255)]
	public string? Address { get; set; }
	[StringLength(255)]
	public string? Details { get; set; }
	[StringLength(255)]
	public string? LocationUrl { get; set; }
	[Column(TypeName = "timestamp")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
	[InverseProperty("Fund")]
	public virtual ICollection<FundCurrency> FundCurrencies { get; set; } = [];
	
}