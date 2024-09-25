using System.ComponentModel.DataAnnotations;
using Common.DataTransferObjects.ApiResponses;

namespace Common.DataTransferObjects;

public class TransferDto
{
	[Required] public Guid FromId { get; set; }
	[Required] public Guid ToId { get; set; }
	[Required] public Guid CurrencyId { get; set; }
	[Required] [Range(0, double.MaxValue)] public double Amount { get; set; }

	public record Response(FundDto From, FundDto To);
}