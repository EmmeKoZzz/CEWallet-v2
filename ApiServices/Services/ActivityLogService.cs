using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.Helpers;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services;

public class ActivityLogService(AppDbContext dbContext) {
	
	/// <summary>Retrieves a paginated list of activity logs with optional filtering.</summary>
	/// <param name="page">The page number (defaults to 0).</param>
	/// <param name="limit">The number of logs per page (defaults to 10).</param>
	/// <param name="filter">Optional filter criteria for activity logs (can be null).</param>
	/// <returns>An asynchronous task returning an IEnumerable of ActivityLogDto objects.</returns>
	public async Task<IEnumerable<ActivityLogDto>> GetAll(int page, int limit, ActivityLogFilterDto? filter = null) {
		// Build the base query with eager loading for related entities
		var query = dbContext.ActivityLogs.Include(entity => entity.User).
			Include(entity => entity.Fund).
			Include(entity => entity.Currency).
			AsQueryable();
		
		// Apply filters if provided
		if (filter != null) ApplyFilters();
		
		// Skip and take records for pagination
		var res = await query.Skip(page * limit).Take(limit).ToArrayAsync();
		
		// Project results to ActivityLogDto objects with mapped data
		return res.Select(
			entity => new ActivityLogDto(
				entity.Id,
				entity.User.Username,
				entity.Fund.Name,
				entity.Currency?.Name,
				entity.Activity,
				entity.TransactionType,
				entity.Amount,
				entity.Details,
				entity.CreatedAt
			)
		);
		
		// Function to handle applying filters based on filter criteria
		void ApplyFilters() {
			var (since, until, fundTransactions, activities, orderByAmount, descendingOrder, amountMin, amountMax, funds, users, currencyId) =
				filter;
			
			// Apply filters based on the provided criteria  
			if (currencyId.HasValue) query = query.Where(entity => entity.FundId == currencyId.Value);
			if (since.HasValue) query = query.Where(entity => entity.CreatedAt >= since.Value);
			if (until.HasValue) query = query.Where(entity => entity.CreatedAt <= until.Value);
			if (amountMin.HasValue) query = query.Where(entity => entity.Amount >= amountMin.Value);
			if (amountMax.HasValue) query = query.Where(entity => entity.Amount <= amountMax.Value);
			if (funds is { Length: > 0 }) query = query.Where(entity => funds.Contains(entity.FundId));
			if (users is { Length: > 0 }) query = query.Where(entity => users.Contains(entity.UserId));
			if (activities is { Length: > 0 }) query = query.Where(entity => activities.Contains(entity.Activity));
			if (fundTransactions is { Length: > 0 }) query = query.Where(entity => fundTransactions.Contains(entity.TransactionType));
			
			// Handle ordering based on amount or createdAt  
			query = orderByAmount switch {
				true when descendingOrder => query.OrderByDescending(entity => entity.Amount),
				true when !descendingOrder => query.OrderBy(entity => entity.Amount),
				false when descendingOrder => query.OrderByDescending(entity => entity.CreatedAt),
				_ => query.OrderBy(entity => entity.CreatedAt)
			};
		}
	}
	
	/// <summary>Logs a fund activity in the database.</summary>
	/// <param name="activity">The type of fund activity.</param>
	/// <param name="fund">The identifier of the fund.</param>
	/// <param name="user">The identifier of the user.</param>
	/// <param name="transactionType">The type of fund transaction (optional).</param>
	/// <param name="amount">The amount involved in the activity (optional).</param>
	/// <param name="details">Additional details about the activity (optional).</param>
	/// <param name="currency">The identifier of the currency (optional).</param>
	public async Task Log(
		FundActivity.Type activity,
		Guid fund,
		Guid user,
		FundTransaction.Type? transactionType = default,
		double? amount = default,
		string? details = default,
		Guid? currency = default
	) {
		// Ensure amount is set to 0 for non-create/delete activities if null
		if (activity is not (FundActivity.Type.CreateFund or FundActivity.Type.DeleteFund) && amount == null) {
			amount = 0;
			FileLogger.Log($"WARNING!!: In {FundActivity.Value(activity)}, amount is null");
		}
		
		// Create a new activity log entity
		var log = new ActivityLog {
			Activity = FundActivity.Value(activity),
			CurrencyId = currency,
			Amount = amount,
			Details = details,
			FundId = fund,
			UserId = user,
			TransactionType = FundTransaction.Value(transactionType)
		};
		
		// Add the log to the database and save changes
		await dbContext.ActivityLogs.AddAsync(log);
	}
	
}