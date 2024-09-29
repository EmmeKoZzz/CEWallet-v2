using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Models;

[PrimaryKey("FundId", "CurrencyId"), Table("Fund_Currency"), Index("FundId", Name = "FundId"),
Index("CurrencyId", Name = "CurrencyId")]
public class FundCurrency {
	
	[Key]
	public Guid FundId { get; set; }
	[Key]
	public Guid CurrencyId { get; set; }
	public double Amount { get; set; }
	[ForeignKey("CurrencyId"), InverseProperty("FundCurrencies")]
	public virtual Currency Currency { get; set; }
	[ForeignKey("FundId"), InverseProperty("FundCurrencies")]
	public virtual Fund Fund { get; set; }
	
}