# .NET 10 Web API — AI Quality Gate Test Repository

A small ASP.NET Core Web API prepared specifically to test your reusable GitHub AI Quality Gate. It includes a clean baseline plus a separate patch set that introduces deliberate review blockers.

## What's included

- `DotNetQualityGate.Api/` — clean .NET 10 Web API baseline using controllers, EF Core SQLite, JWT bearer authentication, and basic input/ownership checks.
- `test-patches/` — intentionally vulnerable replacements for two source files.
- `Apply-TestBlockers.ps1` — copies the vulnerable replacements into the API project so the resulting changes appear in a PR diff.

## Recommended test flow

**Important:** The gate reviews PR changes. Keep the clean baseline on your target branch and apply the blockers only on a feature branch.

1. Create an empty GitHub repository, for example `dotnet-quality-gate-test`.
2. Extract this ZIP and open a terminal in this folder.
3. Initialize Git and push the clean baseline first:

   ```powershell
   git init
   git add README.md DotNetQualityGate.Api
   git commit -m "Add clean .NET 10 Web API baseline"
   git branch -M main
   git remote add origin https://github.com/YOUR-ORG/dotnet-quality-gate-test.git
   git push -u origin main
   ```

4. Create a feature branch and apply the intentional blockers:

   ```powershell
   git switch -c test/intentional-blockers
   .\Apply-TestBlockers.ps1
   git diff
   git add DotNetQualityGate.Api
   git commit -m "Add intentional security and quality blockers for gate testing"
   git push -u origin test/intentional-blockers
   ```

5. Open a pull request from `test/intentional-blockers` into `main`.
6. Add your AI Quality Gate workflow to this repository and configure the same Actions secrets used by your gate:
   - `GEMINI_API_KEY`
   - `REVIEW_STATE_SIGNING_SECRET`
   - `AI_GATE_REPO_READ_TOKEN` only if the central engine repository is private and the default `GITHUB_TOKEN` cannot read it.
7. Ensure the caller workflow's `engine_ref` points to a trusted commit SHA or tag of your central gate repository. Require the **AI Quality Gate** check in the repository ruleset after confirming it runs.

## Expected blocker findings

| File | Deliberate issue | Expected severity |
|---|---|---|
| `Controllers/ProductsController.cs` | SQL injection via `FromSqlRaw` interpolation | Blocker |
| `Controllers/ProductsController.cs` | Missing authorization / IDOR-style resource access | Blocker |
| `Controllers/ProductsController.cs` | Missing request validation | High |
| `Controllers/ProductsController.cs` | Unbounded database result set | High |
| `Program.cs` | Hard-coded fake signing key | Blocker |
| `Program.cs` | JWT issuer/audience validation disabled | Blocker |
| `Program.cs` | CORS allows any origin | High |

The gate's exact labels and verdict depend on your configured review policy and model output; these are the defects the code is intended to expose.

## Optional: build the clean baseline

Requires the .NET 10 SDK.

```powershell
cd DotNetQualityGate.Api
dotnet restore
dotnet build
dotnet run
```

Configure a local development signing key without committing it:

```powershell
dotnet user-secrets init
dotnet user-secrets set "Jwt:Key" "LOCAL-ONLY-REPLACE-WITH-A-32-CHARACTER-OR-LONGER-KEY"
```

The API is a review-test fixture, not a production starter. The patches intentionally weaken security and must never be deployed.
