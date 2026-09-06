@loaddeletion
Feature: Administrative load deletion and synthetic-data isolation
  As an Administrator
  I want to logically delete a load and void its invoice
  So that residual and test data disappears from every operational view but stays on the audit trail

  Scenario: Admin deletes a delivered load and its pending invoice
    Given a delivered load "CPG-BDD-020" billed to the shipper with a pending invoice
    When an admin deletes load "CPG-BDD-020"
    Then the delete request succeeds with status 204
    And load "CPG-BDD-020" is not on the load board
    And load "CPG-BDD-020" is not in the shipper portal
    And the invoice for "CPG-BDD-020" is soft-deleted and Cancelled
    And an audit log entry "LoadDeleted" exists for load "CPG-BDD-020"

  Scenario: A carrier cannot delete a load
    Given a fresh available load "CPG-BDD-021" from "Tampa, FL" to "Macon, GA"
    When the carrier attempts to delete load "CPG-BDD-021"
    Then the delete is rejected with status 403

  Scenario: Deleting a load twice is a no-op
    Given a delivered load "CPG-BDD-022" billed to the shipper with a pending invoice
    When an admin deletes load "CPG-BDD-022"
    Then the delete request succeeds with status 204
    When an admin deletes load "CPG-BDD-022"
    Then the delete request succeeds with status 204

  Scenario: Synthetic E2E loads are filtered from the board but visible to admin
    Given a synthetic load "CPG-E2E-BDD-777" exists in the database
    When the admin requests the load board
    Then load "CPG-E2E-BDD-777" is not on the load board
    When the admin requests the admin load list
    Then load "CPG-E2E-BDD-777" is in the admin load list and flagged synthetic
