# Frontend AI Agent Prompt: Clinic Exception Management Feature

## Context
You are building a clinic exception management feature for a .NET 8 backend (MediQueue) and need to implement the frontend. This document provides complete backend specifications and requirements.

---

## 1. Backend Endpoints Verification ?

### Base URL
`https://localhost:7101/api/workingschedule`

### Available Endpoints

#### 1.1 GET My Exceptions (Clinic Only)
```
GET /api/workingschedule/my-exceptions
Authorization: Bearer <token> (Role: Clinic required)
```

**Method Signature:**
```csharp
[HttpGet("my-exceptions")]
[Authorize(Roles = "Clinic")]
public async Task<ActionResult<List<ClinicExceptionDto>>> GetMyExceptions()
```

**Response:**
```json
[
  {
    "id": 1,
    "exceptionDate": "2026-02-03T00:00:00",
    "reason": "Christmas Holiday"
  }
]
```

#### 1.2 POST Add Exception
```
POST /api/workingschedule/exceptions
Authorization: Bearer <token> (Role: Clinic required)
Content-Type: application/json
```

**Request Body:**
```json
{
  "exceptionDate": "2026-02-03",
  "reason": "Christmas Holiday"
}
```

**Response:**
```json
{
  "id": 1,
  "exceptionDate": "2026-02-03T00:00:00",
  "reason": "Christmas Holiday"
}
```

**Status Codes:**
- `201 Created` - Exception added successfully
- `400 Bad Request` - Validation error or duplicate date
- `404 Not Found` - Clinic not found
- `401 Unauthorized` - Invalid/missing token
- `403 Forbidden` - User is not a clinic

#### 1.3 PUT Update Exception
```
PUT /api/workingschedule/exceptions/{id}
Authorization: Bearer <token> (Role: Clinic required)
Content-Type: application/json
```

**Request Body:**
```json
{
  "exceptionDate": "2026-12-26",
  "reason": "Boxing Day"
}
```

**Response:**
```json
{
  "id": 1,
  "exceptionDate": "2026-12-26T00:00:00",
  "reason": "Boxing Day"
}
```

**Status Codes:**
- `200 OK` - Exception updated successfully
- `404 Not Found` - Exception not found
- `400 Bad Request` - Validation error

#### 1.4 DELETE Exception
```
DELETE /api/workingschedule/exceptions/{id}
Authorization: Bearer <token> (Role: Clinic required)
```

**Response:**
```json
{
  "statusCode": 200,
  "message": "Exception deleted successfully"
}
```

**Status Codes:**
- `200 OK` - Exception deleted successfully
- `404 Not Found` - Exception not found

---

## 2. Data Structure

### Exception Entity (Backend)
```csharp
public class ClinicException
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public DateTime ExceptionDate { get; set; }
    public string Reason { get; set; } // Required, max 500 chars
}
```

### DTOs

**ClinicExceptionDto (Response):**
```typescript
interface ClinicExceptionDto {
  id: number;
  exceptionDate: string; // ISO 8601 format "2026-02-03T00:00:00"
  reason: string;
}
```

**CreateClinicExceptionDto (Request):**
```typescript
interface CreateClinicExceptionDto {
  exceptionDate: string; // Required, date string "2026-02-03"
  reason: string; // Required, max 500 characters
}
```

**UpdateClinicExceptionDto (Request):**
```typescript
interface UpdateClinicExceptionDto {
  exceptionDate: string; // Required
  reason: string; // Required, max 500 characters
}
```

**Important Notes:**
- ? `isClosed` field does NOT exist in the backend
- ? `startTime` and `endTime` fields do NOT exist
- ? Exception = full day closure by default
- ? Only `id`, `exceptionDate`, and `reason` are in the response

---

## 3. Authentication & Authorization

### Required Headers
```
Authorization: Bearer <JWT_TOKEN>
Content-Type: application/json
```

### Role Requirements
- **All exception endpoints require `Roles = "Clinic"`**
- Patients and Admins **cannot** access these endpoints
- Missing token ? `401 Unauthorized`
- Wrong role ? `403 Forbidden`

### How Backend Determines Clinic
```csharp
private async Task<int> GetCurrentClinicIdAsync()
{
    var userId = GetCurrentUserId(); // From JWT claims
    var clinic = await _clinicService.GetClinicByUserIdAsync(userId);
    if (clinic == null)
        throw new InvalidOperationException("Clinic profile not found");
    return clinic.Id;
}
```

**Clinic ID is automatically determined from the authenticated user - don't send it in requests.**

---

## 4. Error Handling

### 405 Method Not Allowed
**The issue you encountered is likely:**
- ? Wrong HTTP method (e.g., POST instead of GET)
- ? Wrong route (e.g., `/exceptions` instead of `/my-exceptions`)
- ? Missing route prefix `/api/workingschedule`

**Correct Routes:**
```
? GET  /api/workingschedule/my-exceptions
? POST /api/workingschedule/exceptions
? PUT  /api/workingschedule/exceptions/{id}
? DELETE /api/workingschedule/exceptions/{id}
```

### Error Response Format
```json
{
  "statusCode": 400,
  "message": "Error description"
}
```

### Common Errors

**400 Bad Request:**
```json
{
  "statusCode": 400,
  "message": "Exception already exists for date 2026-02-03"
}
```

**404 Not Found:**
```json
{
  "statusCode": 404,
  "message": "Exception with ID 5 not found"
}
```

**401 Unauthorized:**
```json
{
  "statusCode": 401,
  "message": "Unauthorized"
}
```

**403 Forbidden:**
```json
{
  "statusCode": 403,
  "message": "You do not have permission to access this resource"
}
```

---

## 5. Validation Rules

### ExceptionDate
- ? Must be a valid date
- ? Can be past or future dates
- ? Stored as `DateTime` (time portion is `00:00:00`)
- ? Duplicate check: Cannot add exception for same clinic + same date
- ?? Send as string: `"2026-02-03"` or `"2026-02-03T00:00:00"`

### Reason
- ? Required field
- ? Maximum 500 characters
- ? Cannot be null or empty
- ? Examples: "Christmas Holiday", "Doctor on Leave", "Clinic Closed for Maintenance"

---

## 5.1 Working Days Validation Rules (Additional Context)

### Working Hours Validation
- ? **When `IsClosed = false`**: StartTime must be before EndTime
- ? **When `IsClosed = true`**: StartTime and EndTime validation is **skipped** (can be any value)
- ? Working hours must be within 24-hour range (00:00:00 to 24:00:00)

### Common Issue Fixed
**Problem**: When updating working days with closed days, you might get:
```json
{"statusCode":400,"message":"Start time must be before end time"}
```

**Solution**: The backend now skips time validation for closed days. When `IsClosed = true`, you can send:
```json
{
  "dayOfWeek": 0,
  "startTime": "00:00:00",
  "endTime": "00:00:00",
  "isClosed": true
}
```

---

## 6. Business Logic

### What Exceptions Are For
- Represent **full-day closures**
- Override regular working day schedule
- Used for holidays, doctor leave, special events, maintenance
- Patients cannot book appointments on exception dates

### Exception Behavior
```
Regular Schedule: Monday 9:00 AM - 5:00 PM
Exception: 2026-02-03 "National Holiday"
Result: Clinic is CLOSED all day on 2026-02-03 (no appointments allowed)
```

### Duplicate Prevention
```csharp
// Backend checks for duplicates
var existing = await GetExceptionByDateAsync(clinicId, dto.ExceptionDate);
if (existing != null)
    throw new InvalidOperationException("Exception already exists for this date");
```

---

## 7. Frontend Implementation Guide

### State Management
```typescript
// Recommended state structure
interface ExceptionState {
  exceptions: ClinicExceptionDto[];
  loading: boolean;
  error: string | null;
  selectedException: ClinicExceptionDto | null;
}
```

### API Service Example
```typescript
class ExceptionService {
  private baseUrl = 'https://localhost:7101/api/workingschedule';
  
  async getMyExceptions(): Promise<ClinicExceptionDto[]> {
    const response = await fetch(`${this.baseUrl}/my-exceptions`, {
  headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      }
  });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    return response.json();
  }

  async addException(dto: CreateClinicExceptionDto): Promise<ClinicExceptionDto> {
    const response = await fetch(`${this.baseUrl}/exceptions`, {
      method: 'POST',
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
   },
      body: JSON.stringify(dto)
    });
    if (!response.ok) {
      const error = await response.json();
      throw new Error(error.message || `HTTP ${response.status}`);
    }
 return response.json();
  }
  
  async updateException(id: number, dto: UpdateClinicExceptionDto): Promise<ClinicExceptionDto> {
    const response = await fetch(`${this.baseUrl}/exceptions/${id}`, {
      method: 'PUT',
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(dto)
    });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
 return response.json();
  }
  
  async deleteException(id: number): Promise<void> {
 const response = await fetch(`${this.baseUrl}/exceptions/${id}`, {
      method: 'DELETE',
      headers: {
        'Authorization': `Bearer ${getToken()}`,
        'Content-Type': 'application/json'
}
    });
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
  }
}
```

### UI Components Needed

1. **Exception List Component**
   - Display all exceptions in a table/list
   - Show: Date, Reason, Actions (Edit, Delete)
   - Sort by date (ascending)
   - Empty state when no exceptions

2. **Add Exception Form**
   - Date picker (exceptionDate)
   - Text input (reason, max 500 chars)
   - Submit button
   - Validation messages
   - Success/error feedback

3. **Edit Exception Modal/Form**
   - Pre-filled date picker
   - Pre-filled reason text
   - Save/Cancel buttons
   - Validation

4. **Delete Confirmation Dialog**
   - Confirm before deletion
   - Show date and reason being deleted

### Recommended Features

- ? Date picker with disabled past dates (optional, backend allows any date)
- ? Character counter for reason field (500 max)
- ? Loading states during API calls
- ? Error messages for failed operations
- ? Success notifications
- ? Refresh list after add/update/delete
- ? Sort exceptions by date
- ? Filter upcoming vs past exceptions

---

## 8. Testing Checklist

### Manual Testing Steps

1. **GET My Exceptions**
```bash
curl -X GET "https://localhost:7101/api/workingschedule/my-exceptions" \
  -H "Authorization: Bearer YOUR_TOKEN"
```
Expected: `200 OK` with array of exceptions

2. **POST Add Exception**
```bash
curl -X POST "https://localhost:7101/api/workingschedule/exceptions" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"exceptionDate":"2026-12-25","reason":"Christmas"}'
```
Expected: `201 Created` with created exception

3. **POST Duplicate Exception (should fail)**
```bash
# Try adding same date again
```
Expected: `400 Bad Request` - "Exception already exists"

4. **PUT Update Exception**
```bash
curl -X PUT "https://localhost:7101/api/workingschedule/exceptions/1" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"exceptionDate":"2026-12-26","reason":"Boxing Day"}'
```
Expected: `200 OK` with updated exception

5. **DELETE Exception**
```bash
curl -X DELETE "https://localhost:7101/api/workingschedule/exceptions/1" \
  -H "Authorization: Bearer YOUR_TOKEN"
```
Expected: `200 OK` with success message

### Edge Cases to Test

- [ ] Empty exception list
- [ ] Adding exception with very long reason (500 chars)
- [ ] Adding exception with invalid date format
- [ ] Updating non-existent exception (404)
- [ ] Deleting non-existent exception (404)
- [ ] Missing authorization token (401)
- [ ] Patient user trying to access (403)
- [ ] Special characters in reason field
- [ ] Dates in different formats (ISO, locale-specific)

---

## 9. Common Pitfalls to Avoid

? **DON'T:**
- Send `isClosed`, `startTime`, `endTime` in requests (they don't exist)
- Try to specify `clinicId` manually (it's determined from JWT)
- Forget `Bearer` prefix in Authorization header
- Use wrong HTTP methods (e.g., GET for create)
- Skip error handling for duplicate dates
- Forget to refresh the exception list after mutations

? **DO:**
- Always include Authorization header
- Validate date format before sending
- Handle all HTTP status codes (200, 201, 400, 401, 403, 404)
- Show user-friendly error messages
- Confirm before deleting
- Refresh data after successful operations
- Store dates consistently (UTC or local timezone)

---

## 10. Backend Technical Details (Reference)

### Controller Implementation
- Located in: `MediQueue.APIs/Controllers/WorkingScheduleController.cs`
- Inherits from: `BaseApiController`
- Route prefix: `[Route("api/[controller]")]`
- Uses: `IWorkingScheduleService` and `IClinicService`

### Service Implementation
- Located in: `MediQueue.Service/WorkingScheduleService.cs`
- Uses: `IUnitOfWork` for database access
- Validation: Checks for duplicate dates
- Auto-detects clinic from authenticated user

### Database
- Table: `ClinicExceptions`
- Fields: `Id`, `ClinicId`, `ExceptionDate`, `Reason`
- Constraints: Unique index on `(ClinicId, ExceptionDate)`

### Controller Methods (Actual Backend Code)

```csharp
/// <summary>
/// Get current clinic's exceptions (Clinic only)
/// </summary>
[HttpGet("my-exceptions")]
[Authorize(Roles = "Clinic")]
public async Task<ActionResult<List<ClinicExceptionDto>>> GetMyExceptions()
{
    var clinicId = await GetCurrentClinicIdAsync();
    var exceptions = await _scheduleService.GetExceptionsAsync(clinicId);
    return Ok(exceptions);
}

/// <summary>
/// Add exception date (Clinic only)
/// </summary>
[HttpPost("exceptions")]
[Authorize(Roles = "Clinic")]
public async Task<ActionResult<ClinicExceptionDto>> AddException(CreateClinicExceptionDto dto)
{
    try
    {
        var clinicId = await GetCurrentClinicIdAsync();
      var exception = await _scheduleService.AddExceptionAsync(clinicId, dto);
    return CreatedAtAction(nameof(GetExceptions), new { id = exception.Id }, exception);
  }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new ApiResponse(404, ex.Message));
    }
  catch (InvalidOperationException ex)
 {
        return BadRequest(new ApiResponse(400, ex.Message));
    }
}

/// <summary>
/// Update exception (Clinic only)
/// </summary>
[HttpPut("exceptions/{id}")]
[Authorize(Roles = "Clinic")]
public async Task<ActionResult<ClinicExceptionDto>> UpdateException(int id, UpdateClinicExceptionDto dto)
{
    try
    {
        var exception = await _scheduleService.UpdateExceptionAsync(id, dto);
        return Ok(exception);
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new ApiResponse(404, ex.Message));
    }
}

/// <summary>
/// Delete exception (Clinic only)
/// </summary>
[HttpDelete("exceptions/{id}")]
[Authorize(Roles = "Clinic")]
public async Task<ActionResult> DeleteException(int id)
{
    try
    {
        await _scheduleService.DeleteExceptionAsync(id);
        return Ok(new ApiResponse(200, "Exception deleted successfully"));
    }
    catch (KeyNotFoundException ex)
    {
        return NotFound(new ApiResponse(404, ex.Message));
    }
}
```

---

## 11. Quick Reference

### Endpoint Summary
| Method | Endpoint | Auth | Purpose |
|--------|----------|------|---------|
| GET | `/my-exceptions` | Clinic | Get all exceptions for logged-in clinic |
| POST | `/exceptions` | Clinic | Add new exception |
| PUT | `/exceptions/{id}` | Clinic | Update exception |
| DELETE | `/exceptions/{id}` | Clinic | Delete exception |

### Field Summary
| Field | Type | Required | Max Length | Notes |
|-------|------|----------|------------|-------|
| id | number | Auto | - | Generated by backend |
| exceptionDate | string | ? | - | ISO 8601 date format |
| reason | string | ? | 500 | Closure reason |

---

## 12. Example Full Workflow

```typescript
// 1. Load exceptions on component mount
async function loadExceptions() {
  try {
    const exceptions = await exceptionService.getMyExceptions();
    setExceptions(exceptions);
  } catch (error) {
    showError('Failed to load exceptions');
  }
}

// 2. Add new exception
async function handleAddException(formData: CreateClinicExceptionDto) {
  try {
    const newException = await exceptionService.addException(formData);
    setExceptions([...exceptions, newException]);
    showSuccess('Exception added successfully');
    closeAddModal();
  } catch (error) {
    if (error.message.includes('already exists')) {
      showError('An exception already exists for this date');
    } else {
      showError('Failed to add exception');
    }
  }
}

// 3. Update exception
async function handleUpdateException(id: number, formData: UpdateClinicExceptionDto) {
  try {
    const updated = await exceptionService.updateException(id, formData);
    setExceptions(exceptions.map(e => e.id === id ? updated : e));
    showSuccess('Exception updated successfully');
    closeEditModal();
  } catch (error) {
    showError('Failed to update exception');
  }
}

// 4. Delete exception
async function handleDeleteException(id: number) {
  if (!confirm('Are you sure you want to delete this exception?')) return;
  
  try {
    await exceptionService.deleteException(id);
    setExceptions(exceptions.filter(e => e.id !== id));
    showSuccess('Exception deleted successfully');
  } catch (error) {
    showError('Failed to delete exception');
  }
}
```

---

## 13. Backend Project Structure

### Workspace Information
- **Location**: `D:\Programming\Graduation Projects\BIS Teams 2026\Projects\MediQueue\MediQueue.Backend-main`
- **Framework**: .NET 8
- **Architecture**: Clean Architecture (4 projects)

### Projects
1. **MediQueue.Core** - Domain entities, DTOs, interfaces
2. **MediQueue.Repository** - Data access, EF Core, migrations
3. **MediQueue.Service** - Business logic, service implementations
4. **MediQueue.APIs** - Web API, controllers, Razor Pages

### Key Files
- Controllers: `MediQueue.APIs/Controllers/WorkingScheduleController.cs`
- Service: `MediQueue.Service/WorkingScheduleService.cs`
- DTOs: `MediQueue.Core/DTOs/WorkingScheduleDtos.cs`
- Entity: `MediQueue.Core/Entities/ClinicException.cs`
- Repository: `MediQueue.Repository/ClinicExceptionRepository.cs`

---

## Summary

? **Backend endpoints exist and are working**
? **Authentication requires Clinic role**
? **Exception data structure: id, exceptionDate, reason (NO isClosed, startTime, endTime)**
? **Clinic ID is auto-determined from JWT token**
? **Full CRUD operations available**
? **Duplicate date prevention built-in**

**Your 405 error was likely from:**
- Wrong HTTP method
- Wrong route URL
- Missing `/api/workingschedule` prefix

**Now you can confidently build the frontend knowing the exact backend contract!**

---

## Additional Resources

### Swagger Documentation
Access the API documentation at: `https://localhost:7101/swagger`

### Testing with Postman
1. Import collection with base URL: `https://localhost:7101`
2. Set Authorization header with Bearer token
3. Test all 4 endpoints (GET, POST, PUT, DELETE)

### Contact Backend Team
- Repository: https://github.com/keroraed/MediQueue.Backend-main
- Branch: main

---

**Document Version**: 1.0  
**Last Updated**: January 2026  
**Backend Version**: .NET 8

---

## Changelog

### Version 1.1 (Latest)
- **Fixed**: Validation error for closed working days
  - Issue: `{"statusCode":400,"message":"Start time must be before end time"}`
  - Solution: Working hours validation now skipped when `IsClosed = true`
  - Impact: Can now successfully update working days with closed days
  - File: `MediQueue.Service/WorkingScheduleService.cs`

### Version 1.0
- Initial documentation release
- Complete API specification for exception management
