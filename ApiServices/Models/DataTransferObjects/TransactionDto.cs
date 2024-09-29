using System.ComponentModel.DataAnnotations;

namespace ApiServices.Models.DataTransferObjects;

public record TransactionDto(
	[Required] Guid Source,
	[Required] Guid Currency,
	[Required, Range(0, double.MaxValue)] double Amount,
	[StringLength(255)] string Details
);