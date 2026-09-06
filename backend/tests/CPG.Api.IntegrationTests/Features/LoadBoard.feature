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

  Scenario: A shipper posts a load and the carrier runs it through to delivery
    Given an authenticated Shipper posting freight
    When the shipper posts the load "CPG-BDD-010" from "Tampa, FL" to "Savannah, GA"
    Then the load "CPG-BDD-010" is on the board with status "Available"
    Given an authenticated Carrier on the load board
    When the carrier accepts load "CPG-BDD-010"
    Then the accept response reports status "Dispatched"
    When the carrier departs load "CPG-BDD-010"
    Then the depart response reports status "InTransit"
    When the carrier delivers load "CPG-BDD-010"
    Then the deliver response reports status "Delivered"

  Scenario: Departing a load that is already in transit is rejected
    Given an authenticated Carrier on the load board
    And an available load "CPG-BDD-011" from "Ocala, FL" to "Macon, GA"
    When the carrier accepts load "CPG-BDD-011"
    Then the accept response reports status "Dispatched"
    When the carrier departs load "CPG-BDD-011"
    Then the depart response reports status "InTransit"
    When the carrier departs load "CPG-BDD-011"
    Then the request fails with status 409
