# VitalHelse

Nettbutikk for helse- og hudpleieprodukter, bygget med ASP.NET Core 9 MVC, Identity og SQLite.
Prosjektet kjører lokalt uten noen hemmelige nøkler.

## Kom i gang

Krav: [.NET 9 SDK](https://dotnet.microsoft.com/download) eller nyere (fungerer også med .NET 10).

```bash
./run.sh          # macOS / Linux
.\run.ps1         # Windows PowerShell (eller run.bat)
```

Eller manuelt: `dotnet run --project vitalhelse` og åpne <http://localhost:1515>.

Databasen (`vitalhelse/app.db`) opprettes og fylles med produkter og kategorier ved første oppstart,
og beholdes mellom kjøringer. Slett filen, eller sett `Demo__ResetDatabase=true`, for å starte på nytt.

## Demobrukere

Passord for alle: `Password1.`

| Rolle | E-post |
|---|---|
| Admin | admin@vitalhelse.no |
| Ansatt (Staff) | staff@vitalhelse.no |
| Bedriftskunde | business@vitalhelse.no |
| Privatkunde | customer@vitalhelse.no |

Du kan også registrere en ny konto; i demo-modus bekreftes den automatisk.

## Ferdig utfylte data

Ved første oppstart fylles databasen med et realistisk utvalg: 100+ produkter med tags og størrelser,
rabattsatser per kategori, fire kampanjer (to aktive), rabattkoder, forsidebannere, seks kunder med adresser og 12 bestillinger.

Rabattkoder du kan prøve i kassen: `VITAL10` (10 %), `VELKOMMEN15` (15 %). `SOMMER20` er utløpt og `ANSATT30` er deaktivert.
Kampanjer slås av automatisk når sluttdatoen passeres (sjekkes hver halvtime).

### Bytte forsidebannere

Overskriv `vitalhelse/wwwroot/images/banners/banner-1.jpg` … `banner-3.jpg` med egne bilder (21:9, f.eks. 2520×1080, JPG).
Se `vitalhelse/wwwroot/images/banners/README.md`.

## Priser, rabatter og MVA

- Alle priser i databasen er inkl. 25 % MVA. Bedriftskunder og ansatte kan bytte mellom «MVA» og «Eks. MVA» i toppmenyen; eks. MVA regnes alltid ut fra prisen.
- Kampanjepriser regnes ut av kampanjene (aktive, innenfor start- og sluttdato) og rabattsatsene per kategori. De oppdateres automatisk når en kampanje endres, utløper, eller prisen på et produkt endres.
- Handlekurv, kasse, ordre og Stripe bruker samme utregning (`Services/CartPricingService.cs`), så summene er like overalt. Rabattkoder gjelder varene etter kampanjepris og sjekkes på nytt når bestillingen legges inn.
- Ordrehistorikken viser prisene kunden faktisk betalte.
- Produkter kan skjules (avkrysning i produktskjemaet). Skjulte produkter ser bare ansatte.

## Demo-modus

Uten nøkler er eksterne tjenester erstattet av lokale varianter:

| Tjeneste | Uten nøkler | Slå på ekte |
|---|---|---|
| Stripe (betaling) | «Fullfør bestilling (demo)» oppretter ordren direkte, ingen betaling | `STRIPE_*` i `.env` |
| E-post (SendGrid/SMTP) | E-post skrives til konsollen, kontoer bekreftes automatisk | `AUTHMESSAGESENDEROPTIONS__SENDERGRIDKEY`, `EMAIL__*` |
| Tripletex (produktsynk) | Av | `Tripletex__Enabled=true` + tokens |

Kopier `.env.example` til `.env` og fyll inn verdiene for å bruke de ekte tjenestene.

## Struktur

- `vitalhelse/` – webapplikasjonen (Controllers, Views, Services, Data/Seed, wwwroot)
- `docs/` – databasemodell, sekvensdiagram og planleggingsdokument
