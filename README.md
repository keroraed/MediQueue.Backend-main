# 🏥 MediQueue - Complete System Documentation

**Version**: 1.0  
**Last Updated**: December 2024  
**Framework**: .NET 8  
**Database**: SQL Server  
**Security Score**: ⭐⭐⭐⭐⭐ (5/5)

---

## 📋 Table of Contents

1. [System Overview](#system-overview)
2. [Quick Start Guide](#quick-start-guide)
3. [Architecture & Design](#architecture--design)
4. [Database Schema](#database-schema)
5. [Authentication & Security](#authentication--security)
6. [Role-Based Registration](#role-based-registration)
7. [API Endpoints Reference](#api-endpoints-reference)
8. [Security Audit](#security-audit)
9. [Database Migrations](#database-migrations)
10. [Deployment Guide](#deployment-guide)
11. [Troubleshooting](#troubleshooting)

---

## 1. System Overview

### 📝 Project Description

**MediQueue** is a comprehensive clinic queue management and appointment booking system built with:
- **Backend**: ASP.NET Core 8 Web API
- **Database**: SQL Server with Entity Framework Core
- **Authentication**: JWT Bearer Tokens with Role-Based Authorization
- **Architecture**: Clean Architecture (Core → Repository → Service → API)

### 🎯 Key Features

- **Patient Features**:
  - Register and create profile with health information
  - Search and discover clinics by specialty/location
  - Book appointments with clinics
  - View appointment history and upcoming appointments
  - Cancel appointments
  - Rate and review clinics
  - Real-time queue position tracking

- **Clinic Features**:
  - Register with complete profile (doctor info, specialty, location)
  - Manage working hours and schedule
  - Set exceptions for holidays/closures
  - Manage appointment queue
  - Update appointment status
  - View ratings and reviews
  - Manage clinic information

- **Admin Features**:
  - System oversight dashboard
- User management
  - Seeded admin account

### 🛠️ Technology Stack

| Layer | Technology |
|-------|------------|
| **Backend** | ASP.NET Core 8 Web API |
| **ORM** | Entity Framework Core 8 |
| **Database** | SQL Server |
| **Authentication** | ASP.NET Identity + JWT Bearer |
| **Validation** | FluentValidation |
| **Mapping** | AutoMapper |
| **API Documentation** | Swagger/OpenAPI |

---

## 2. Quick Start Guide

### ⚙️ Prerequisites

- **.NET 8 SDK** or later
- **SQL Server** (LocalDB, Express, or Full)
- **Visual Studio 2022** or **VS Code**
- **Git** (optional)

### 🚀 Installation Steps

#### Step 1: Clone/Download Project
```bash
cd "D:\Programming\Graduation Projects\BIS 2026 Teams\Backend Projects\MediQueue.Backend-main\"
```

#### Step 2: Configure Connection Strings

Open `MediQueue.APIs/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MediQueueDB;Trusted_Connection=true;TrustServerCertificate=true",
    "IdentityConnection": "Server=(localdb)\\mssqllocaldb;Database=MediQueueIdentityDB;Trusted_Connection=true;TrustServerCertificate=true"
  },
  "JWT": {
    "Key": "YourSuperSecretKeyHereMinimum32Characters!",
    "ValidIssuer": "https://localhost:7101",
    "ValidAudience": "https://localhost:7101",
    "DurationInDays": "2"
  }
}
```

#### Step 3: Apply Migrations

```bash
# Navigate to API project
cd MediQueue.APIs

# Apply Identity database migrations
dotnet ef database update --context AppIdentityDbContext

# Apply business database migrations  
dotnet ef database update --context StoreContext
```

#### Step 4: Run Application

```bash
dotnet run
```

Or press **F5** in Visual Studio.

#### Step 5: Access Application

- **API**: https://localhost:7101
- **Swagger UI**: https://localhost:7101/swagger

### 🔑 Default Admin Credentials

After seeding, use these credentials:

**Admin**:
- Email: `admin@mediqueue.com`
- Password: `Admin@123`

⚠️ **Important**: Only one admin user is seeded. All clinic and patient accounts must be created manually through the frontend using the registration endpoints.

---

## 3. Architecture & Design

### 🏗️ Clean Architecture Layers

```
┌─────────────────────────────────────────┐
│      MediQueue.APIs (Web API)      │
│  - Controllers              │
│  - Middleware   │
│  - Extensions                 │
└─────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────┐
│  MediQueue.Service (Business)        │
│  - Service Implementations      │
│  - Business Logic               │
└─────────────────────────────────────────┘
         ↓
┌─────────────────────────────────────────┐
│   MediQueue.Repository (Data)      │
│  - Repository Implementations       │
│  - DbContext       │
│  - Migrations   │
└─────────────────────────────────────────┘
    ↓
┌─────────────────────────────────────────┐
│     MediQueue.Core (Domain)             │
│  - Entities         │
│  - Interfaces     │
│  - DTOs   │
│  - Enums       │
└─────────────────────────────────────────┘
```

### 📁 Project Structure

**MediQueue.Core** (Domain Layer):
- `Entities/` - Domain entities
- `DTOs/` - Data Transfer Objects
- `Enums/` - Enumerations
- `Services/` - Service interfaces
- `Repositories/` - Repository interfaces

**MediQueue.Repository** (Data Layer):
- `Data/` - DbContext and configurations
- `Identity/` - Identity DbContext
- `Repositories/` - Repository implementations
- `Migrations/` - EF Core migrations

**MediQueue.Service** (Business Layer):
- Service implementations
- Business logic
- Validation

**MediQueue.APIs** (Presentation Layer):
- `Controllers/` - API endpoints
- `Middlewares/` - Custom middleware
- `Extensions/` - Service extensions
- `Areas/Admin/` - Admin dashboard

---

## 4. Database Schema

### 🗄️ Two Database Approach

The system uses **two separate databases**:

1. **MediQueueIdentityDB** - Authentication & Users
2. **MediQueueDB** - Business data

### Identity Database (MediQueueIdentityDB)

**AppUser Table** (AspNetUsers):
```
├─ Id (string, PK)
├─ DisplayName (string, required)
├─ Email (string, required, unique)
├─ UserName (string, required, unique)
├─ PhoneNumber (string)
├─ EmailConfirmed (bool)
├─ PasswordHash (string)
├─ DateCreated (DateTime)
├─ LastLoginDate (DateTime?)
│
├─ Patient-specific fields:
│  ├─ DateOfBirth (DateTime?)
│  ├─ Gender (string?)
│  ├─ BloodType (string?)
│  ├─ EmergencyContact (string?)
│  └─ EmergencyContactPhone (string?)
│
└─ Legacy fields (for backward compatibility):
   ├─ DoctorId (int?)
   └─ HospitalId (int?)
```

**Roles Table** (AspNetRoles):
- Admin
- Clinic
- Patient

**OTP Table**:
```
├─ Id (int, PK)
├─ Email (string)
├─ Code (string, hashed)
├─ ExpirationDate (DateTime)
├─ IsUsed (bool)
├─ FailedAttempts (int)
├─ LockedUntil (DateTime?)
├─ Purpose (enum: EmailVerification | PasswordReset)
├─ ResetToken (string?)
└─ ResetTokenExpiration (DateTime?)
```

### Business Database (MediQueueDB)

**Core Entities**:

**User** (Business):
```
├─ Id (int, PK)
├─ Email (string, unique)
├─ PasswordHash (string)
├─ Role (string: Admin | Clinic | Patient)
├─ IsVerified (bool)
├─ IsActive (bool)
└─ CreatedAt (DateTime)
```

**ClinicProfile**:
```
├─ Id (int, PK)
├─ UserId (int, FK → User)
├─ DoctorName (string, required)
├─ Specialty (string, required)
├─ Description (string?)
├─ SlotDurationMinutes (int, default: 30)
├─ AverageRating (double, default: 0)
└─ TotalRatings (int, default: 0)
```

**ClinicAddress**:
```
├─ Id (int, PK)
├─ ClinicId (int, FK → ClinicProfile, unique)
├─ Country (string, required)
├─ City (string, required)
├─ Area (string, required)
├─ Street (string, required)
├─ Building (string, required)
└─ Notes (string?)
```

**ClinicPhone**:
```
├─ Id (int, PK)
├─ ClinicId (int, FK → ClinicProfile)
└─ PhoneNumber (string, required)
```

**ClinicWorkingDay**:
```
├─ Id (int, PK)
├─ ClinicId (int, FK → ClinicProfile)
├─ DayOfWeek (enum: Sunday-Saturday)
├─ IsOpen (bool)
├─ StartTime (TimeSpan?)
├─ EndTime (TimeSpan?)
└─ MaxPatients (int?)
```

**ClinicException**:
```
├─ Id (int, PK)
├─ ClinicId (int, FK → ClinicProfile)
├─ Date (DateTime)
├─ Reason (string)
└─ IsClosed (bool)
```

**Appointment**:
```
├─ Id (int, PK)
├─ ClinicId (int, FK → ClinicProfile)
├─ PatientId (int, FK → User)
├─ AppointmentDate (DateTime)
├─ SlotTime (TimeSpan)
├─ QueueNumber (int)
├─ Status (enum: Scheduled | InProgress | Completed | Cancelled | NoShow | Delayed)
├─ PatientName (string)
├─ PatientPhone (string)
├─ PatientEmail (string?)
├─ Notes (string?)
├─ BookedAt (DateTime)
├─ UpdatedAt (DateTime?)
└─ EstimatedWaitMinutes (int?)
```

**ClinicRating**:
```
├─ Id (int, PK)
├─ ClinicId (int, FK → ClinicProfile)
├─ PatientId (int, FK → User)
├─ Rating (int, 1-5)
├─ Comment (string?)
├─ CreatedAt (DateTime)
└─ UpdatedAt (DateTime?)
```

### 🔗 Entity Relationships

```
User (1) ─── (0..1) ClinicProfile
ClinicProfile (1) ─── (0..1) ClinicAddress
ClinicProfile (1) ─── (*) ClinicPhone
ClinicProfile (1) ─── (*) ClinicWorkingDay
ClinicProfile (1) ─── (*) ClinicException
ClinicProfile (1) ─── (*) Appointment
User (1) ─── (*) Appointment (as Patient)
User (1) ─── (*) ClinicRating (as Patient)
ClinicProfile (1) ─── (*) ClinicRating
```

---

## 5. Authentication & Security

### 🔐 JWT Token-Based Authentication

#### Token Structure

```json
{
  "header": {
    "alg": "HS256",
    "typ": "JWT"
  },
  "payload": {
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress": "user@example.com",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname": "John Doe",
    "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": "123",
    "http://schemas.microsoft.com/ws/2008/06/identity/claims/role": "Patient",
    "role": "Patient",
    "exp": 1703980800,
    "iss": "https://localhost:7101",
    "aud": "https://localhost:7101"
  },
  "signature": "..."
}
```

#### Token Claims

| Claim Type | Description | Example |
|------------|-------------|---------|
| `email` | User's email | `patient@example.com` |
| `givenname` | Display name | `John Doe` |
| `nameidentifier` | User ID (from Identity) | `user-guid-123` |
| `role` | User's role | `Patient`, `Clinic`, or `Admin` |
| `exp` | Expiration timestamp | Unix timestamp |
| `iss` | Issuer | `https://localhost:7101` |
| `aud` | Audience | `https://localhost:7101` |

### 🔒 Authorization Flow

```
┌─────────┐
│   Client    │
└─────────┘
       │ 1. POST /api/account/login
       │    { email, password }
       ↓
┌─────────┐
│   Server    │
│    │
│ 2. Validate credentials
│ 3. Get user roles
│ 4. Generate JWT with role claims
│ 5. Sign token with secret key
│    ↓
└─────────┘
       │ 6. Return { token, email, displayName, role }
       ↓
┌─────────┐
│   Client  │
│  │
│ 7. Store token
│ 8. Add to Authorization header
│  │
└─────────┘
     │ 9. Request with token
 │    Authorization: Bearer <token>
       ↓
┌─────────┐
│   Server    │
│         │
│ 10. Validate token signature
│ 11. Extract role from claims
│ 12. Check [Authorize(Roles = "...")] 
│ 13. ✅ Grant access or ❌ 403 Forbidden
│  │
└─────────┘
       │ 14. Response
    ↓
┌─────────┐
│   Client  │
└─────────┘
```

### 🛡️ Security Features

#### 1. **Single Role Per User**
- Each user has exactly ONE role
- No role confusion or privilege escalation
- Roles: `Admin`, `Clinic`, or `Patient`

#### 2. **Email Verification**
- OTP-based email verification
- 6-digit code valid for 10 minutes
- Rate limiting (60-second cooldown)
- Lockout after 5 failed attempts (30 minutes)

#### 3. **Password Security**
- Minimum 6 characters
- Must contain at least one digit
- Hashed using ASP.NET Identity (PBKDF2)
- Password reset via OTP

#### 4. **Token Security**
- Cryptographically signed (HMAC SHA256)
- 2-day expiration (configurable)
- Cannot be forged without secret key
- Validated on every request

#### 5. **Role-Based Authorization**
- Controller level: `[Authorize]` - Any authenticated user
- Action level: `[Authorize(Roles = "Patient")]` - Specific role
- Server-side validation (client cannot bypass)

### 📦 UserDTO Structure

```csharp
public class UserDTO
{
    public required string DisplayName { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
    public required string Role { get; set; } // Single role only
}
```

**All fields are required and non-nullable.**

---

## 6. Role-Based Registration

### 👥 Registration Types

The system supports **three registration types**:

1. **Patient Registration** (`/api/account/register/patient`)
2. **Clinic Registration** (`/api/account/register/clinic`)
3. **Legacy Registration** (`/api/account/register`) - Backward compatible

### 🏥 Patient Registration

**Endpoint**: `POST /api/account/register/patient`

**Request Body**:
```json
{
  "email": "patient@example.com",
  "displayName": "John Doe",
  "phoneNumber": "+201234567890",
  "password": "SecureP@ss123",
  "dateOfBirth": "1990-05-15",
  "gender": "Male",
  "bloodType": "A+",
  "emergencyContact": "Jane Doe",
  "emergencyContactPhone": "+209876543210"
}
```

**Required Fields**:
- `email` (valid email format)
- `displayName` (min 3 characters)
- `phoneNumber` (valid phone format)
- `password` (6-100 characters)

**Optional Patient Fields**:
- `dateOfBirth` (DateTime)
- `gender` (Male | Female | Other)
- `bloodType` (A+ | A- | B+ | B- | AB+ | AB- | O+ | O-)
- `emergencyContact` (string)
- `emergencyContactPhone` (string)

**What Happens**:
1. Creates `AppUser` in Identity database with patient fields
2. Assigns "Patient" role
3. Creates `User` entity in business database
4. Sends email verification OTP
5. Returns success message

**Response**:
```json
{
  "statusCode": 200,
  "message": "Patient registration successful. Please check your email to verify your account."
}
```

### 🏥 Clinic Registration

**Endpoint**: `POST /api/account/register/clinic`

**Request Body**:
```json
{
  "email": "dr.ahmed@clinic.com",
  "displayName": "Ahmed Hassan",
  "phoneNumber": "+201234567890",
  "password": "SecureP@ss123",
  "doctorName": "Ahmed Hassan",
  "specialty": "Cardiology",
  "description": "Experienced cardiologist",
  "slotDurationMinutes": 30,
  "country": "Egypt",
  "city": "Cairo",
  "area": "Nasr City",
  "street": "Abbas El Akkad Street",
  "building": "Building 25",
  "addressNotes": "Next to City Stars Mall",
  "additionalPhones": ["+201987654321"]
}
```

**Required Fields**:
- `email`, `displayName`, `phoneNumber`, `password`
- `doctorName` (min 3 characters)
- `specialty` (min 3 characters)
- `country`, `city`, `area`, `street`, `building`

**Optional Clinic Fields**:
- `description`
- `slotDurationMinutes` (5-120, default: 30)
- `addressNotes`
- `additionalPhones` (array of phone numbers)

**What Happens**:
1. Creates `AppUser` in Identity database
2. Assigns "Clinic" role
3. Creates `User` entity in business database
4. **Creates `ClinicProfile`** with doctor details
5. **Creates `ClinicAddress`** with location
6. **Creates `ClinicPhone`** entries (if additional phones provided)
7. Sends email verification OTP
8. Returns success message

**Clinic is fully set up and ready to accept appointments!**

**Response**:
```json
{
  "statusCode": 200,
  "message": "Clinic registration successful. Please check your email to verify your account."
}
```

### 🔄 Registration Flow Comparison

**Patient Flow**:
```
Register → Verify Email → ✅ Start booking appointments
```

**Clinic Flow**:
```
Register → Verify Email → Set working hours → ✅ Start accepting appointments
```

### ✅ Validation Rules

**Email**:
- Valid email format
- Unique (not already registered)

**Password**:
- 6-100 characters
- At least one digit

**Phone**:
- Valid phone format
- International format recommended (+country code)

**Slot Duration** (Clinic only):
- Range: 5-120 minutes
- Default: 30 minutes

---

## 7. API Endpoints Reference

### 🌐 Base URL

- **Development**: `https://localhost:7101/api`
- **Swagger UI**: `https://localhost:7101/swagger`

### 🔑 Authentication Endpoints

#### POST `/account/register/patient`
Register a new patient account.

**Authorization**: None (Public)  
**Request Body**: `RegisterPatientDTO`  
**Response**: Success message

#### POST `/account/register/clinic`
Register a new clinic account.

**Authorization**: None (Public)  
**Request Body**: `RegisterClinicDTO`  
**Response**: Success message

#### POST `/account/verify-email`
Verify email with OTP code.

**Authorization**: None (Public)  
**Request Body**:
```json
{
  "email": "user@example.com",
  "otpCode": "123456"
}
```
**Response**: `UserDTO` with token

#### POST `/account/login`
Login to get JWT token.

**Authorization**: None (Public)  
**Request Body**:
```json
{
  "email": "user@example.com",
  "password": "password123"
}
```
**Response**: `UserDTO` with token

#### GET `/account/GetCurrentUser`
Get current authenticated user information.

**Authorization**: `Bearer <token>` (Any authenticated user)  
**Response**: `UserDTO`

#### POST `/account/logout`
Logout (server-side session cleanup).

**Authorization**: `Bearer <token>` (Any authenticated user)  
**Response**: Success message

### 📅 Appointment Endpoints

#### POST `/appointments/book`
Book a new appointment (Patient only).

**Authorization**: `Bearer <token>` (Role: Patient)  
**Request Body**:
```json
{
  "clinicId": 1,
  "appointmentDate": "2024-12-20",
  "slotTime": "10:00:00",
  "patientName": "John Doe",
  "patientPhone": "+201234567890",
  "patientEmail": "john@example.com",
  "notes": "First visit"
}
```
**Response**: `AppointmentDto`

#### GET `/appointments/patient/upcoming`
Get patient's upcoming appointments (Patient only).

**Authorization**: `Bearer <token>` (Role: Patient)  
**Response**: `List<AppointmentDto>`

#### GET `/appointments/patient/history`
Get patient's appointment history (Patient only).

**Authorization**: `Bearer <token>` (Role: Patient)  
**Response**: `PatientAppointmentHistoryDto`

#### POST `/appointments/{id}/cancel`
Cancel an appointment (Patient only).

**Authorization**: `Bearer <token>` (Role: Patient)  
**Response**: Success message

#### GET `/appointments/clinic/queue`
Get clinic's appointment queue for a date (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Query Params**: `date=2024-12-20`  
**Response**: `ClinicQueueDto`

#### POST `/appointments/{id}/start`
Mark appointment as in progress (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Response**: `AppointmentDto`

#### POST `/appointments/{id}/complete`
Mark appointment as completed (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Response**: `AppointmentDto`

### 🏥 Clinic Endpoints

#### GET `/clinics/{id}`
Get clinic profile by ID (Public).

**Authorization**: None (Public)  
**Response**: `ClinicProfileDto`

#### GET `/clinics/search`
Search clinics by filters (Public).

**Authorization**: None (Public)  
**Query Params**:
- `specialty` (optional)
- `city` (optional)
- `minRating` (optional)

**Response**: `List<ClinicProfileDto>`

#### GET `/clinics/my-profile`
Get current clinic's profile (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Response**: `ClinicProfileDto`

#### PUT `/clinics/{id}`
Update clinic profile (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Request Body**: `UpdateClinicProfileDto`  
**Response**: `ClinicProfileDto`

#### POST `/clinics/{id}/address`
Create or update clinic address (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Request Body**: `CreateClinicAddressDto`  
**Response**: `ClinicAddressDto`

### ⭐ Rating Endpoints

#### POST `/ratings`
Submit a rating for a clinic (Patient only).

**Authorization**: `Bearer <token>` (Role: Patient)  
**Request Body**:
```json
{
  "clinicId": 1,
  "rating": 5,
  "comment": "Excellent service!"
}
```
**Response**: `ClinicRatingDto`

#### GET `/ratings/clinic/{clinicId}`
Get all ratings for a clinic (Public).

**Authorization**: None (Public)  
**Query Params**:
- `pageNumber` (default: 1)
- `pageSize` (default: 10)

**Response**: `ClinicRatingSummaryDto`

#### GET `/ratings/clinic/{clinicId}/my-rating`
Get patient's rating for a clinic (Patient only).

**Authorization**: `Bearer <token>` (Role: Patient)  
**Response**: `ClinicRatingDto`

### 📆 Working Schedule Endpoints

#### GET `/workingschedule/working-days`
Get all working days for clinic (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Response**: `List<ClinicWorkingDayDto>`

#### PUT `/workingschedule/working-days`
Bulk update working days (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Request Body**: `BulkUpdateWorkingDaysDto`  
**Response**: `List<ClinicWorkingDayDto>`

#### POST `/workingschedule/exceptions`
Add exception date (holiday/closure) (Clinic only).

**Authorization**: `Bearer <token>` (Role: Clinic)  
**Request Body**:
```json
{
  "date": "2024-12-25",
  "reason": "Christmas Holiday",
  "isClosed": true
}
```
**Response**: `ClinicExceptionDto`

#### GET `/workingschedule/{clinicId}/available`
Check if clinic is available on a date (Public).

**Authorization**: None (Public)  
**Query Params**: `date=2024-12-20`  
**Response**: `{ clinicId, date, isAvailable }`

---

## 8. Security Audit

### 🔍 Complete Endpoint Security Analysis

#### ✅ Security Score: ⭐⭐⭐⭐⭐ (5/5)

**Total Endpoints**: 63  
**Protected Endpoints**: 42  
**Public Endpoints**: 21 (Intentionally public)

### 🛡️ How Security Works

1. **JWT Token Validation**:
   ```
   Request → JWT Middleware → Validate Signature → Extract Claims → [Authorize] Check → ✅ or ❌
   ```

2. **Role is Read from JWT, NOT from**:
   - ❌ Request body
   - ❌ localStorage (client storage)
   - ❌ URL parameters
   - ✅ JWT claims (server-validated)

3. **User Cannot Fake Role**:
   - JWT is cryptographically signed
   - Changing localStorage doesn't change JWT
   - Server validates signature before trusting claims
   - Invalid signature = 401 Unauthorized

### 🚫 Attack Prevention

#### Attack 1: Modify Role in Browser
```javascript
// Attacker tries:
localStorage.setItem('user', JSON.stringify({ role: 'Admin' }));
```
**Result**: ❌ **BLOCKED** - Server reads role from JWT, not localStorage

#### Attack 2: Forge JWT Token
```
Authorization: Bearer fake.token.with.admin.role
```
**Result**: ❌ **BLOCKED** - Signature validation fails

#### Attack 3: Patient Accesses Clinic Endpoint
```
POST /api/appointments/clinic/queue
Authorization: Bearer <patient-token>
```
**Result**: ❌ **BLOCKED** - `[Authorize(Roles = "Clinic")]` returns 403 Forbidden

#### Attack 4: Expired Token
```
Authorization: Bearer <expired-token>
```
**Result**: ❌ **BLOCKED** - Token expiration validation fails

#### Attack 5: No Token
```
GET /api/appointments/patient/history
(No Authorization header)
```
**Result**: ❌ **BLOCKED** - `[Authorize]` returns 401 Unauthorized

### 📊 Endpoint Security Breakdown

| Controller | Protected | Public | Security Status |
|------------|-----------|--------|-----------------|
| AccountController | 9 | 12 | ✅ Correct |
| AppointmentsController | 13 | 0 | ✅ Perfect |
| ClinicsController | 7 | 6 | ✅ Correct |
| RatingsController | 5 | 1 | ✅ Correct |
| WorkingScheduleController | 8 | 2 | ✅ Correct |
| **TOTAL** | **42** | **21** | **✅ SECURE** |

### ✅ Security Best Practices Implemented

- [x] JWT with signed claims
- [x] Role-based authorization
- [x] HTTPS enforced
- [x] Email verification required
- [x] OTP with lockout mechanism
- [x] Password hashing (PBKDF2)
- [x] Token expiration (2 days)
- [x] Single role per user
- [x] Server-side validation only
- [x] No client-side security trust

---

## 9. Database Migrations

### 🔧 Migration Strategy

The system uses **two separate databases**, each requiring separate migrations:

1. **Identity Database** (`AppIdentityDbContext`)
2. **Business Database** (`StoreContext`)

### 🆕 Create New Migration

#### For Identity Database:
```bash
dotnet ef migrations add AddPatientFieldsToAppUser \
  --project MediQueue.Repository \
  --startup-project MediQueue.APIs \
  --context AppIdentityDbContext \
  --output-dir Identity/Migrations
```

#### For Business Database:
```bash
dotnet ef migrations add AddNewFeature \
  --project MediQueue.Repository \
  --startup-project MediQueue.APIs \
  --context StoreContext \
  --output-dir Data/Migrations
```

### ✅ Apply Migrations

#### Apply All Migrations:
```bash
# Identity database
dotnet ef database update --context AppIdentityDbContext

# Business database
dotnet ef database update --context StoreContext
```

#### Apply to Specific Migration:
```bash
dotnet ef database update MigrationName --context AppIdentityDbContext
```

### ⏪ Rollback Migration

```bash
# Rollback to previous migration
dotnet ef database update PreviousMigrationName --context AppIdentityDbContext

# Remove last migration (if not applied)
dotnet ef migrations remove --context AppIdentityDbContext
```

### 🔄 Reset Database

#### Drop and Recreate:
```bash
# Drop database
dotnet ef database drop --context AppIdentityDbContext --force

# Apply all migrations
dotnet ef database update --context AppIdentityDbContext
```

#### SQL Script:
```sql
USE master;
GO

-- Drop databases
DROP DATABASE IF EXISTS MediQueueIdentityDB;
DROP DATABASE IF EXISTS MediQueueDB;
GO
```

Then run the application - migrations will be applied automatically.

### 📋 Current Migrations

**Identity Database**:
- Initial Identity setup (AspNetUsers, AspNetRoles, etc.)
- Add Otp table
- Add patient-specific fields (DateOfBirth, Gender, BloodType, etc.)

**Business Database**:
- Initial schema (User, ClinicProfile, Appointment, etc.)
- Add clinic-related tables (Address, Phone, WorkingDay, Exception)
- Add rating system

---

## 10. Deployment Guide

### 🚀 Production Deployment Checklist

#### 1. Update Configuration

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=production-server;Database=MediQueueDB;User Id=sa;Password=xxx;TrustServerCertificate=true",
    "IdentityConnection": "Server=production-server;Database=MediQueueIdentityDB;User Id=sa;Password=xxx;TrustServerCertificate=true"
  },
  "JWT": {
    "Key": "CHANGE-THIS-TO-A-STRONG-SECRET-KEY-AT-LEAST-32-CHARACTERS",
    "ValidIssuer": "https://your-domain.com",
    "ValidAudience": "https://your-domain.com",
    "DurationInDays": "2"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft": "Warning"
    }
  }
}
```

#### 2. Security Checklist

- [ ] Change JWT secret key (strong, 32+ characters)
- [ ] Update connection strings with production credentials
- [ ] Enable HTTPS enforcement
- [ ] Update CORS origins
- [ ] Change default admin password
- [ ] Remove or secure Swagger UI
- [ ] Enable logging and monitoring
- [ ] Set up error handling
- [ ] Configure rate limiting
- [ ] Set up backup strategy

#### 3. Database Setup

```bash
# Create production databases
CREATE DATABASE MediQueueDB;
CREATE DATABASE MediQueueIdentityDB;

# Apply migrations
dotnet ef database update --context AppIdentityDbContext --connection "production-connection-string"
dotnet ef database update --context StoreContext --connection "production-connection-string"
```

#### 4. Build & Publish

```bash
# Build for production
dotnet build --configuration Release

# Publish
dotnet publish --configuration Release --output ./publish
```

#### 5. Deploy to Server

Options:
- **IIS** (Windows Server)
- **Azure App Service**
- **Docker Container**
- **Linux + Nginx + Kestrel**

#### 6. Post-Deployment

- Test all endpoints
- Verify database connections
- Check logging
- Test authentication flow
- Verify CORS settings
- Monitor performance
- Set up health checks

### 🐳 Docker Deployment (Optional)

**Dockerfile**:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["MediQueue.APIs/MediQueue.APIs.csproj", "MediQueue.APIs/"]
COPY ["MediQueue.Service/MediQueue.Service.csproj", "MediQueue.Service/"]
COPY ["MediQueue.Repository/MediQueue.Repository.csproj", "MediQueue.Repository/"]
COPY ["MediQueue.Core/MediQueue.Core.csproj", "MediQueue.Core/"]
RUN dotnet restore "MediQueue.APIs/MediQueue.APIs.csproj"
COPY . .
WORKDIR "/src/MediQueue.APIs"
RUN dotnet build "MediQueue.APIs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MediQueue.APIs.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MediQueue.APIs.dll"]
```

**docker-compose.yml**:
```yaml
version: '3.8'
services:
  api:
    build: .
  ports:
  - "8080:80"
environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Server=db;Database=MediQueueDB;User Id=sa;Password=YourStrong@Password
      - ConnectionStrings__IdentityConnection=Server=db;Database=MediQueueIdentityDB;User Id=sa;Password=YourStrong@Password
    depends_on:
      - db
  
  db:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
- ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong@Password
    ports:
      - "1433:1433"
    volumes:
      - sqldata:/var/opt/mssql

volumes:
  sqldata:
```

---

## 11. Troubleshooting

### ❓ Common Issues & Solutions

#### Issue: Database Connection Failed

**Error**: `Unable to connect to database`

**Solutions**:
1. Check connection string in `appsettings.json`
2. Ensure SQL Server is running
3. Verify database names are correct
4. Check firewall settings
5. Test connection:
   ```bash
   dotnet ef database update --context AppIdentityDbContext --verbose
   ```

#### Issue: Migration Fails

**Error**: `The migration 'XXX' has already been applied`

**Solution**:
```bash
# Check applied migrations
dotnet ef migrations list --context AppIdentityDbContext

# Rollback to previous
dotnet ef database update PreviousMigration --context AppIdentityDbContext

# Remove unapplied migration
dotnet ef migrations remove --context AppIdentityDbContext
```

#### Issue: 401 Unauthorized

**Error**: API returns 401 even with valid credentials

**Solutions**:
1. Check JWT configuration in `appsettings.json`
2. Verify token is included in Authorization header: `Bearer <token>`
3. Check token expiration (2 days default)
4. Ensure email is verified (`EmailConfirmed = true`)
5. Test token generation:
   ```bash
   POST /api/account/login
   ```

#### Issue: 403 Forbidden

**Error**: API returns 403 when accessing endpoint

**Solutions**:
1. Check user role matches endpoint requirement
2. Verify `[Authorize(Roles = "...")]` attribute
3. Decode JWT to verify role claim:
   - Visit https://jwt.io
   - Paste token
   - Check `role` claim

4. Example roles:
   - Patient endpoints: Need `"role": "Patient"`
   - Clinic endpoints: Need `"role": "Clinic"`
   - Admin endpoints: Need `"role": "Admin"`

#### Issue: OTP Not Received

**Error**: Email with OTP code not arriving

**Solutions**:
1. Check email configuration in `appsettings.json`
2. Verify email service is configured correctly
3. Check spam/junk folder
4. Verify OTP in database:
   ```sql
   SELECT * FROM Otps WHERE Email = 'user@example.com' ORDER BY ExpirationDate DESC
 ```
5. Check for rate limiting (60-second cooldown)

#### Issue: Hot Reload Warning

**Error**: `ENC0046: Updating a complex statement...`

**Solution**: This is just a warning, not an error. Stop and restart the application to apply changes.

#### Issue: Seed Data Not Loading

**Error**: Database is empty after migration

**Solutions**:
1. Check `Program.cs` has seeding code
2. Verify seed methods are called on startup
3. Manually trigger seeding:
   ```csharp
   await AppIdentityDbContextSeed.SeedUsersAsync(userManager, roleManager);
   ```
4. Check for errors in seed data (duplicate emails, etc.)

#### Issue: CORS Error in Frontend

**Error**: `Access to XMLHttpRequest blocked by CORS policy`

**Solutions**:
1. Add frontend URL to CORS policy in `Program.cs`:
   ```csharp
   options.AddPolicy("CorsPolicy", policy =>
   {
       policy.AllowAnyHeader()
           .AllowAnyMethod()
         .WithOrigins("http://localhost:4200"); // Add your frontend URL
   });
   ```

2. Ensure `app.UseCors("CorsPolicy");` is called before `app.UseAuthorization();`

#### Issue: Build Errors

**Error**: Project won't compile

**Solutions**:
1. Restore NuGet packages:
   ```bash
dotnet restore
   ```

2. Clean and rebuild:
 ```bash
   dotnet clean
   dotnet build
   ```

3. Check for missing dependencies
4. Verify .NET 8 SDK is installed:
   ```bash
   dotnet --version
   ```

### 📞 Getting Help

If you encounter issues not listed here:

1. Check error logs in:
   - Console output
   - `bin/Debug/net8.0/logs/`
   - Application Insights (if configured)

2. Enable detailed logging in `appsettings.Development.json`:
   ```json
   {
     "Logging": {
   "LogLevel": {
      "Default": "Debug",
     "Microsoft.EntityFrameworkCore": "Information"
       }
     }
   }
   ```

3. Check Swagger UI for API documentation and testing:
   - https://localhost:7101/swagger

4. Verify database state:
   ```sql
   -- Check user roles
   SELECT u.Email, r.Name AS Role
   FROM AspNetUsers u
   JOIN AspNetUserRoles ur ON u.Id = ur.UserId
   JOIN AspNetRoles r ON ur.RoleId = r.Id;
   
   -- Check OTPs
   SELECT * FROM Otps WHERE Email = 'user@example.com';
   
   -- Check appointments
   SELECT * FROM Appointments WHERE PatientId = 1;
   ```

---

## 📚 Additional Resources

### 📖 Related Documentation

- **ASP.NET Core 8**: https://docs.microsoft.com/aspnet/core
- **Entity Framework Core**: https://docs.microsoft.com/ef/core
- **ASP.NET Identity**: https://docs.microsoft.com/aspnet/core/security/authentication/identity
- **JWT Authentication**: https://jwt.io

### 🔧 External Tools

- **Postman**: API testing and documentation
- **Swagger/OpenAPI**: API documentation (built-in)
- **SQL Server Management Studio**: Database management
- **Azure Data Studio**: Cross-platform database tool

### 🎲 Seeded Data

After running migrations, the system includes minimal seed data:

**Roles**:
- Admin
- Clinic
- Patient

**Identity Users** (in MediQueueIdentityDB):
- **1 Admin user** - System Administrator

**Default Admin Credentials**:
- Email: `admin@mediqueue.com`
- Password: `Admin@123`

⚠️ **Important Notes**:
- ✅ **Only one admin user is seeded**
- ❌ **No clinic or patient users are seeded**
- 📝 **All clinic and patient accounts must be created manually through the frontend** using the registration endpoints:
  - `/api/account/register/patient`
  - `/api/account/register/clinic`

🔐 **Change the admin password in production!**

---

## 📝 Changelog

### Version 1.0 (December 2024)

**Initial Release**:
- Complete JWT authentication system
- Role-based authorization (Admin, Clinic, Patient)
- Patient and Clinic registration endpoints
- Appointment booking and queue management
- Clinic profile management
- Rating and review system
- Working schedule and exception handling
- OTP-based email verification
- Two-database architecture
- Comprehensive API documentation
- Security audit and best practices

**Seeding Changes**:
- Removed all seeded clinic users
- Removed all seeded patient users
- Kept only one admin user for system administration
- All user accounts now created through frontend registration
