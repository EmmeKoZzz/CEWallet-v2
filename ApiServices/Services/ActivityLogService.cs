using ApiServices.Configuration;
using ApiServices.Models;
using ApiServices.Models.Constants;
using ApiServices.Models.DataTransferObjects;

namespace ApiServices.Services;

public class ActivityLogService(AppDbContext dbContext) {
	
	public async Task GetAll(int page = default, int limit = default, LogFilterDto? filterDto = default) {
		var res = dbContext.ActivityLogs.AsQueryable();
	}
	
	public async Task Log(
		FundActivity.Type activity,
		FundTransaction.Type transactionType,
		Guid fund,
		Guid user,
		double amount,
		string? details = default,
		Guid? currency = default
	) {
		// Create log
		var log = new ActivityLog {
			Activity = FundActivity.Value(activity),
			CurrencyId = currency,
			Amount = amount,
			Details = details,
			FundId = fund,
			UserId = user,
			TransactionType = FundTransaction.Value(transactionType)
		};
		
		await dbContext.ActivityLogs.AddAsync(log);
		await dbContext.SaveChangesAsync();
	}
	
}