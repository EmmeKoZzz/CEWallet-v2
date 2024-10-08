using System.Linq.Expressions;
using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.DataTransferObjects.Filters;
using ApiServices.Helpers;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services;

public class ActivityLogService(AppDbContext dbContext) {
	
	DbSet<ActivityLog> Repo { get; } = dbContext.ActivityLogs;
	
	/// <summary> Retrieves a paginated list of activity logs based on specified filters and ordering.</summary>
	/// <param name="page">The zero-based page number of the results to retrieve.</param>
	/// <param name="limit">The maximum number of items to return per page.</param>
	/// <param name="filter">Optional. An ActivityLogFilter object containing criteria to filter the results.</param>
	/// <returns>
	/// A Task that represents the asynchronous operation. The task result contains a PaginationDto&lt;ActivityLogDto&gt;
	/// representing the paginated list of activity logs matching the specified criteria.
	/// </returns>
	public async Task<PaginationDto<ActivityLogDto>> GetAll(int page, int limit, ActivityLogFilter? filter = null) {
		var query = Repo.Include(entity => entity.User).Include(entity => entity.Fund).Include(entity => entity.Currency).AsQueryable();
		
		ApplyFilters();
		ApplyOrdering();
		
		var totalCount = await query.CountAsync();
		var paginatedResults = await query.Skip(page * limit).Take(limit).ToArrayAsync();
		
		return new(paginatedResults.Select(MapToActivityLogDto), page, limit, totalCount);
		
		void ApplyFilters() {
			if (filter == null) return;
			if (filter.Since.HasValue) query = query.Where(entity => entity.CreatedAt >= filter.Since);
			if (filter.Until.HasValue) query = query.Where(entity => entity.CreatedAt <= filter.Until);
			if (filter.AmountMin.HasValue) query = query.Where(entity => entity.Amount >= filter.AmountMin);
			if (filter.AmountMax.HasValue) query = query.Where(entity => entity.Amount <= filter.AmountMax);
			if (filter.Activities?.Length > 0) query = query.Where(entity => filter.Activities.Contains(entity.Activity));
			if (filter.FundTransactions?.Length > 0) query = query.Where(entity => filter.FundTransactions.Contains(entity.TransactionType));
			if (filter.Currencies?.Length > 0) query = query.Where(entity => filter.Currencies.Contains(entity.CurrencyId ?? new Guid()));
			
			if (filter.Funds?.Length > 0) {
				Expression<Func<ActivityLog, bool>> condition = log => log.Fund.Name.Contains(filter.Funds[0]);
				condition = filter.Funds.Aggregate(condition, (c, fundName) => c.Or(log => log.Fund.Name.Contains(fundName)));
				query = query.Where(condition);
			}
			
			if (filter.Users?.Length > 0) {
				Expression<Func<ActivityLog, bool>> condition = log => log.User.Username.Contains(filter.Users[0]);
				condition = filter.Users.Aggregate(condition, (c, username) => c.Or(log => log.User.Username.Contains(username)));
				query = query.Where(condition);
			}
		}
		
		void ApplyOrdering() {
			query = filter?.AmountOrCreateDate switch {
				true when filter.Desc => query.OrderByDescending(entity => entity.Amount),
				true when !filter.Desc => query.OrderBy(entity => entity.Amount),
				false when !filter.Desc => query.OrderBy(entity => entity.CreatedAt),
				_ => query.OrderByDescending(entity => entity.CreatedAt)
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
		await Repo.AddAsync(log);
	}
	
	static ActivityLogDto MapToActivityLogDto(ActivityLog entity) {
		return new(
			entity.Id,
			entity.User.Username,
			entity.Fund.Name,
			entity.Currency?.Name,
			entity.Activity,
			entity.TransactionType,
			entity.Amount,
			entity.Details,
			entity.CreatedAt
		);
	}
	
}