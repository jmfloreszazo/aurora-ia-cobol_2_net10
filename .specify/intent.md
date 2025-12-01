# Intención del Proyecto: Modernización CardDemo

## 🎯 Propósito de Negocio

Modernizar la aplicación mainframe **CardDemo** (sistema de gestión de tarjetas de crédito en COBOL/CICS/VSAM) hacia una arquitectura moderna, escalable y mantenible utilizando:

- **Backend**: .NET 10 Web API con arquitectura limpia
- **Frontend**: React con TypeScript
- **Base de datos**: SQL Server con Entity Framework Core

## 📋 Objetivos Principales

### Funcionales
1. **Preservar toda la funcionalidad de negocio** del sistema legacy
2. **Mejorar la experiencia de usuario** con interfaz web moderna
3. **Mantener integridad de datos** en la migración

### Técnicos
1. Eliminar dependencias de mainframe (CICS, VSAM, JCL)
2. Implementar API RESTful con estándares modernos
3. Separar frontend/backend para escalabilidad independiente
4. Introducir tests automatizados basados en contratos

### Estratégicos
1. Reducir costos de infraestructura mainframe
2. Facilitar mantenimiento con tecnologías modernas
3. Habilitar integración con sistemas cloud y móviles
4. Atraer talento con stack tecnológico actualizado

## 👥 Stakeholders

- **Negocio**: Requiere continuidad operativa sin downtime
- **Usuarios Finales**: Esperan mejor UX manteniendo funcionalidad
- **IT/DevOps**: Buscan reducir complejidad y costos operativos
- **Desarrollo**: Necesitan tecnologías modernas y productivas

## 🎭 Contexto del Sistema Legacy

### Sistema Actual: CardDemo COBOL
- **Plataforma**: IBM Mainframe (CICS/COBOL)
- **Almacenamiento**: VSAM (KSDS con AIX)
- **Procesamiento Batch**: JCL
- **Usuarios**: 2 roles (Regular User, Admin)

### Módulos Principales
1. **Autenticación** (COSGN00C) - Login/Logout
2. **Gestión de Cuentas** (CAVW, CAUP) - Ver/Actualizar cuentas
3. **Gestión de Tarjetas** (CCLI, CCDL, CCUP) - Listar/Ver/Actualizar tarjetas
4. **Transacciones** (CT00, CT01, CT02) - Listar/Ver/Agregar transacciones
5. **Reportes** (CR00) - Generación de reportes
6. **Pagos** (CB00) - Procesamiento de pagos
7. **Administración** (CU00-03) - Gestión de usuarios

### Entidades Principales
- **Customer** (Cliente) - Datos personales
- **Account** (Cuenta) - Cuentas bancarias
- **Card** (Tarjeta) - Tarjetas de crédito
- **Transaction** (Transacción) - Movimientos
- **User** (Usuario) - Credenciales y permisos

## 🚀 Visión de la Solución Moderna

### Arquitectura Objetivo
```
┌─────────────────────────────────────────┐
│    Frontend React (SPA)                 │
│    - TypeScript + Vite                  │
│    - React Router + Axios               │
│    - Material-UI / TailwindCSS          │
└─────────────────┬───────────────────────┘
                  │ HTTPS/REST
┌─────────────────▼───────────────────────┐
│    Backend .NET 10 Web API              │
│    - Clean Architecture                 │
│    - CQRS + MediatR                     │
│    - JWT Authentication                 │
│    - AutoMapper + FluentValidation      │
└─────────────────┬───────────────────────┘
                  │ EF Core
┌─────────────────▼───────────────────────┐
│    SQL Server Database                  │
│    - Normalized schema                  │
│    - Indexes + Constraints              │
└─────────────────────────────────────────┘
```

### Principios de Diseño
1. **Separation of Concerns**: Capas independientes y testables
2. **Domain-Driven Design**: Modelos ricos en lógica de negocio
3. **API-First**: Contratos claros y versionados
4. **Security by Default**: Autenticación/Autorización desde día 1
5. **Testing as Documentation**: Gherkin como especificación ejecutable

## 📊 Métricas de Éxito

| Métrica | Objetivo |
|---------|----------|
| Cobertura de funcionalidad | 100% paridad con legacy |
| Tests Gherkin pasando | > 95% |
| Tiempo de respuesta API | < 200ms (p95) |
| Disponibilidad | > 99.5% |
| Drift Spec ↔ Code | < 2% |

## 🔄 Metodología: AURORA-IA

Este proyecto sigue **AURORA-IA** (AI-Unified Requirements, Orchestration, Reasoning & Automation):

1. **Intención** (este documento) → Define el QUÉ y POR QUÉ
2. **Especificación Viva** (spec.md) → Casos de uso y reglas de negocio
3. **Plan Técnico** (plan.md) → Arquitectura y decisiones técnicas
4. **Contratos Gherkin** (features/) → Comportamientos verificables
5. **Generación Asistida** → IA propone implementación base
6. **Implementación Humana** → Refinamiento y ajustes
7. **Verificación CI/CD** → Tests automatizados + validaciones
8. **Evolución Continua** → Iteración basada en feedback

## 🏁 Criterios de Aceptación

- [ ] Todas las pantallas CICS replicadas como vistas React
- [ ] Todos los programas COBOL convertidos a servicios .NET
- [ ] Base de datos migrada de VSAM a SQL Server
- [ ] Tests contractuales (Gherkin) ejecutándose en CI/CD
- [ ] Documentación técnica completa (ADRs, diagramas)
- [ ] Plan de rollback y migración de datos definido

---

**Versión**: 1.0  
**Fecha**: 2025-12-01  
**Autor**: Equipo de Modernización  
**Método**: AURORA-IA™
