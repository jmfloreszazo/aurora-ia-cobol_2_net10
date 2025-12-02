Feature: Report Generation
  As a registered user
  I want to generate various reports
  So that I can analyze my account activity and financial data

  Background:
    Given I am logged in as "USER0001"
    And the following accounts exist:
      | AccountId   | CustomerId | CurrentBalance | CreditLimit |
      | 10000000001 | 100000001  | 1500.00        | 5000.00     |
    And account "10000000001" has transactions for the past 12 months

  # ============================================
  # Monthly Reports (CORPT00C equivalent)
  # ============================================

  Scenario: Generate monthly transaction report
    When I navigate to the reports page
    And I select report type "Monthly Transaction Report"
    And I select account "10000000001"
    And I select month "November" and year "2025"
    And I click "Generate Report"
    Then I should see a report with the following summary:
      | Field                | Description              |
      | Period               | November 2025            |
      | Total Transactions   | Count of transactions    |
      | Total Debits         | Sum of purchases/fees    |
      | Total Credits        | Sum of payments          |
      | Net Change           | Credits - Debits         |
      | Ending Balance       | Balance at month end     |

  Scenario: Monthly report includes transaction breakdown
    When I generate a monthly report
    Then the report should include a breakdown by category:
      | Category        | Transaction Count | Total Amount |
      | Restaurants     | 5                 | $250.00      |
      | Gas Stations    | 3                 | $120.00      |
      | Online Shopping | 8                 | $450.00      |
      | Payments        | 2                 | -$500.00     |

  Scenario: Monthly report shows daily spending pattern
    When I generate a monthly report
    Then I should see a daily spending chart
    And I should see which days had the most activity
    And I should see average daily spending

  # ============================================
  # Yearly Reports
  # ============================================

  Scenario: Generate yearly transaction summary
    When I select report type "Yearly Summary"
    And I select year "2025"
    And I click "Generate Report"
    Then I should see a summary for each month:
      | Month     | Transactions | Debits    | Credits   | Net      |
      | January   | 15           | $800.00   | $500.00   | -$300.00 |
      | February  | 12           | $650.00   | $600.00   | -$50.00  |
      | ...       | ...          | ...       | ...       | ...      |
    And I should see yearly totals

  Scenario: Yearly report includes trends
    When I generate a yearly report
    Then I should see spending trends over the year
    And I should see comparison to previous year (if available)
    And I should see average monthly spending

  # ============================================
  # Custom Date Range Reports
  # ============================================

  Scenario: Generate custom date range report
    When I select report type "Custom Date Range"
    And I set start date "2025-10-01"
    And I set end date "2025-11-30"
    And I click "Generate Report"
    Then the report should only include transactions in that range
    And the summary should reflect the custom period

  Scenario: Validate date range
    When I set end date before start date
    Then I should see a validation error "End date must be after start date"
    
    When I set a date range exceeding 2 years
    Then I should see a warning "Large date range may take longer to generate"

  # ============================================
  # Report Filtering
  # ============================================

  Scenario: Filter report by transaction type
    When I generate a report
    And I filter by transaction type "Purchases Only"
    Then only purchase transactions should be included
    And payment transactions should be excluded

  Scenario: Filter report by merchant category
    When I generate a report
    And I filter by category "Restaurants"
    Then only restaurant transactions should be included
    And category totals should reflect the filter

  Scenario: Filter report by amount range
    When I generate a report
    And I filter by amount range $50.00 to $200.00
    Then only transactions within that amount range should be included

  # ============================================
  # Report Export
  # ============================================

  Scenario: Export report to PDF
    Given I have generated a monthly report
    When I click "Export to PDF"
    Then a PDF file should be downloaded
    And the PDF should include:
      | Section            |
      | Report Header      |
      | Account Summary    |
      | Transaction Table  |
      | Category Breakdown |
      | Charts/Graphs      |

  Scenario: Export report to Excel
    Given I have generated a report
    When I click "Export to Excel"
    Then an Excel file should be downloaded
    And the file should have separate sheets for:
      | Sheet Name         |
      | Summary            |
      | Transactions       |
      | Categories         |
      | Trends             |

  Scenario: Export report to CSV
    Given I have generated a report
    When I click "Export to CSV"
    Then a CSV file should be downloaded
    And the file should be importable into other systems

  # ============================================
  # Comparative Reports
  # ============================================

  Scenario: Compare two periods
    When I select report type "Period Comparison"
    And I select period 1: "October 2025"
    And I select period 2: "November 2025"
    And I click "Generate Comparison"
    Then I should see a side-by-side comparison:
      | Metric             | October   | November  | Change    |
      | Total Transactions | 20        | 25        | +25%      |
      | Total Spending     | $800.00   | $950.00   | +18.75%   |
      | Avg Transaction    | $40.00    | $38.00    | -5%       |

  # ============================================
  # Account Statement
  # ============================================

  Scenario: Generate account statement
    When I select report type "Account Statement"
    And I select account "10000000001"
    And I select statement period "November 2025"
    Then I should see a formal statement including:
      | Section                |
      | Account Information    |
      | Statement Period       |
      | Previous Balance       |
      | Payments Received      |
      | New Charges            |
      | Interest Charges       |
      | Current Balance        |
      | Minimum Payment Due    |
      | Payment Due Date       |
      | Transaction Details    |

  # ============================================
  # Spending Analysis
  # ============================================

  Scenario: View spending by category pie chart
    When I navigate to the reports page
    And I select "Spending Analysis"
    Then I should see a pie chart showing spending by category
    And each slice should show percentage of total

  Scenario: View spending trends line chart
    When I select "Spending Trends"
    And I select time period "Last 6 Months"
    Then I should see a line chart showing monthly spending
    And I should see trend indicators (up/down)

  Scenario: Top merchants analysis
    When I generate a spending analysis
    Then I should see top 10 merchants by:
      | Column           |
      | Merchant Name    |
      | Transaction Count|
      | Total Amount     |
      | % of Spending    |

  # ============================================
  # Scheduled Reports
  # ============================================

  Scenario: Schedule recurring report
    When I configure a scheduled report:
      | Setting        | Value               |
      | Report Type    | Monthly Summary     |
      | Frequency      | Monthly             |
      | Day of Month   | 1st                 |
      | Email To       | user@example.com    |
    Then the report should be scheduled
    And I should receive it automatically each month

  # ============================================
  # Report Access Control
  # ============================================

  Scenario: User can only see reports for their accounts
    Given user "USER0001" owns account "10000000001"
    And user "USER0002" owns account "10000000099"
    When I attempt to generate a report for account "10000000099"
    Then I should see an error "You do not have access to this account"

  Scenario: Admin can generate reports for any account
    Given I am logged in as "ADMIN001" with role "ADMIN"
    When I generate a report for any account
    Then the report should be generated successfully
