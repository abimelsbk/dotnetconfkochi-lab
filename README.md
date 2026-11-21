# ApiSecurityLab: Breaking ASP.NET Core APIs

**.NET Conf 2026 - Kochi** · Hands-on workshop by Abimel S B Kulumala

In this lab you will attack a small invoice API, find a real security bug, fix it in C#,
and prove the fix with automated tests.

The bug is **Broken Object Level Authorization (BOLA)**, also called **IDOR**.
It is number 1 in the OWASP API Security Top 10 (API1:2023).

> **Rules**
> - Everything runs on **your own laptop** with **fake data**. Never deploy this code.
> - Only test systems you own or have written permission to test. Trying this on real
>   websites or apps is illegal under the IT Act.
> - Found a real bug somewhere? Report it to that company's security team or bug bounty program.

---

## Contents

- [The story](#the-story)
- [Part A: Install the tools (do this at home)](#part-a-install-the-tools-do-this-at-home)
- [Part B: Get the code and build it](#part-b-get-the-code-and-build-it)
- [Part C: Create two test users](#part-c-create-two-test-users)
- [Part D: Start the API](#part-d-start-the-api)
- [The missions](#the-missions)
- [Stuck? See the answer](#stuck-see-the-answer)
- [Troubleshooting](#troubleshooting)
- [What is inside this project](#what-is-inside-this-project)

---

## The story

| User  | Owns invoices          |
|-------|------------------------|
| Alice | 101, 102               |
| Bob   | 202 (Rs 1,80,000)      |

Alice is logged in correctly with her own token.
Your job: as Alice, read and change **Bob's** invoice. Then stop it from happening.

---

## Part A: Install the tools (do this at home)

Downloads are large. Please finish Part A and Part B **before** the workshop.
Venue Wi-Fi may be slow.

### A1. Install the .NET 10 SDK

You need the **SDK** (developer kit), not only the "Runtime".

**Windows**

Option 1, one command in PowerShell:
```powershell
winget install Microsoft.DotNet.SDK.10
```
Option 2, download: go to https://dotnet.microsoft.com/download/dotnet/10.0 and under **SDK**
choose **Windows x64 Installer** (or **Arm64** for ARM laptops). Run it.

**macOS**

Go to https://dotnet.microsoft.com/download/dotnet/10.0 and under **SDK** choose
**macOS Arm64** (Apple M1/M2/M3/M4) or **macOS x64** (Intel). Run the `.pkg` installer.

**Linux**

Follow the steps for your distribution: https://learn.microsoft.com/dotnet/core/install/linux

**Check it worked.** Close all terminals, open a new one, and run:
```bash
dotnet --version
```
You should see a number starting with `10.` (for example `10.0.100`). `11.` also works.

> If you see "No .NET SDKs were found", the SDK is not installed yet, or the terminal
> was open during installation. Install the SDK and open a **new** terminal.

### A2. Install Visual Studio Code

Download from https://code.visualstudio.com and install it.

Then install two extensions. In VS Code press `Ctrl+Shift+X` (`Cmd+Shift+X` on Mac) and install:

| Extension | Publisher | Why |
|-----------|-----------|-----|
| **C# Dev Kit** | Microsoft | C# support |
| **REST Client** | Huachao Mao | Send the attack requests from `.http` files |

Or run these two commands in a terminal:
```bash
code --install-extension ms-dotnettools.csdevkit
code --install-extension humao.rest-client
```

> Using **Visual Studio 2026** or **JetBrains Rider** instead? That is fine. Open
> `ApiSecurityLab.slnx`. Both can run `.http` files without any extension.

### A3. Install Git

- **Windows:** `winget install Git.Git`, or download from https://git-scm.com/download/win
- **macOS:** run `xcode-select --install` in Terminal, or download from https://git-scm.com/download/mac
- **Linux:** `sudo apt install git` (Ubuntu/Debian) or your distribution's package manager

Check it worked:
```bash
git --version
```

---

## Part B: Get the code and build it

### B1. Get the code

**Option 1, from GitHub:**
```bash
git clone https://github.com/abimelsbk/dotnetconfkochi-lab.git
cd dotnetconfkochi-lab
```
No Git? On the GitHub page click **Code > Download ZIP**, unzip it, and open a terminal inside the folder.
(A ZIP has no branches, so the "Stuck?" section needs the `git clone` way.)

**Option 2, from a pen drive:** copy the lab folder to your laptop,
then open a terminal inside it.

In this guide, "the main lab folder" means the folder that has `ApiSecurityLab.slnx` in it.

> **Windows tip:** if the folder path has spaces, put it in quotes:
> `cd "C:\My Projects\ApiSecurityLab"`

### B2. Open it in VS Code

```bash
code .
```
Open the VS Code terminal with **Ctrl + `** (the key below Esc).

### B3. Check you are on the starting branch

```bash
git branch
```
You should see `* lab-vulnerable`. If not:
```bash
git checkout lab-vulnerable
```

### B4. Download packages and build

From the main lab folder (the one with `ApiSecurityLab.slnx`):
```bash
dotnet restore
dotnet build
```
You should see **Restore complete** and then **Build succeeded**.
The first restore needs internet and can take a minute.

---

## Part C: Create two test users

The lab uses real JWT login tokens, made by a .NET tool for development.

### C1. Create the tokens

```bash
cd ApiSecurityLab.Api
dotnet user-jwts create --name alice
dotnet user-jwts create --name bob
```
Each command prints a long token that starts with `eyJ`. Copy both somewhere for a moment.

This tool also stores a secret signing key on your laptop (outside the project folder)
and updates `appsettings.Development.json`. That is expected.

### C2. Paste the tokens into the request file

Open `ApiSecurityLab.Api/Requests/invoices.http` and replace the placeholders:
```http
@aliceToken = PASTE_ALICE_TOKEN_HERE
@bobToken = PASTE_BOB_TOKEN_HERE
```
Paste each whole token, on one line, with no spaces or quotes.

### C3. (Optional) Tokens for the web page

The API also has a small web page. To use it, copy the example file and paste the same tokens into it.

Windows (PowerShell):
```powershell
copy wwwroot\demo-tokens.example.js wwwroot\demo-tokens.js
```
macOS / Linux:
```bash
cp wwwroot/demo-tokens.example.js wwwroot/demo-tokens.js
```
Then open `wwwroot/demo-tokens.js` and replace `PASTE_ALICE_TOKEN_HERE` and `PASTE_BOB_TOKEN_HERE`.

---

## Part D: Start the API

Still inside the `ApiSecurityLab.Api` folder:
```bash
dotnet watch run --launch-profile http
```
Wait for:
```text
Now listening on: http://localhost:5080
```
**Leave this terminal running.** Open a second terminal (the **+** button in the terminal panel) when you need one.

- `watch` means: when you save a code file, the running app updates by itself.
- `--launch-profile http` means: use port 5080 and Development settings. Without it, your tokens will not work.

**Web page:** open http://localhost:5080 in your browser. Switch between Alice and Bob at the top right.

**To stop the API:** click in its terminal and press `Ctrl+C`.

---

## The missions

### Mission 1: Be Alice, and get Bob's invoice

Open `ApiSecurityLab.Api/Requests/invoices.http`.
Above each request you will see a small grey link: **Send Request**. Click it.
The response opens on the right. (Shortcut: click inside a request and press `Ctrl+Alt+R`.)

Send the requests **in order**:

| # | Request | What you should see |
|---|---------|---------------------|
| 1 | Alice reads invoice 101 | `200 OK`, Alice's invoice |
| 2 | Alice lists her invoices | Only 101 and 102 |
| 3 | **Alice reads invoice 202** | **`200 OK` with Bob's invoice. This is the bug.** |
| 4 | **Alice changes invoice 202** | **`200 OK`. Alice just edited Bob's invoice.** |
| 5 | Bob reads invoice 202 | The amount is now `1` instead of `180000` |
| 6 | No token at all | `401 Unauthorized` |

Think about it:
- Did Alice steal a password? Did she break the login?
- What did she change in request 3 compared to request 1?

> Restarting the API resets all data, so Bob's amount goes back to 180000.

### Mission 2: Find the bug

Open `ApiSecurityLab.Api/Controllers/InvoicesController.cs`.

Look at the `GetInvoice` method. Find the line that loads the invoice from the database.
Ask yourself: **does this line check who owns the invoice?**

<details>
<summary>Hint</summary>

Look at `db.Invoices.FindAsync(id)`. It asks for "invoice number `id`" and nothing else.
`id` comes from the URL, and the attacker controls the URL.
The method never uses `User`, which tells it who is calling.

</details>

Also look at the class: it has `[Authorize]`. Why didn't that stop Alice?

<details>
<summary>Answer</summary>

`[Authorize]` only checks that the caller is logged in. Alice is logged in.
It runs before the invoice is loaded, so it cannot know who owns invoice 202.

</details>

### Mission 3: Fix `GetInvoice`

Change the method so it only returns an invoice that belongs to the caller.

<details>
<summary>Hint 1</summary>

Get the caller's ID from the token:
`User.FindFirstValue(ClaimTypes.NameIdentifier)` gives `"alice"` for Alice's token.

</details>

<details>
<summary>Hint 2</summary>

Ask the database for the invoice with this ID **and** this owner, in one query.

</details>

<details>
<summary>Answer</summary>

Replace:
```csharp
var invoice = await db.Invoices.FindAsync(id);
```
with:
```csharp
var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
var invoice = await db.Invoices
    .SingleOrDefaultAsync(i => i.Id == id && i.OwnerId == userId);
```
The next line, `if (invoice is null) return NotFound();`, now returns 404 for
"does not exist" **and** for "not yours". The attacker cannot tell which.

</details>

Save with `Ctrl+S`. Watch the API terminal. If `dotnet watch` asks whether to restart, press `a` (Always).

### Mission 4: Retest

Send the requests again:

| # | Request | Before your fix | After your fix |
|---|---------|-----------------|----------------|
| 1 | Alice reads 101 | 200 | **200** (Alice still works) |
| 3 | Alice reads 202 | 200, Bob's data | **404** (blocked) |
| 4 | Alice changes 202 | 200 | **Still 200!** |

Request 4 still works. Why?

<details>
<summary>Answer</summary>

You fixed only `GetInvoice`. `UpdateInvoice` has the same bug.
Every endpoint that takes an ID needs its own check. Fixing one endpoint is not fixing the app.

</details>

### Mission 5: Fix `UpdateInvoice`

Add an owner check to `UpdateInvoice`, **after** the invoice is loaded and **before** anything is changed.

You can reuse the idea from Mission 3, or use the authorization handler that is already in
`Authorization/InvoiceAuthorizationHandler.cs` (registered in `Program.cs` as the `"InvoiceOwner"` policy).

<details>
<summary>Answer (using the handler)</summary>

1. Change the class line so the controller also receives the authorization service:
```csharp
public class InvoicesController(AppDbContext db, IAuthorizationService authz) : ControllerBase
```
2. In `UpdateInvoice`, add these two lines right after `if (invoice is null) return NotFound();`:
```csharp
var result = await authz.AuthorizeAsync(User, invoice, "InvoiceOwner");
if (!result.Succeeded) return NotFound();
```

</details>

Save, then send request 4 again. It should now return **404**, and request 5 shows Bob's amount unchanged.
(If Bob's amount is still 1 from before, restart the API to reset the data.)

### Mission 6: Prove it with tests

Open a **second terminal**, go to the main lab folder, and run:
```bash
dotnet test
```

| When | Result |
|------|--------|
| Before any fix | **2 failed**, 3 passed. The tests catch the bug. |
| After Missions 3 and 5 | **5 passed** |

Open `ApiSecurityLab.Tests/InvoiceAuthorizationTests.cs` and read `Alice_cannot_read_bobs_invoice`.
It tests with **two users**. Most real projects only test with one, and that is why this bug survives.

### Bonus missions (finished early?)

1. Add a `DELETE /api/invoices/{id}` endpoint. Protect it, and write a test for it.
2. Change the fix to return **403** instead of 404. Update the tests. Which is better, and why?
3. Add `OwnerId` to `Models/UpdateInvoiceRequest.cs` and copy it into the invoice in `UpdateInvoice`.
   What can Alice do now? (This is OWASP API3:2023, mass assignment.)
4. Paste Alice's token into https://jwt.io and look at the `sub` claim. Change it to `bob` and use
   the edited token. What happens, and why?

---

## Stuck? See the answer

The finished, fixed version is on the `lab-secure` branch.

1. Stop the API (`Ctrl+C`).
2. Throw away your own changes to the controller:
   ```bash
   git checkout -- ApiSecurityLab.Api/Controllers/InvoicesController.cs
   ```
   Do **not** use `git stash`. It would also hide the tokens you pasted.
3. Switch branch:
   ```bash
   git checkout lab-secure
   ```
4. Start the API again (Part D) and send all requests. Requests 3 and 4 now return 404.

To go back to the starting version: `git checkout lab-vulnerable`.

Compare the two versions:
```bash
git diff lab-vulnerable lab-secure
```

---

## Troubleshooting

| Problem | Fix |
|---------|-----|
| `No .NET SDKs were found` | Install the .NET 10 **SDK** (Part A1), then open a **new** terminal. |
| `error NU1100: Unable to resolve ...` | Check your internet. This project includes `nuget.config` for nuget.org. On office or college networks, a firewall or proxy may block nuget.org: try a phone hotspot. |
| Every request returns `401` | Token pasted wrong (copy the whole `eyJ...` text, one line), or the API was not started with `--launch-profile http`. Make new tokens inside `ApiSecurityLab.Api` and paste again. |
| No **Send Request** links in the `.http` file | Install the **REST Client** extension. Check the bottom right of VS Code says **HTTP**, not Plain Text. |
| "Connection refused" or the request keeps loading | The API is not running. Start it (Part D) and wait for `Now listening on`. |
| `address already in use` / port 5080 busy | Another copy of the API is running. Stop it with `Ctrl+C`, or close old terminals. |
| `git` says "dubious ownership" | Run the command it suggests, for example: `git config --global --add safe.directory "*"` |
| `git checkout` refuses to switch branch | You changed a file. Run `git checkout -- ApiSecurityLab.Api/Controllers/InvoicesController.cs` first. |
| `git status` shows every file as modified, with no real changes | Windows line endings. Run `git config core.autocrlf true` once. |
| The web page says "No tokens found" | Do Part C3. |
| My fix does nothing | Make sure you saved (`Ctrl+S`) and changed the right method. Restart the API if `dotnet watch` did not reload. |

---

## What is inside this project

```text
dotnetconfkochi-lab/
├── ApiSecurityLab.slnx                         the solution (opens both projects)
├── nuget.config                                package source (nuget.org)
├── ApiSecurityLab.Api/                         the API
│   ├── Program.cs                              startup: login, authorization rules, database, web page
│   ├── Controllers/InvoicesController.cs       THE FILE YOU FIX
│   ├── Models/Invoice.cs                       what an invoice looks like
│   ├── Models/UpdateInvoiceRequest.cs          what a user may change (no OwnerId, on purpose)
│   ├── Data/AppDbContext.cs                    the in-memory database
│   ├── Data/SeedData.cs                        Alice's and Bob's fake invoices
│   ├── Authorization/InvoiceAuthorizationHandler.cs   the "you must own this invoice" rule
│   ├── Requests/invoices.http                  the attack requests
│   ├── Properties/launchSettings.json          port 5080 and Development mode
│   └── wwwroot/                                the small web page
└── ApiSecurityLab.Tests/                       automated tests
    ├── InvoiceAuthorizationTests.cs            5 tests, including two-user BOLA tests
    └── TestAuthHandler.cs                      fake login used only by the tests
```

| Branch | What it has |
|--------|-------------|
| `lab-vulnerable` | The bug. Start here. |
| `lab-secure` | The fixed version. |

---

## Six rules to take home

1. Logging in is not permission. `[Authorize]` only checks login.
2. Take the user's identity from the token, never from the URL or the request body.
3. Check ownership on every read, update and delete.
4. Return 404 for objects that are not yours, so you do not confirm they exist.
5. Random IDs (GUIDs) are not a fix. They hide IDs, they do not check access.
6. Test every API with two users: log in as one, try to touch the other's data.

---

## Learn more

- OWASP API1:2023 Broken Object Level Authorization:
  https://api-security.owasp.org/editions/2023/en/0xa1-broken-object-level-authorization/
- Microsoft Learn, Resource-based authorization in ASP.NET Core:
  https://learn.microsoft.com/aspnet/core/security/authorization/resourcebased
- Microsoft Learn, Manage JWTs in development with dotnet user-jwts:
  https://learn.microsoft.com/aspnet/core/security/authentication/jwt-authn
- ACSC, CISA and NSA advisory AA23-208A on IDOR:
  https://www.cisa.gov/news-events/cybersecurity-advisories/aa23-208a

Questions after the workshop? Find me on Instagram: **@abimelsbk**
