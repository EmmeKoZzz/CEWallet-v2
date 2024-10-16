using AngleSharp;
using ApiServices.Configuration;
using ApiServices.Constants;
using ApiServices.DataTransferObjects;
using ApiServices.DataTransferObjects.ApiResponses;
using ApiServices.Helpers;
using ApiServices.Helpers.Structs;
using ApiServices.Models;
using Microsoft.EntityFrameworkCore;
using static System.Net.HttpStatusCode;

namespace ApiServices.Services;

public class CurrencyService(AppDbContext dbContext, ActivityLogService logService) {
	public static async Task<Dictionary<string, float>> InformalForeignExchange() {
		var config = AngleSharp.Configuration.Default.WithDefaultLoader();
		var browsingContext = BrowsingContext.New(config);

		var document = await browsingContext.OpenAsync("https://eltoque.com");

		const string selector =
			"#super-page-wrapper > div:nth-child(5) > div > div > div.sc-540929e5-0.fQaxen > div.content > table > tbody tr";

		var rows = document.QuerySelectorAll(selector);

		var currencyData = new Dictionary<string, float>();

		foreach (var row in rows) {
			var currencyElement = row.QuerySelector(".name-cell .currency");
			var priceElement = row.QuerySelector(".price-cell .price-text");

			if (currencyElement == null || priceElement == null) continue;

			var currency = currencyElement.TextContent[1..].Trim();

			var priceText = priceElement.TextContent.Trim();
			if (float.TryParse(priceText.Split(' ')[0], out var price)) currencyData[currency] = price;
		}

		return currencyData;
	}

	public async Task<List<CurrencyDto>> GetAll(bool funds = false) {
		var query = dbContext.Currencies.Where(entity => entity.Active).AsQueryable();

		if (funds)
			query = query.Include(e => e.FundCurrencies).
				ThenInclude(fc => fc.Fund).
				ThenInclude(f => f.FundCurrencies).
				ThenInclude(fc => fc.Currency);
		var currencies = await query.ToListAsync();

		var response = new List<CurrencyDto>();
		foreach (var currency in currencies) response.Add(await MapCurrencyToDto(currency, funds, true));

		return response; // Await the completion of all mapping tasks.  
	}

	public async Task<ServiceFlag<CurrencyDto>> Add(AddCurrencyDto info) {
		var currency = await dbContext.Currencies.SingleOrDefaultAsync(entity => entity.Name == info.Name);

		switch (currency) {
			case {
				Active: false
			}:
				currency.Active = true;

				break;
			case {
				Active: true
			}: return new ServiceFlag<CurrencyDto>(BadRequest, Message: "Currency already exists.");
			default:
				currency = new Currency { Name = info.Name };
				await dbContext.Currencies.AddAsync(currency);

				break;
		}

		return new ServiceFlag<CurrencyDto>(OK, await MapCurrencyToDto(currency));
	}

	public async Task<ServiceFlag<CurrencyDto>> Update(AddCurrencyDto info, Guid id) {
		var currency = await dbContext.Currencies.FindAsync(id);

		if (currency == null) return new ServiceFlag<CurrencyDto>(NotFound);

		currency.Name = info.Name;

		return new ServiceFlag<CurrencyDto>(OK, await MapCurrencyToDto(currency));
	}

	public async Task<ServiceFlag<(CurrencyDto, Tuple<Guid, double>[])>> Delete(Guid id) {
		var currency = await dbContext.Currencies.FindAsync(id);

		if (currency == null) return new ServiceFlag<(CurrencyDto, Tuple<Guid, double>[])>(NotFound);
		var fundsRelated = dbContext.FundCurrencies.Where(entity => entity.CurrencyId == id);
		var fundIds = await fundsRelated.Select(entity => new Tuple<Guid, double>(entity.FundId, entity.Amount)).
			ToArrayAsync();

		dbContext.FundCurrencies.RemoveRange(fundsRelated);
		currency.Active = false;

		return new ServiceFlag<(CurrencyDto, Tuple<Guid, double>[])>(OK, (await MapCurrencyToDto(currency), fundIds));
	}


	#region Helpers

	private async Task<CurrencyDto> MapCurrencyToDto(Currency entity, bool funds = false, bool balance = false) {
		var dto = new CurrencyDto { Currency = entity.Name, Id = entity.Id };

		if (balance) dto.TotalBalance = await GetTotalBalance(entity);

		if (funds)
			dto.Funds = entity.FundCurrencies.Select(
					currencies => new FundDto(
						currencies.FundId,
						currencies.Fund.Name,
						currencies.Fund.CreatedAt,
						currencies.Fund.LocationUrl,
						currencies.Fund.Address,
						currencies.Fund.Details,
						currencies.Fund.FundCurrencies.Select(
							currency => new FundDto.CurrencyAmount(currency.Currency.Name, currency.Amount)))).
				ToList();

		return dto;
	}

	private async Task<double> GetTotalBalance(Currency currency) {
		return await dbContext.FundCurrencies.Where(entity => entity.CurrencyId == currency.Id).
			SumAsync(entity => entity.Amount);
	}

	#endregion
}