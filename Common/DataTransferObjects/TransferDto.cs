using System.ComponentModel.DataAnnotations;
using Common.DataTransferObjects.ApiResponses;

namespace Common.DataTransferObjects;

public record TransferDto(
	Guid Source,
	[Required] Guid Destination,
	Guid Currency,
	double Amount) : TransactionDto(Source, Currency, Amount)
{
	public record Response(FundDto Source, FundDto Destination);
}