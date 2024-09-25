using System.Net;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using Common.DataTransferObjects;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.EntityFrameworkCore;

namespace ApiServices.Services
{
	public class CurrencyService(AppDbContext dbContext)
	{
		/// <summary>Retrieves currencies (optional with funds).</summary>
		public async Task<IEnumerable<CurrencyDto>> GetAll(bool funds = false) =>
			funds
				? await dbContext.Currencies
					.Include(e => e.FundCurrencies)
					.ThenInclude(fc => fc.Fund)
					.ThenInclude(f => f.FundCurrencies)
					.ThenInclude(fc => fc.Currency)
					.Select(entity => new CurrencyDto
					{
						Currency = entity.Name,
						Id = entity.Id,
						ValorUrl = entity.ValueUrl,
						Funds = entity.FundCurrencies.Select(fundCurrency => new FundDto
						{
							Id = fundCurrency.FundId,
							Name = fundCurrency.Fund.Name,
							LocationUrl = fundCurrency.Fund.LocationUrl,
							CreateAt = fundCurrency.Fund.CreatedAt,
							Currencies = fundCurrency.Fund.FundCurrencies.Select(currency =>
								new FundDto.FundCurrency(currency.Currency.Name, currency.Amount))
						})
					})
					.ToArrayAsync()
				: await dbContext.Currencies.Select(entity => new CurrencyDto
					{
						Currency = entity.Name,
						Id = entity.Id,
						ValorUrl = entity.ValueUrl,
					})
					.ToArrayAsync();

		/// <summary>Adds a new currency.</summary>
		public async Task<CurrencyDto> Add(AddCurrencyDto info)
		{
			var currency = await dbContext.Currencies.SingleOrDefaultAsync(entity => entity.Name == info.Name);
			if (currency is { Active: false })
			{
				currency.Active = true;
			}
			else
			{
				currency = new Currency { Name = info.Name, ValueUrl = info.ValorUrl };
				await dbContext.Currencies.AddAsync(currency);
			}

			await dbContext.SaveChangesAsync();
			return new CurrencyDto
				{ Id = currency.Id, Funds = new List<FundDto>(), Currency = info.Name, ValorUrl = info.ValorUrl };
		}

		/// <summary>Updates a currency.</summary>
		public async Task<ServiceFlag<CurrencyDto>> Update(AddCurrencyDto info, Guid id)
		{
			var currency = await dbContext.Currencies.FindAsync(id);
			if (currency == null) return new ServiceFlag<CurrencyDto>(HttpStatusCode.NotFound);

			currency.Name = info.Name;
			currency.ValueUrl = info.ValorUrl;

			await dbContext.SaveChangesAsync();
			return new ServiceFlag<CurrencyDto>(HttpStatusCode.OK,
				new CurrencyDto { Id = currency.Id, Currency = currency.Name, ValorUrl = currency.ValueUrl });
		}

		/// <summary>Deletes a currency.</summary>
		public async Task<ServiceFlag<CurrencyDto>> Delete(Guid id)
		{
			var currency = await dbContext.Currencies.FindAsync(id);
			if (currency == null) return new ServiceFlag<CurrencyDto>(HttpStatusCode.NotFound);
			var trx = await dbContext.Database.BeginTransactionAsync();
			try
			{
				// TODO Log all funds that have this currency to 0 (Activity Log Table)
				dbContext.FundCurrencies.RemoveRange(dbContext.FundCurrencies.Where(entity => entity.CurrencyId == id));
				currency.Active = false;
				await dbContext.SaveChangesAsync();
				await trx.CommitAsync();
			}
			catch (Exception e)
			{
				await trx.RollbackAsync();
				return new ServiceFlag<CurrencyDto>(HttpStatusCode.InternalServerError, Message: e.Message);
			}

			return new ServiceFlag<CurrencyDto>(HttpStatusCode.OK,
				new CurrencyDto
				{
					Currency = currency.Name, Funds = new List<FundDto>(), Id = Guid.Empty, ValorUrl = currency.ValueUrl
				});
		}
	}
}