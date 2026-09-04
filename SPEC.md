

CPG Enterprises Logistics Platform: Master FDE & SSD Execution Specification

1. Directivas de Spec-Driven Development (SSD) y Restricciones Anti-Alucinación para Claude Code

Para garantizar que Claude Code genere código ejecutable y libre de alucinaciones, se imponen las siguientes reglas obligatorias:








Tipado Estricto: En C#, activar obligatoriamente <Nullable>enable</Nullable>. En TypeScript, el uso de any está estrictamente prohibido; se deben tipar explícitamente todos los DTOs y props.





Estructura de Directorios C-4 / Clean Architecture:








src/Domain/: Entidades de negocio, objetos de valor y excepciones de dominio.





src/Application/: Casos de uso, interfaces de repositorio, validaciones con FluentValidation y CQRS (MediatR).





src/Infrastructure/: Implementación de Entity Framework Core, contextos de PostgreSQL, clientes externos y brokers de mensajes (MassTransit/RabbitMQ).





src/Presentation/Controllers/: Endpoints Web API limpios con decoradores de autorización y contratos OpenAPI.





src/frontend/: Aplicación React + Tailwind estructurada por características (feature-first).



2. Ingeniería Desplegada en el Terreno (FDE) - Requisitos del Mundo Real





Idempotencia de Red: Las operaciones de creación de cargas (POST /api/loads) deben obligatoriamente recibir una cabecera Idempotency-Key: <UUID> para prevenir duplicidades ante cortes de señal celular de los transportistas en autopistas.





Trazabilidad Distribuida: Todos los servicios deben propagar las cabeceras de OpenTelemetry (traceparent) para auditoría de punta a punta.





Control de Concurrencia optimista: Las tablas transaccionales en PostgreSQL deben incluir una columna de versión (xmin o campo RowVersion) para evitar condiciones de carrera en la asignación de cargas.



3. Especificaciones Ejecutables en Sintaxis Gherkin (BDD)

US-01: Sistema de Autenticación y Control de Accesos Basado en Roles (RBAC)

Gherkin

Feature: RBAC and Secure Authentication
  As a System Administrator
  I want to restrict access to platform modules based on user roles (Admin, Carrier, Shipper)
  So that sensitive logistics data and load operations remain secure and compliant

  Scenario: Successful login with valid credentials
    Given a user exists with email "admin@cpgorlando.com" and role "Admin"
    When the user sends a POST request to "/api/auth/login" with valid credentials
    Then the response status code should be 200
    And the response body should contain a valid JWT access token and a refresh token

  Scenario: Unauthorized access to admin endpoints
    Given an authenticated user with role "Carrier"
    When the user attempts to send a GET request to "/api/admin/audit-logs"
    Then the response status code should be 403 Forbidden
    And the response body must contain an error message "Access denied"


US-02: Calculador Interactivo de Tarifas Móviles (Mobile Freight & Rate Calculator)

Gherkin

Feature: Dynamic Rate Calculation for Specialized Freight
  As a Shipper
  I want to calculate precise shipping rates in real-time
  So that I can budget for cold chain, heavy haul, or FDOT concrete transport accurately

  Scenario: Calculating rate for a Cold Chain refrigerated shipment
    Given a Shipper requests a rate calculation for service type "Cold Chain"
    And origin is "Miami, FL" and destination is "Orlando, FL"
    And cargo weight is 35000 lbs with target temperature of -20°C
    When the client invokes POST "/api/rates/calculate"
    Then the system should return HTTP status 200 OK
    And the computation time must be less than 500 milliseconds
    And the response must break down base rate, cold chain surcharge, and fuel surcharge


US-03: Portal de Transportistas y Gestión de Cumplimiento (Carrier Load Board & Compliance)

Gherkin

Feature: Carrier Document Compliance and Verification
  As a Carrier
  I want to upload my mandatory legal documents (COI, Insurance, FDOT permits)
  So that my account status updates from Pending to Verified to accept high-value loads

  Scenario: Successfully uploading a Certificate of Insurance (COI)
    Given an authenticated Carrier with ID "CAR-001" and status "Pending Compliance"
    When the carrier uploads a valid PDF file "coi_insurance.pdf" of size 2.4 MB via POST "/api/compliance/upload"
    Then the system should store the file securely in cloud blob storage
    And the carrier compliance record should update to status "Under Review"
    And an audit log entry must be recorded in PostgreSQL with timestamp and user ID


US-04: Landing Pages Verticales de Nicho con Captura de Leads (CRO)

Gherkin

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


4. Contratos de API Esenciales (OpenAPI / DTOs de Referencia)

Endpoint de Tarificación (POST /api/rates/calculate)





Request Body:




JSON

{
  "serviceType": "ColdChain",
  "originZip": "33101",
  "destinationZip": "32801",
  "weightLbs": 35000,
  "targetTemperatureCelsius": -20.0
}




Response Body (200 OK):




JSON

{
  "baseRate": 1200.50,
  "coldChainSurcharge": 350.00,
  "fuelSurcharge": 180.25,
  "totalEstimated": 1730.75,
  "currency": "USD",
  "calculatedAt": "2026-09-03T16:20:53Z"
}


