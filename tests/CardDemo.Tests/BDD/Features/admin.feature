Feature: User Administration
  As an administrator
  I want to manage user accounts in the system
  So that I can control access and maintain security

  Background:
    Given I am logged in as "ADMIN" with role "ADMIN"
    And the following users exist:
      | UserId   | FirstName | LastName | UserType | Active |
      | USER01 | John      | Doe      | USER     | Yes    |
      | USER0002 | Jane      | Smith    | USER     | Yes    |
      | ADMIN002 | Bob       | Johnson  | ADMIN    | Yes    |

  Scenario: View list of all users
    When I navigate to the user management page
    Then I should see 4 users in the list (including myself)
    And I should see user "USER01" with role "USER"
    And I should see user "ADMIN002" with role "ADMIN"

  Scenario: Add a new regular user
    When I navigate to the user management page
    And I click the "Add User" button
    And I enter the following user information:
      | Field      | Value       |
      | User ID    | USER0003    |
      | Password   | NewPass123  |
      | First Name | Alice       |
      | Last Name  | Williams    |
      | User Type  | USER        |
    And I click the "Save" button
    Then I should see a success message "User created successfully"
    And user "USER0003" should appear in the user list
    And the password should be stored encrypted

  Scenario: Add a new admin user
    When I add a new user with:
      | UserId   | FirstName | LastName | UserType | Password  |
      | ADMIN003 | Charlie   | Brown    | ADMIN    | Admin@456 |
    Then the user should be created successfully
    And the user "ADMIN003" should have role "ADMIN"

  Scenario: Validate user ID is unique
    When I attempt to create a user with ID "USER01"
    Then I should see an error message "User ID already exists"
    And the user should not be created

  Scenario: Validate user ID format
    When I attempt to create a user with ID "abc"
    Then I should see a validation error "User ID must be 8 characters"
    When I attempt to create a user with ID "USER@001"
    Then I should see a validation error "User ID can only contain letters and numbers"

  Scenario: Validate password requirements
    When I attempt to create a user with password "123"
    Then I should see a validation error "Password must be at least 8 characters"
    When I attempt to create a user with password "short"
    Then I should see a validation error "Password must be at least 8 characters"

  Scenario: View user details
    When I navigate to the user management page
    And I click on user "USER01"
    Then I should see the user details page
    And I should see the following information:
      | Field      | Value    |
      | User ID    | USER01 |
      | First Name | John     |
      | Last Name  | Doe      |
      | User Type  | USER     |
      | Status     | Active   |
    And the password field should be masked

  Scenario: Update user information
    Given I am viewing user "USER01" details
    When I click the "Edit" button
    And I change the first name to "Jonathan"
    And I change the last name to "Doe-Smith"
    And I click the "Save" button
    Then I should see a success message "User updated successfully"
    And the first name should be "Jonathan"
    And the last name should be "Doe-Smith"

  Scenario: Update user password
    Given I am viewing user "USER01" details
    When I click the "Change Password" button
    And I enter new password "NewSecure123"
    And I confirm the new password "NewSecure123"
    And I click the "Update Password" button
    Then I should see a success message "Password updated successfully"
    And the new password should be encrypted and stored
    And a password change notification should be sent to the user

  Scenario: Password confirmation must match
    When I am changing password for user "USER01"
    And I enter new password "NewPass123"
    And I enter confirmation password "DifferentPass"
    Then I should see a validation error "Passwords do not match"

  Scenario: Delete user account
    Given I am viewing user "USER0002" details
    When I click the "Delete" button
    And I confirm the deletion by entering "DELETE"
    Then I should see a success message "User deleted successfully"
    And user "USER0002" should no longer appear in the user list
    And the user should be soft-deleted (not permanently removed)

  Scenario: Cannot delete currently logged in user
    Given I am logged in as "ADMIN"
    When I attempt to delete user "ADMIN"
    Then I should see an error message "Cannot delete currently logged in user"

  Scenario: Deactivate user account
    Given user "USER01" is active
    When I view user "USER01" details
    And I click the "Deactivate" button
    And I confirm the deactivation
    Then I should see a success message "User deactivated successfully"
    And user "USER01" status should be "Inactive"
    And the user should not be able to login

  Scenario: Reactivate user account
    Given user "USER01" is inactive
    When I view user "USER01" details
    And I click the "Reactivate" button
    Then I should see a success message "User reactivated successfully"
    And user "USER01" status should be "Active"
    And the user should be able to login again

  Scenario: Filter users by role
    When I navigate to the user management page
    And I select role filter "USER"
    Then I should only see users with role "USER"
    And I should not see admin users

  Scenario: Filter users by status
    Given some users are inactive
    When I navigate to the user management page
    And I select status filter "Active"
    Then I should only see active users
    And inactive users should not be displayed

  Scenario: Search users by name
    When I navigate to the user management page
    And I enter "John" in the search box
    And I click the search button
    Then I should see users with "John" in their name
    And the results should include "USER01"

  Scenario: Sort users by user ID
    When I navigate to the user management page
    And I select sort by "User ID (A-Z)"
    Then users should be ordered alphabetically by user ID

  Scenario: View user activity log
    Given user "USER01" has recent login activity
    When I view user "USER01" details
    And I click on the "Activity Log" tab
    Then I should see a list of recent activities:
      | Activity Type | Date       | Details            |
      | Login         | 2025-11-30 | Successful login   |
      | Login Failed  | 2025-11-29 | Invalid password   |
      | Update        | 2025-11-28 | Changed first name |

  Scenario: Reset user password (admin-initiated)
    Given user "USER01" forgot their password
    When I view user "USER01" details
    And I click the "Reset Password" button
    And I confirm the password reset
    Then a temporary password should be generated
    And the temporary password should be sent to user's email
    And the user should be required to change password on next login

  Scenario: Unlock locked user account
    Given user "USER01" is locked due to failed login attempts
    When I view user "USER01" details
    And I click the "Unlock Account" button
    And I confirm the unlock
    Then the account should be unlocked
    And the failed login counter should be reset
    And the user should be able to login

  Scenario: Regular user cannot access user management
    Given I am logged in as "USER01" with role "USER"
    When I attempt to navigate to the user management page
    Then I should receive a 403 Forbidden error
    And I should see a message "You do not have permission to access this page"

  Scenario: Audit user management actions
    When I create a new user "USER0003"
    Then an audit log entry should be created with:
      | Field          | Value                  |
      | Action         | User Created           |
      | Performed By   | ADMIN               |
      | Target User    | USER0003               |
      | Timestamp      | Current date and time  |

  Scenario: Bulk user import from CSV
    When I navigate to the user management page
    And I click the "Import Users" button
    And I upload a CSV file with 10 valid user records
    And I click "Import"
    Then I should see a success message "10 users imported successfully"
    And all 10 users should appear in the user list

  Scenario: Validate bulk import data
    When I upload a CSV file with invalid user data
    Then I should see validation errors for each invalid record
    And I should be able to review and correct errors
    And only valid records should be imported

  Scenario: Export users to CSV
    Given there are 20 users in the system
    When I navigate to the user management page
    And I click the "Export Users" button
    Then a CSV file should be downloaded
    And the file should contain all 20 user records
    And the file should not include passwords

  Scenario: View user permissions summary
    When I view user "USER01" details
    And I click on the "Permissions" tab
    Then I should see a list of permissions based on user role
    And USER role should have permissions:
      | Permission           | Allowed |
      | View Accounts        | Yes     |
      | Modify Accounts      | Yes     |
      | View Transactions    | Yes     |
      | Manage Users         | No      |

  Scenario: Enforce strong password policy
    When I create or update a user password
    Then the password must meet the following criteria:
      | Criterion                    | Requirement |
      | Minimum length               | 8 characters |
      | Contains uppercase           | At least 1   |
      | Contains lowercase           | At least 1   |
      | Contains digit               | At least 1   |
      | Does not match User ID       | Required     |

  Scenario: Prevent common passwords
    When I attempt to set password "Password123"
    Then I should see a warning "This password is commonly used and not secure"
    And I should be encouraged to choose a stronger password
