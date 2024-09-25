using System.ComponentModel.DataAnnotations;
using Common.DataTransferObjects.ApiResponses;

namespace Common.DataTransferObjects;

public record TransactionDto(
	[property: Required] Guid Source,
	[property: Required] Guid Currency,
	[property: Required, Range(0, double.MaxValue)]
	double Amount);