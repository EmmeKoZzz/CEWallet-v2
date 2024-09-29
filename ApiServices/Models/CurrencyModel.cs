using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Models;

[Table("Currency"), Index("Name", Name = "Name", IsUnique = true)]
public class Currency {
	
	[Key]
	public Guid Id { get; set; } = Guid.NewGuid();
	[StringLength(50)]
	public string Name { get; set; } = null!;
	public bool Active { get; set; } = true;
	[InverseProperty("Currency")]
	public virtual ICollection<FundCurrency> FundCurrencies { get; set; } = [];
	
}