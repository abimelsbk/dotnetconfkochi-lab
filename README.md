# ApiSecurityLab: Breaking ASP.NET Core APIs

A deliberately vulnerable invoice API for learning **Broken Object Level Authorization (BOLA / IDOR)**,
OWASP API Security Top 10 risk **API1:2023**.

Workshop: .NET Conf Kochi, "Breaking ASP.NET Core APIs: A Hands-on Journey from Exploitation to Secure Code".

> Runs only on localhost with fake data. Never deploy this code.
> Only test systems you own or have written permission to test.

## The story

| User  | Owns invoices |
|-------|---------------|
| Alice | 101, 102      |
| Bob   | 202           |

Alice is logged in correctly. But in the vulnerable version she can read and change Bob's invoice
just by changing `101` to `202` in the URL.

## Branches

| Branch           | What it has                                        |
|------------------|----------------------------------------------------|
| `lab-vulnerable` | The bug. Start here.                               |
| `lab-secure`     | The fix (ownership check + resource-based authorization). |

Switch with `git checkout lab-secure` (run `git stash` first if you changed files).

## Run it

You need the **.NET 10 SDK** (or .NET 11), **VS Code** with the **C# Dev Kit** and **REST Client** extensions.

```bash
cd ApiSecurityLab.Api
dotnet user-jwts create --name alice
dotnet user-jwts create --name bob
```

Each command prints a long token. Paste them into:

- `Requests/invoices.http` (the `@aliceToken` and `@bobToken` lines), and
- `wwwroot/demo-tokens.js` (copy it from `demo-tokens.example.js` first).

Then start the API:

```bash
dotnet watch run --launch-profile http
```

- API and web page: http://localhost:5080
- Open `Requests/invoices.http` and click **Send Request** above each request, in order.

## Tasks

- [ ] Read your own invoice as Alice (request 1)
- [ ] Try to read Bob's invoice 202 as Alice (request 3)
- [ ] Try to change Bob's invoice as Alice (request 4), then check it as Bob (request 5)
- [ ] Fix `GetInvoice` in `Controllers/InvoicesController.cs` so it checks the owner
- [ ] Retest: Alice gets 404 for 202, Bob still gets 200 for 202
- [ ] Run `dotnet test` from the root folder

Stuck? `git checkout lab-secure`.

## Tests

```bash
dotnet test
```

- On `lab-vulnerable`: 2 tests fail (Alice can read and update Bob's invoice). The tests catch the bug.
- On `lab-secure`: all 5 pass.

## Project layout

```text
ApiSecurityLab/
├── ApiSecurityLab.slnx
├── ApiSecurityLab.Api/
│   ├── Program.cs                              setup: auth, database, web page
│   ├── Controllers/InvoicesController.cs       THE FILE THAT CHANGES between branches
│   ├── Models/Invoice.cs
│   ├── Models/UpdateInvoiceRequest.cs          no OwnerId on purpose
│   ├── Data/AppDbContext.cs
│   ├── Data/SeedData.cs                        Alice and Bob's fake invoices
│   ├── Authorization/InvoiceAuthorizationHandler.cs
│   ├── Requests/invoices.http                  the attack requests
│   └── wwwroot/index.html                      small demo web page
└── ApiSecurityLab.Tests/
    ├── TestAuthHandler.cs                      fake login for tests only
    └── InvoiceAuthorizationTests.cs            5 tests incl. two-user BOLA tests
```

## Learn more

- OWASP API1:2023 Broken Object Level Authorization: https://api-security.owasp.org/editions/2023/en/0xa1-broken-object-level-authorization/
- Microsoft Learn, Resource-based authorization: https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased
- CISA advisory AA23-208A on IDOR: https://www.cisa.gov/news-events/cybersecurity-advisories/aa23-208a
