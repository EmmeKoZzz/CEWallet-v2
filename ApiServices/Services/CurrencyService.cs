using AngleSharp;
using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using Common.DataTransferObjects;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.EntityFrameworkCore;
using static System.Net.HttpStatusCode;

namespace ApiServices.Services;

public class CurrencyService(AppDbContext dbContext)
{
	/// <summary>Retrieves informal foreign exchange rates from a public website (eltoque.com).</summary>
	/// <returns>A dictionary containing informal foreign exchange rates.</returns>
	public static async Task<Dictionary<string, float>> InformalForeignExchange()
	{
		// Configure AngleSharp for web scraping
		var config = AngleSharp.Configuration.Default.WithDefaultLoader();
		var browsingContext = BrowsingContext.New(config);

		// Download the webpage content
		var document = await browsingContext.OpenAsync("https://eltoque.com");

		// Define the CSS selector for table rows containing currency data
		const string selector =
			"#super-page-wrapper > div:nth-child(5) > div > div > div.sc-540929e5-0.fQaxen > div.content > table > tbody tr";
		var rows = document.QuerySelectorAll(selector);

		// Initialize dictionary to store currency data
		var currencyData = new Dictionary<string, float>();

		// Loop through each table row
		foreach (var row in rows)
		{
			// Extract currency code and price elements
			var currencyElement = row.QuerySelector(".name-cell .currency");
			var priceElement = row.QuerySelector(".price-cell .price-text");

			// Check if both elements are found, skip if not
			if (currencyElement == null || priceElement == null) continue;

			// Extract currency code (remove leading character and trim)
			var currency = currencyElement.TextContent[1..].Trim();

			// Extract price text and convert to float
			var priceText = priceElement.TextContent.Trim();
			if (float.TryParse(priceText.Split(' ')[0], out var price))
				currencyData[currency] = price;
		}

		// Return the dictionary containing informal foreign exchange rates
		return currencyData;
	}

	/// <summary>Retrieves all active currencies from the database, with an optional flag to include related fund information.</summary>  
	/// <param name="funds">Indicates whether to include fund-related data in the response.</param>  
	/// <returns>An asynchronous task returning an enumerable collection of CurrencyDto objects.</returns>  
	public async Task<CurrencyDto[]> GetAll(bool funds = false)
	{
		// Create a query to retrieve only active currencies.  
		var query = dbContext.Currencies
			.Where(entity => entity.Active)
			.AsQueryable();

		// If the 'funds' parameter is true, include related fund data in the query.  
		if (funds)
			query = query
				.Include(e => e.FundCurrencies) // Include FundCurrencies related to Currencies.  
				.ThenInclude(fc => fc.Fund) // Include the Fund related to each FundCurrency.  
				.ThenInclude(f => f.FundCurrencies) // Include FundCurrencies related to those Funds.  
				.ThenInclude(fc => fc.Currency); // Include Currency related to those FundCurrencies.  

		// Execute the query and fetch all matching currencies asynchronously.  
		var currencies = await query.ToListAsync();

		// Map the retrieved Currency entities to CurrencyDto objects.  
		var tasks = currencies.Select(entity => MapCurrencyToDto(entity, funds, true));
		return await Task.WhenAll(tasks); // Await the completion of all mapping tasks.  
	}

	/// <summary>Adds a new currency to the database or reactivates an existing inactive currency.</summary>  
	/// <param name="info">The information for the currency to be added.</param>  
	/// <returns>An asynchronous task returning a ServiceFlag containing the result of the operation.</returns>  
	public async Task<ServiceFlag<CurrencyDto>> Add(AddCurrencyDto info)
	{
		// Check if the currency already exists in the database.  
		var currency = await dbContext.Currencies.SingleOrDefaultAsync(entity => entity.Name == info.Name);

		switch (currency)
		{
			// If the currency exists but is inactive, reactivate it.  
			case { Active: false }:
				currency.Active = true;
				break;
			// If the currency is already active, return a BadRequest response.  
			case { Active: true }:
				return new ServiceFlag<CurrencyDto>(BadRequest, Message: "Currency already exists.");
			// If the currency does not exist, create a new Currency entity.  
			default:
				currency = new Currency { Name = info.Name };
				await dbContext.Currencies.AddAsync(currency);
				break;
		}

		// Save the changes to the database.  
		await dbContext.SaveChangesAsync();
		// Return the result, including the newly added or reactivated currency.  
		return new ServiceFlag<CurrencyDto>(OK, await MapCurrencyToDto(currency));
	}

	/// <summary>Updates an existing currency in the database.</summary>  
	/// <param name="info">The updated currency information.</param>  
	/// <param name="id">The unique identifier of the currency to be updated.</param>  
	/// <returns>An asynchronous task returning a ServiceFlag containing the result of the operation.</returns>  
	public async Task<ServiceFlag<CurrencyDto>> Update(AddCurrencyDto info, Guid id)
	{
		// Find the currency by its unique identifier.  
		var currency = await dbContext.Currencies.FindAsync(id);
		// If the currency does not exist, return a NotFound response.  
		if (currency == null) return new ServiceFlag<CurrencyDto>(NotFound);

		// Update the currency's properties with the provided info.  
		currency.Name = info.Name;

		// Save the changes to the database.  
		await dbContext.SaveChangesAsync();
		// Return the result, confirming the update was successful.  
		return new ServiceFlag<CurrencyDto>(OK, await MapCurrencyToDto(currency));
	}

	/// <summary>  
	/// Deletes a currency from the database by marking it as inactive.  
	/// </summary>  
	/// <param name="id">The unique identifier of the currency to be deleted.</param>  
	/// <returns>An asynchronous task returning a ServiceFlag containing the result of the operation.</returns>  
	public async Task<ServiceFlag<CurrencyDto>> Delete(Guid id)
	{
		// Find the currency by its unique identifier.  
		var currency = await dbContext.Currencies.FindAsync(id);
		// If the currency does not exist, return a NotFound response.  
		if (currency == null) return new ServiceFlag<CurrencyDto>(NotFound);

		// Begin a database transaction for the deletion process.  
		var trx = await dbContext.Database.BeginTransactionAsync();
		try
		{
			// TODO: Log all funds that have this currency to 0 (Activity Log Table).  
			// Remove all FundCurrencies associated with this currency.  
			dbContext.FundCurrencies.RemoveRange(dbContext.FundCurrencies.Where(entity => entity.CurrencyId == id));
			// Mark the currency as inactive (soft delete).  
			currency.Active = false;

			// Save the changes to the database and commit the transaction.  
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
		}
		catch (Exception e)
		{
			// If an error occurs, roll back the transaction and return an error response.  
			await trx.RollbackAsync();
			return new ServiceFlag<CurrencyDto>(InternalServerError, Message: e.Message);
		}

		// Return the result, indicating the deletion was successful.  
		return new ServiceFlag<CurrencyDto>(OK, await MapCurrencyToDto(currency));
	}

	/* HELPERS... */
	/// <summary> Maps a Currency entity to a CurrencyDto object.</summary>  
	/// <param name="entity">The Currency entity to be mapped.</param>  
	/// <param name="funds">A flag indicating whether to include fund information in the DTO.</param>
	/// <param name="balance">A flag indicating whether to include total balance information in the DTO.</param>
	/// <returns>A Task representing the asynchronous operation, containing the mapped CurrencyDto.</returns>  
	private async Task<CurrencyDto> MapCurrencyToDto(Currency entity, bool funds = false, bool balance = false)
	{
		// Create the CurrencyDto object and map basic properties.  
		var dto = new CurrencyDto
		{
			Currency = entity.Name, // Set the name of the currency.  
			Id = entity.Id, // Set the unique identifier of the currency.  
		};

		// If total balance information is requested, get it.  
		if (balance)
			dto.TotalBalance = await GetTotalBalance(entity); // Set the total balance calculated from FundCurrencies.  

		// If funds information is requested, map the associated fund currencies.  
		if (funds)
		{
			dto.Funds = entity.FundCurrencies.Select(currencies => new FundDto(
				currencies.FundId,
				currencies.Fund.Name,
				currencies.Fund.CreatedAt,
				currencies.Fund.LocationUrl,
				currencies.Fund.Address,
				currencies.Fund.Details,
				currencies.Fund.FundCurrencies.Select(currency =>
					new FundDto.FundCurrency(currency.Currency.Name, currency.Amount)) // Map FundCurrency information.  
			)).ToList(); // Materialize the collection to avoid multiple enumerations later.  
		}

		return dto; // Return the fully constructed CurrencyDto.  
	}

	/// <summary>Calculates the total balance of a currency in the database by summing amounts from FundCurrencies.</summary>  
	/// <param name="currency">The Currency object for which to calculate the total balance.</param>  
	/// <returns>A Task representing the asynchronous operation, containing the total balance as a double.</returns>  
	private async Task<double> GetTotalBalance(Currency currency) =>
		await dbContext.FundCurrencies
			.Where(entity => entity.CurrencyId == currency.Id) // Filter FundCurrencies by the specified currency ID.  
			.SumAsync(entity => entity.Amount); // Sum the Amounts of the filtered FundCurrencies.
}