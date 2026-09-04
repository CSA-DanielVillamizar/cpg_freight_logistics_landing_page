# US-01 — RBAC & Secure Authentication · Reporte de cierre

Estado: **100% en verde**. Slice vertical de autenticación y autorización implementado
end-to-end (backend + frontend + BDD).

## Condición de éxito — verificación

| Check | Resultado |
| --- | --- |
| `dotnet build backend/CPG.sln` | ✅ 0 warnings / 0 errors |
| `dotnet test backend/CPG.sln` | ✅ **13 passed, 3 skipped, 0 failed** (los 2 escenarios US-01 en verde; 3 `@ignore` restantes son US-02/03/04) |
| `npm run build` / `npm run lint` / `tsc --noEmit` (`frontend/`) | ✅ los tres limpios |
| Login válido → 200 + JWT + refresh + user | ✅ verificado (curl + BDD + UI) |
| Credenciales inválidas → 401 | ✅ |
| Refresh válido → 200 con rotación; reuso del token rotado → 401 | ✅ |
| Validación de campos (email/password vacíos) → 400 con `errors` | ✅ |
| `Admin` → `GET /api/admin/audit-logs` → 200 | ✅ |
| `Carrier` → `GET /api/admin/audit-logs` → **403 + "Access denied"** | ✅ (coincide literalmente con SPEC.md US-01 escenario 2) |
| Anónimo → endpoint protegido → 401 | ✅ |
| Frontend: login, persistencia de sesión, guard por rol, sign-out | ✅ (Admin entra a `/admin/audit-logs`; Carrier es redirigido) |

## Backend

### Seguridad / tokens
- `IPasswordHasher` (Application) + `BCryptPasswordHasher` (Infrastructure, work factor 12).
- `JwtTokenService` implementado: firma HS256, claims `sub`/`email`/`jti`/`name`/`role`,
  access token 15 min (config), refresh token opaco (64 bytes aleatorios) 7 días.
- `TokenPair` extendido con la expiración del refresh token.

### CQRS (`Features/Authentication/`)
- `LoginCommand` + `LoginCommandValidator` + `LoginCommandHandler` — verificación uniforme
  (no revela si el email existe), persiste el `RefreshToken`, devuelve `AuthResponse`.
- `RefreshTokenCommand` + validator + handler — valida vigencia/revocación, **rota** el token
  (revoca el presentado, emite par nuevo).
- `UnauthorizedException` (Application) → HTTP 401 en `GlobalExceptionHandler`.
- DTOs: `LoginRequest`, `RefreshRequest`, `AuthResponse`, `AuthenticatedUser`.

### Endpoints
- `POST /api/auth/login`, `POST /api/auth/refresh` (`AuthController` vía `ISender`).
- `GET /api/admin/audit-logs` protegido con `[Authorize(Policy = AdminOnly)]`.

### RBAC
- Políticas `AdminOnly` / `CarrierOnly` / `ShipperOnly` operativas
  (`RequireRole` + `RoleClaimType = ClaimTypes.Role`).
- `CpgAuthorizationResultHandler` (`IAuthorizationMiddlewareResultHandler`): escribe cuerpo
  RFC 7807 en fallo de autorización — 403 con `"Access denied"`, 401 con `"Authentication required"`.

### Seeding
- `ApplicationDbContextInitialiser` — aplica migraciones + siembra 3 usuarios RBAC:
  `admin@cpgorlando.com` / `carrier@cpgorlando.com` / `shipper@cpgorlando.com`,
  contraseña `Passw0rd!` (constante `SeedPassword`, solo fuera de Production).
- Ejecutado en el arranque (`Program`) para todos los entornos excepto Production, y por el
  host de `WebApplicationFactory` en los tests.

### Fix de configuración (WebApplicationFactory)
Las lecturas de `IConfiguration` durante el registro de servicios (connection strings,
`Jwt:SigningKey`) ocurrían **antes** de `builder.Build()`, por lo que los overrides de
`WebApplicationFactory` no se aplicaban (login devolvía 500: `IDX10703` clave de firma
vacía; y los tests apuntaban a la infra de dev en vez de a los contenedores). Corregido:
- `AddDbContext` lee el connection string dentro del factory `(sp, options) =>` vía `IConfiguration` del contenedor.
- MassTransit/RabbitMQ lo lee dentro de `UsingRabbitMq((context, cfg) => …)`.
- `IBlobStorage` se resuelve por factory según `IOptions<BlobStorageOptions>`.
- `JwtBearerOptions` se configura vía `Configure<IOptions<JwtOptions>>` (binding perezoso).
- Health check Npgsql usa `Func<IServiceProvider,string>`.

## Frontend (`src/features/auth/`)

- `authContext.ts` (Context) + `AuthProvider.tsx` (Context API + `useState`/`useRef`,
  persistencia en `localStorage`, `try/catch` en storage).
- `useAuth()` hook; `RequireRole` guard (redirige a `/login` si anónimo, a `/` si rol incorrecto).
- `authApi` (`login`, `refresh`) + integración en el cliente HTTP:
  `registerAuthBridge` inyecta `Authorization: Bearer <token>` y ejecuta el flujo
  **401 → refresh → retry (una vez) → onAuthLost/logout**.
- `LoginPage` (design tokens: Chivo, navy, hazard-orange, componentes `Card`/`Input`/`Button`),
  con atajos a las cuentas seed en dev.
- `AuditLogsPage` (admin-only) + ruta `/admin/audit-logs` envuelta en `<RequireRole role="Admin">`.
- `SiteHeader` muestra email + "Sign out" autenticado, "Sign in" si no; link "Audit log" solo para Admin.

## BDD

- `Features/Authentication.feature`: etiqueta `@ignore` **retirada**.
- `StepDefinitions/AuthenticationStepDefinitions.cs` — implementa los 8 steps Given/When/Then.
- `Hooks/InfrastructureHooks.cs` + `Support/TestApp.cs` — un stack de Testcontainers
  (PostgreSQL + RabbitMQ + Azurite) por corrida, compartido por todos los escenarios;
  `CpgApiFactory` hospeda la API contra esos contenedores.
- `Support/ScenarioState.cs` — estado por escenario (HttpClient, última respuesta).

## Pendiente / siguiente

- US-02 Rate Calculator (quita `@ignore` de `RateCalculation.feature`).
- Endurecer refresh-token (familia/reuse-detection, límite por usuario) cuando haya más tráfico real.
