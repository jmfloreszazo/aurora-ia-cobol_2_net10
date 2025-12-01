Feature: Transaction Management
  As a registered user
  I want to view, add, and manage transactions
  So that I can track my credit card activity

  Background:
    Given I am logged in as "USER0001"
    And the following accounts exist:
      | AccountId   | CustomerId | CurrentBalance | CreditLimit | ActiveStatus |
      | 10000000001 | 100000001  | 1500.00        | 5000.00     | Y            |
    And the following cards exist:
      | CardNumber       | AccountId   | ActiveStatus | ExpirationDate |
      | 4556737586899855 | 10000000001 | Y            | 12/2025        |
    And the following transaction types exist:
      | TypeCode | Description   | CategoryCode |
      | 01       | Purchase      | 5010         |
      | 02       | ATM Withdrawal| 6011         |
      | 03       | Payment       | 0000         |
      | 04       | Reversal      | 0000         |

  Scenario: View list of transactions for an account
    Given account "10000000001" has the following transactions:
      | TransactionId    | Date       | Type     | Description        | Amount  | Merchant      |
      | TX00000000000001 | 2025-11-28 | Purchase | Grocery Shopping   | -125.50 | Whole Foods   |
      | TX00000000000002 | 2025-11-27 | Purchase | Gas Station        | -45.00  | Shell         |
      | TX00000000000003 | 2025-11-26 | Payment  | Payment Received   | 500.00  | Online        |
    When I navigate to the transactions page for account "10000000001"
    Then I should see 3 transactions in the list
    And the transactions should be sorted by date descending

  Scenario: View detailed information for a specific transaction
    Given transaction "TX00000000000001" exists
    When I navigate to the transactions page
    And I click on transaction "TX00000000000001"
    Then I should see the transaction details page
    And I should see the following transaction information:
      | Field              | Value                |
      | Transaction ID     | TX00000000000001     |
      | Date               | 11/28/2025           |
      | Type               | Purchase             |
      | Description        | Grocery Shopping     |
      | Amount             | -$125.50             |
      | Merchant Name      | Whole Foods          |
      | Card Number        | **** **** **** 9855  |
      | Status             | Processed            |

  Scenario: Add a new purchase transaction
    When I navigate to the transactions page
    And I click the "Add Transaction" button
    And I enter the following transaction details:
      | Field           | Value                |
      | Card Number     | 4556737586899855     |
      | Type            | Purchase             |
      | Category        | Restaurants          |
      | Amount          | 75.50                |
      | Description     | Dinner at restaurant |
      | Merchant Name   | The Grill House      |
      | Merchant City   | New York             |
    And I click the "Submit" button
    Then I should see a success message "Transaction added successfully"
    And a new transaction ID should be generated
    And the account balance should be updated to "$1,575.50"

  Scenario: Validate transaction amount is required
    When I navigate to the transactions page
    And I click the "Add Transaction" button
    And I leave the amount field empty
    And I click the "Submit" button
    Then I should see a validation error "Amount is required"

  Scenario: Validate transaction amount must be non-zero
    When I add a transaction with amount "0.00"
    Then I should see a validation error "Amount must be greater than zero"

  Scenario: Reject transaction exceeding credit limit
    Given account "10000000001" has:
      | Current Balance | Credit Limit | Available Credit |
      | 4500.00         | 5000.00      | 500.00           |
    When I attempt to add a transaction with amount "750.00"
    Then I should see an error message "Transaction exceeds available credit limit"
    And the transaction should not be created
    And the account balance should remain "$4,500.00"

  Scenario: Process payment transaction (credit)
    When I add a payment transaction with amount "300.00"
    Then the transaction should be created with positive amount "+300.00"
    And the account balance should be reduced to "$1,200.00"
    And the transaction type should be "Payment"

  Scenario: Process purchase transaction (debit)
    When I add a purchase transaction with amount "50.00"
    Then the transaction should be created with negative amount "-50.00"
    And the account balance should be increased to "$1,550.00"
    And the transaction type should be "Purchase"

  Scenario: Cannot create transaction with inactive card
    Given card "4556737586899855" has status "Inactive"
    When I attempt to add a transaction with this card
    Then I should see an error message "Card invalid or expired"
    And the transaction should not be created

  Scenario: Cannot create transaction with expired card
    Given card "4556737586899855" has expiration date "11/2024"
    And today's date is "12/01/2025"
    When I attempt to add a transaction with this card
    Then I should see an error message "Card invalid or expired"
    And the transaction should not be created

  Scenario: Filter transactions by date range
    Given account "10000000001" has transactions from "2025-11-01" to "2025-11-30"
    When I navigate to the transactions page
    And I set the date filter from "2025-11-20" to "2025-11-28"
    And I click "Apply Filter"
    Then I should only see transactions within that date range
    And transactions outside this range should not be displayed

  Scenario: Filter transactions by transaction type
    Given account "10000000001" has various transaction types
    When I navigate to the transactions page
    And I select transaction type filter "Purchase"
    And I click "Apply Filter"
    Then I should only see transactions of type "Purchase"
    And no payment or withdrawal transactions should be displayed

  Scenario: Filter transactions by category
    Given account "10000000001" has transactions in different categories
    When I navigate to the transactions page
    And I select category filter "Restaurants"
    And I click "Apply Filter"
    Then I should only see transactions in the "Restaurants" category

  Scenario: Search transactions by merchant name
    When I navigate to the transactions page
    And I enter "Whole Foods" in the merchant search box
    And I click the search button
    Then I should only see transactions from "Whole Foods"

  Scenario: Calculate running balance for transactions
    Given account "10000000001" starts with balance "1000.00"
    And has the following transactions:
      | Date       | Type     | Amount  |
      | 2025-11-26 | Payment  | 500.00  |
      | 2025-11-27 | Purchase | -45.00  |
      | 2025-11-28 | Purchase | -125.50 |
    When I view the transaction list
    Then I should see the running balance for each transaction:
      | Date       | Amount   | Running Balance |
      | 2025-11-28 | -125.50  | 1,170.50        |
      | 2025-11-27 | -45.00   | 1,045.00        |
      | 2025-11-26 | +500.00  | 500.00          |

  Scenario: Sort transactions by amount
    When I navigate to the transactions page
    And I select sort by "Amount (Highest First)"
    Then transactions should be ordered by amount descending
    And payment transactions should appear before purchases

  Scenario: Export transactions to CSV
    Given I am on the transactions page
    And there are 10 transactions displayed
    When I click the "Export to CSV" button
    Then a CSV file should be downloaded
    And the file should contain all 10 transactions
    And the file should include headers: Transaction ID, Date, Type, Description, Amount, Merchant

  Scenario: Export transactions to PDF
    Given I am on the transactions page
    And I have filtered transactions for November 2025
    When I click the "Export to PDF" button
    Then a PDF file should be downloaded
    And the PDF should include a transaction summary
    And the PDF should include account information
    And the PDF should be formatted as a statement

  Scenario: Generate monthly transaction report
    Given account "10000000001" has transactions in November 2025
    When I navigate to the reports page
    And I select "Monthly Transaction Report"
    And I select month "November 2025"
    And I click "Generate Report"
    Then I should see a report with the following sections:
      | Section                    | Content                          |
      | Total Transactions         | Count of all transactions        |
      | Total Debits               | Sum of all purchases             |
      | Total Credits              | Sum of all payments              |
      | Net Change                 | Credits minus debits             |
      | Transactions by Category   | Breakdown by category            |
      | Top Merchants              | Merchants with most transactions |

  Scenario: View transactions grouped by category
    Given account "10000000001" has transactions in multiple categories
    When I navigate to the transactions page
    And I select view "Group by Category"
    Then transactions should be grouped by their categories
    And each category should show total amount
    And categories should be sorted by total amount descending

  Scenario: Detect duplicate transactions
    Given I have just added transaction "TX00000000000001"
    When I attempt to add another transaction with the same details within 5 minutes
    Then I should see a warning "Potential duplicate transaction detected"
    And I should be asked to confirm "Do you want to proceed?"

  Scenario: Reverse a transaction
    Given transaction "TX00000000000001" exists with amount "-125.50"
    And the transaction status is "Processed"
    When I view the transaction details
    And I click the "Reverse Transaction" button
    And I enter reversal reason "Customer dispute"
    And I confirm the reversal
    Then a new reversal transaction should be created
    And the reversal transaction amount should be "+125.50"
    And the original transaction should be linked to the reversal
    And the account balance should be adjusted accordingly

  Scenario: Cannot reverse already reversed transaction
    Given transaction "TX00000000000001" has been reversed
    When I attempt to reverse it again
    Then I should see an error message "Transaction has already been reversed"

  Scenario: Cannot modify processed transactions
    Given transaction "TX00000000000001" has status "Processed"
    When I attempt to edit the transaction
    Then the edit button should be disabled
    And I should see a message "Processed transactions cannot be modified"

  Scenario: Pagination of transaction list
    Given account "10000000001" has 50 transactions
    And the page size is set to 20
    When I navigate to the transactions page
    Then I should see 20 transactions on page 1
    And I should see a pagination control
    And the pagination should show "Page 1 of 3"
    When I click "Next Page"
    Then I should see the next 20 transactions
    And the pagination should show "Page 2 of 3"

  Scenario: Real-time balance update after transaction
    Given I am viewing account "10000000001" details
    And the current balance is "$1,500.00"
    When I add a new transaction with amount "-50.00"
    Then the account balance should immediately update to "$1,550.00"
    And I should see a notification "Balance updated"

  Scenario: Transaction with merchant information
    When I add a transaction with complete merchant details:
      | Field         | Value           |
      | Merchant Name | Amazon.com      |
      | Merchant City | Seattle         |
      | Merchant ZIP  | 98101           |
    Then the transaction should be created successfully
    And all merchant information should be stored
    And I should be able to filter transactions by this merchant

  Scenario: Add transaction note/memo
    Given I am adding a new transaction
    When I enter a note "Business expense - client meeting"
    And I save the transaction
    Then the note should be stored with the transaction
    And I should be able to view the note in transaction details
    And I should be able to search transactions by note content
