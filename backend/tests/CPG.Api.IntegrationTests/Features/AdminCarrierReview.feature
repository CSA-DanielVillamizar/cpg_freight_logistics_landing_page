@admin @compliance
Feature: Admin carrier compliance review
  As an Administrator
  I want to review the documents a carrier filed and approve or reject them
  So that only vetted carriers are cleared to accept high-value loads

  Scenario: Approving a carrier that is under review
    Given an administrator is authenticated
    And a carrier "Northbound Freight LLC" with a filed COI is under review
    When the administrator lists carriers filtered by status "UnderReview"
    Then the carrier "Northbound Freight LLC" appears in the list
    When the administrator approves the carrier
    Then the review response reports status "Verified"
    And the carrier's compliance status in PostgreSQL is "Verified"
    And an audit log entry "CarrierComplianceReviewed" is recorded for the carrier

  Scenario: Reviewing a carrier with no documents is rejected
    Given an administrator is authenticated
    And a carrier "Empty Docs Transport" with no documents
    When the administrator approves the carrier
    Then the review request fails with status 409
