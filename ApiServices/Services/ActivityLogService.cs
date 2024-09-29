using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using ApiServices.Models.Constants;
using ApiServices.Models.DataTransferObjects;

namespace ApiServices.Services;

public class ActivityLogService(AppDbContext dbContext) {
	
	public async Task GetAll(int page = default, int limit = default, LogFilterDto? filter = default) {
		var res = dbContext.ActivityLogs.AsQueryable();
		switch (filter) {
			case { }: break;
		}
	}
	
	public async Task Log(
		FundActivity.Type activity,
		Guid fund,
		Guid user,
		FundTransaction.Type? transactionType = default,
		double? amount = default,
		string? details = default,
		Guid? currency = default
	) {
		if (activity is not (FundActivity.Type.CreateFund or FundActivity.Type.DeleteFund) && amount == null) {
			amount = 0;
			FileLogger.Log($"WARNING!!: In {FundActivity.Value(activity)}, amount is null");
		}
		
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