using System.ComponentModel.DataAnnotations;
using Common.DataTransferObjects.ApiResponses;

namespace Common.DataTransferObjects;

public record TransactionDto(
	[Required] Guid Source,
	[Required] Guid Currency,
	[Required, Range(0, double.MaxValue)] double Amount);