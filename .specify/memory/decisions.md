# CardDemo Modernization - Technical Decisions

## ADR-001: Clean Architecture with CQRS
**Date**: 2025-12-01  
**Status**: Accepted

### Context
Need to structure a complex business application that mirrors COBOL mainframe functionality.

### Decision
Implement Clean Architecture with CQRS pattern using MediatR.

### Consequences
- ✅ Clear separation of concerns
- ✅ Independently testable layers
- ✅ Easy to understand request/response flow
- ⚠️ More boilerplate code
- ⚠️ Learning curve for team

---

## ADR-002: Entity Framework Core
**Date**: 2025-12-01  
**Status**: Accepted

### Context
Need ORM to migrate from VSAM to SQL Server.

### Decision
Use EF Core with Code-First approach.

### Consequences
- ✅ Rapid development with migrations
- ✅ LINQ for type-safe queries
- ✅ Good for complex relationships
- ⚠️ N+1 query risks (mitigated with Include)

---

## ADR-003: JWT Authentication
**Date**: 2025-12-01  
**Status**: Accepted

### Context
Need stateless authentication for API.

### Decision
JWT with access tokens (15min) + refresh tokens (7 days).

### Consequences
- ✅ Stateless, scalable
- ✅ Contains user claims
- ✅ Works well with SPA
- ⚠️ Token revocation complexity

---

## ADR-004: React with TypeScript
**Date**: 2025-12-01  
**Status**: Accepted

### Context
Need modern SPA to replace CICS screens.

### Decision
React 18 + TypeScript + Vite + TailwindCSS.

### Consequences
- ✅ Type safety
- ✅ Fast development with Vite
- ✅ Component reusability
- ⚠️ Bundle size management needed

---

## ADR-005: Batch Jobs as Services
**Date**: 2025-12-02  
**Status**: Accepted

### Context
COBOL batch programs run via JCL. Need modern equivalent.

### Decision
Implement as scoped services triggered via API endpoints.

### Consequences
- ✅ Can be called from UI or scheduled
- ✅ Returns detailed results
- ✅ Maintains COBOL logic
- ⚠️ Long-running jobs need timeout handling

---

## ADR-006: COBOL-Compatible Export Format
**Date**: 2025-12-02  
**Status**: Accepted

### Context
Legacy systems may need data in COBOL format.

### Decision
Support FixedWidth export format matching COBOL record layouts.

### Consequences
- ✅ Backward compatibility
- ✅ Easy migration verification
- ✅ Also support CSV/JSON
- ⚠️ Fixed-width format is rigid

---

## ADR-007: In-Memory Testing Database
**Date**: 2025-12-01  
**Status**: Accepted

### Context
Need fast, isolated tests.

### Decision
Use EF Core InMemory provider for unit tests.

### Consequences
- ✅ Fast test execution
- ✅ No external dependencies
- ✅ Isolated test data
- ⚠️ Some SQL features not available
