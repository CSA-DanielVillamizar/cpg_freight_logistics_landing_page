# US-02 — Dynamic Rate Calculation · Reporte de cierre

Estado: **100% en verde**. Motor de tarificación en tiempo real + calculadora React,
implementado end-to-end con BDD.

## Condición de éxito — verificación

| Check | Resultado |
| --- | --- |
| `dotnet build backend/CPG.sln` | ✅ 0 warnings / 0 errors |
| `dotnet test backend/CPG.sln` | ✅ **US-02 en verde** — `Calculating rate for a Cold Chain refrigerated shipment` passed; suite completa sin fallos |
| `npm run build` / `npm run lint` / `tsc --noEmit` | ✅ los tres limpios |
| `POST /api/rates/calculate` responde `200` con el desglose del contrato SPEC.md §4 | ✅ |
| Escenario exacto SPEC.md: Cold Chain, Miami→Orlando, 35000 lbs, -20 °C | ✅ `coldChainSurcharge = 350.00` (idéntico al ejemplo de SPEC.md) |
| **Rendimiento < 500 ms** | ✅ cómputo real (`X-Rate-Compute-Ms`): ~0.7–4 ms en caliente, ~14 ms en el primer request (tras warm-up de startup) |
| Validación (ZIP inválido, peso fuera de rango, Cold Chain sin temperatura) → `400` | ✅ |
| Calculadora React funcional y responsiva | ✅ (verificado en navegador: desglose base $1,242.99 / cold-chain $350.00 / fuel $186.45 / total $1,779.44) |

## Backend

### Motor de tarificación (`Features/Rates/Engine/`) — Application layer, puro y en memoria
- **`IRateEngine`** → `RateEngine`: orquesta distancia → tarifa base → cadena de recargos → total.
- **Strategy** (`IServiceRateStrategy`): una estrategia de tarifa base por línea de servicio —
  `ColdChainRateStrategy`, `HeavyHaulRateStrategy`, `FlatbedRateStrategy`, `FdotConcreteRateStrategy`
  (cada una con su `$/milla`, `$/lb` y mínimo).
- **Chain of Responsibility** (`SurchargeHandler`): `ColdChainSurchargeHandler` →
  `FuelSurchargeHandler`. Cold-chain = `|Δ°C bajo cero| × lbs × 0.0005`; fuel = `15% × base`.
- **`IDistanceCalculator`** → `ZipCentroidDistanceCalculator`: tabla ZIP3 → centroide
  (lat/lon) embebida (Florida + corredor sureste + hubs nacionales) + haversine × 1.18
  (factor de circuito vial). Sin geocoding externo → O(1), microsegundos.

### CQRS (`Features/Rates/Calculate/`)
- `CalculateRateQuery` (+ `FromRequest`/`ToRequest`) + `CalculateRateQueryValidator`
  (ZIP `^\d{5}$`, peso 1–200 000, Cold Chain exige temperatura −60..30) + `CalculateRateQueryHandler`
  (delega en `IRateEngine`, sin I/O → `Task.FromResult`).

### API
- `RatesController.Calculate` (`[AllowAnonymous]`) → `sender.Send`, mide con `Stopwatch` y
  emite las cabeceras `X-Rate-Compute-Ms` y `Server-Timing: rate;dur=…`.
- **Warm-up de startup** (`WarmUpExtensions.WarmUpRateEngineAsync`): tras `Build()` se dispara
  un `CalculateRateQuery` de descarte para pagar JIT + compilación de validadores antes del
  primer request real → garantiza el presupuesto de 500 ms también en frío.

## Frontend (`src/features/rates/`)
- `serviceLines.ts` — las 4 líneas de servicio con metadata (`requiresTemperature`).
- `useRateCalculator.ts` — hook sobre `POST /api/rates/calculate` (anónimo), expone
  `fieldErrors` (mapea `problem.errors` → mensajes por campo) y `errorMessage`.
- `RateCalculatorPage.tsx` — selector de línea de servicio como botones (patrón de los PNG),
  origen/destino/peso/temperatura (condicional), métricas, layout `lg:grid-cols-[1.15fr_0.85fr]`
  (desktop 2 columnas, mobile apilado). Errores de validación por campo inline.
- `RateBreakdown.tsx` — panel oscuro con desglose monoespaciado: tarifa base, recargo
  cold-chain, recargo combustible, total en hazard-orange, timestamp y moneda.

## BDD
- `Features/RateCalculation.feature`: `@ignore` **retirado**.
- `StepDefinitions/RateCalculationStepDefinitions.cs` — 7 steps; mapea `"Miami, FL"`→`33101`,
  `"Orlando, FL"`→`32801`, `"Cold Chain"`→`ColdChain`; el `When` hace un request de warm-up y
  luego el medido; asserts: `200 OK`, `X-Rate-Compute-Ms < 500`, y los 3 componentes del
  desglose presentes con `total ≈ base + cold + fuel`.
- Unit tests (`CPG.Application.UnitTests/RateEngineTests.cs`): desglose Cold Chain (350.00 exacto),
  recargo cold-chain = 0 para no-reefer, 1 000 cálculos en < 1 ms de media.

## Números de calibración (ejemplo SPEC.md)

Cold Chain · 33101→32801 (~236 mi road) · 35 000 lbs · −20 °C:

| Componente | Valor | Fórmula |
| --- | --- | --- |
| Base | $1 242.99 | `236 mi × $4.00 + 35 000 lb × $0.008` |
| Cold-chain surcharge | **$350.00** | `20 °C × 35 000 lb × $0.0005` |
| Fuel surcharge | $186.45 | `15% × base` |
| **Total** | **$1 779.44** | suma |

## Siguiente
- US-03 (Carrier Compliance) — quita `@ignore` de `CarrierCompliance.feature`.
