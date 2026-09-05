@loadboard
Feature: Carrier & Shipper Load Workspace
  As a Carrier
  I want to see available freight on the load board and accept it
  So that a load is assigned to me transactionally and the dispatch desk is notified

  Scenario: Viewing and accepting an available load
    Given an authenticated Carrier on the load board
    And an available load "CPG-BDD-001" from "Miami, FL" to "Dallas, TX"
    When the carrier requests the board filtered by status "Available"
    Then the board response includes load "CPG-BDD-001"
    When the carrier accepts load "CPG-BDD-001"
    Then the accept response reports status "Dispatched"
    And the load "CPG-BDD-001" is assigned to the carrier in PostgreSQL
    And an audit log entry "LoadAccepted" is recorded for the load
    And the dispatch desk is notified through the broker

  Scenario: Accepting a load that is no longer available is rejected
    Given an authenticated Carrier on the load board
    And an already delivered load "CPG-BDD-002"
    When the carrier accepts load "CPG-BDD-002"
    Then the request fails with status 409
