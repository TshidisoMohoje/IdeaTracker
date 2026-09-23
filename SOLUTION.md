# IdeaTracker System Documentation

## App walkthrough

[Watch the Loom video explaining how the app works](https://www.loom.com/share/5407ff9ce20d467686fda85a89ee793c)

## Front end

The **"Ideas Tracker"** application functions as a client-side **React Single Page Application (SPA)** with **TypeScript** for static type-safety, interfacing with a **RESTful .NET backend API** and using browser **LocalStorage** as a local state synchronization cache.

## Core Architectural & Technical Summary

- **State Management & Persistence Architecture:** The application implements a hybrid data persistence pattern.

## Reactive Form Architecture & Client-Side Validation

The form handles input gathering using controlled components or specialized hooks.

- **Data Types & Constraints:** The Description field utilizes a multi-line `HTMLTextAreaElement`. The Status dropdown (`HTMLSelectElement`) initializes to "Proposed" and enforces immutability via the `disabled` HTML attribute, ensuring deterministic payloads.
- **Conditional Form Submission Logic:** Form submissibility is gated by a validation predicate requiring at least one boolean checkbox parameter to be true within the tags array selection list (**Security**, **Identification**, **Technology**, **Efficiency**, **Entertainment**, **Evolution**).

## Data Table with Inline Editing (CRUD & Filtering)

Below the form, a dynamic tabular interface serves as the primary data presentation layer, updating automatically upon new record insertions.

## Backend

### 1. System Architecture

The application is built on top of **ASP.NET Core** using a decoupled, layered approach that promotes clean separation of concerns.

- **Presentation Layer (Controllers):** `IdeasController` exposes RESTful endpoints, handles HTTP routing, acts as the API entry point, and triggers data validation.
- **Business Logic Layer (Services):** `IdeaService` encapsulates all internal application rules, data orchestration, object mapping, and custom domain exception handling.
- **Data Access Layer (Data):** `ApplicationDbContext` manages communication with the underlying database via Entity Framework Core.
- **Cross-Cutting Concerns:**

A pragmatic and cost-effective deployment path on Microsoft Azure for this architecture involves hosting the React SPA via Azure Static Web Apps, deploying the .NET REST API on Azure App Service, and provisioning an Azure SQL Database for the persistence layer.

Here is the recommended setup and migration strategy:

## 🚀 Azure Deployment Path

For a pragmatic and cost-effective production deployment, use the following Azure services and configuration approach:

- **Frontend Hosting:** Deploy the React SPA to [Azure Static Web Apps](https://learn.microsoft.com/en-us/azure/static-web-apps/overview). This provides global content delivery (CDN), free SSL certificates, and seamless GitHub Actions or Azure DevOps integration for continuous deployment out of the box.
- **Backend API Hosting:** Host the .NET Core API on a Linux-based [Azure App Service](https://learn.microsoft.com/en-us/azure/app-service/) (Basic or Premium plan depending on expected traffic). This supports native .NET runtimes, environment-variable configuration for database connection strings, and auto-scaling.
- **Database Layer:** Provision an **Azure SQL Database**, using the Serverless tier to optimize costs during low-use periods. Store connection strings securely in [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/general/overview), or inject them through App Service application settings as encrypted environment variables.

## 🔄 Database Migrations Workflow

For a robust production environment, avoid running Entity Framework (EF) Core migrations automatically on application startup (`context.Database.Migrate()`) because of potential concurrency issues and permission vulnerabilities during scaling. Configure the deployment pipeline to use the following CI/CD-driven migration workflow instead:

1. **Generation:** During the build step in the deployment pipeline (GitHub Actions or Azure Pipelines), run the command `dotnet ef migrations script --output bundle.sql --idempotent`. This generates an idempotent SQL script containing all pending database structural updates.
2. **Execution:** Configure the deployment pipeline to apply this script directly to the target **Azure SQL Database** using an explicit SQL deployment task, such as the Azure SQL Deploy task. Run this step *before* swapping the new API version into the production App Service slot.
3. **Security:** Configure the pipeline to authenticate with a dedicated deployment Service Principal or Managed Identity that has `db_ddladmin` and `db_datawriter` privileges. Keep the runtime API's connection string limited strictly to least-privilege data operations (`db_datareader`/`db_datawriter`).