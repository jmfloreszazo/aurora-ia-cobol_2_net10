Feature: Credit Card Management
  As a registered user
  I want to view and manage my credit cards
  So that I can monitor card status and perform card operations

  Background:
    Given I am logged in as "USER0001"
    And the following accounts exist:
      | AccountId   | CustomerId | CreditLimit | ActiveStatus |
      | 10000000001 | 100000001  | 5000.00     | Y            |
      | 10000000002 | 100000001  | 2000.00     | Y            |
    And the following cards exist:
      | CardNumber       | AccountId   | CardType | EmbossedName | ExpirationDate | ActiveStatus |
      | 4556737586899855 | 10000000001 | VISA     | JOHN DOE     | 12/2025        | Y            |
      | 5454545454545454 | 10000000001 | MC       | JOHN DOE     | 03/2026        | Y            |
      | 3782822463100005 | 10000000002 | AMEX     | J DOE        | 11/2024        | N            |

  Scenario: View list of all credit cards
    When I navigate to the credit cards page
    Then I should see 3 cards in the list
    And card "4556737586899855" should be displayed as "**** **** **** 9855"
    And card "5454545454545454" should be displayed as "**** **** **** 5454"
    And card "3782822463100005" should show status "Inactive"

  Scenario: View detailed information for a specific card
    When I navigate to the credit cards page
    And I click on card ending in "9855"
    Then I should see the card details page
    And I should see the following card information:
      | Field           | Value             |
      | Card Number     | **** **** **** 9855 |
      | Full Number     | 4556737586899855  |
      | Card Type       | VISA              |
      | Embossed Name   | JOHN DOE          |
      | Expiration Date | 12/2025           |
      | Status          | Active            |
      | Account ID      | 10000000001       |

  Scenario: Filter cards by card type
    When I navigate to the credit cards page
    And I select card type filter "VISA"
    Then I should see only 1 card
    And the displayed card should end in "9855"

  Scenario: Filter cards by active status
    When I navigate to the credit cards page
    And I select the filter "Active Only"
    Then I should see 2 cards in the list
    And I should not see card ending in "0005"

  Scenario: Update embossed name on card
    Given I am on the card details page for card "4556737586899855"
    When I click the "Edit" button
    And I change the embossed name to "JOHN M DOE"
    And I click the "Save" button
    Then I should see a success message "Card updated successfully"
    And the embossed name should be "JOHN M DOE"

  Scenario: Validate embossed name length
    Given I am on the card details page for card "4556737586899855"
    When I click the "Edit" button
    And I enter an embossed name with 51 characters
    And I click the "Save" button
    Then I should see a validation error "Embossed name must be 50 characters or less"

  Scenario: Deactivate a credit card
    Given I am on the card details page for card "4556737586899855"
    And the card status is "Active"
    When I click the "Deactivate" button
    And I confirm the deactivation
    Then I should see a success message "Card deactivated successfully"
    And the card status should be "Inactive"
    And I should receive a confirmation notification

  Scenario: Reactivate an inactive card
    Given I am on the card details page for card "3782822463100005"
    And the card status is "Inactive"
    When I click the "Reactivate" button
    And I confirm the reactivation
    Then I should see a success message "Card reactivated successfully"
    And the card status should be "Active"

  Scenario: Cannot reactivate expired card
    Given card "3782822463100005" has expiration date "11/2024"
    And today's date is after "11/2024"
    When I attempt to reactivate the card
    Then I should see an error message "Cannot reactivate expired card"
    And the card status should remain "Inactive"

  Scenario: Display expiration warning for card expiring soon
    Given card "4556737586899855" expires in 30 days
    When I view the card details
    Then I should see a warning message "This card expires soon. Request a replacement."
    And I should see a "Request Replacement" button

  Scenario: Card number validation (Luhn algorithm)
    Given I am adding a new card
    When I enter card number "1234567890123456"
    And I click the "Validate" button
    Then I should see an error message "Invalid card number"

  Scenario: Valid card number passes Luhn check
    Given I am adding a new card
    When I enter card number "4556737586899855"
    And I click the "Validate" button
    Then I should see a success message "Valid card number"
    And the card type should be automatically detected as "VISA"

  Scenario: Detect card type from card number
    When I enter card number starting with "4"
    Then the card type should be set to "VISA"
    When I enter card number starting with "5"
    Then the card type should be set to "MC"
    When I enter card number starting with "3"
    Then the card type should be set to "AMEX"

  Scenario: View transactions for a specific card
    Given card "4556737586899855" has 5 transactions
    When I view the card details
    And I click on the "Recent Transactions" tab
    Then I should see 5 transactions listed
    And the transactions should be sorted by date descending

  Scenario: Group cards by account
    When I navigate to the credit cards page
    And I enable "Group by Account" view
    Then I should see cards grouped under their respective accounts
    And account "10000000001" should show 2 cards
    And account "10000000002" should show 1 card

  Scenario: Search for card by last 4 digits
    When I navigate to the credit cards page
    And I enter "9855" in the search box
    And I click the search button
    Then I should see only 1 card
    And the card should end in "9855"

  Scenario: Mask card number in list view
    When I navigate to the credit cards page
    Then all card numbers should be displayed in masked format
    And full card numbers should not be visible
    And only the last 4 digits should be shown

  Scenario: Reveal full card number with security verification
    Given I am on the card details page for card "4556737586899855"
    When I click the "Show Full Number" button
    And I enter my password for verification
    Then the full card number "4556737586899855" should be displayed
    And it should auto-hide after 30 seconds

  Scenario: Sort cards by expiration date
    When I navigate to the credit cards page
    And I select sort by "Expiration Date (Earliest First)"
    Then the cards should be ordered as:
      | CardNumber       | ExpirationDate |
      | 3782822463100005 | 11/2024        |
      | 4556737586899855 | 12/2025        |
      | 5454545454545454 | 03/2026        |

  Scenario: Cannot perform transactions with inactive card
    Given card "3782822463100005" has status "Inactive"
    When I attempt to create a transaction with this card
    Then I should receive an error "Cannot process transaction with inactive card"
    And the transaction should not be created

  Scenario: Request card replacement
    Given I am on the card details page for card "4556737586899855"
    When I click the "Request Replacement" button
    And I select reason "Lost"
    And I confirm the request
    Then I should see a success message "Replacement card requested"
    And the current card should be deactivated
    And a replacement card should be initiated
    And I should receive a confirmation email
