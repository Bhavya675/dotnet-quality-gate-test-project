# Intentionally vulnerable PR patch set

These files are designed to be copied over the matching files in `DotNetQualityGate.Api/` on a feature branch. They are not production code.

Expected findings:
- `Controllers/ProductsController.cs`: SQL injection, missing authentication/authorization and resource ownership checks, missing input validation, unbounded query.
- `Program.cs`: hard-coded credential, JWT validation weakened, permissive CORS.

After copying the files, inspect the diff and open a pull request. The gate only reviews files changed in the PR, so do not commit these patches to the target branch first.
