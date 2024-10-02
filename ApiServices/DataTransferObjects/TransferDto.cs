using System.ComponentModel.DataAnnotations;
using ApiServices.DataTransferObjects.ApiResponses;

namespace ApiServices.DataTransferObjects;

public record TransferDto(Guid Source, [Required] Guid Destination, Guid Currency, double Amount, string Details)
	: TransactionDto(Source, Currency, Amount, Details) {
	
	public record Response(FundDto Source, FundDto Destination);
	
}