# MediQueue System - Technical Documentation

## System Overview

MediQueue is a clinic queue management and appointment booking system that connects patients with clinics. The system uses a two-database architecture with JWT-based authentication and role-based access control.

---

## System Architecture

### Multi-Layer Architecture

**Presentation Layer (APIs)**
- Handles HTTP requests/responses
- JWT token validation
- Role-based authorization enforcement
- Error handling and response formatting

**Business Layer (Service)**
- Core business logic implementation
- Validation rules enforcement
- Cross-entity operations coordination
- Transaction management

**Data Access Layer (Repository)**
- Database operations
- Entity Framework Core implementations
- Specification pattern for queries
- Unit of Work pattern for transactions

**Domain Layer (Core)**
- Entity definitions
- Business interfaces
- Data Transfer Objects (DTOs)
- Enums and constants

### Two-Database Strategy

**Identity Database (MediQueueIdentityDB)**
- Manages user authentication
- Stores user credentials (hashed passwords)
- Handles ASP.NET Identity framework
- Manages OTP verification codes
- Stores user roles and claims

**Business Database (MediQueueDB)**
- Stores clinic profiles and information
- Manages appointments and queues
- Handles ratings and reviews
- Maintains working schedules
- Tracks clinic addresses and phones

**Why Two Databases?**
- Separation of concerns: authentication vs business data
- Security isolation: credential storage separate from business operations
- Independent scaling: authentication database can scale differently
- Compliance: sensitive data isolated for regulatory requirements

---

## User Roles and Capabilities

### Admin Role
**Purpose**: System administration and oversight

**Capabilities**:
- Access system dashboard
- View all users and clinics
- Monitor system health
- Manage system-wide settings

**Restrictions**:
- Cannot book appointments
- Cannot act as patient or clinic
- Single admin account (seeded)

### Clinic Role
**Purpose**: Healthcare providers managing their practice

**Capabilities**:
- Create and manage clinic profile
- Set doctor information (name, specialty)
- Configure working hours (days and times)
- Add holiday/closure exceptions
- View appointment queue
- Update appointment status (InProgress, Completed, Delayed)
- See patient appointment details
- View received ratings and reviews

**Restrictions**:
- Cannot book appointments as patient
- Cannot modify other clinics' data
- Cannot cancel patient appointments (only patients can)
- Must verify email before full access

### Patient Role
**Purpose**: End users seeking medical appointments

**Capabilities**:
- Search and browse clinics
- Filter by specialty, location, rating
- **View clinic working hours and schedules** (public access)
- **Check clinic availability for specific dates** (public access)
- **See upcoming clinic closures/holidays** (public access)
- Book appointments at available clinics
- View upcoming appointments
- Check queue position
- Cancel own appointments
- View appointment history
- Rate and review clinics after visit
- Update personal profile

**Restrictions**:
- Cannot create clinic profiles
- Cannot manage clinic schedules
- Cannot see other patients' appointments
- Must verify email to book appointments

---

## Core System Flows

### Registration and Onboarding

**Patient Registration Flow**:
1. User submits registration form with email, password, display name, phone
2. Optional: adds health information (date of birth, gender, blood type, emergency contact)
3. System creates Identity account (AppUser)
4. System assigns "Patient" role
5. System generates 6-digit OTP code
6. System hashes OTP and stores with 10-minute expiration
7. System sends verification email with OTP
8. Account created but email unverified (limited access)

**Clinic Registration Flow**:
1. Clinic submits registration with email, password, doctor details
2. Required: doctor name, specialty, phone
3. Required: full address (country, city, area, street, building)
4. Optional: description, slot duration, additional phones
5. System creates Identity account (AppUser)
6. System assigns "Clinic" role
7. System creates ClinicProfile with doctor information
8. System creates ClinicAddress with location details
9. System creates ClinicPhone entries
10. System initializes 7 WorkingDay records (all closed by default)
11. System generates and sends OTP
12. Clinic fully configured and ready to accept appointments after verification

**Email Verification Flow**:
1. User receives email with 6-digit OTP
2. User submits email and OTP code
3. System retrieves OTP record from database
4. System checks if OTP is expired (>10 minutes)
5. System checks if OTP is locked (>5 failed attempts)
6. System hashes submitted OTP
7. System compares hashed OTP with stored hash
8. If match: marks email as verified, issues JWT token
9. If no match: increments failed attempts, locks after 5 failures
10. User can resend OTP (60-second cooldown between requests)

### Authentication and Authorization

**Login Flow**:
1. User submits email and password
2. System validates credentials against Identity database
3. System checks if email is verified
4. System retrieves user's role from Identity
5. System generates JWT token containing:
   - User ID (from Identity)
   - Email address
   - Display name
   - Role (Admin/Clinic/Patient)
   - Token expiration (2 days default)
6. System signs token with secret key
7. System returns token to client
8. Client stores token (typically in localStorage or memory)

**Request Authorization Flow**:
1. Client sends request with Authorization header: "Bearer <token>"
2. JWT middleware intercepts request
3. Middleware validates token signature
4. Middleware checks token expiration
5. Middleware extracts claims (user ID, email, role)
6. Controller checks [Authorize] attribute
7. If role-specific: checks [Authorize(Roles = "Patient")]
8. If role matches: request proceeds
9. If role doesn't match: returns 403 Forbidden
10. If token invalid: returns 401 Unauthorized

**Security Principles**:
- Role stored in JWT claims (server-validated)
- Client cannot modify role without invalidating signature
- Token cryptographically signed (HMAC SHA256)
- Password hashed using PBKDF2 algorithm
- OTP hashed using SHA256
- Rate limiting on OTP requests (60-second cooldown)
- Account lockout after 5 failed OTP attempts (30 minutes)

### Appointment Booking

**Patient Booking Process**:
1. Patient searches for clinics by specialty/location/rating
2. Patient selects clinic and views profile
3. **Patient views clinic working schedule** (new: GET /workingschedule/{id}/schedule)
4. Patient sees operating days, hours, and upcoming closures
5. Patient chooses appointment date
6. System validates date is in future
7. System checks if clinic is open on that day
8. System checks working day schedule
9. System checks for exceptions (holidays/closures)
10. System checks daily capacity against MaxPatients setting
11. System counts existing appointments for that date
12. If capacity available: proceeds to booking
13. System uses database lock to get next queue number
14. System creates appointment with Booked status
15. System returns appointment with queue number
16. Patient receives confirmation

**Queue Number Assignment**:
- Each date has independent queue numbering (starts at 1)
- Database row-level locking prevents duplicate numbers
- Sequential numbering: first booking = #1, second = #2, etc.
- Queue number never changes after assignment
- Cancelled appointments don't affect queue numbers

**Capacity Management**:
- Clinic sets MaxPatients per working day
- System enforces hard limit (no overbooking)
- Formula: if current_count >= max_patients, reject booking
- Real-time capacity checking at booking time

### Queue Management

**Clinic Queue View**:
1. Clinic accesses queue for specific date
2. System retrieves all appointments for clinic and date
3. System orders by queue number (ascending)
4. System includes appointment status
5. System shows patient information
6. Clinic sees complete queue state

**Appointment Status Progression**:
- **Booked**: Initial state after patient books
- **InProgress**: Clinic marks when patient enters consultation
- **Delayed**: Clinic marks if appointment running late
- **Completed**: Clinic marks after consultation finished
- **Canceled**: Patient cancels before appointment

**Status Update Rules**:
- Only clinic can mark InProgress, Completed, Delayed
- Only patient can mark Canceled
- Cannot cancel completed appointments
- Cannot complete canceled appointments

### Clinic Working Schedule

**Working Days Configuration**:
1. System creates 7 WorkingDay records on clinic registration
2. Each record represents one day (Sunday through Saturday)
3. Default: all days closed (IsOpen = false)
4. Clinic updates bulk working days:
   - Sets IsOpen = true for operating days
   - Sets StartTime (e.g., 09:00:00)
   - Sets EndTime (e.g., 17:00:00)
   - Sets MaxPatients (daily capacity)
5. System validates EndTime > StartTime
6. System saves configuration

**Exception Dates (Holidays/Closures)**:
1. Clinic adds exception for specific date
2. Provides reason (e.g., "National Holiday", "Doctor Vacation")
3. Marks IsClosed = true
4. System prevents bookings on exception dates
5. Existing appointments on exception dates should be handled manually

**Availability Checking Algorithm**:
```
For given date:
1. Get day of week (e.g., Monday)
2. Find WorkingDay record for that day
3. If IsOpen = false → NOT AVAILABLE
4. Check if date has exception record
5. If exception exists and IsClosed = true → NOT AVAILABLE
6. Otherwise → AVAILABLE
```

### Rating and Review System

**Submission Flow**:
1. Patient selects clinic to rate
2. Patient assigns rating (1-5 stars)
3. Patient optionally adds text comment
4. System validates patient hasn't already rated this clinic
5. System validates patient has completed appointment (optional business rule)
6. System creates ClinicRating record
7. System recalculates clinic's average rating
8. System updates clinic's TotalRatings count

**Rating Calculation**:
- AverageRating = SUM(all ratings) / COUNT(ratings)
- Stored as decimal value (e.g., 4.3)
- Rounded to one decimal place
- Updated on every new rating
- Displayed in clinic search results

**Rating Update Rules**:
- One rating per patient per clinic
- Patient can update existing rating
- Update replaces previous rating
- Average rating recalculated on update
- Patient can delete rating (decrements total)

---

## Business Rules and Validations

### Registration Validations
- Email must be valid format and unique
- Password minimum 6 characters, must contain digit
- Phone number must be valid format
- Display name minimum 3 characters
- Clinic: doctor name and specialty minimum 3 characters
- Clinic: slot duration range 5-120 minutes
- Patient: blood type must be valid value if provided

### Appointment Validations
- Appointment date must be in future
- Clinic must be available (working day + no exceptions)
- Clinic capacity not exceeded
- Patient cannot double-book same clinic/date
- Patient can only cancel own appointments
- Clinic can only manage own appointments
- Cannot cancel completed appointments

### Working Schedule Validations
- End time must be after start time
- MaxPatients must be positive integer
- Cannot delete working days (can only mark closed)
- Exception dates cannot be in past

### Rating Validations
- Rating value must be 1-5
- Comment length limit (if implemented)
- Patient cannot rate clinic multiple times
- Cannot rate without valid clinic ID

---

## Error Handling Strategy

### HTTP Status Codes
- **200 OK**: Successful request with data
- **201 Created**: Resource successfully created
- **400 Bad Request**: Validation error or business rule violation
- **401 Unauthorized**: Missing or invalid token
- **403 Forbidden**: Valid token but insufficient permissions
- **404 Not Found**: Resource doesn't exist
- **500 Internal Server Error**: Unhandled exception

### Error Response Format
```json
{
  "statusCode": 400,
  "message": "Detailed error message",
  "details": "Additional context (optional)"
}
```

### Common Error Scenarios
- Duplicate email registration → 400 "Email already in use"
- Invalid credentials → 400 "Invalid email or password"
- Expired OTP → 400 "OTP has expired"
- Wrong role access → 403 "Insufficient permissions"
- Booking past date → 400 "Cannot book in the past"
- Capacity exceeded → 400 "Clinic capacity is full"
- Clinic not available → 400 "Clinic not available on this date"

---

## Data Integrity and Transactions

### Transaction Boundaries
- Registration: Single transaction across Identity + Business databases
- Appointment booking: Single transaction (clinic validation + appointment creation)
- Rating submission: Single transaction (create rating + update clinic stats)
- Working schedule update: Single transaction (update all days)

### Concurrency Handling
- **Appointment Queue Numbers**: Row-level locking prevents duplicates
- **Capacity Checking**: Atomic read and increment
- **Rating Updates**: Optimistic concurrency using version fields
- **OTP Validation**: Atomic increment of failed attempts

### Data Consistency Rules
- Cascade delete: Deleting clinic removes appointments, ratings, schedule
- Soft delete: Users marked inactive rather than deleted
- Referential integrity: Foreign keys enforced at database level
- Unique constraints: Email unique in Identity database

---

## Performance Considerations

### Database Indexing Strategy
- Index on AppUser.Email (frequent login lookups)
- Index on Appointment.ClinicId + AppointmentDate (queue queries)
- Index on ClinicProfile.AppUserId (user-to-clinic lookups)
- Index on OTP.Email + ExpirationDate (verification lookups)
- Composite index on ClinicRating (ClinicId, PatientId) for uniqueness

### Query Optimization
- Eager loading for related entities (Include/ThenInclude)
- Pagination for large result sets (ratings, appointments)
- Projection for list views (select only needed fields)
- Specification pattern for reusable queries

### Caching Opportunities
- Clinic profiles (rarely change)
- Working schedules (static configuration)
- Rating averages (update on write)
- OTP validation (cache recent attempts for rate limiting)

---

## Security Measures

### Authentication Security
- Passwords hashed with PBKDF2 (ASP.NET Identity default)
- OTPs hashed with SHA256 before storage
- JWT tokens signed with HMAC SHA256
- Token expiration enforced (2 days)
- Refresh tokens not implemented (re-login required)

### Authorization Security
- Role-based access control (RBAC)
- Single role per user (no role escalation)
- Server-side role validation
- Attribute-based authorization on controllers

### API Security
- HTTPS enforced in production
- CORS policy restricts allowed origins
- Rate limiting on OTP endpoints
- Input validation on all endpoints
- SQL injection prevented by EF Core parameterization

### Data Security
- Sensitive data in separate database
- Email verification required for full access
- Account lockout after failed attempts
- No sensitive data in logs or error messages

---

## Monitoring and Observability

### Logging Strategy
- Structured logging using ILogger
- Log levels: Debug, Information, Warning, Error, Critical
- Exception middleware captures unhandled errors
- Authentication failures logged
- Business operation outcomes logged

### Key Metrics to Monitor
- Failed login attempts per user
- OTP verification success rate
- Appointment booking success rate
- Average queue wait times
- API response times
- Database query performance
- Error rate by endpoint

---

## Deployment Configuration

### Environment-Specific Settings
**Development**:
- LocalDB connections
- Detailed error messages
- Swagger UI enabled
- CORS allows localhost

**Production**:
- Azure SQL or hosted SQL Server
- Generic error messages
- Swagger UI disabled
- CORS restricted to production domains
- Strong JWT secret key (32+ characters)
- Connection string encryption
- HTTPS enforcement

### Database Migration Strategy
- Code-first migrations
- Separate migrations for each database
- Migration scripts versioned in source control
- Rollback plan for each migration
- Backup before migration in production

---

## Scalability Considerations

### Horizontal Scaling
- Stateless API design (JWT in request, no server session)
- Database connection pooling
- Load balancer distribution
- Multiple API instances

### Vertical Scaling
- Database server resources (CPU, RAM)
- Connection pool size tuning
- Query optimization
- Index maintenance

### Future Enhancements
- Implement caching layer (Redis)
- Add message queue for email sending
- Implement real-time notifications (SignalR)
- Add read replicas for reporting queries
- Implement API rate limiting per user
