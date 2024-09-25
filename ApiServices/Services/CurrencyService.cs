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
		public async Task<IEnumerable<CurrencyDto>> GetAll(bool funds = false)
		{
			// If fonds are not requested, return only the active currencies.  
			if (!funds)
				return await dbContext.Currencies
					.Where(entity => entity.Active)
					.Select(entity => new CurrencyDto
					{
						Currency = entity.Name, // Map currency name to DTO property  
						Id = entity.Id, // Map currency ID to DTO property  
						ValueUrl = entity.ValueUrl // Map value URL to DTO property  
					})
					.ToArrayAsync(); // Execute the query asynchronously and convert the results to an array.  

			// If funds are requested, retrieve currencies along with their associated funds and fund currencies.  
			var res = await dbContext.Currencies
				.Where(entity => entity.Active) // Filter to include only active currencies  
				.Include(e => e.FundCurrencies) // Include related FundCurrency entities  
				.ThenInclude(fc => fc.Fund) // Include related Fund entities from FundCurrency  
				.ThenInclude(f => f.FundCurrencies) // Include currencies from the Fund  
				.ThenInclude(fc => fc.Currency) // Include the Currency for each FundCurrency  
				.ToArrayAsync(); // Execute the query asynchronously and convert the results to an array.  

			// Map the retrieved currencies and their associated funds to DTOs.  
			return res.Select(entity => new CurrencyDto
			{
				Currency = entity.Name, // Map currency name to DTO property  
				Id = entity.Id, // Map currency ID to DTO property  
				ValueUrl = entity.ValueUrl, // Map value URL to DTO property  
				Funds = entity.FundCurrencies.Select(currencies => new FundDto(
					currencies.FundId, // Map Fund ID to DTO  
					currencies.Fund.Name, // Map Fund name to DTO  
					currencies.Fund.CreatedAt, // Map Fund creation date to DTO  
					LocationUrl: currencies.Fund.LocationUrl, // Map Fund location URL to DTO  
					Currencies: currencies.Fund.FundCurrencies.Select(currency =>
						new FundDto.FundCurrency(currency.Currency.Name, currency.Amount)) // Map currencies for the fund  
				))
			});
		}

		/// <summary>Adds a new currency.</summary>
		public async Task<ServiceFlag<CurrencyDto>> Add(AddCurrencyDto info)
		{
			var currency = await dbContext.Currencies.SingleOrDefaultAsync(entity => entity.Name == info.Name);

			switch (currency)
			{
				case { Active: false }:
					currency.Active = true;
					break;
				case { Active: true }:
					return new ServiceFlag<CurrencyDto>(HttpStatusCode.BadRequest, Message: "Currency already exists.");
				default:
					currency = new Currency { Name = info.Name, ValueUrl = info.UrlValue };
					await dbContext.Currencies.AddAsync(currency);
					break;
			}

			await dbContext.SaveChangesAsync();
			return new ServiceFlag<CurrencyDto>(HttpStatusCode.OK, new CurrencyDto
				{ Id = currency.Id, Funds = [], Currency = currency.Name, ValueUrl = currency.ValueUrl });
		}

		/// <summary>Updates a currency.</summary>
		public async Task<ServiceFlag<CurrencyDto>> Update(AddCurrencyDto info, Guid id)
		{
			var currency = await dbContext.Currencies.FindAsync(id);
			if (currency == null) return new ServiceFlag<CurrencyDto>(HttpStatusCode.NotFound);

			currency.Name = info.Name;
			currency.ValueUrl = info.UrlValue;

			await dbContext.SaveChangesAsync();
			return new ServiceFlag<CurrencyDto>(HttpStatusCode.OK,
				new CurrencyDto { Id = currency.Id, Currency = currency.Name, ValueUrl = currency.ValueUrl });
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
					Currency = currency.Name, Funds = new List<FundDto>(), Id = Guid.Empty, ValueUrl = currency.ValueUrl
				});
		}
	}
}