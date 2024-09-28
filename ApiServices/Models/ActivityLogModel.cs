using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;


namespace ApiServices.Models;

[Table("ActivityLog"), Index("CurrencyId",
	 Name = "CurrencyId"), Index("FundId",
	 Name = "FundId"), Index("UserId",
	 Name = "UserId")]
public class ActivityLog {
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();
	public Guid UserId { get; set; }
	public Guid FundId { get; set; }
	public Guid? CurrencyId { get; set; }
	[StringLength(50)]
	public string Activity { get; set; } = null!;
	[StringLength(50)]
	public string? TransactionType { get; set; }
	public double Amount { get; set; }
	[StringLength(255)]
	public string? Details { get; set; }
	[Column(TypeName = "timestamp")]
	public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}