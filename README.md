# CardDemo - Modernización COBOL a .NET 10 + React

## 📋 Resumen Ejecutivo

Este proyecto es la **modernización completa** de la aplicación mainframe **CardDemo** (gestión de tarjetas de crédito en COBOL/CICS/VSAM) hacia una arquitectura moderna utilizando:

- **Backend**: .NET 10 Web API con Clean Architecture + CQRS
- **Frontend**: React 18 + TypeScript + Vite
- **Base de Datos**: SQL Server con Entity Framework Core
- **Metodología**: **AURORA-IA™** (AI-Unified Requirements, Orchestration, Reasoning & Automation)

---

## 🎯 Estado del Proyecto

### ✅ Fase 1: Especificación (AURORA-IA) - **COMPLETADO**

Siguiendo la metodología AURORA-IA, se han creado los siguientes artefactos en `.specify/`:

| Artefacto | Descripción | Estado |
|-----------|-------------|--------|
| **intent.md** | Intención del proyecto, objetivos de negocio, stakeholders | ✅ Completo |
| **spec.md** | Especificación viva: casos de uso, entidades, reglas de negocio | ✅ Completo |
| **plan.md** | Plan técnico: arquitectura, ADRs, tecnologías, CI/CD | ✅ Completo |
| **features/*.feature** | Contratos Gherkin (autenticación, cuentas, tarjetas, transacciones, admin) | ✅ Completo |

#### Contratos de Comportamiento (Gherkin)
- ✅ `authentication.feature` - 11 escenarios de autenticación JWT
- ✅ `accounts.feature` - 15 escenarios de gestión de cuentas
- ✅ `cards.feature` - 18 escenarios de gestión de tarjetas
- ✅ `transactions.feature` - 25 escenarios de transacciones
- ✅ `admin.feature` - 18 escenarios de administración de usuarios

**Total**: 87 escenarios de comportamiento documentados y verificables

---

### 🚧 Fase 2: Generación de Backend (.NET 10) - **EN PROGRESO**

#### Estructura del Proyecto (Clean Architecture)

```
CardDemo.sln
│
├── src/
│   ├── CardDemo.Api/                    # 🌐 API Layer (Controllers, Middleware)
│   │   ├── Controllers/                 # REST endpoints
│   │   ├── Filters/                     # Exception filters, validators
│   │   ├── Middleware/                  # JWT, logging, CORS
│   │   └── Program.cs                   # App configuration
│   │
│   ├── CardDemo.Application/            # 📱 Application Layer (CQRS, DTOs)
│   │   ├── Commands/                    # Write operations
│   │   │   ├── Accounts/
│   │   │   ├── Cards/
│   │   │   ├── Transactions/
│   │   │   └── Users/
│   │   ├── Queries/                     # Read operations
│   │   │   ├── Accounts/
│   │   │   ├── Cards/
│   │   │   ├── Transactions/
│   │   │   └── Users/
│   │   ├── DTOs/                        # Data Transfer Objects
│   │   ├── Mappings/                    # AutoMapper profiles
│   │   ├── Validators/                  # FluentValidation rules
│   │   └── Interfaces/                  # Service contracts
│   │
│   ├── CardDemo.Domain/                 # 🏛️ Domain Layer (Entities, Logic)
│   │   ├── Entities/
│   │   │   ├── User.cs
│   │   │   ├── Customer.cs
│   │   │   ├── Account.cs
│   │   │   ├── Card.cs
│   │   │   ├── Transaction.cs
│   │   │   ├── TransactionType.cs
│   │   │   └── TransactionCategory.cs
│   │   ├── ValueObjects/                # Card numbers, money
│   │   ├── Enums/
│   │   │   ├── UserRole.cs
│   │   │   ├── AccountStatus.cs
│   │   │   └── CardType.cs
│   │   ├── Exceptions/                  # Domain exceptions
│   │   └── Common/
│   │       └── BaseEntity.cs            # Audit fields
│   │
│   └── CardDemo.Infrastructure/         # 🔧 Infrastructure (Data, Services)
│       ├── Data/
│       │   ├── CardDemoDbContext.cs     # EF Core DbContext
│       │   ├── Configurations/          # Entity configurations
│       │   └── Migrations/              # EF migrations
│       ├── Repositories/                # Repository implementations
│       │   ├── UserRepository.cs
│       │   ├── AccountRepository.cs
│       │   ├── CardRepository.cs
│       │   └── TransactionRepository.cs
│       ├── Services/
│       │   ├── AuthService.cs           # JWT generation
│       │   ├── PasswordHasher.cs        # BCrypt hashing
│       │   └── AuditService.cs          # Audit logging
│       └── Extensions/
│           └── ServiceCollectionExtensions.cs
│
├── tests/
│   ├── CardDemo.Tests/                  # 🧪 Unit & Integration Tests
│   │   ├── Unit/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── Validators/
│   │   ├── Integration/
│   │   │   ├── Controllers/
│   │   │   └── Repositories/
│   │   └── Contracts/                   # SpecFlow (Gherkin runners)
│   │       ├── AuthenticationSteps.cs
│   │       ├── AccountsSteps.cs
│   │       └── TransactionsSteps.cs
│   │
│   └── CardDemo.Contracts.Tests/        # 📝 Contract Tests (SpecFlow)
│       └── Features/                    # Gherkin .feature files
│
└── .specify/                            # 📐 AURORA-IA Specifications
    ├── intent.md
    ├── spec.md
    ├── plan.md
    ├── features/                        # Gherkin specifications
    └── tasks/                           # Task breakdown
```

#### Paquetes NuGet Instalados

| Proyecto | Paquetes |
|----------|----------|
| **CardDemo.Api** | EF Core Design, JWT Bearer, Swashbuckle, Serilog, FluentValidation.AspNetCore |
| **CardDemo.Application** | MediatR, AutoMapper, FluentValidation |
| **CardDemo.Infrastructure** | EF Core SQL Server, BCrypt.Net |
| **CardDemo.Tests** | xUnit, Moq, FluentAssertions, SpecFlow, WebApplicationFactory |

---

### 📅 Fase 3: Frontend React - **PENDIENTE**

Estructura planificada:

```
client/
├── public/
├── src/
│   ├── api/                    # Axios clients
│   ├── components/
│   │   ├── common/             # Reusable components
│   │   ├── layout/             # Header, Sidebar, Footer
│   │   └── features/           # Feature-specific
│   ├── pages/
│   │   ├── Auth/
│   │   ├── Accounts/
│   │   ├── Cards/
│   │   ├── Transactions/
│   │   └── Admin/
│   ├── hooks/                  # Custom hooks (useAuth, useAccounts)
│   ├── context/                # React Context (Auth, Theme)
│   ├── utils/                  # Formatters, validators
│   ├── App.tsx
│   └── main.tsx
├── package.json
├── vite.config.ts
└── tsconfig.json
```

**Stack Tecnológico**:
- React 18 + TypeScript
- Vite (build tool)
- React Router v6
- Material-UI v5
- Axios + SWR
- React Hook Form + Yup

---

## 🏛️ Arquitectura Técnica

### Capas de Clean Architecture

```
┌─────────────────────────────────────────────────────────┐
│                      API Layer                          │
│  Controllers → Middleware → Filters → Program.cs        │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                 Application Layer                       │
│  Commands (CQRS) ← MediatR → Queries                    │
│  DTOs ← AutoMapper → Validators (FluentValidation)      │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│                   Domain Layer                          │
│  Entities → Value Objects → Enums → Business Logic      │
└────────────────────────┬────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────┐
│               Infrastructure Layer                      │
│  DbContext → Repositories → External Services           │
└────────────────────────┬────────────────────────────────┘
                         │
                   ┌─────▼──────┐
                   │ SQL Server │
                   └────────────┘
```

### Patrón CQRS con MediatR

**Commands (Write)**:
```csharp
CreateAccountCommand → CreateAccountCommandHandler → Account Entity → Repository.Add()
```

**Queries (Read)**:
```csharp
GetAccountByIdQuery → GetAccountByIdQueryHandler → Repository.GetById() → AccountDto
```

---

## 🔒 Seguridad

### Autenticación JWT
- **Access Token**: 15 minutos de vida
- **Refresh Token**: 7 días de vida
- **Claims**: UserId, Role, Name
- **Almacenamiento**: HttpOnly cookies (frontend)

### Contraseñas
- **Algoritmo**: BCrypt con 12 salt rounds
- **Validación**: Mínimo 8 caracteres, mayúsculas, minúsculas, números

### Autorización
```csharp
[Authorize(Roles = "ADMIN")]          // Solo administradores
[Authorize(Roles = "USER,ADMIN")]     // Usuarios o admins
[AllowAnonymous]                      // Sin autenticación
```

---

## 🗄️ Modelo de Datos

### Entidades Principales

```sql
Users (UserId PK, PasswordHash, FirstName, LastName, UserType, CreatedAt, UpdatedAt)
Customers (CustomerId PK, FirstName, LastName, SSN UK, DateOfBirth, FICOScore, ...)
Accounts (AccountId PK, CustomerId FK, CurrentBalance, CreditLimit, ActiveStatus, ...)
Cards (CardNumber PK, AccountId FK, CardType, ExpirationDate, ActiveStatus, ...)
Transactions (TransactionId PK, AccountId FK, CardNumber FK, Amount, Date, ...)
TransactionTypes (TypeCode PK, Description)
TransactionCategories (CategoryCode PK, Description)
```

### Relaciones
- Customer 1:N Accounts
- Account 1:N Cards
- Account 1:N Transactions
- Card 1:N Transactions

---

## 🧪 Testing

### Estrategia de Pirámide de Tests

```
          ╱──────────────╲
         ╱   E2E (5%)     ╲      Playwright
        ╱──────────────────╲
       ╱  Integration (15%) ╲    WebApplicationFactory
      ╱──────────────────────╲
     ╱    Unit Tests (80%)    ╲  xUnit + Moq
    ╱──────────────────────────╲
```

### Contract Tests (SpecFlow + Gherkin)
- 87 escenarios de comportamiento documentados
- Ejecutados en CI/CD como gate de calidad
- Cobertura objetivo: > 95%

### Comando para ejecutar tests:
```powershell
dotnet test
dotnet test --logger:"console;verbosity=detailed"
dotnet test /p:CollectCoverage=true /p:CoverageReportFormat=cobertura
```

---

## 🚀 Deployment

### Development
```powershell
# Backend
cd src/CardDemo.Api
dotnet run

# Frontend (pendiente)
cd client
npm run dev
```

**URLs**:
- API: https://localhost:5001
- Swagger: https://localhost:5001/swagger
- React: http://localhost:5173

### Production (Azure)
- **API**: Azure App Service (Linux)
- **Frontend**: Azure Static Web Apps
- **Database**: Azure SQL Database
- **Monitoring**: Application Insights
- **CI/CD**: Azure DevOps Pipelines

---

## 📊 Métricas AURORA-IA

| Métrica | Objetivo | Estado Actual |
|---------|----------|---------------|
| Escenarios Gherkin definidos | > 80 | ✅ 87 |
| Cobertura de funcionalidad | 100% | 🚧 60% (en progreso) |
| Tests Gherkin pasando | > 95% | ⏳ Pendiente |
| Drift Spec ↔ Code | < 2% | ⏳ Pendiente |
| Tiempo ciclo intención → PR | -50% | ⏳ Pendiente |

---

## 🛠️ Comandos Útiles

### .NET
```powershell
# Restaurar dependencias
dotnet restore

# Compilar solución
dotnet build

# Ejecutar API
dotnet run --project src/CardDemo.Api

# Ejecutar tests
dotnet test

# Crear migración EF Core
dotnet ef migrations add InitialCreate --project src/CardDemo.Infrastructure --startup-project src/CardDemo.Api

# Aplicar migraciones
dotnet ef database update --project src/CardDemo.Infrastructure --startup-project src/CardDemo.Api
```

### Git
```powershell
git status
git add .
git commit -m "feat: implement account management module"
git push origin main
```

---

## 📚 Documentación

### AURORA-IA Artifacts
- **Intent**: `.specify/intent.md` - Visión y objetivos del proyecto
- **Spec**: `.specify/spec.md` - Casos de uso, entidades, reglas de negocio
- **Plan**: `.specify/plan.md` - Arquitectura, ADRs, tecnologías
- **Contracts**: `.specify/features/*.feature` - Contratos Gherkin

### API Documentation
- **Swagger/OpenAPI**: https://localhost:5001/swagger (en ejecución)
- **Postman Collection**: Disponible en `/docs/postman/`

### Architecture Decision Records (ADRs)
- ADR-001: Clean Architecture con CQRS
- ADR-002: Entity Framework Core como ORM
- ADR-003: JWT para Autenticación
- ADR-004: React con TypeScript
- ADR-005: SQL Server como Base de Datos

---

## 👥 Equipo

- **Arquitecto**: Define intención, coherencia y decisiones
- **Developers**: Implementan y refinan código
- **QA**: Revisa escenarios Gherkin
- **Product Owner**: Valida funcionalidad

---

## 📝 Próximos Pasos

### Sprint Actual (Week 1-2)
- [x] Estructura AURORA-IA (.specify/)
- [x] Definir Intención (intent.md)
- [x] Crear Especificación Viva (spec.md)
- [x] Diseñar Plan Técnico (plan.md)
- [x] Crear Contratos Gherkin (87 escenarios)
- [x] Crear estructura Clean Architecture
- [x] Instalar paquetes NuGet
- [ ] Implementar Domain Entities
- [ ] Implementar DbContext + Migrations
- [ ] Implementar Authentication (JWT)
- [ ] Implementar CRUD Accounts

### Sprint Siguiente (Week 3-4)
- [ ] Implementar CRUD Cards
- [ ] Implementar CRUD Transactions
- [ ] Implementar Admin Users
- [ ] Crear frontend React
- [ ] Integrar frontend con API
- [ ] Tests contractuales (SpecFlow)

---

## 🏆 Conclusión

Este proyecto representa una **modernización completa y metodológica** de una aplicación mainframe legacy utilizando:

✅ **Metodología AURORA-IA** para trazabilidad intención → código  
✅ **Clean Architecture** para mantenibilidad y testabilidad  
✅ **CQRS** para separación de responsabilidades  
✅ **Contratos Gherkin** (87 escenarios) para verificación de comportamiento  
✅ **Stack moderno**: .NET 10 + React + SQL Server  
✅ **CI/CD** con Azure DevOps  

**Objetivo**: Reducir costos de mainframe, mejorar mantenibilidad y habilitar integración cloud/móvil.

---

**Versión**: 1.0  
**Última Actualización**: 2025-12-01  
**Método**: AURORA-IA™  
**Licencia**: Apache 2.0
