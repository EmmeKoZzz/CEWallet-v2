namespace ApiServices.Models.DataTransferObjects.ApiResponses;

public record ActivityLogDto(
	Guid Id,
	string User,
	string Fund,
	string? Currency,
	string Activity,
	string? TransactionType,
	double? Amount,
	string? Details,
	DateTime CreatedAt
);