using System.Linq.Expressions;
using ApiServices.Configuration;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.DataTransferObjects.Filters;
using ApiServices.Helpers;
using ApiServices.Helpers.Structs;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.HttpStatusCode;

namespace ApiServices.Services;

public class FundService(AppDbContext dbContext) {
	private DbSet<Fund> Repo { get; } = dbContext.Funds;

	public async Task<PaginationDto<FundDto>> GetAllDev(int page, int size, FundFilter? filter) {
		var query = Repo.Where(entity => entity.Active).
			Include(entity => entity.FundCurrencies).
			ThenInclude(entity => entity.Currency).
			Include(entity => entity.User).
			ThenInclude(u => u!.Role).
			AsQueryable();

		ApplyFilters();
		ApplyOrdering();

		var res = await query.Skip(page * size).Take(size).ToArrayAsync();

		return new PaginationDto<FundDto>(res.Select(CreateFundDto), page, size, await query.CountAsync());

		void ApplyFilters() {
			if (filter == null) return;
			if (filter.FundNames?.Length > 0) {
				Expression<Func<Fund, bool>> cond = fund => fund.Name.Contains(filter.FundNames[0]);
				cond = filter.FundNames.Skip(1).Aggregate(cond, (c, fn) => c.Or(fund => fund.Name.Contains(fn)));
				query = query.Where(cond);
			}

			if (filter.Usernames?.Length > 0) {
				Expression<Func<Fund, bool>> cond = fund =>
					fund.User != null && fund.User.Username.Contains(filter.Usernames[0]);
				cond = filter.Usernames.Skip(1).
					Aggregate(cond, (c, n) => c.Or(fund => fund.User != null && fund.User.Username.Contains(n)));
				query = query.Where(cond);
			}

			if (filter.Currencies?.Length > 0) {
				Expression<Func<Fund, bool>> cond = fund =>
					fund.FundCurrencies.Any(c => c.CurrencyId == filter.Currencies[0]);
				cond = filter.Currencies.Aggregate(
					cond,
					(c, currencyId) => c.Or(fund => fund.FundCurrencies.Any(fc => fc.CurrencyId == currencyId)));

				query = query.Where(cond);
			}
		}

		void ApplyOrdering() {
			query = filter?.OrderBy switch {
				FundFilter.OrderByOptions.Usernames when filter.Descending => query.OrderByDescending(
					entity => entity.User!.Username),
				FundFilter.OrderByOptions.Usernames when !filter.Descending => query.OrderBy(
					entity => entity.User!.Username),
				FundFilter.OrderByOptions.CreateAt when filter.Descending => query.OrderByDescending(
					entity => entity.CreatedAt),
				FundFilter.OrderByOptions.CreateAt when !filter.Descending => query.OrderBy(entity => entity.CreatedAt),
				_ => filter?.Descending switch {
					false => query.OrderByDescending(entity => entity.Name),
					_ => query.OrderBy(entity => entity.Name)
				}
			};
		}
	}

	public async Task<IEnumerable<FundDto>> GetAll() {
		var res = await Repo.Where(entity => entity.Active).ToArrayAsync();

		return res.Select(CreateFundDto);
	}

	public async Task<ServiceFlag<FundDto>> Get(Guid id) {
		var res = await Repo.Include(entity => entity.User).
			ThenInclude(entity => entity!.Role).
			Include(fund => fund.FundCurrencies).
			ThenInclude(fundCurrency => fundCurrency.Currency).
			SingleOrDefaultAsync(entity => entity.Active && entity.Id == id);

		return res != null ? new ServiceFlag<FundDto>(OK, CreateFundDto(res)) : new ServiceFlag<FundDto>(NotFound);
	}

	public async Task<IEnumerable<FundDto>> GetByUser(Guid id) {
		var res = await Repo.Where(entity => entity.Active && entity.UserId == id).
			Include(entity => entity.FundCurrencies).
			ThenInclude(entity => entity.Currency).
			ToArrayAsync();

		return res.Select(CreateFundDto);
	}

	public async Task<FundDto> Add(AddFundDto info) {
		var fund = new Fund {
			Name = info.Name, LocationUrl = info.LocationUrl, Address = info.Address, Details = info.Details
		};

		await Repo.AddAsync(fund);

		return CreateFundDto(fund);
	}

	public async Task<ServiceFlag<FundDto>> Update(AddFundDto info, Guid id) {
		var fund = await Repo.FindAsync(id);

		if (fund == null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		fund.Name = info.Name;
		fund.LocationUrl = info.LocationUrl;
		fund.Address = info.Address;
		fund.Details = info.Details;

		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	public async Task<ServiceFlag<TransferDto.Response>> Transfer(TransferDto info) {
		if (info.Destination == info.Source)
			return new ServiceFlag<TransferDto.Response>(BadRequest, Message: "Destination is the same Source.");

		var query = Repo.Include(entity => entity.FundCurrencies).ThenInclude(entity => entity.Currency);

		var (fromFund, toFund) = (await query.SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Source),
			await query.SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Destination));

		if (fromFund == null || toFund == null)
			return new ServiceFlag<TransferDto.Response>(
				NotFound,
				Message: $"Fund with ID: {(fromFund == null ? info.Source : info.Destination)} not found.");

		var fromFundCurrency = fromFund.FundCurrencies.SingleOrDefault(currency => currency.CurrencyId == info.Currency);

		if (fromFundCurrency == null || fromFundCurrency.Amount < info.Amount)
			return new ServiceFlag<TransferDto.Response>(
				BadRequest,
				Message: "Fund has not enough of this currency to make this operation.");

		if (fromFundCurrency.Amount - info.Amount is 0) dbContext.FundCurrencies.Remove(fromFundCurrency);
		else fromFundCurrency.Amount -= info.Amount;

		var toFundCurrency = toFund.FundCurrencies.SingleOrDefault(currency => currency.CurrencyId == info.Currency);
		if (toFundCurrency == null) {
			toFundCurrency =
				new FundCurrency { FundId = info.Destination, CurrencyId = info.Currency, Amount = info.Amount };
			toFund.FundCurrencies.Add(toFundCurrency);
		} else { toFundCurrency.Amount += info.Amount; }

		var fromFundDto = CreateFundDto(fromFund);
		var toFundDto = CreateFundDto(toFund);

		return new ServiceFlag<TransferDto.Response>(OK, new TransferDto.Response(fromFundDto, toFundDto));
	}

	public async Task<ServiceFlag<FundDto>> Withdraw(TransactionDto info) {
		var fund = await Repo.Include(entity => entity.FundCurrencies).
			ThenInclude(fundCurrency => fundCurrency.Currency).
			SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Source);

		if (fund == null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		var currency = fund.FundCurrencies.SingleOrDefault(entity => entity.CurrencyId == info.Currency);

		if (currency == null || currency.Amount < info.Amount)
			return new ServiceFlag<FundDto>(
				BadRequest,
				Message: "Fund has not enough of this currency to make this operation.");

		if (currency.Amount - info.Amount is 0) dbContext.FundCurrencies.Remove(currency);
		else currency.Amount -= info.Amount;

		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	public async Task<ServiceFlag<FundDto>> Deposit(TransactionDto info) {
		var fund = await Repo.Include(entity => entity.FundCurrencies). // Include related FundCurrency entities  
			ThenInclude(fundCurrency => fundCurrency.Currency). // Include the Currency details for each FundCurrency  
			SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Source); // Search for the fund by ID  

		if (fund == null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		var test = await dbContext.Currencies.FindAsync(info.Currency);
		if (test == null) return new ServiceFlag<FundDto>(BadRequest, Message: "This currency doesn't exist.");

		var currency = fund.FundCurrencies.SingleOrDefault(entity => entity.CurrencyId == info.Currency);

		if (currency == null)
			fund.FundCurrencies.Add(
				new FundCurrency { CurrencyId = info.Currency, FundId = fund.Id, Amount = info.Amount });
		else currency.Amount += info.Amount;

		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	public async Task<ServiceFlag<FundDto>> AttachUser(Guid userId, Guid fundId) {
		var fund = await Repo.SingleOrDefaultAsync(entity => entity.Active && entity.Id == fundId);
		var user = userId != Guid.Empty
			? await dbContext.Users.Include(entity => entity.Role).SingleOrDefaultAsync(entity => entity.Id == userId)
			: null;

		if (fund == null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		if (userId == Guid.Empty) fund.UserId = null;
		else if (user == null) return new ServiceFlag<FundDto>(NotFound, Message: "User not found.");
		else fund.UserId = user.Id;

		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	public async Task<ServiceFlag<FundDto>> Delete(Guid id) {
		var fund = await Repo.SingleOrDefaultAsync(entity => entity.Active && entity.Id == id);
		if (fund is null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		fund.Name += $" CERRADO {DateTime.Now.ToShortDateString()}";
		fund.Active = false;

		dbContext.FundCurrencies.RemoveRange(dbContext.FundCurrencies.Where(entity => entity.FundId == id));

		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	#region Helpers

	private static FundDto CreateFundDto(Fund fund) {
		return new FundDto(
			fund.Id,
			fund.Name,
			fund.CreatedAt,
			fund.LocationUrl,
			fund.Address,
			fund.Details,
			fund.FundCurrencies?.Select(c => new FundDto.CurrencyAmount(c.Currency.Name, c.Amount)),
			fund.User is not null
				? new UserDto(fund.User.Id, fund.User.Username, fund.User.Email, fund.User.Role.Name, fund.User.CreatedAt)
				: default);
	}

	#endregion
}