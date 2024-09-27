using System.ComponentModel.DataAnnotations;
using Common.DataTransferObjects.ApiResponses;

namespace Common.DataTransferObjects;

public record TransferDto(
	Guid Source,
	[Required] Guid Destination,
	Guid Currency,
	double Amount,
	string Details) : TransactionDto(Source, Currency, Amount, Details)
{
	public record Response(FundDto Source, FundDto Destination);
}