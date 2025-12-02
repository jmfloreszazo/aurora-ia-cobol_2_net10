# CardDemo Modernization Constitution

## Core Principles

### I. COBOL Parity First
Every feature implemented must maintain functional parity with the original COBOL/CICS system:
- Preserve all business logic from legacy programs (COSGN00C, COACTVWC, COCRDSLC, COTRN00C, etc.)
- Maintain data integrity and relationships from VSAM files
- Support all original user workflows and screens
- Batch processing must replicate JCL job behavior

### II. Clean Architecture (NON-NEGOTIABLE)
The solution follows Clean Architecture principles:
- **Domain Layer**: Pure business logic, no external dependencies
- **Application Layer**: Use cases, CQRS commands/queries, DTOs
- **Infrastructure Layer**: Data access, external services
- **API Layer**: Controllers, middleware, presentation concerns
- Each layer can only depend on inner layers

### III. Test-Driven Migration
All migration work follows TDD principles:
- Write Gherkin scenarios first (from COBOL behavior)
- Implement tests that verify COBOL parity
- Implement code to pass tests
- Refactor while maintaining green tests
- Minimum 80% code coverage required

### IV. API-First Design
RESTful API is the primary interface:
- OpenAPI/Swagger documentation required
- Consistent response format across all endpoints
- Proper HTTP status codes and error handling
- JWT authentication on all protected endpoints
- Role-based authorization (USER/ADMIN)

### V. Security by Default
Security is embedded in all layers:
- Passwords hashed with bcrypt (salt rounds=12)
- JWT tokens with short expiration (15 min access, 7 day refresh)
- Input validation on all endpoints (FluentValidation)
- SQL injection prevention via parameterized queries
- CORS configured for specific origins
- Audit logging for critical operations

### VI. Observable & Debuggable
All components must be observable:
- Structured logging with Serilog
- Request/response logging middleware
- Performance metrics collection
- Health check endpoints
- Error tracking and alerting

## Technology Stack

### Backend (Mandatory)
- .NET 10 Web API
- Entity Framework Core (Code-First)
- MediatR for CQRS
- FluentValidation for input validation
- AutoMapper for DTO mapping
- xUnit + FluentAssertions + Moq for testing

### Frontend (Mandatory)
- React 18 with TypeScript
- Vite as build tool
- TailwindCSS for styling
- React Query for server state
- React Router for navigation
- Axios for API communication

### Database (Mandatory)
- SQL Server (or SQL Server compatible)
- Normalized schema based on VSAM structure
- Proper indexes for performance
- Foreign key constraints enforced

## Development Workflow

### Branch Strategy
- `main`: Production-ready code only
- `develop`: Integration branch
- `feature/*`: Individual features
- `bugfix/*`: Bug fixes
- All merges via Pull Request with review

### Code Review Requirements
- At least 1 approval required
- All tests must pass
- No decrease in code coverage
- Lint/format checks must pass
- Gherkin scenarios verified

### Quality Gates
- Unit tests: > 80% coverage
- Integration tests: Critical paths covered
- Contract tests: All Gherkin scenarios pass
- Performance: API < 200ms p95
- Security: No critical vulnerabilities

## Governance

This constitution supersedes all other development practices for the CardDemo modernization project.

Amendments require:
1. Documentation of proposed change
2. Impact analysis
3. Team consensus
4. Update to this document
5. Communication to all stakeholders

**Version**: 1.0 | **Ratified**: 2025-12-01 | **Last Amended**: 2025-12-02
