# Transcribed verbatim from SPEC.md section 3, US-04.
# @ignore until the lead generation slice (US-04) is implemented in a later phase.
@ignore @us04 @leads
Feature: Corporate Lead Generation via Niche Landing Pages
  As a Commercial Director
  I want high-converting landing pages for niche logistics to capture qualified enterprise leads
  So that our sales team can follow up on high-margin contracts

  Scenario: Submitting an enterprise inquiry for FDOT Concrete Barricades logistics
    Given a prospective client visits the "FDOT Concrete Barricades" vertical landing page
    When the client fills out the contact form with company name "Apex Construction", email "contact@apex.com", and cargo details
    And submits the form via POST "/api/leads"
    Then the system should validate all mandatory fields successfully
    And save the lead record in the PostgreSQL database with status "New"
    And dispatch an asynchronous event via RabbitMQ to notify the commercial team
