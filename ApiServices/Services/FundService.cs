using ApiServices.Configuration;
using ApiServices.Helpers;
using ApiServices.Models;
using Common.DataTransferObjects;
using Common.DataTransferObjects.ApiResponses;
using Microsoft.EntityFrameworkCore;
using static System.Net.HttpStatusCode;

namespace ApiServices.Services;

public class FundService(AppDbContext dbContext)
{
	/// <summary> Retrieves a collection of all active funds. </summary>
	/// <returns>An array of FundDto objects representing all active funds.</returns>
	public async Task<IEnumerable<FundDto>> GetAll()
	{
		var res = await dbContext.Funds.Where(entity => entity.Active).ToArrayAsync();
		return res.Select(CreateFundDto);
	}

	/// <summary> Retrieves a specific fund by its ID. </summary>
	/// <param name="id">The unique identifier of the fund.</param>
	/// <returns>A ServiceFlag object containing either a FundDto representing the retrieved fund on success (with OK), 
	/// or null with NotFound if the fund is not found.</returns>
	public async Task<ServiceFlag<FundDto>> Get(Guid id)
	{
		var res = await dbContext.Funds
			.Include(entity => entity.User)
			.ThenInclude(entity => entity!.Role)
			.Include(fund => fund.FundCurrencies)
			.ThenInclude(fundCurrency => fundCurrency.Currency)
			.SingleOrDefaultAsync(entity => entity.Active && entity.Id == id);

		return res != null
			? new ServiceFlag<FundDto>(OK, CreateFundDto(res))
			: new ServiceFlag<FundDto>(NotFound);
	}

	/// <summary> Retrieves all funds associated with a specific user. </summary>
	/// <param name="id">The unique identifier of the user.</param>
	/// <returns>An array of FundDto objects representing all funds owned by the user, 
	/// or an empty array if the user has no funds.</returns>
	public async Task<IEnumerable<FundDto>> GetByUser(Guid id)
	{
		var res = await dbContext.Funds
			.Where(entity => entity.Active && entity.UserId == id)
			.Include(entity => entity.FundCurrencies)
			.ThenInclude(entity => entity.Currency)
			.ToArrayAsync();

		return res.Select(CreateFundDto);
	}

	/// <summary> Creates a new fund based on the provided information. </summary>
	/// <param name="info">An AddFundDto object containing details about the new fund.</param>
	/// <returns>A FundDto object representing the newly created fund.</returns>
	public async Task<FundDto> Add(AddFundDto info)
	{
		var fund = new Fund { Name = info.Name, LocationUrl = info.LocationUrl };
		await dbContext.Funds.AddAsync(fund);
		await dbContext.SaveChangesAsync();
		return CreateFundDto(fund);
	}

	/// <summary> Updates an existing fund with the provided information. </summary>
	/// <param name="info">An AddFundDto object containing updated details for the fund.</param>
	/// <param name="id">The unique identifier of the fund to be updated.</param>
	/// <returns>A ServiceFlag object containing either a FundDto representing the updated fund on success (with OK), 
	/// or null with NotFound if the fund is not found.</returns>
	public async Task<ServiceFlag<FundDto>> Update(AddFundDto info, Guid id)
	{
		var fund = await dbContext.Funds.FindAsync(id);
		if (fund == null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		fund.Name = info.Name;
		fund.LocationUrl = info.LocationUrl;
		await dbContext.SaveChangesAsync();

		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	/// <summary> Transfers a specified amount of currency from one fund to another. </summary>  
	/// <param name="info">Information about the transfer, including from/to fund IDs and amount to transfer.</param>  
	/// <returns>A ServiceFlag containing the result of the transfer operation, including success or error information.</returns>  
	public async Task<ServiceFlag<TransferDto.Response>> Transfer(TransferDto info)
	{
		if (info.Destination == info.Source)
			return new ServiceFlag<TransferDto.Response>(BadRequest, Message: "Destination is the same Source.");
		// Prepare a query to fetch funds with associated currencies from the database.  
		var query = dbContext.Funds.Include(entity => entity.FundCurrencies).ThenInclude(entity => entity.Currency);

		// Initiate asynchronous tasks to fetch the source and destination funds.  
		var (fromFund, toFund) =
			(await query.SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Source),
				await query.SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Destination));

		// Check if either fund was not found and return a not found response if so.  
		if (fromFund == null || toFund == null)
			return new ServiceFlag<TransferDto.Response>(NotFound,
				Message: $"Fund with ID: {(fromFund == null ? info.Source : info.Destination)} not found.");

		// Check if the source fund has the specified currency and sufficient amount for the transfer.  
		var fromFundCurrency =
			fromFund.FundCurrencies.SingleOrDefault(currency => currency.CurrencyId == info.Currency);
		if (fromFundCurrency == null || fromFundCurrency.Amount < info.Amount)
		{
			return new ServiceFlag<TransferDto.Response>(BadRequest,
				Message: "Fund has not enough of this currency to make this operation.");
		}

		// Begin a database transaction for the transfer operation.  
		await using var trx = await dbContext.Database.BeginTransactionAsync();
		try
		{
			// Reduce the amount of currency in the source fund.  
			fromFundCurrency.Amount -= info.Amount;

			// Retrieve the fund currency in the destination fund. If it doesn't exist, create a new one.  
			var toFundCurrency = toFund.FundCurrencies.SingleOrDefault(currency => currency.CurrencyId == info.Currency);
			if (toFundCurrency == null)
			{
				toFundCurrency = new FundCurrency
				{
					FundId = info.Destination,
					CurrencyId = info.Currency,
					Amount = info.Amount
				};
				toFund.FundCurrencies.Add(toFundCurrency); // Add new currency record if it didn't exist.  
			}
			else
			{
				// Increase the amount in the existing currency record of the destination fund.  
				toFundCurrency.Amount += info.Amount;
			}

			// Save the changes to the database and commit the transaction.  
			await dbContext.SaveChangesAsync();
			await trx.CommitAsync();
		}
		catch (Exception e)
		{
			// Roll back the transaction in case of an error and return an internal server error.  
			await trx.RollbackAsync();
			return new ServiceFlag<TransferDto.Response>(InternalServerError, Message: e.Message);
		}

		// Create DTOs for both funds to return in the response.  
		var fromFundDto = CreateFundDto(fromFund);
		var toFundDto = CreateFundDto(toFund);

		// Return a successful response with the DTOs of both funds.  
		return new ServiceFlag<TransferDto.Response>(OK, new TransferDto.Response(fromFundDto, toFundDto));
	}

	/// <summary>Withdraws a specified amount of currency from a fund.</summary>  
	/// <param name="info">Transaction details containing currency type and amount to withdraw.</param>  
	/// <returns>A ServiceFlag containing the result of the withdrawal operation,   
	/// including the FundDto if successful or an error message if unsuccessful.</returns>  
	public async Task<ServiceFlag<FundDto>> Withdraw(TransactionDto info)
	{
		// Retrieve the fund from the database, including its associated currencies.  
		var fund = await dbContext.Funds
			.Include(entity => entity.FundCurrencies) // Include related FundCurrency entities  
			.ThenInclude(fundCurrency => fundCurrency.Currency) // Include the Currency details for each FundCurrency  
			.SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Source); // Search for the fund by ID  

		// Check if the fund was found; if not, return a not found response.  
		if (fund == null) return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		// Check if the fund has the specified currency and if there is enough amount to withdraw.  
		var currency = fund.FundCurrencies.SingleOrDefault(entity => entity.CurrencyId == info.Currency);
		if (currency == null || currency.Amount < info.Amount)
		{
			// Return a bad request response if currency not found or insufficient amount.  
			return new ServiceFlag<FundDto>(BadRequest,
				Message: "Fund has not enough of this currency to make this operation.");
		}

		// Deduct the withdrawal amount from the fund's currency.  
		if (currency.Amount - info.Amount is 0) dbContext.FundCurrencies.Remove(currency);
		else currency.Amount -= info.Amount;

		// Save the changes to the database asynchronously.  
		await dbContext.SaveChangesAsync();

		// Create and return a successful response containing the updated fund details.  
		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	/// <summary>  
	/// Deposits a specified amount of currency into a fund.  
	/// </summary>  
	/// <param name="info">Transaction details containing currency type and amount to deposit.</param>  
	/// <returns>A ServiceFlag containing the result of the deposit operation,   
	/// including the FundDto if successful or an error message if unsuccessful.</returns>  
	public async Task<ServiceFlag<FundDto>> Deposit(TransactionDto info)
	{
		// Retrieve the fund from the database, including its associated currencies.  
		var fund = await dbContext.Funds
			.Include(entity => entity.FundCurrencies) // Include related FundCurrency entities  
			.ThenInclude(fundCurrency => fundCurrency.Currency) // Include the Currency details for each FundCurrency  
			.SingleOrDefaultAsync(entity => entity.Active && entity.Id == info.Source); // Search for the fund by ID  

		// Check if the fund was found; if not, return a not found response.  
		if (fund == null)
			return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		// Check if the specified currency exists in the database.  
		var test = await dbContext.Currencies.FindAsync(info.Currency);
		if (test == null)
			return new ServiceFlag<FundDto>(BadRequest, Message: "This currency doesn't exist.");

		// Check if the fund has the specified currency.  
		var currency = fund.FundCurrencies.SingleOrDefault(entity => entity.CurrencyId == info.Currency);

		// If the currency doesn't exist for this fund, create a new FundCurrency entry.  
		if (currency == null)
		{
			fund.FundCurrencies.Add(new FundCurrency
			{
				CurrencyId = info.Currency, // Set the currency ID  
				FundId = fund.Id, // Set the fund ID  
				Amount = info.Amount // Set the amount to be deposited  
			});
		}
		else
		{
			// If the currency already exists, simply increase its amount.  
			currency.Amount += info.Amount;
		}

		// Save the changes to the database asynchronously.  
		await dbContext.SaveChangesAsync();

		// Create and return a successful response containing the updated fund details.  
		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	/// <summary>Attaches a user to a specified fund by associating the user with the fund.</summary>  
	/// <param name="userId">The unique identifier of the user to be attached to the fund.</param>  
	/// <param name="fundId">The unique identifier of the fund to which the user will be attached.</param>  
	/// <returns>A ServiceFlag containing the result of the attach operation,   
	/// including the FundDto if successful or an error message if unsuccessful.</returns>  
	public async Task<ServiceFlag<FundDto>> AttachUser(Guid userId, Guid fundId)
	{
		// Start asynchronous tasks to retrieve both the fund and the user concurrently.  
		var fundTask = dbContext.Funds.SingleOrDefaultAsync(entity => entity.Active && entity.Id == fundId);
		var userTask = dbContext.Users.Include(entity => entity.Role).SingleOrDefaultAsync(entity => entity.Id == userId);

		// Wait for both tasks to complete.  
		await Task.WhenAll(fundTask, userTask);

		// Retrieve the results from the tasks.  
		var (fund, user) = (await fundTask, await userTask);

		// Check if either the fund or the user was not found.  
		if (fund == null || user == null)
			return new ServiceFlag<FundDto>(NotFound, Message: $"{(fund == null ? "Fund" : "User")} not found.");

		// Attach the user to the fund.  
		fund.User = user;

		// Save the changes to the database asynchronously.  
		await dbContext.SaveChangesAsync();

		// Return a successful response containing the updated fund details.  
		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	/// <summary>Marks a fund as inactive (deletes it logically) and removes associated FundCurrency entries.</summary>  
	/// <param name="id">The unique identifier of the fund to be deleted.</param>  
	/// <returns>A ServiceFlag containing the result of the delete operation,   
	/// including the FundDto if successful or a not found message if unsuccessful.</returns>  
	public async Task<ServiceFlag<FundDto>> Delete(Guid id)
	{
		// Retrieve the fund from the database if it exists and is active.  
		var fund = await dbContext.Funds
			.SingleOrDefaultAsync(entity => entity.Active && entity.Id == id);

		// If the fund is not found, return a not found response.  
		if (fund is null)
			return new ServiceFlag<FundDto>(NotFound, Message: "Fund not found.");

		// Mark the fund as inactive (soft delete).  
		fund.Name += $" - Deleted --{DateTime.Now}--";
		fund.Active = false;

		// Remove all associated FundCurrencies for this fund.  
		dbContext.FundCurrencies.RemoveRange(
			dbContext.FundCurrencies.Where(entity => entity.FundId == id)
		);

		// Save all changes to the database asynchronously.  
		await dbContext.SaveChangesAsync();

		// Return a successful response containing the deleted fund's details.  
		return new ServiceFlag<FundDto>(OK, CreateFundDto(fund));
	}

	/*
	 * PRIVATE
	 */

	// Creates a FundDto object from a Fund entity.  
	private static FundDto CreateFundDto(Fund fund) => new(
		fund.Id,
		fund.Name,
		fund.CreatedAt,
		Currencies: fund.FundCurrencies is not null
			? fund.FundCurrencies.Select(c => new FundDto.FundCurrency(c.Currency.Name, c.Amount))
			: default,
		LocationUrl: fund.LocationUrl,
		User: fund.User is not null
			? new UserDto(
				fund.User.Id,
				fund.User.Username,
				fund.User.Role.Name,
				fund.User.CreatedAt)
			: default
	);
}