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
	private DbSet<ActivityLog> Repo { get; } = dbContext.ActivityLogs;

	public async Task<PaginationDto<ActivityLogDto>> GetAll(int page,
		int size,
		ActivityLogFilter? filter = null,
		User? assessor = null) {
		var query = Repo.Include(entity => entity.User).
			Include(entity => entity.Fund).
			Include(entity => entity.Currency).
			AsQueryable();

		if (assessor != null && assessor.Role.Name == UserRole.Value(UserRole.Type.Assessor)) {
			filter ??= new ActivityLogFilter();
			await dbContext.Entry(assessor).Collection(u => u.Funds).LoadAsync();
			filter.Funds = [..assessor.Funds.Select(funds => funds.Name), ..filter.Funds ?? []];
			if (filter.Funds.Length == 0) { return new PaginationDto<ActivityLogDto>([], page, size, 0); }
		}

		ApplyFilters();
		ApplyOrdering();

		var totalCount = await query.CountAsync();
		var paginatedResults = await query.Skip(page * size).Take(size).ToArrayAsync();

		return new PaginationDto<ActivityLogDto>(paginatedResults.Select(MapToActivityLogDto), page, size, totalCount);

		void ApplyFilters() {
			if (filter == null) return;
			if (filter.Since.HasValue) query = query.Where(entity => entity.CreatedAt >= filter.Since);
			if (filter.Until.HasValue) query = query.Where(entity => entity.CreatedAt <= filter.Until);
			if (filter.AmountMin.HasValue) query = query.Where(entity => entity.Amount >= filter.AmountMin);
			if (filter.AmountMax.HasValue) query = query.Where(entity => entity.Amount <= filter.AmountMax);
			if (filter.Activities?.Length > 0) query = query.Where(entity => filter.Activities.Contains(entity.Activity));
			if (filter.FundTransactions?.Length > 0)
				query = query.Where(entity => filter.FundTransactions.Contains(entity.TransactionType));
			if (filter.Currencies?.Length > 0)
				query = query.Where(entity => filter.Currencies.Contains(entity.CurrencyId ?? new Guid()));

			if (filter.Funds?.Length > 0) {
				Expression<Func<ActivityLog, bool>> condition = log => log.Fund.Name.Contains(filter.Funds[0]);
				condition = filter.Funds.Aggregate(
					condition,
					(c, fundName) => c.Or(log => log.Fund.Name.Contains(fundName)));
				query = query.Where(condition);
			}

			if (filter.Users?.Length > 0) {
				Expression<Func<ActivityLog, bool>> condition = log => log.User.Username.Contains(filter.Users[0]);
				condition = filter.Users.Aggregate(
					condition,
					(c, username) => c.Or(log => log.User.Username.Contains(username)));
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

	public async Task Log(FundActivity.Type activity,
		Guid fund,
		Guid user,
		FundTransaction.Type? transactionType = default,
		double? amount = default,
		string? details = default,
		Guid? currency = default) {
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

		await Repo.AddAsync(log);
	}

	#region Helpers

	private static ActivityLogDto MapToActivityLogDto(ActivityLog entity) {
		return new ActivityLogDto(
			entity.Id,
			entity.User.Active ? entity.User.Username : entity.User.Username[..^36],
			entity.Fund.Active ? entity.Fund.Name : entity.Fund.Name[..^36],
			entity.Currency?.Name,
			entity.Activity,
			entity.TransactionType,
			entity.Amount,
			entity.Details,
			entity.CreatedAt);
	}

	#endregion
}