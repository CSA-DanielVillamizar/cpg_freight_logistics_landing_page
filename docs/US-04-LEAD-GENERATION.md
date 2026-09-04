# US-04 — Corporate Lead Generation via Niche Landing Pages · Reporte de cierre

Estado: **100% en verde**. Motor de captura de leads (CRO) público, event-driven, con las 4
landing pages verticales conectadas.

## Condición de éxito — verificación

| Check | Resultado |
| --- | --- |
| `dotnet test backend/CPG.sln` | ✅ **toda la suite en verde, SIN skips** (ver reporte de cierre del proyecto) |
| BDD `Submitting an enterprise inquiry for FDOT Concrete Barricades logistics` | ✅ **passed [4 s]** |
| `npm run build` / `npm run lint` / `tsc --noEmit` | ✅ los tres limpios |
| `POST /api/leads` público (sin JWT) → `200` con status `New` | ✅ |
| Validación estricta anti-spam / anti-inyección → `400` | ✅ (markup `<>`, links `http/www`, email/teléfono/slug malformados) |
| Lead persistido en PostgreSQL con estado `New` | ✅ |
| Evento `CorporateLeadGenerated` publicado y consumido de RabbitMQ | ✅ el consumer escribe `CommercialTeamNotified`/`Lead`; el BDD hace *poll* |
| Front y back integrados | ✅ verificado en navegador: formulario en la vertical FDOT → 200 → toast + estado "Thank you" → fila en `leads` + audit row |

## Backend

### Dominio
- `Lead` es ahora un agregado con **factory** `RegisterFromLandingPage(...)`: normaliza los
  campos, arranca en `LeadStatus.New` (setter privado) y levanta
  `CorporateLeadGeneratedDomainEvent`. `ContactName` y `Phone` pasaron a obligatorios
  (migración `LeadContactRequired`).

### CQRS (`Features/Leads/`)
- `CreateLeadCommand` + `CreateLeadCommandValidator` (FluentValidation, `partial` con
  `GeneratedRegex`): empresa/nombre 2–200, email válido ≤256, **teléfono** `^[+()\-.\s0-9]{7,40}$`,
  **slug** kebab-case, `CargoDetails` 5–2000 **sin `<>` y sin `http(s)://` / `www.`**,
  `ServiceType` `IsInEnum` cuando viene.
- `CreateLeadCommandHandler` — `Lead.RegisterFromLandingPage(...)` → `Leads.Add()` → un
  `SaveChangesAsync` → el domain event se despacha a RabbitMQ post-commit por
  `DispatchDomainEventsInterceptor`.
- `CorporateLeadGeneratedDomainEventHandler` → `IEventBus` →
  `CorporateLeadGeneratedIntegrationEvent` (RabbitMQ).

### Infraestructura
- `LeadNotificationConsumer : IConsumer<CorporateLeadGeneratedIntegrationEvent>` — consume de
  RabbitMQ y registra `CommercialTeamNotified` (`EntityName = "Lead"`) en `audit_log`.

### API
- `LeadsController.Create` (`[AllowAnonymous]`) → `200 OK` con `{ id, status }`.

## Frontend (`src/features/landing/` + `src/features/leads/`)
- **`verticalContent.ts`** — contenido completo por vertical (eyebrow, headline, subhead,
  badges, métricas, 4 tarjetas de servicio con tag/spec, 3 *proof points*, testimonial,
  encabezado de formulario, placeholder) extraído de los PNG. 4 verticales:
  `fdot-concrete-barricades`, `refrigerated-cold-chain`, `flatbed-heavy-haul`,
  `mobile-rate-calculator`.
- **`VerticalLandingPage.tsx`** — hero navy → métricas → catálogo de equipos → *proof points*
  + formulario de cotización → testimonial. Responsivo desktop-first (grids `md:`/`sm:`),
  tokens del design system (Chivo, navy `#0B192C`, hazard-orange `#EA580C`, mono).
- **`LeadCaptureForm.tsx`** — company, nombre, teléfono, email, textarea de detalles;
  `POST /api/leads` (anónimo); errores por campo desde `problem.errors`; **toast** `sonner`
  de éxito + estado "Thank you".
- `LandingPage.tsx` usa `VERTICAL_CONTENT`; se eliminó el `verticals.ts` antiguo.

## BDD (`Features/LeadGeneration.feature` — **último `@ignore` retirado**)
`StepDefinitions/LeadGenerationStepDefinitions.cs`:
- **Given**: mapea `"FDOT Concrete Barricades"` → `fdot-concrete-barricades`.
- **When**: rellena empresa `Apex Construction`, email `contact@apex.com`, detalles de carga
  (+ nombre y teléfono válidos) y hace `POST /api/leads`.
- **Then**: `200 OK` + status `New`; consulta la tabla `leads` (status/empresa/email/slug);
  *poll* ≤20 s a que aparezca `CommercialTeamNotified`/`Lead` (evento consumido de RabbitMQ
  efímero).

Unit tests: `CreateLeadCommandValidatorTests` (7 casos: happy path + rechazos de email,
markup, links, slug, teléfono), `DomainPrimitivesTests` actualizado a la factory.

## Siguiente
Ver `docs/PROJECT-CLOSE.md` — reporte de cierre del proyecto completo.
