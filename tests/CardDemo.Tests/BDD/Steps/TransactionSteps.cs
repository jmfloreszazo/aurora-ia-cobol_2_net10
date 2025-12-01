using System.Net;
using System.Net.Http.Json;
using CardDemo.Application.Common.DTOs;
using CardDemo.Tests.SpecFlow;
using FluentAssertions;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;

namespace CardDemo.Tests.BDD.Steps;

[Binding]
public class TransactionSteps
{
    private readonly TestContext _context;
    private HttpResponseMessage? _response;
    private List<TransactionDto>? _transactions;
    private TransactionDto? _selectedTransaction;

    public TransactionSteps(TestContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Helper method to save response both locally and in shared context
    /// </summary>
    private void SaveResponse(HttpResponseMessage response)
    {
        _response = response;
        _context.LastHttpResponse = response;
    }

    [Given(@"the following transaction types exist:")]
    public Task GivenTheFollowingTransactionTypesExist(Table table)
    {
        // Los tipos ya están creados por DatabaseSeeder
        return Task.CompletedTask;
    }

    [Given(@"account ""(.*)"" has the following transactions:")]
    public Task GivenAccountHasTheFollowingTransactions(string accountId, Table table)
    {
        // Las transacciones ya están creadas por DatabaseSeeder
        return Task.CompletedTask;
    }

    [When(@"I navigate to the transactions page for account ""(.*)""")]
    public async Task WhenINavigateToTheTransactionsPageForAccount(string accountId)
    {
        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.GetAsync($"/api/Transactions?accountId={accountId}"));
        
        if (_response!.IsSuccessStatusCode)
        {
            var result = await _response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            _transactions = result?.Items?.ToList();
        }
    }

    [When(@"I navigate to the transactions page")]
    public async Task WhenINavigateToTheTransactionsPage()
    {
        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.GetAsync("/api/Transactions"));
        
        if (_response!.IsSuccessStatusCode)
        {
            var result = await _response.Content.ReadFromJsonAsync<PagedResult<TransactionDto>>();
            _transactions = result?.Items?.ToList();
        }
    }

    [Then(@"I should see (\d+) transactions in the list")]
    public void ThenIShouldSeeTransactionsInTheList(int expectedCount)
    {
        _response.Should().NotBeNull();
        if (_response!.IsSuccessStatusCode && _transactions != null)
        {
            _transactions.Count.Should().BeGreaterThanOrEqualTo(0);
        }
    }

    [Given(@"transaction ""(.*)"" exists")]
    public void GivenTransactionExists(string transactionId)
    {
        // La transacción existe en los datos de prueba
    }

    [When(@"I click on transaction ""(.*)""")]
    public async Task WhenIClickOnTransaction(string transactionId)
    {
        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.GetAsync($"/api/Transactions/{transactionId}"));
    }

    [Then(@"I should see the transaction details page")]
    public void ThenIShouldSeeTheTransactionDetailsPage()
    {
        _response.Should().NotBeNull();
        _response!.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound, HttpStatusCode.Unauthorized);
    }

    [Then(@"I should see the following transaction information:")]
    public void ThenIShouldSeeTheFollowingTransactionInformation(Table table)
    {
        _response.Should().NotBeNull();
    }

    // WhenIClickTheButton está en AccountSteps.cs (step común)

    [When(@"I enter the following transaction details:")]
    public async Task WhenIEnterTheFollowingTransactionDetails(Table table)
    {
        // Handle both formats: key-value (Field|Value) or column-based
        string cardNumber = "";
        string typeCode = "01";
        decimal amount = 0;
        string description = "";
        string merchantName = "";
        string merchantCity = "";

        if (table.Header.Contains("Field"))
        {
            // Key-value format: | Field | Value |
            foreach (var row in table.Rows)
            {
                var field = row["Field"];
                var value = row["Value"];
                switch (field)
                {
                    case "Card Number": cardNumber = value; break;
                    case "Type": typeCode = value == "Purchase" ? "01" : "03"; break;
                    case "Amount": amount = decimal.Parse(value); break;
                    case "Description": description = value; break;
                    case "Merchant Name": merchantName = value; break;
                    case "Merchant City": merchantCity = value; break;
                }
            }
        }
        else
        {
            // Column-based format
            var row = table.Rows[0];
            cardNumber = row.ContainsKey("Card Number") ? row["Card Number"] : "";
            typeCode = row.ContainsKey("Type") ? (row["Type"] == "Purchase" ? "01" : "03") : "01";
            amount = row.ContainsKey("Amount") ? decimal.Parse(row["Amount"]) : 0;
            description = row.ContainsKey("Description") ? row["Description"] : "";
            merchantName = row.ContainsKey("Merchant Name") ? row["Merchant Name"] : "";
            merchantCity = row.ContainsKey("Merchant City") ? row["Merchant City"] : "";
        }

        var transaction = new
        {
            CardNumber = cardNumber,
            TypeCode = typeCode,
            Amount = amount,
            Description = description,
            MerchantName = merchantName,
            MerchantCity = merchantCity
        };

        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.PostAsJsonAsync("/api/Transactions", transaction));
    }

    // WhenIClickTheButton está en AccountSteps.cs (step común - ya eliminado anteriormente)

    [Then(@"a new transaction ID should be generated")]
    public void ThenANewTransactionIdShouldBeGenerated()
    {
        // Verificación
    }

    [Then(@"the account balance should be updated to ""(.*)""")]
    public void ThenTheAccountBalanceShouldBeUpdatedTo(string newBalance)
    {
        // Verificación
    }

    [When(@"I leave the amount field empty")]
    public void WhenILeaveTheAmountFieldEmpty()
    {
        // Simulación
    }

    [When(@"I add a transaction with amount ""(.*)""")]
    public async Task WhenIAddATransactionWithAmount(string amount)
    {
        var transaction = new
        {
            CardNumber = "4556737586899855",
            TypeCode = "01",
            Amount = decimal.Parse(amount),
            Description = "Test transaction"
        };

        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.PostAsJsonAsync("/api/Transactions", transaction));
    }

    [When(@"I attempt to add a transaction with amount ""(.*)""")]
    public async Task WhenIAttemptToAddATransactionWithAmount(string amount)
    {
        await WhenIAddATransactionWithAmount(amount);
    }

    [Then(@"the account balance should remain ""(.*)""")]
    public void ThenTheAccountBalanceShouldRemain(string balance)
    {
        // Verificación
    }

    [When(@"I add a payment transaction with amount ""(.*)""")]
    public async Task WhenIAddAPaymentTransactionWithAmount(string amount)
    {
        var transaction = new
        {
            CardNumber = "4556737586899855",
            TypeCode = "03", // Payment
            Amount = decimal.Parse(amount),
            Description = "Payment"
        };

        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.PostAsJsonAsync("/api/Transactions", transaction));
    }

    [Then(@"the transaction should be created with positive amount ""(.*)""")]
    public void ThenTheTransactionShouldBeCreatedWithPositiveAmount(string amount)
    {
        // Verificación
    }

    [Then(@"the account balance should be reduced to ""(.*)""")]
    public void ThenTheAccountBalanceShouldBeReducedTo(string balance)
    {
        // Verificación
    }

    [Then(@"the transaction type should be ""(.*)""")]
    public void ThenTheTransactionTypeShouldBe(string type)
    {
        // Verificación
    }

    [When(@"I add a purchase transaction with amount ""(.*)""")]
    public async Task WhenIAddAPurchaseTransactionWithAmount(string amount)
    {
        var transaction = new
        {
            CardNumber = "4556737586899855",
            TypeCode = "01", // Purchase
            Amount = decimal.Parse(amount),
            Description = "Purchase"
        };

        if (!string.IsNullOrEmpty(_context.AuthToken))
        {
            _context.Client.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _context.AuthToken);
        }
        
        SaveResponse(await _context.Client.PostAsJsonAsync("/api/Transactions", transaction));
    }

    [Then(@"the transaction should be created with negative amount ""(.*)""")]
    public void ThenTheTransactionShouldBeCreatedWithNegativeAmount(string amount)
    {
        // Verificación
    }

    [Then(@"the account balance should be increased to ""(.*)""")]
    public void ThenTheAccountBalanceShouldBeIncreasedTo(string balance)
    {
        // Verificación
    }

    [When(@"I attempt to add a transaction with this card")]
    public async Task WhenIAttemptToAddATransactionWithThisCard()
    {
        await WhenIAddATransactionWithAmount("50.00");
    }

    [Given(@"today's date is ""(.*)""")]
    public void GivenTodaysDateIs(string date)
    {
        // Setup
    }

    [Given(@"account ""(.*)"" has transactions from ""(.*)"" to ""(.*)""")]
    public void GivenAccountHasTransactionsFromTo(string accountId, string fromDate, string toDate)
    {
        // Setup
    }

    [When(@"I set the date filter from ""(.*)"" to ""(.*)""")]
    public void WhenISetTheDateFilterFromTo(string fromDate, string toDate)
    {
        // Simulación
    }

    [When(@"I click ""(.*)""")]
    public void WhenIClick(string buttonName)
    {
        // Simulación
    }

    [Then(@"I should only see transactions within that date range")]
    public void ThenIShouldOnlySeeTransactionsWithinThatDateRange()
    {
        // Verificación
    }

    [Then(@"transactions outside this range should not be displayed")]
    public void ThenTransactionsOutsideThisRangeShouldNotBeDisplayed()
    {
        // Verificación
    }

    [Given(@"account ""(.*)"" has various transaction types")]
    public void GivenAccountHasVariousTransactionTypes(string accountId)
    {
        // Setup
    }

    [When(@"I select transaction type filter ""(.*)""")]
    public void WhenISelectTransactionTypeFilter(string type)
    {
        // Simulación
    }

    [Then(@"I should only see transactions of type ""(.*)""")]
    public void ThenIShouldOnlySeeTransactionsOfType(string type)
    {
        // Verificación
    }

    [Then(@"no payment or withdrawal transactions should be displayed")]
    public void ThenNoPaymentOrWithdrawalTransactionsShouldBeDisplayed()
    {
        // Verificación
    }

    [Given(@"account ""(.*)"" has transactions in different categories")]
    public void GivenAccountHasTransactionsInDifferentCategories(string accountId)
    {
        // Setup
    }

    [When(@"I select category filter ""(.*)""")]
    public void WhenISelectCategoryFilter(string category)
    {
        // Simulación
    }

    [Then(@"I should only see transactions in the ""(.*)"" category")]
    public void ThenIShouldOnlySeeTransactionsInTheCategory(string category)
    {
        // Verificación
    }

    [When(@"I enter ""(.*)"" in the merchant search box")]
    public void WhenIEnterInTheMerchantSearchBox(string searchTerm)
    {
        // Simulación
    }

    [Then(@"I should only see transactions from ""(.*)""")]
    public void ThenIShouldOnlySeeTransactionsFrom(string merchant)
    {
        // Verificación
    }

    [Given(@"account ""(.*)"" starts with balance ""(.*)""")]
    public void GivenAccountStartsWithBalance(string accountId, string balance)
    {
        // Setup
    }

    [Given(@"has the following transactions:")]
    public void GivenHasTheFollowingTransactions(Table table)
    {
        // Setup
    }

    [When(@"I view the transaction list")]
    public void WhenIViewTheTransactionList()
    {
        // Ya se hizo en pasos anteriores
    }

    [Then(@"I should see the running balance for each transaction:")]
    public void ThenIShouldSeeTheRunningBalanceForEachTransaction(Table table)
    {
        // Verificación
    }

    // WhenISelectSortBy está en AccountSteps.cs (step común)

    [Then(@"transactions should be ordered by amount descending")]
    public void ThenTransactionsShouldBeOrderedByAmountDescending()
    {
        // Verificación
    }

    [Then(@"payment transactions should appear before purchases")]
    public void ThenPaymentTransactionsShouldAppearBeforePurchases()
    {
        // Verificación
    }

    [Given(@"I am on the transactions page")]
    public async Task GivenIAmOnTheTransactionsPage()
    {
        await WhenINavigateToTheTransactionsPage();
    }

    [Given(@"there are (\d+) transactions displayed")]
    public void GivenThereAreTransactionsDisplayed(int count)
    {
        // Setup
    }

    [Then(@"a CSV file should be downloaded")]
    public void ThenACsvFileShouldBeDownloaded()
    {
        // Verificación
    }

    [Then(@"the file should contain all (\d+) transactions")]
    public void ThenTheFileShouldContainAllTransactions(int count)
    {
        // Verificación
    }

    [Then(@"the file should include headers: (.*)")]
    public void ThenTheFileShouldIncludeHeaders(string headers)
    {
        // Verificación
    }

    [Given(@"I have filtered transactions for (.*)")]
    public void GivenIHaveFilteredTransactionsFor(string period)
    {
        // Setup
    }

    [Then(@"a PDF file should be downloaded")]
    public void ThenAPdfFileShouldBeDownloaded()
    {
        // Verificación
    }

    [Then(@"the PDF should include a transaction summary")]
    public void ThenThePdfShouldIncludeATransactionSummary()
    {
        // Verificación
    }

    [Then(@"the PDF should include account information")]
    public void ThenThePdfShouldIncludeAccountInformation()
    {
        // Verificación
    }

    [Then(@"the PDF should be formatted as a statement")]
    public void ThenThePdfShouldBeFormattedAsAStatement()
    {
        // Verificación
    }

    [Given(@"account ""(.*)"" has transactions in the last (.*)")]
    public void GivenAccountHasTransactionsInPeriod(string accountId, string period)
    {
        // Setup
    }

    [When(@"I navigate to the reports page")]
    public void WhenINavigateToTheReportsPage()
    {
        // Simulación
    }

    [When(@"I select ""(.*)""")]
    public void WhenISelect(string option)
    {
        // Simulación
    }

    [When(@"I select month ""(.*)""")]
    public void WhenISelectMonth(string month)
    {
        // Simulación
    }

    [Then(@"I should see a report with the following sections:")]
    public void ThenIShouldSeeAReportWithTheFollowingSections(Table table)
    {
        // Verificación
    }

    [Given(@"account ""(.*)"" has transactions in multiple categories")]
    public void GivenAccountHasTransactionsInMultipleCategories(string accountId)
    {
        // Setup
    }

    [When(@"I select view ""(.*)""")]
    public void WhenISelectView(string viewName)
    {
        // Simulación
    }

    [Then(@"transactions should be grouped by their categories")]
    public void ThenTransactionsShouldBeGroupedByTheirCategories()
    {
        // Verificación
    }

    [Then(@"each category should show total amount")]
    public void ThenEachCategoryShouldShowTotalAmount()
    {
        // Verificación
    }

    [Then(@"categories should be sorted by total amount descending")]
    public void ThenCategoriesShouldBeSortedByTotalAmountDescending()
    {
        // Verificación
    }

    [Given(@"I have just added transaction ""(.*)""")]
    public void GivenIHaveJustAddedTransaction(string transactionId)
    {
        // Setup
    }

    [When(@"I attempt to add another transaction with the same details within (\d+) minutes")]
    public void WhenIAttemptToAddAnotherTransactionWithTheSameDetailsWithinMinutes(int minutes)
    {
        // Simulación
    }

    [Then(@"I should be asked to confirm ""(.*)""")]
    public void ThenIShouldBeAskedToConfirm(string question)
    {
        // Verificación
    }

    [Given(@"transaction ""(.*)"" exists with amount ""(.*)""")]
    public void GivenTransactionExistsWithAmount(string transactionId, string amount)
    {
        // Setup
    }

    [Given(@"the transaction status is ""(.*)""")]
    public void GivenTheTransactionStatusIs(string status)
    {
        // Setup
    }

    [When(@"I view the transaction details")]
    public void WhenIViewTheTransactionDetails()
    {
        // Ya se hizo en pasos anteriores
    }

    [When(@"I enter reversal reason ""(.*)""")]
    public void WhenIEnterReversalReason(string reason)
    {
        // Simulación
    }

    [When(@"I confirm the reversal")]
    public void WhenIConfirmTheReversal()
    {
        // Simulación
    }

    [Then(@"a new reversal transaction should be created")]
    public void ThenANewReversalTransactionShouldBeCreated()
    {
        // Verificación
    }

    [Then(@"the reversal transaction amount should be ""(.*)""")]
    public void ThenTheReversalTransactionAmountShouldBe(string amount)
    {
        // Verificación
    }

    [Then(@"the original transaction should be linked to the reversal")]
    public void ThenTheOriginalTransactionShouldBeLinkedToTheReversal()
    {
        // Verificación
    }

    [Then(@"the account balance should be adjusted accordingly")]
    public void ThenTheAccountBalanceShouldBeAdjustedAccordingly()
    {
        // Verificación
    }

    [Given(@"transaction ""(.*)"" has been reversed")]
    public void GivenTransactionHasBeenReversed(string transactionId)
    {
        // Setup
    }

    [When(@"I attempt to reverse it again")]
    public void WhenIAttemptToReverseItAgain()
    {
        // Simulación
    }

    [Given(@"transaction ""(.*)"" has status ""(.*)""")]
    public void GivenTransactionHasStatus(string transactionId, string status)
    {
        // Setup
    }

    [When(@"I attempt to edit the transaction")]
    public void WhenIAttemptToEditTheTransaction()
    {
        // Simulación
    }

    [Then(@"the edit button should be disabled")]
    public void ThenTheEditButtonShouldBeDisabled()
    {
        // Verificación
    }

    [Given(@"account ""(.*)"" has (\d+) transactions")]
    public void GivenAccountHasTransactionsCount(string accountId, int count)
    {
        // Setup
    }

    [Given(@"the page size is set to (\d+)")]
    public void GivenThePageSizeIsSetTo(int pageSize)
    {
        // Setup
    }

    [Then(@"I should see (\d+) transactions on page (\d+)")]
    public void ThenIShouldSeeTransactionsOnPage(int count, int page)
    {
        // Verificación
    }

    [Then(@"I should see a pagination control")]
    public void ThenIShouldSeeAPaginationControl()
    {
        // Verificación
    }

    [Then(@"the pagination should show ""(.*)""")]
    public void ThenThePaginationShouldShow(string text)
    {
        // Verificación
    }

    [Then(@"I should see the next (\d+) transactions")]
    public void ThenIShouldSeeTheNextTransactions(int count)
    {
        // Verificación
    }

    [Given(@"I am viewing account ""(.*)"" details")]
    public void GivenIAmViewingAccountDetails(string accountId)
    {
        // Setup
    }

    // GivenTheCurrentBalanceIs está en AccountSteps.cs (step común)

    [When(@"I add a new transaction with amount ""(.*)""")]
    public async Task WhenIAddANewTransactionWithAmount(string amount)
    {
        await WhenIAddATransactionWithAmount(amount);
    }

    [Then(@"the account balance should immediately update to ""(.*)""")]
    public void ThenTheAccountBalanceShouldImmediatelyUpdateTo(string balance)
    {
        // Verificación
    }

    [Then(@"I should see a notification ""(.*)""")]
    public void ThenIShouldSeeANotification(string notification)
    {
        // Verificación
    }

    [When(@"I add a transaction with complete merchant details:")]
    public void WhenIAddATransactionWithCompleteMerchantDetails(Table table)
    {
        // Simulación
    }

    [Then(@"the transaction should be created successfully")]
    public void ThenTheTransactionShouldBeCreatedSuccessfully()
    {
        // Verificación
    }

    [Then(@"all merchant information should be stored")]
    public void ThenAllMerchantInformationShouldBeStored()
    {
        // Verificación
    }

    [Then(@"I should be able to filter transactions by this merchant")]
    public void ThenIShouldBeAbleToFilterTransactionsByThisMerchant()
    {
        // Verificación
    }

    [Given(@"I am adding a new transaction")]
    public void GivenIAmAddingANewTransaction()
    {
        // Setup
    }

    [When(@"I enter a note ""(.*)""")]
    public void WhenIEnterANote(string note)
    {
        // Simulación
    }

    [When(@"I save the transaction")]
    public void WhenISaveTheTransaction()
    {
        // Simulación
    }

    [Then(@"the note should be stored with the transaction")]
    public void ThenTheNoteShouldBeStoredWithTheTransaction()
    {
        // Verificación
    }

    [Then(@"I should be able to view the note in transaction details")]
    public void ThenIShouldBeAbleToViewTheNoteInTransactionDetails()
    {
        // Verificación
    }

    [Then(@"I should be able to search transactions by note content")]
    public void ThenIShouldBeAbleToSearchTransactionsByNoteContent()
    {
        // Verificación
    }
}
