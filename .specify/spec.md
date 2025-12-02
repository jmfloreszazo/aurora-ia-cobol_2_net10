# Especificación Viva: CardDemo Modernizado

## 📌 Visión General

Sistema de gestión de tarjetas de crédito que permite a usuarios y administradores gestionar cuentas, tarjetas, transacciones y usuarios del sistema.

---

## 🎭 Actores del Sistema

### Usuario Regular (Regular User)
- **Rol**: USER
- **Capacidades**:
  - Ver y actualizar sus cuentas
  - Gestionar sus tarjetas de crédito
  - Consultar, agregar y reportar transacciones
  - Realizar pagos de facturas
  - Consultar autorizaciones pendientes

### Administrador (Admin User)
- **Rol**: ADMIN
- **Capacidades**:
  - Todas las de Usuario Regular
  - Gestión completa de usuarios (CRUD)
  - Mantenimiento de tipos de transacciones

---

## 📦 Entidades del Dominio

### 1. User (Usuario del Sistema)
**Propósito**: Gestionar credenciales y permisos de acceso

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| UserId | string(8) | Identificador único | PK, obligatorio |
| Password | string(8) | Contraseña encriptada | Obligatorio, min 8 chars |
| FirstName | string(25) | Nombre | Obligatorio |
| LastName | string(25) | Apellido | Obligatorio |
| UserType | enum | USER o ADMIN | Obligatorio |
| CreatedAt | datetime | Fecha creación | Auto |
| UpdatedAt | datetime | Última modificación | Auto |

**Reglas de Negocio**:
- UserId debe ser único en el sistema
- Password debe almacenarse hasheado (bcrypt/PBKDF2)
- Cambio de password requiere password actual
- Admin puede crear/modificar cualquier usuario
- Usuario regular solo puede ver/modificar sus datos

---

### 2. Customer (Cliente)
**Propósito**: Datos personales del cliente bancario

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| CustomerId | int(9) | ID del cliente | PK, obligatorio |
| FirstName | string(25) | Nombre | Obligatorio |
| MiddleName | string(25) | Segundo nombre | Opcional |
| LastName | string(25) | Apellido | Obligatorio |
| AddressLine1 | string(50) | Dirección línea 1 | Obligatorio |
| AddressLine2 | string(50) | Dirección línea 2 | Opcional |
| AddressLine3 | string(50) | Dirección línea 3 | Opcional |
| StateCode | string(2) | Código de estado | Obligatorio |
| CountryCode | string(3) | Código de país | Obligatorio |
| ZipCode | string(10) | Código postal | Obligatorio |
| PhoneNumber1 | string(15) | Teléfono primario | Obligatorio |
| PhoneNumber2 | string(15) | Teléfono secundario | Opcional |
| SSN | string(9) | Número seguridad social | Único, obligatorio |
| GovernmentId | string(20) | ID gubernamental | Obligatorio |
| DateOfBirth | date | Fecha nacimiento | Obligatorio, > 18 años |
| FICOScore | int(3) | Score crediticio | 300-850 |
| EFTAccountId | string(10) | Cuenta transferencia | Opcional |
| PrimaryCardHolder | bool | Titular principal | Default: true |

**Reglas de Negocio**:
- SSN debe ser único
- Edad mínima: 18 años
- FICO Score válido: 300-850
- Dirección completa obligatoria

---

### 3. Account (Cuenta Bancaria)
**Propósito**: Representar cuenta de crédito del cliente

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| AccountId | long(11) | ID de cuenta | PK, obligatorio |
| CustomerId | int(9) | ID del cliente | FK → Customer |
| ActiveStatus | string(1) | Estado (Y/N) | Obligatorio |
| CurrentBalance | decimal(10,2) | Saldo actual | Default: 0.00 |
| CreditLimit | decimal(10,2) | Límite de crédito | Obligatorio, > 0 |
| CashCreditLimit | decimal(10,2) | Límite efectivo | Obligatorio, > 0 |
| OpenDate | date | Fecha apertura | Obligatorio |
| ExpirationDate | date | Fecha expiración | > OpenDate |
| ReissueDate | date | Fecha reemisión | Opcional |
| CurrentCycleCredit | decimal(10,2) | Créditos ciclo actual | Default: 0.00 |
| CurrentCycleDebit | decimal(10,2) | Débitos ciclo actual | Default: 0.00 |
| ZipCode | string(10) | Código postal | Obligatorio |
| GroupId | string(10) | ID de grupo | Opcional |

**Reglas de Negocio**:
- CurrentBalance no puede exceder CreditLimit
- CashCreditLimit ≤ CreditLimit
- ExpirationDate > OpenDate
- ActiveStatus solo puede ser 'Y' o 'N'
- Saldo negativo = cliente debe dinero

---

### 4. Card (Tarjeta de Crédito)
**Propósito**: Tarjeta física asociada a una cuenta

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| CardNumber | string(16) | Número de tarjeta | PK, obligatorio |
| AccountId | long(11) | ID de cuenta | FK → Account |
| CardType | string(10) | Tipo tarjeta | Obligatorio |
| EmbossedName | string(50) | Nombre impreso | Obligatorio |
| ExpirationDate | string(10) | MM/AAAA | Obligatorio, futuro |
| ActiveStatus | string(1) | Estado (Y/N) | Obligatorio |

**Reglas de Negocio**:
- CardNumber debe pasar algoritmo de Luhn
- ExpirationDate debe ser futura
- Una cuenta puede tener múltiples tarjetas
- Solo tarjetas activas pueden usarse

---

### 5. Transaction (Transacción)
**Propósito**: Registro de movimientos en cuentas

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| TransactionId | string(16) | ID único | PK, obligatorio |
| AccountId | long(11) | ID de cuenta | FK → Account |
| CardNumber | string(16) | Número de tarjeta | FK → Card |
| TransactionType | string(2) | Tipo transacción | FK → TransactionType |
| CategoryCode | int(4) | Código categoría | FK → TransactionCategory |
| TransactionSource | string(10) | Origen | Obligatorio |
| Description | string(100) | Descripción | Obligatorio |
| Amount | decimal(10,2) | Monto | Obligatorio, != 0 |
| MerchantId | string(9) | ID comerciante | Opcional |
| MerchantName | string(50) | Nombre comerciante | Opcional |
| MerchantCity | string(50) | Ciudad comerciante | Opcional |
| MerchantZip | string(10) | Código postal | Opcional |
| OrigTransactionId | string(16) | ID transacción original | Para reversas |
| TransactionDate | datetime | Fecha/hora transacción | Obligatorio |
| ProcessedFlag | string(1) | Procesado (Y/N) | Default: N |

**Reglas de Negocio**:
- Amount positivo = crédito, negativo = débito
- Transacción debe asociarse a cuenta activa
- Tarjeta debe estar activa y no expirada
- Transacciones procesadas no se pueden modificar

---

### 6. TransactionType (Tipo de Transacción)
**Propósito**: Catálogo de tipos de transacciones

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| TypeCode | string(2) | Código | PK, obligatorio |
| TypeDescription | string(50) | Descripción | Obligatorio |
| CategoryCode | int(4) | Categoría | FK → TransactionCategory |

**Ejemplos**:
- 01: Compra
- 02: Retiro ATM
- 03: Pago
- 04: Reversa

---

### 7. TransactionCategory (Categoría de Transacción)
**Propósito**: Categorización de transacciones

| Campo | Tipo | Descripción | Reglas |
|-------|------|-------------|--------|
| CategoryCode | int(4) | Código | PK, obligatorio |
| CategoryDescription | string(50) | Descripción | Obligatorio |

**Ejemplos**:
- 5010: Restaurantes
- 5411: Supermercados
- 5812: Entretenimiento
- 6011: Retiros ATM

---

## 🔄 Casos de Uso Principales

### UC-001: Autenticación de Usuario
**Actor**: Usuario Regular / Administrador  
**Precondición**: Usuario registrado en el sistema

**Flujo Principal**:
1. Usuario accede a la página de login
2. Sistema muestra formulario (UserId, Password)
3. Usuario ingresa credenciales
4. Sistema valida credenciales contra base de datos
5. Sistema genera JWT token con claims (UserId, Role)
6. Sistema redirige según rol:
   - ADMIN → Admin Menu
   - USER → Main Menu

**Flujos Alternativos**:
- **4a**: Credenciales inválidas
  - Sistema muestra error "Invalid credentials"
  - Sistema permanece en login
- **4b**: Usuario bloqueado (3 intentos fallidos)
  - Sistema muestra "Account locked. Contact admin"
  - Sistema registra evento de seguridad

**Postcondición**: Usuario autenticado con sesión válida

---

### UC-002: Ver Lista de Cuentas
**Actor**: Usuario Regular  
**Precondición**: Usuario autenticado

**Flujo Principal**:
1. Usuario selecciona "View Accounts"
2. Sistema consulta cuentas asociadas al CustomerId del usuario
3. Sistema muestra lista con:
   - Account ID
   - Current Balance
   - Credit Limit
   - Status
   - Expiration Date
4. Usuario puede seleccionar cuenta para ver detalles

**Postcondición**: Lista de cuentas mostrada

---

### UC-003: Actualizar Cuenta
**Actor**: Usuario Regular  
**Precondición**: Usuario visualizando cuenta

**Flujo Principal**:
1. Usuario modifica campos editables (CreditLimit, ZipCode)
2. Usuario presiona "Update"
3. Sistema valida datos
4. Sistema actualiza registro en DB
5. Sistema muestra mensaje "Account updated successfully"

**Flujos Alternativos**:
- **3a**: Datos inválidos
  - Sistema muestra errores de validación
  - Usuario corrige y reintenta

**Postcondición**: Cuenta actualizada

---

### UC-004: Listar Tarjetas
**Actor**: Usuario Regular  
**Precondición**: Usuario autenticado

**Flujo Principal**:
1. Usuario selecciona "Credit Card List"
2. Sistema consulta tarjetas del cliente
3. Sistema muestra lista con:
   - Card Number (enmascarado: **** **** **** 1234)
   - Account ID
   - Card Type
   - Expiration Date
   - Status
4. Usuario puede ver detalles o actualizar tarjeta

**Postcondición**: Lista de tarjetas mostrada

---

### UC-005: Agregar Transacción
**Actor**: Usuario Regular  
**Precondición**: Usuario autenticado, cuenta activa

**Flujo Principal**:
1. Usuario selecciona "Add Transaction"
2. Sistema muestra formulario:
   - Card Number (dropdown de tarjetas activas)
   - Transaction Type
   - Category
   - Amount
   - Description
   - Merchant Info (opcional)
3. Usuario completa datos y presiona "Submit"
4. Sistema valida:
   - Tarjeta activa y no expirada
   - Amount > 0
   - Saldo suficiente (si es débito)
5. Sistema genera TransactionId único
6. Sistema registra transacción
7. Sistema actualiza CurrentBalance de la cuenta
8. Sistema muestra confirmación con TransactionId

**Flujos Alternativos**:
- **4a**: Saldo insuficiente
  - Sistema muestra "Insufficient credit limit"
  - Transacción rechazada
- **4b**: Tarjeta inactiva/expirada
  - Sistema muestra "Card invalid or expired"
  - Transacción rechazada

**Postcondición**: Transacción registrada y saldo actualizado

---

### UC-006: Generar Reporte de Transacciones
**Actor**: Usuario Regular  
**Precondición**: Usuario autenticado

**Flujo Principal**:
1. Usuario selecciona "Transaction Reports"
2. Sistema muestra filtros:
   - Account (dropdown)
   - Date Range (From/To)
   - Transaction Type (opcional)
3. Usuario selecciona criterios y presiona "Generate"
4. Sistema consulta transacciones que coincidan
5. Sistema muestra reporte con:
   - Transaction ID
   - Date
   - Description
   - Amount
   - Merchant
   - Running Balance
6. Usuario puede exportar a PDF/Excel

**Postcondición**: Reporte generado

---

### UC-007: Realizar Pago de Factura
**Actor**: Usuario Regular  
**Precondición**: Usuario autenticado, cuenta con saldo deudor

**Flujo Principal**:
1. Usuario selecciona "Bill Payment"
2. Sistema muestra cuenta seleccionada con:
   - Current Balance (deuda)
   - Minimum Payment Due
3. Usuario ingresa monto a pagar
4. Sistema valida monto ≤ Current Balance
5. Sistema procesa pago:
   - Crea transacción tipo "Payment"
   - Actualiza CurrentBalance (reduce deuda)
6. Sistema muestra confirmación

**Flujos Alternativos**:
- **3a**: Monto > Balance
  - Sistema ajusta automáticamente al balance
  - Muestra advertencia

**Postcondición**: Pago registrado, balance actualizado

---

### UC-008: Gestionar Usuarios (Admin)
**Actor**: Administrador  
**Precondición**: Usuario con rol ADMIN autenticado

**Flujo Principal (Crear)**:
1. Admin selecciona "Add User"
2. Sistema muestra formulario
3. Admin ingresa datos (UserId, Password, Name, UserType)
4. Admin presiona "Save"
5. Sistema valida UserId único
6. Sistema crea usuario con password hasheado
7. Sistema muestra confirmación

**Flujo Principal (Actualizar)**:
1. Admin selecciona usuario de la lista
2. Sistema muestra formulario prellenado
3. Admin modifica campos
4. Admin presiona "Update"
5. Sistema actualiza datos
6. Sistema muestra confirmación

**Flujo Principal (Eliminar)**:
1. Admin selecciona usuario
2. Admin presiona "Delete"
3. Sistema solicita confirmación
4. Admin confirma
5. Sistema elimina usuario (soft delete)
6. Sistema muestra confirmación

**Flujos Alternativos**:
- **5a (Crear)**: UserId duplicado
  - Sistema muestra "User ID already exists"
  - Admin modifica UserId

**Postcondición**: Usuario creado/actualizado/eliminado

---

## 🔒 Reglas de Negocio Transversales

### RN-001: Validación de Tarjeta
- Número de tarjeta debe pasar algoritmo de Luhn
- Tarjeta debe estar activa (ActiveStatus = 'Y')
- Tarjeta no debe estar expirada (ExpirationDate > hoy)

### RN-002: Límite de Crédito
- CurrentBalance + nueva transacción débito ≤ CreditLimit
- Transacciones que excedan límite son rechazadas
- Advertencia cuando balance > 80% del límite

### RN-003: Procesamiento Batch
- Intereses calculados mensualmente (ciclo de facturación)
- Transacciones procesadas (ProcessedFlag='Y') no modificables
- Estados de cuenta generados automáticamente

### RN-004: Auditoría
- Todas las operaciones críticas registradas (audit log)
- Cambios de password requieren password actual
- Intentos fallidos de login registrados

### RN-005: Seguridad
- Passwords almacenados con hash + salt (bcrypt)
- JWT tokens con expiración de 30 minutos
- Refresh tokens para sesiones extendidas
- HTTPS obligatorio en producción

### RN-006: Concurrencia
- Operaciones sobre saldo usan locking optimista
- Versioning en entidades críticas (Account, Transaction)

---

## 🔍 Consultas Principales

### Q-001: Listar Cuentas por Cliente
```
GET /api/customers/{customerId}/accounts
Retorna: List<Account>
Filtros: status, activeOnly
```

### Q-002: Obtener Detalles de Cuenta
```
GET /api/accounts/{accountId}
Retorna: AccountDetailDto (incluye Customer, Cards)
```

### Q-003: Listar Transacciones por Cuenta
```
GET /api/accounts/{accountId}/transactions
Retorna: PagedList<Transaction>
Filtros: dateFrom, dateTo, type, category
Paginación: page, pageSize
```

### Q-004: Buscar Tarjeta por Número
```
GET /api/cards/{cardNumber}
Retorna: CardDto
Validación: Luhn, existencia
```

### Q-005: Reporte de Transacciones
```
GET /api/reports/transactions
Parámetros: accountId, dateFrom, dateTo, groupBy
Retorna: TransactionReportDto (con totales, gráficos)
```

---

## 📊 Comandos Principales

### C-001: Crear Usuario
```
POST /api/users
Body: { userId, password, firstName, lastName, userType }
Validaciones: UserId único, password fuerte
Retorna: UserDto (sin password)
```

### C-002: Actualizar Cuenta
```
PUT /api/accounts/{accountId}
Body: { creditLimit, cashCreditLimit, zipCode }
Validaciones: Límites válidos, cuenta existe
Retorna: AccountDto
```

### C-003: Agregar Transacción
```
POST /api/transactions
Body: { accountId, cardNumber, typeCode, categoryCode, 
        amount, description, merchantInfo }
Validaciones: Saldo, tarjeta activa, límites
Efectos: Actualiza Account.CurrentBalance
Retorna: TransactionDto con ID generado
```

### C-004: Procesar Pago
```
POST /api/payments
Body: { accountId, amount, paymentMethod }
Validaciones: Amount ≤ CurrentBalance
Efectos: Crea transacción tipo "Payment", reduce deuda
Retorna: PaymentConfirmationDto
```

### C-005: Actualizar Tarjeta
```
PUT /api/cards/{cardNumber}
Body: { embossedName, activeStatus }
Validaciones: CardNumber existe, estado válido
Retorna: CardDto
```

---

## 🎯 Criterios de Aceptación Generales

✅ **Funcionalidad**
- Todas las operaciones COBOL replicadas en API REST
- Paridad 100% en lógica de negocio
- Sin pérdida de datos en migración

✅ **Performance**
- API: < 200ms (p95) para queries simples
- API: < 500ms (p95) para queries complejas
- Frontend: < 2s carga inicial, < 500ms navegación

✅ **Seguridad**
- Autenticación JWT en todos los endpoints
- Autorización basada en roles (USER/ADMIN)
- Datos sensibles encriptados en BD
- CORS configurado correctamente

✅ **UX**
- Interfaz responsive (desktop/tablet/mobile)
- Mensajes de error claros y accionables
- Feedback visual en operaciones async
- Accesibilidad WCAG 2.1 AA

✅ **Testing**
- Cobertura de tests > 80%
- Tests contractuales (Gherkin) pasando
- Tests E2E para flujos críticos

---

## 📝 Notas de Implementación

### Priorización de Módulos
1. **MVP (Phase 1)**: Autenticación + Cuentas + Tarjetas ✅
2. **Phase 2**: Transacciones + Reportes ✅
3. **Phase 3**: Pagos + Administración avanzada ✅
4. **Phase 4**: Batch Processing + Features opcionales ✅

### Migraciones de Datos
- Script de migración VSAM → SQL Server ✅
- Validación de integridad post-migración ✅
- Plan de rollback con backups ✅

### Integraciones Futuras
- API Gateway para rate limiting
- Azure Application Insights para monitoring
- Redis para caching de queries frecuentes

---

## ⚙️ Especificaciones de Batch Processing

### BP-001: Transaction Posting (CBTRN01C/02C)
**Propósito**: Procesar transacciones pendientes al final del día

**Entrada**:
- Transacciones con ProcessedFlag = 'N'

**Proceso**:
1. Obtener todas las transacciones no procesadas
2. Para cada transacción:
   - Validar tarjeta activa y no expirada
   - Validar cuenta activa
   - Verificar límite de crédito para débitos
   - Si válida: actualizar balance y marcar ProcessedFlag='Y'
   - Si inválida: registrar en log y saltar

**Salida**:
```json
{
  "processed": 150,
  "skipped": 3,
  "errors": [],
  "executedAt": "2025-01-15T23:00:00Z",
  "duration": "00:00:45"
}
```

**Reglas de Negocio**:
- RN-BP-001: Solo procesar tarjetas activas (ActiveStatus='Y')
- RN-BP-002: Solo procesar tarjetas no expiradas (ExpirationDate > today)
- RN-BP-003: Solo procesar cuentas activas (ActiveStatus='Y')
- RN-BP-004: Rechazar si débito excede límite de crédito disponible
- RN-BP-005: Actualizar CurrentCycleDebit/Credit según tipo de transacción

---

### BP-002: Interest Calculation (CBACT02C)
**Propósito**: Calcular y aplicar intereses diarios a cuentas con saldo

**Entrada**:
- Cuentas activas con CurrentBalance > 0

**Proceso**:
1. Calcular tasa diaria: APR / 365 = 19.99% / 365 = 0.0548%
2. Para cada cuenta con saldo positivo:
   - Calcular interés: Balance × TasaDiaria
   - Crear transacción tipo 'IN' (Interest)
   - Actualizar CurrentBalance sumando interés

**Salida**:
```json
{
  "accountsProcessed": 500,
  "totalInterestCharged": 2345.67,
  "averageInterest": 4.69,
  "executedAt": "2025-01-15T02:00:00Z"
}
```

**Reglas de Negocio**:
- RN-BP-006: APR fijo de 19.99% (configurable)
- RN-BP-007: Solo aplicar a cuentas con saldo > 0
- RN-BP-008: Redondear interés a 2 decimales
- RN-BP-009: Crear transacción de interés automáticamente

---

### BP-003: Statement Generation (CBSTM03A/B)
**Propósito**: Generar estados de cuenta mensuales

**Entrada**:
- Cuentas en fecha de cierre de ciclo

**Proceso**:
1. Identificar cuentas con fecha de corte = hoy
2. Para cada cuenta:
   - Calcular Previous Balance
   - Sumar Total Debits (compras, intereses)
   - Sumar Total Credits (pagos)
   - Calcular New Balance
   - Calcular Minimum Payment (2% o $25, lo que sea mayor)
   - Generar formato de estado de cuenta

**Salida**:
```json
{
  "statementsGenerated": 150,
  "statements": [
    {
      "accountId": "12345678901",
      "previousBalance": 1234.56,
      "totalDebits": 789.23,
      "totalCredits": 500.00,
      "interestCharged": 15.67,
      "newBalance": 1539.46,
      "minimumPayment": 45.00,
      "dueDate": "2025-02-10"
    }
  ]
}
```

**Reglas de Negocio**:
- RN-BP-010: Minimum Payment = MAX(2% of Balance, $25)
- RN-BP-011: Due Date = Statement Date + 25 días
- RN-BP-012: Incluir detalle de transacciones del ciclo

---

### BP-004: Data Export (CBEXPORT)
**Propósito**: Exportar datos en formatos COBOL-compatible y modernos

**Formatos Soportados**:

| Formato | Descripción | Uso |
|---------|-------------|-----|
| FIXED | Campos longitud fija | Sistemas legacy |
| CSV | Comma-separated | Excel, análisis |
| JSON | JavaScript Object Notation | APIs, web |

**Entidades Exportables**:
- Accounts (cuentas)
- Transactions (transacciones)
- Customers (clientes)
- Cards (tarjetas)

**Layout Fixed-Width (Accounts)**:
```
Pos 01-11:  AccountId (11 numeric)
Pos 12-36:  CustomerName (25 alpha)
Pos 37:     ActiveStatus (1 alpha Y/N)
Pos 38-49:  CurrentBalance (10.2 decimal)
Pos 50-61:  CreditLimit (10.2 decimal)
Pos 62-73:  CashCreditLimit (10.2 decimal)
Pos 74-81:  OpenDate (YYYYMMDD)
Pos 82-89:  ExpirationDate (YYYYMMDD)
Pos 90-97:  ReissueDate (YYYYMMDD)
Total: 97 bytes por registro
```

---

### BP-005: Nightly Batch Cycle
**Propósito**: Ejecutar todos los procesos batch en secuencia correcta

**Secuencia de Ejecución**:
1. **23:00** - Transaction Posting
2. **02:00** - Interest Calculation
3. **04:00** - Statement Generation (si fecha de corte)
4. **05:00** - Data Export (backup)

**Orquestación**:
```csharp
public async Task<NightlyBatchResult> RunNightlyBatchAsync()
{
    var results = new NightlyBatchResult();
    
    // Step 1: Post transactions
    results.Posting = await _postingService.PostPendingTransactionsAsync();
    
    // Step 2: Calculate interest
    results.Interest = await _interestService.CalculateDailyInterestAsync();
    
    // Step 3: Generate statements
    results.Statements = await _statementService.GenerateStatementsAsync();
    
    // Step 4: Export data
    results.Export = await _exportService.ExportAllAsync("json");
    
    results.CompletedAt = DateTime.UtcNow;
    return results;
}
```

---

## 💳 Especificaciones de Billing (COBIL00C)

### BL-001: Bill Payment
**Propósito**: Permitir pagos parciales o totales de facturas

**Endpoint**: `POST /api/payments`

**Entrada**:
```json
{
  "accountId": "12345678901",
  "amount": 500.00,
  "paymentMethod": "ACH",
  "sourceAccount": "XXXX4567"
}
```

**Validaciones**:
- RN-BL-001: Amount debe ser > 0
- RN-BL-002: Amount no puede exceder CurrentBalance
- RN-BL-003: Cuenta debe estar activa

**Proceso**:
1. Validar datos de entrada
2. Crear transacción tipo 'PA' (Payment)
3. Reducir CurrentBalance en Amount
4. Incrementar CurrentCycleCredit
5. Registrar en audit log

**Salida**:
```json
{
  "success": true,
  "transactionId": "PAY20250115001234",
  "newBalance": 1039.46,
  "message": "Payment processed successfully"
}
```

---

### BL-002: Pay Full Balance
**Propósito**: Pagar el saldo completo de una cuenta

**Endpoint**: `POST /api/payments/pay-full`

**Entrada**:
```json
{
  "accountId": "12345678901",
  "paymentMethod": "ACH"
}
```

**Proceso**:
1. Obtener CurrentBalance de la cuenta
2. Crear transacción de pago por el total
3. Establecer CurrentBalance = 0
4. Actualizar ciclo de créditos

**Salida**:
```json
{
  "success": true,
  "transactionId": "PAY20250115001235",
  "amountPaid": 1539.46,
  "newBalance": 0.00,
  "message": "Full balance paid successfully"
}
```

---

## 📊 Especificaciones de Reports (CORPT00C)

### RP-001: Monthly Transaction Report
**Propósito**: Resumen de transacciones del mes actual

**Endpoint**: `GET /api/reports/monthly?accountId={id}&month={MM}&year={YYYY}`

**Salida**:
```json
{
  "accountId": "12345678901",
  "period": "2025-01",
  "summary": {
    "totalDebits": 2345.67,
    "totalCredits": 1000.00,
    "netChange": 1345.67,
    "transactionCount": 45
  },
  "byCategory": [
    { "category": "Restaurants", "amount": 456.78, "count": 12 },
    { "category": "Gas", "amount": 234.56, "count": 8 }
  ],
  "transactions": [ /* detailed list */ ]
}
```

---

### RP-002: Yearly Summary Report
**Propósito**: Resumen anual de actividad de cuenta

**Endpoint**: `GET /api/reports/yearly?accountId={id}&year={YYYY}`

**Salida**:
```json
{
  "accountId": "12345678901",
  "year": 2025,
  "monthlyBreakdown": [
    { "month": "January", "debits": 2345.67, "credits": 1000.00 },
    { "month": "February", "debits": 1890.23, "credits": 800.00 }
  ],
  "yearTotal": {
    "totalDebits": 28456.78,
    "totalCredits": 15000.00,
    "interestPaid": 890.34,
    "averageBalance": 3456.78
  }
}
```

---

### RP-003: Custom Date Range Report
**Propósito**: Reporte personalizado por rango de fechas

**Endpoint**: `GET /api/reports/custom?accountId={id}&from={date}&to={date}`

**Filtros Adicionales**:
- `type`: Tipo de transacción (Purchase, Payment, Interest)
- `category`: Categoría (Restaurants, Gas, etc.)
- `minAmount`: Monto mínimo
- `maxAmount`: Monto máximo

---

### RP-004: Export Report
**Propósito**: Exportar reportes en diferentes formatos

**Endpoint**: `GET /api/reports/export?format={pdf|excel|csv}`

**Formatos**:
- **PDF**: Documento formateado para impresión
- **Excel**: Spreadsheet con gráficos
- **CSV**: Datos planos para análisis

---

**Versión**: 2.0  
**Última Actualización**: 2025-01-15  
**Método**: AURORA-IA™  
**Estado**: ✅ Especificación Completa - PROYECTO FINALIZADO
