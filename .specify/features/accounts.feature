Feature: Account Management
  As a registered user
  I want to view and manage my credit card accounts
  So that I can monitor my balances and credit limits

  Background:
    Given I am logged in as "USER0001"
    And the following customers exist:
      | CustomerId | FirstName | LastName | SSN       |
      | 100000001  | John      | Doe      | 123456789 |
    And the following accounts exist:
      | AccountId   | CustomerId | CurrentBalance | CreditLimit | ActiveStatus | OpenDate   |
      | 10000000001 | 100000001  | 1500.00        | 5000.00     | Y            | 2023-01-15 |
      | 10000000002 | 100000001  | 250.50         | 2000.00     | Y            | 2023-06-20 |
      | 10000000003 | 100000001  | 0.00           | 10000.00    | N            | 2022-12-01 |

  Scenario: View list of all accounts for logged in user
    When I navigate to the accounts page
    Then I should see 3 accounts in the list
    And I should see account "10000000001" with balance "$1,500.00"
    And I should see account "10000000002" with balance "$250.50"
    And I should see account "10000000003" with status "Inactive"

  Scenario: View detailed information for a specific account
    When I navigate to the accounts page
    And I click on account "10000000001"
    Then I should see the account details page
    And I should see the following account information:
      | Field              | Value           |
      | Account ID         | 10000000001     |
      | Current Balance    | $1,500.00       |
      | Credit Limit       | $5,000.00       |
      | Available Credit   | $3,500.00       |
      | Cash Credit Limit  | $1,000.00       |
      | Status             | Active          |
      | Open Date          | 01/15/2023      |

  Scenario: Filter accounts by active status
    When I navigate to the accounts page
    And I select the filter "Active Only"
    Then I should see 2 accounts in the list
    And I should not see account "10000000003"

  Scenario: Update account credit limit
    Given I am on the account details page for account "10000000001"
    When I click the "Edit" button
    And I change the credit limit to "7500.00"
    And I click the "Save" button
    Then I should see a success message "Account updated successfully"
    And the credit limit should be "$7,500.00"
    And the available credit should be "$6,000.00"

  Scenario: Attempt to set credit limit below current balance
    Given I am on the account details page for account "10000000001"
    And the current balance is "$1,500.00"
    When I click the "Edit" button
    And I change the credit limit to "1000.00"
    And I click the "Save" button
    Then I should see an error message "Credit limit cannot be less than current balance"
    And the credit limit should remain "$5,000.00"

  Scenario: Attempt to set invalid credit limit (negative)
    Given I am on the account details page for account "10000000001"
    When I click the "Edit" button
    And I change the credit limit to "-500.00"
    And I click the "Save" button
    Then I should see a validation error "Credit limit must be greater than zero"

  Scenario: View account with no transactions
    Given account "10000000003" has no transactions
    When I navigate to account "10000000003" details
    And I click on the "Transactions" tab
    Then I should see a message "No transactions found for this account"

  Scenario: Calculate available credit correctly
    Given account "10000000001" has:
      | Current Balance | Credit Limit |
      | 1500.00         | 5000.00      |
    When I view the account details
    Then the available credit should be calculated as "$3,500.00"

  Scenario: Deactivate an account
    Given I am on the account details page for account "10000000001"
    And the account status is "Active"
    When I click the "Deactivate" button
    And I confirm the deactivation
    Then I should see a success message "Account deactivated successfully"
    And the account status should be "Inactive"
    And a confirmation email should be sent

  Scenario: Cannot update inactive account
    Given account "10000000003" has status "Inactive"
    When I navigate to account "10000000003" details
    Then the "Edit" button should be disabled
    And I should see a message "This account is inactive and cannot be modified"

  Scenario: View credit utilization percentage
    Given account "10000000001" has:
      | Current Balance | Credit Limit |
      | 1500.00         | 5000.00      |
    When I view the account details
    Then I should see credit utilization of "30%"
    And I should see a utilization status indicator "Good"

  Scenario: High credit utilization warning
    Given account "10000000001" has:
      | Current Balance | Credit Limit |
      | 4500.00         | 5000.00      |
    When I view the account details
    Then I should see credit utilization of "90%"
    And I should see a utilization status indicator "High"
    And I should see a warning message "Your credit utilization is high"

  Scenario: View accounts sorted by balance
    When I navigate to the accounts page
    And I select sort by "Balance (Highest First)"
    Then the accounts should be ordered as:
      | AccountId   | Balance  |
      | 10000000001 | 1500.00  |
      | 10000000002 | 250.50   |
      | 10000000003 | 0.00     |

  Scenario: Search for account by account ID
    When I navigate to the accounts page
    And I enter "10000000002" in the search box
    And I click the search button
    Then I should see only 1 account
    And the displayed account ID should be "10000000002"

  Scenario: No results found for invalid account search
    When I navigate to the accounts page
    And I enter "99999999999" in the search box
    And I click the search button
    Then I should see a message "No accounts found matching your search"
