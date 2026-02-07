# AI Development Guidelines

This folder contains context and rules for AI agents working on this repository.

## Project Structure

- `CaManager/`: ASP.NET Core Web Application.
- `CaManager.Tests/`: xUnit Test Project.
- `infra/`: Terraform Infrastructure as Code.
- `.github/workflows/`: CI/CD Pipelines.

## Technology Stack

- **Framework**: .NET 8 (ASP.NET Core MVC)
- **Cloud**: Azure (Key Vault, App Service, Managed Identity, Private Link)
- **IaC**: Terraform
- **Testing**: xUnit, Moq

## Development Rules

### 1. Semantic Commit Messages

**CRITICAL**: All commits MUST follow the Semantic (Conventional) Commit message format.

**Format**: `<type>(<scope>): <subject>`

**Types**:
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `style`: Changes that do not affect the meaning of the code (white-space, formatting, etc)
- `refactor`: A code change that neither fixes a bug nor adds a feature
- `perf`: A code change that improves performance
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to the build process or auxiliary tools and libraries such as documentation generation

**Examples**:
- `feat(auth): add entra id authentication support`
- `fix(signing): correct OID mapping for SHA256`
- `infra(kv): enable private endpoint for key vault`
- `test(controller): add tests for certificate creation`

### 2. Code Quality

- Ensure all new features are accompanied by unit tests in `CaManager.Tests`.
- Run `dotnet test` before confirming any changes.
- Do not check in secrets or sensitive configuration.

### 3. Infrastructure

- Avoid "ClickOps" in the Azure Portal; if a change is needed, update the Terraform configuration.

### 4. Versioning

-   **Version Check**: The `GithubVersionCheckService` queries `api.github.com` for the latest release.
-   **Local Development**: The `Result` defaults to `0.0.0-dev` when running locally (set in `CaManager.csproj`). This ensures you can verify the "Update Available" alert logic without needing a matching Git tag.
-   **Release**: The CI/CD pipeline injects the actual version (Git Tag) during build.
