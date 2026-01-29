# MediQueue System - Complete Test Cases

## Test Case Organization

This document provides comprehensive test scenarios covering all system functionality. Each test case includes:
- **Test ID**: Unique identifier
- **Feature**: Functionality being tested
- **Scenario**: What is being tested
- **Preconditions**: Required system state
- **Test Steps**: Actions to perform
- **Expected Result**: What should happen
- **Status Indicators**: ✅ Pass / ❌ Fail

---

## 1. AUTHENTICATION & REGISTRATION TEST CASES

### TC-001: Patient Registration Success
**Feature**: Patient Registration  
**Scenario**: New patient successfully registers with valid data

**Preconditions**: None

**Test Steps**:
1. POST to `/api/account/register/patient`
2. Body:
```json
{
  "email": "john.doe@email.com",
  "displayName": "John Doe",
  "phoneNumber": "+201234567890",
  "password": "Test@123",
  "dateOfBirth": "1990-01-15",
  "gender": "Male",
  "bloodType": "O+",
  "emergencyContact": "Jane Doe",
  "emergencyContactPhone": "+209876543210"
}
```

**Expected Result**:
- HTTP 200 OK
- Response: "Patient registration successful. Please check your email to verify your account."
- User created in Identity database with Patient role
- OTP generated and sent to email
- EmailConfirmed = false

**Validation Checks**:
- ✅ Check database: AppUser exists with provided email
- ✅ Check database: User has "Patient" role assigned
- ✅ Check database: OTP record created with 10-minute expiration
- ✅ Verify email sent (check logs or email service)

---

### TC-002: Patient Registration - Duplicate Email
**Feature**: Patient Registration  
**Scenario**: Registration fails with already-used email

**Preconditions**: User with email "existing@email.com" already exists

**Test Steps**:
1. POST to `/api/account/register/patient`
2. Body: Use email "existing@email.com"

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Email address is already in use"
- No new user created

**Validation Checks**:
- ✅ Database user count unchanged
- ✅ No new OTP generated

---

### TC-003: Patient Registration - Invalid Password
**Feature**: Patient Registration  
**Scenario**: Registration fails with password missing digit

**Preconditions**: None

**Test Steps**:
1. POST to `/api/account/register/patient`
2. Body: password = "testpassword" (no digit)

**Expected Result**:
- HTTP 400 Bad Request
- Error message about password requirements

---

### TC-004: Clinic Registration Success
**Feature**: Clinic Registration  
**Scenario**: New clinic successfully registers with complete profile

**Preconditions**: None

**Test Steps**:
1. POST to `/api/account/register/clinic`
2. Body:
```json
{
  "email": "dr.ahmed@clinic.com",
  "displayName": "Dr. Ahmed Hassan",
  "phoneNumber": "+201234567890",
  "password": "Clinic@123",
  "doctorName": "Dr. Ahmed Hassan",
  "specialty": "Cardiology",
  "description": "Experienced cardiologist with 15 years practice",
  "slotDurationMinutes": 30,
  "country": "Egypt",
  "city": "Cairo",
  "area": "Nasr City",
  "street": "Abbas El Akkad",
  "building": "25",
  "addressNotes": "Near City Stars",
  "additionalPhones": ["+201987654321"]
}
```

**Expected Result**:
- HTTP 200 OK
- Response: "Clinic registration successful. Please check your email to verify your account."
- User created with Clinic role
- ClinicProfile created with doctor details
- ClinicAddress created
- ClinicPhone records created (2 phones)
- 7 ClinicWorkingDay records created (all closed)
- OTP sent

**Validation Checks**:
- ✅ AppUser exists in Identity database
- ✅ User has "Clinic" role
- ✅ ClinicProfile exists with matching AppUserId
- ✅ ClinicAddress exists with all fields
- ✅ ClinicPhone count = 2 (primary + additional)
- ✅ ClinicWorkingDay count = 7 (one per weekday)
- ✅ All WorkingDays have IsOpen = false
- ✅ OTP created

---

### TC-005: Email Verification Success
**Feature**: Email Verification  
**Scenario**: User verifies email with correct OTP

**Preconditions**: 
- User registered but not verified
- Valid OTP exists in database

**Test Steps**:
1. Get OTP from email or database (for testing)
2. POST to `/api/account/verify-email`
3. Body:
```json
{
  "email": "john.doe@email.com",
  "otpCode": "123456"
}
```

**Expected Result**:
- HTTP 200 OK
- Response includes UserDTO with:
  - displayName
  - email
  - token (JWT)
  - role = "Patient"
- EmailConfirmed = true in database
- OTP marked as used

**Validation Checks**:
- ✅ Token is valid JWT
- ✅ Token contains correct role claim
- ✅ Database: EmailConfirmed = true
- ✅ Database: OTP.IsUsed = true

---

### TC-006: Email Verification - Expired OTP
**Feature**: Email Verification  
**Scenario**: Verification fails with expired OTP

**Preconditions**: 
- User registered
- OTP created more than 10 minutes ago

**Test Steps**:
1. Wait 11 minutes after registration
2. POST to `/api/account/verify-email` with OTP

**Expected Result**:
- HTTP 400 Bad Request
- Response: "OTP has expired"
- EmailConfirmed remains false

---

### TC-007: Email Verification - Wrong OTP (Lockout)
**Feature**: Email Verification  
**Scenario**: Account locks after 5 failed OTP attempts

**Preconditions**: User registered with valid OTP

**Test Steps**:
1. POST to `/api/account/verify-email` with wrong OTP - Attempt 1
2. POST with wrong OTP - Attempt 2
3. POST with wrong OTP - Attempt 3
4. POST with wrong OTP - Attempt 4
5. POST with wrong OTP - Attempt 5
6. POST with correct OTP - Attempt 6

**Expected Result**:
- Attempts 1-4: HTTP 400 "Invalid OTP code"
- Attempt 5: HTTP 400 "Invalid OTP code"
- Attempt 6: HTTP 400 "Too many failed attempts. Account locked for 30 minutes."

**Validation Checks**:
- ✅ Database: OTP.FailedAttempts = 5
- ✅ Database: OTP.LockedUntil = CurrentTime + 30 minutes
- ✅ EmailConfirmed remains false

---

### TC-008: Resend OTP Success
**Feature**: OTP Resend  
**Scenario**: User requests new OTP after 60-second cooldown

**Preconditions**: 
- User registered
- Last OTP sent more than 60 seconds ago

**Test Steps**:
1. Wait 61 seconds after last OTP
2. POST to `/api/account/resend-otp`
3. Body:
```json
{
  "email": "john.doe@email.com"
}
```

**Expected Result**:
- HTTP 200 OK
- New OTP generated
- Old OTP marked as used
- New email sent

**Validation Checks**:
- ✅ New OTP record in database
- ✅ Previous OTP IsUsed = true
- ✅ New OTP has fresh expiration (10 minutes)

---

### TC-009: Resend OTP - Rate Limit
**Feature**: OTP Resend  
**Scenario**: Resend fails within 60-second cooldown

**Preconditions**: OTP sent less than 60 seconds ago

**Test Steps**:
1. Request OTP resend immediately after previous send

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Please wait 60 seconds before requesting a new code"

---

### TC-010: Login Success
**Feature**: User Login  
**Scenario**: Verified user logs in with correct credentials

**Preconditions**: User registered and email verified

**Test Steps**:
1. POST to `/api/account/login`
2. Body:
```json
{
  "email": "john.doe@email.com",
  "password": "Test@123"
}
```

**Expected Result**:
- HTTP 200 OK
- Response UserDTO:
```json
{
  "displayName": "John Doe",
  "email": "john.doe@email.com",
  "token": "eyJhbGc...",
  "role": "Patient"
}
```

**Validation Checks**:
- ✅ Token is valid JWT
- ✅ Token contains email claim
- ✅ Token contains role claim = "Patient"
- ✅ Token expires in 2 days

---

### TC-011: Login - Unverified Email
**Feature**: User Login  
**Scenario**: Login fails for unverified account

**Preconditions**: User registered but email not verified

**Test Steps**:
1. POST to `/api/account/login` with correct credentials

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Email not verified. Please verify your email."

---

### TC-012: Login - Wrong Password
**Feature**: User Login  
**Scenario**: Login fails with incorrect password

**Preconditions**: User exists

**Test Steps**:
1. POST to `/api/account/login`
2. Body: correct email, wrong password

**Expected Result**:
- HTTP 401 Unauthorized
- Response: "Invalid email or password"

---

### TC-013: Get Current User
**Feature**: User Profile  
**Scenario**: Authenticated user retrieves own profile

**Preconditions**: User logged in with valid token

**Test Steps**:
1. GET `/api/account/GetCurrentUser`
2. Header: `Authorization: Bearer <token>`

**Expected Result**:
- HTTP 200 OK
- Response UserDTO with user information

---

### TC-014: Get Current User - No Token
**Feature**: User Profile  
**Scenario**: Request fails without authentication

**Preconditions**: None

**Test Steps**:
1. GET `/api/account/GetCurrentUser`
2. No Authorization header

**Expected Result**:
- HTTP 401 Unauthorized

---

## 2. CLINIC MANAGEMENT TEST CASES

### TC-015: Get Clinic Profile (Public)
**Feature**: Clinic Profile  
**Scenario**: Anyone can view clinic public profile

**Preconditions**: Clinic exists with ID = 1

**Test Steps**:
1. GET `/api/clinics/1`
2. No authentication required

**Expected Result**:
- HTTP 200 OK
- Response ClinicProfileDto with:
  - Doctor name
  - Specialty
  - Description
  - Address
  - Phone numbers
  - Average rating

---

### TC-016: Get My Clinic Profile
**Feature**: Clinic Profile  
**Scenario**: Clinic user views own profile

**Preconditions**: Logged in as clinic user

**Test Steps**:
1. GET `/api/clinics/my-profile`
2. Header: `Authorization: Bearer <clinic-token>`

**Expected Result**:
- HTTP 200 OK
- Response with clinic's own profile data

---

### TC-017: Get My Clinic Profile - Patient Token
**Feature**: Authorization  
**Scenario**: Patient cannot access clinic-only endpoint

**Preconditions**: Logged in as patient

**Test Steps**:
1. GET `/api/clinics/my-profile`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 403 Forbidden
- Response: Access denied

---

### TC-018: Update Clinic Profile
**Feature**: Clinic Management  
**Scenario**: Clinic updates own information

**Preconditions**: Logged in as clinic, clinic ID = 1

**Test Steps**:
1. PUT `/api/clinics/1`
2. Header: `Authorization: Bearer <clinic-token>`
3. Body:
```json
{
  "doctorName": "Dr. Ahmed Hassan",
  "specialty": "Pediatric Cardiology",
  "description": "Updated description",
  "slotDurationMinutes": 45
}
```

**Expected Result**:
- HTTP 200 OK
- Updated clinic profile returned
- Database reflects changes

**Validation Checks**:
- ✅ Database specialty = "Pediatric Cardiology"
- ✅ Database slotDurationMinutes = 45

---

### TC-019: Search Clinics by Specialty
**Feature**: Clinic Search  
**Scenario**: Public search for cardiology clinics

**Preconditions**: Multiple clinics exist with different specialties

**Test Steps**:
1. GET `/api/clinics/search?specialty=Cardiology`
2. No authentication required

**Expected Result**:
- HTTP 200 OK
- Array of clinics with specialty containing "Cardiology"
- Results include ratings and addresses

**Validation Checks**:
- ✅ All returned clinics have specialty matching filter
- ✅ Results sorted by rating (highest first)

---

### TC-020: Search Clinics by City
**Feature**: Clinic Search  
**Scenario**: Filter clinics by location

**Preconditions**: Clinics in multiple cities

**Test Steps**:
1. GET `/api/clinics/search?city=Cairo`

**Expected Result**:
- HTTP 200 OK
- Only clinics in Cairo returned

---

### TC-021: Search Clinics by Minimum Rating
**Feature**: Clinic Search  
**Scenario**: Filter highly-rated clinics

**Preconditions**: Clinics with various ratings

**Test Steps**:
1. GET `/api/clinics/search?minRating=4.0`

**Expected Result**:
- HTTP 200 OK
- Only clinics with averageRating >= 4.0

---

## 3. WORKING SCHEDULE TEST CASES

### TC-022: Get Working Days (Public)
**Feature**: Working Schedule  
**Scenario**: Anyone can view clinic working schedule

**Preconditions**: Clinic ID=1 exists with configured schedule

**Test Steps**:
1. GET `/api/workingschedule/1/working-days`
2. No authentication required

**Expected Result**:
- HTTP 200 OK
- Array of 7 WorkingDay objects (Sunday to Saturday)
- Each shows: dayOfWeek, isOpen, startTime, endTime, maxPatients

**Validation Checks**:
- ✅ Public access (no token required)
- ✅ Returns complete week schedule

---

### TC-022A: Get Current Clinic Working Days
**Feature**: Working Schedule  
**Scenario**: Clinic views own working schedule

**Preconditions**: Logged in as clinic

**Test Steps**:
1. GET `/api/workingschedule/my-working-days`
2. Header: `Authorization: Bearer <clinic-token>`

**Expected Result**:
- HTTP 200 OK
- Array of 7 WorkingDay objects for current clinic

---

### TC-022B: Get Specific Working Day (Public)
**Feature**: Working Schedule  
**Scenario**: View clinic schedule for specific day

**Preconditions**: Clinic ID=1 exists

**Test Steps**:
1. GET `/api/workingschedule/1/working-days/1` (Monday)
2. No authentication required

**Expected Result**:
- HTTP 200 OK
- WorkingDay object for Monday with hours

---

### TC-022C: Get Clinic Exceptions (Public)
**Feature**: Schedule Exceptions  
**Scenario**: Anyone can view clinic closures

**Preconditions**: Clinic has exception dates

**Test Steps**:
1. GET `/api/workingschedule/1/exceptions`
2. No authentication required

**Expected Result**:
- HTTP 200 OK
- List of exception dates with reasons

---

### TC-023: Update Working Days (Bulk)
**Feature**: Working Schedule  
**Scenario**: Clinic sets operating hours for all days

**Preconditions**: Logged in as clinic

**Test Steps**:
1. PUT `/api/workingschedule/working-days`
2. Header: `Authorization: Bearer <clinic-token>`
3. Body:
```json
{
  "workingDays": [
    {
      "dayOfWeek": 0,
      "isOpen": false
    },
    {
      "dayOfWeek": 1,
      "isOpen": true,
      "startTime": "09:00:00",
      "endTime": "17:00:00",
      "maxPatients": 20
    },
    {
      "dayOfWeek": 2,
      "isOpen": true,
      "startTime": "09:00:00",
      "endTime": "17:00:00",
      "maxPatients": 20
    },
    ... (for all 7 days)
  ]
}
```

**Expected Result**:
- HTTP 200 OK
- Updated working schedule returned
- Database reflects changes

**Validation Checks**:
- ✅ Sunday (0) isOpen = false
- ✅ Monday (1) isOpen = true, times set correctly
- ✅ Database working days match request

---

### TC-024: Update Working Days - Invalid Time Range
**Feature**: Working Schedule Validation  
**Scenario**: Update fails when end time before start time

**Preconditions**: Logged in as clinic

**Test Steps**:
1. PUT `/api/workingschedule/working-days`
2. Body includes:
```json
{
  "dayOfWeek": 1,
  "isOpen": true,
  "startTime": "17:00:00",
  "endTime": "09:00:00"
}
```

**Expected Result**:
- HTTP 400 Bad Request
- Response: "End time must be after start time"

---

### TC-025: Add Exception Date
**Feature**: Schedule Exceptions  
**Scenario**: Clinic marks holiday closure

**Preconditions**: Logged in as clinic

**Test Steps**:
1. POST `/api/workingschedule/exceptions`
2. Header: `Authorization: Bearer <clinic-token>`
3. Body:
```json
{
  "date": "2026-12-25",
  "reason": "Christmas Holiday",
  "isClosed": true
}
```

**Expected Result**:
- HTTP 200 OK
- Exception created in database
- Bookings prevented for this date

**Validation Checks**:
- ✅ ClinicException record exists for date
- ✅ IsClosed = true
- ✅ Reason stored correctly

---

### TC-026: Check Clinic Availability
**Feature**: Availability Check  
**Scenario**: Check if clinic open on specific date (Public)

**Preconditions**: 
- Clinic has working days configured
- Monday is open (09:00-17:00)

**Test Steps**:
1. GET `/api/workingschedule/1/available?date=2026-02-02` (Monday)
2. No authentication

**Expected Result**:
- HTTP 200 OK
- Response:
```json
{
  "clinicId": 1,
  "date": "2026-02-02",
  "isAvailable": true
}
```

---

### TC-027: Check Availability - Exception Date
**Feature**: Availability Check  
**Scenario**: Clinic unavailable due to exception

**Preconditions**: Exception exists for 2026-12-25

**Test Steps**:
1. GET `/api/workingschedule/1/available?date=2026-12-25`

**Expected Result**:
- HTTP 200 OK
- isAvailable = false

---

### TC-028: Check Availability - Non-Working Day
**Feature**: Availability Check  
**Scenario**: Clinic closed on Sunday

**Preconditions**: Sunday marked as closed

**Test Steps**:
1. GET `/api/workingschedule/1/available?date=2026-02-01` (Sunday)

**Expected Result**:
- HTTP 200 OK
- isAvailable = false

---

### TC-028A: Get Clinic Schedule (Public)
**Feature**: Working Schedule Visibility  
**Scenario**: Patient views complete clinic operating hours

**Preconditions**: 
- Clinic ID=1 exists
- Working days configured (Mon-Fri open 9-5)
- Has exception for 2026-12-25

**Test Steps**:
1. GET `/api/workingschedule/1/schedule`
2. No authentication required

**Expected Result**:
- HTTP 200 OK
- Response ClinicScheduleDto:
```json
{
  "clinicId": 1,
  "workingDays": [
    {
      "id": 1,
      "dayOfWeek": 0,
      "dayName": "Sunday",
      "isOpen": false,
      "startTime": null,
      "endTime": null,
      "maxPatients": null
    },
    {
      "id": 2,
      "dayOfWeek": 1,
      "dayName": "Monday",
      "isOpen": true,
      "startTime": "09:00:00",
      "endTime": "17:00:00",
      "maxPatients": 20
    },
    {
      "id": 3,
      "dayOfWeek": 2,
      "dayName": "Tuesday",
      "isOpen": true,
      "startTime": "09:00:00",
      "endTime": "17:00:00",
      "maxPatients": 20
    }
    // ... rest of week
  ],
  "exceptions": [
    {
      "id": 1,
      "exceptionDate": "2026-12-25",
      "reason": "Christmas Holiday",
      "isClosed": true
    }
  ]
}
```

**Validation Checks**:
- ✅ All 7 days of week returned
- ✅ Open days show complete time information
- ✅ Closed days have isOpen = false
- ✅ Only future exception dates included
- ✅ Patient can view without authentication

---

### TC-028B: Get Clinic Schedule - Non-Existent Clinic
**Feature**: Error Handling  
**Scenario**: Request schedule for non-existent clinic

**Preconditions**: Clinic ID=999 doesn't exist

**Test Steps**:
1. GET `/api/workingschedule/999/schedule`

**Expected Result**:
- HTTP 404 Not Found
- Response: "Clinic not found"

---

## 4. APPOINTMENT BOOKING TEST CASES

### TC-029: Book Appointment Success
**Feature**: Appointment Booking  
**Scenario**: Patient books appointment at available clinic

**Preconditions**:
- Logged in as patient
- Clinic ID=1 exists and open on 2026-02-02 (Monday)
- Capacity not exceeded

**Test Steps**:
1. POST `/api/appointments/book`
2. Header: `Authorization: Bearer <patient-token>`
3. Body:
```json
{
  "clinicId": 1,
  "appointmentDate": "2026-02-02"
}
```

**Expected Result**:
- HTTP 200 OK
- Response AppointmentDto:
```json
{
  "id": 1,
  "clinicId": 1,
  "patientId": "user-guid",
  "appointmentDate": "2026-02-02",
  "queueNumber": 1,
  "status": "Booked",
  "createdAt": "2026-01-29T..."
}
```

**Validation Checks**:
- ✅ Appointment created in database
- ✅ QueueNumber = 1 (first booking)
- ✅ Status = Booked
- ✅ PatientId matches logged-in user

---

### TC-030: Book Appointment - Second Patient
**Feature**: Queue Numbering  
**Scenario**: Second patient gets queue number 2

**Preconditions**: First appointment already booked (#1)

**Test Steps**:
1. Login as different patient
2. POST `/api/appointments/book` for same clinic and date

**Expected Result**:
- HTTP 200 OK
- queueNumber = 2

---

### TC-031: Book Appointment - Past Date
**Feature**: Appointment Validation  
**Scenario**: Booking fails for past date

**Preconditions**: Logged in as patient

**Test Steps**:
1. POST `/api/appointments/book`
2. Body: appointmentDate = "2026-01-28" (yesterday)

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Cannot book appointments in the past"

---

### TC-032: Book Appointment - Clinic Closed
**Feature**: Appointment Validation  
**Scenario**: Booking fails when clinic closed

**Preconditions**: 
- Logged in as patient
- Sunday marked as closed for clinic

**Test Steps**:
1. POST `/api/appointments/book`
2. Body: appointmentDate = "2026-02-01" (Sunday)

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Clinic is not available on this date"

---

### TC-033: Book Appointment - Exception Date
**Feature**: Appointment Validation  
**Scenario**: Booking fails on holiday

**Preconditions**: 
- Exception exists for 2026-12-25
- Clinic normally open on Thursdays

**Test Steps**:
1. POST `/api/appointments/book`
2. Body: appointmentDate = "2026-12-25"

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Clinic is not available on this date"

---

### TC-034: Book Appointment - Capacity Full
**Feature**: Capacity Management  
**Scenario**: Booking fails when daily limit reached

**Preconditions**:
- Clinic maxPatients = 2 for Monday
- 2 appointments already booked for 2026-02-02

**Test Steps**:
1. POST `/api/appointments/book`
2. Body: appointmentDate = "2026-02-02"

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Clinic capacity is full for this date"

---

### TC-035: Get Appointment Details
**Feature**: Appointment View  
**Scenario**: User views appointment information

**Preconditions**: 
- Appointment ID=1 exists
- Logged in as patient who owns appointment

**Test Steps**:
1. GET `/api/appointments/1`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 200 OK
- AppointmentDetailsDto with full information

---

### TC-036: Get Patient Upcoming Appointments
**Feature**: Patient Dashboard  
**Scenario**: Patient views future appointments

**Preconditions**: 
- Logged in as patient
- Patient has 2 upcoming appointments
- Patient has 1 past appointment

**Test Steps**:
1. GET `/api/appointments/patient/upcoming`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 200 OK
- Array of 2 appointments (only future ones)
- Sorted by date (earliest first)

**Validation Checks**:
- ✅ Only appointments with date >= today
- ✅ Only appointments with status != Completed, Canceled

---

### TC-037: Get Patient Appointment History
**Feature**: Patient History  
**Scenario**: Patient views all past appointments

**Preconditions**: 
- Patient has completed and canceled appointments

**Test Steps**:
1. GET `/api/appointments/patient/history`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 200 OK
- Response includes:
  - Total appointments count
  - Completed appointments list
  - Canceled appointments list
  - Grouped by status

---

### TC-038: Cancel Appointment Success
**Feature**: Appointment Cancellation  
**Scenario**: Patient cancels own appointment

**Preconditions**: 
- Logged in as patient
- Appointment ID=1 exists with status = Booked
- Patient owns this appointment

**Test Steps**:
1. POST `/api/appointments/1/cancel`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 200 OK
- Response: "Appointment canceled successfully"
- Database status = Canceled

**Validation Checks**:
- ✅ Appointment.Status = Canceled
- ✅ Queue number unchanged

---

### TC-039: Cancel Appointment - Wrong Patient
**Feature**: Authorization  
**Scenario**: Patient cannot cancel other's appointment

**Preconditions**: 
- Logged in as Patient A
- Appointment ID=1 belongs to Patient B

**Test Steps**:
1. POST `/api/appointments/1/cancel`
2. Header: `Authorization: Bearer <patient-A-token>`

**Expected Result**:
- HTTP 400 Bad Request
- Response: "You can only cancel your own appointments"

---

### TC-040: Cancel Appointment - Already Completed
**Feature**: Cancellation Validation  
**Scenario**: Cannot cancel completed appointment

**Preconditions**: Appointment status = Completed

**Test Steps**:
1. POST `/api/appointments/1/cancel`

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Cannot cancel completed appointments"

---

### TC-041: Cancel Appointment - Already Canceled
**Feature**: Cancellation Validation  
**Scenario**: Cannot cancel twice

**Preconditions**: Appointment status = Canceled

**Test Steps**:
1. POST `/api/appointments/1/cancel`

**Expected Result**:
- HTTP 400 Bad Request
- Response: "Appointment is already canceled"

---

## 5. CLINIC QUEUE MANAGEMENT TEST CASES

### TC-042: Get Clinic Queue
**Feature**: Queue Management  
**Scenario**: Clinic views today's queue

**Preconditions**: 
- Logged in as clinic
- 3 appointments booked for 2026-02-02
- Queue numbers: 1, 2, 3

**Test Steps**:
1. GET `/api/appointments/clinic/queue?date=2026-02-02`
2. Header: `Authorization: Bearer <clinic-token>`

**Expected Result**:
- HTTP 200 OK
- Response ClinicQueueDto:
```json
{
  "date": "2026-02-02",
  "clinicId": 1,
  "totalAppointments": 3,
  "appointments": [
    {
      "id": 1,
      "queueNumber": 1,
      "patientName": "John Doe",
      "status": "Booked"
    },
    {
      "id": 2,
      "queueNumber": 2,
      "patientName": "Jane Smith",
      "status": "Booked"
    },
    {
      "id": 3,
      "queueNumber": 3,
      "patientName": "Bob Wilson",
      "status": "Booked"
    }
  ]
}
```

**Validation Checks**:
- ✅ Appointments sorted by queue number
- ✅ All appointments for specified date

---

### TC-043: Update Appointment to InProgress
**Feature**: Status Management  
**Scenario**: Clinic marks patient as being seen

**Preconditions**: 
- Logged in as clinic
- Appointment ID=1, status = Booked

**Test Steps**:
1. POST `/api/appointments/1/start`
2. Header: `Authorization: Bearer <clinic-token>`

**Expected Result**:
- HTTP 200 OK
- Updated appointment with status = InProgress
- Database reflects change

---

### TC-044: Complete Appointment
**Feature**: Status Management  
**Scenario**: Clinic marks consultation complete

**Preconditions**: Appointment ID=1, status = InProgress

**Test Steps**:
1. POST `/api/appointments/1/complete`
2. Header: `Authorization: Bearer <clinic-token>`

**Expected Result**:
- HTTP 200 OK
- status = Completed

---

### TC-045: Update Status - Wrong Clinic
**Feature**: Authorization  
**Scenario**: Clinic cannot update other clinic's appointments

**Preconditions**: 
- Logged in as Clinic A
- Appointment ID=1 belongs to Clinic B

**Test Steps**:
1. POST `/api/appointments/1/start`
2. Header: `Authorization: Bearer <clinic-A-token>`

**Expected Result**:
- HTTP 403 Forbidden
- Response: "You can only manage your own appointments"

---

### TC-046: Mark Appointment Delayed
**Feature**: Queue Management  
**Scenario**: Clinic indicates running late

**Preconditions**: Appointment exists

**Test Steps**:
1. PUT `/api/appointments/1/status`
2. Body:
```json
{
  "status": "Delayed"
}
```

**Expected Result**:
- HTTP 200 OK
- status = Delayed

---

## 6. RATING AND REVIEW TEST CASES

### TC-047: Submit Rating Success
**Feature**: Clinic Rating  
**Scenario**: Patient rates clinic after visit

**Preconditions**: 
- Logged in as patient
- Patient has completed appointment at clinic ID=1

**Test Steps**:
1. POST `/api/ratings`
2. Header: `Authorization: Bearer <patient-token>`
3. Body:
```json
{
  "clinicId": 1,
  "rating": 5,
  "comment": "Excellent doctor, very professional and caring"
}
```

**Expected Result**:
- HTTP 200 OK
- Response ClinicRatingDto
- Database: ClinicRating record created
- Clinic averageRating updated
- Clinic totalRatings incremented

**Validation Checks**:
- ✅ ClinicRating exists with rating = 5
- ✅ Clinic.TotalRatings increased by 1
- ✅ Clinic.AverageRating recalculated

---

### TC-048: Submit Rating - Duplicate
**Feature**: Rating Validation  
**Scenario**: Patient cannot rate same clinic twice

**Preconditions**: Patient already rated clinic ID=1

**Test Steps**:
1. POST `/api/ratings` with clinic ID=1

**Expected Result**:
- HTTP 400 Bad Request
- Response: "You have already rated this clinic"

---

### TC-049: Submit Rating - Invalid Value
**Feature**: Rating Validation  
**Scenario**: Rating must be 1-5

**Preconditions**: Logged in as patient

**Test Steps**:
1. POST `/api/ratings`
2. Body: rating = 6

**Expected Result**:
- HTTP 400 Bad Request
- Validation error about rating range

---

### TC-050: Get Clinic Ratings (Public)
**Feature**: Rating Display  
**Scenario**: Anyone can view clinic ratings

**Preconditions**: Clinic ID=1 has 3 ratings

**Test Steps**:
1. GET `/api/ratings/clinic/1?pageNumber=1&pageSize=10`
2. No authentication

**Expected Result**:
- HTTP 200 OK
- Response ClinicRatingSummaryDto:
```json
{
  "clinicId": 1,
  "averageRating": 4.3,
  "totalRatings": 3,
  "ratings": [
    {
      "id": 1,
      "rating": 5,
      "comment": "Excellent",
      "patientName": "John Doe",
      "createdAt": "2026-01-20T..."
    },
    {
      "id": 2,
      "rating": 4,
      "comment": "Very good",
      "patientName": "Jane Smith",
      "createdAt": "2026-01-15T..."
    },
    {
      "id": 3,
      "rating": 4,
      "comment": "Professional",
      "patientName": "Bob Wilson",
      "createdAt": "2026-01-10T..."
    }
  ]
}
```

**Validation Checks**:
- ✅ Sorted by date (newest first)
- ✅ Average calculated correctly: (5+4+4)/3 = 4.33

---

### TC-051: Get My Rating for Clinic
**Feature**: Patient Rating View  
**Scenario**: Patient checks their submitted rating

**Preconditions**: 
- Logged in as patient
- Patient rated clinic ID=1

**Test Steps**:
1. GET `/api/ratings/clinic/1/my-rating`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 200 OK
- Patient's ClinicRatingDto returned

---

### TC-052: Update Existing Rating
**Feature**: Rating Update  
**Scenario**: Patient modifies previous rating

**Preconditions**: Patient already rated clinic

**Test Steps**:
1. PUT `/api/ratings/{ratingId}`
2. Body:
```json
{
  "rating": 3,
  "comment": "Updated review"
}
```

**Expected Result**:
- HTTP 200 OK
- Rating updated
- Clinic average recalculated

**Validation Checks**:
- ✅ Old rating value replaced
- ✅ Average rating reflects new value

---

## 7. AUTHORIZATION TEST CASES

### TC-053: Patient Access Patient Endpoint
**Feature**: Role Authorization  
**Scenario**: Correct role can access

**Preconditions**: Logged in as patient

**Test Steps**:
1. GET `/api/appointments/patient/upcoming`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 200 OK

---

### TC-054: Clinic Access Patient Endpoint
**Feature**: Role Authorization  
**Scenario**: Wrong role cannot access

**Preconditions**: Logged in as clinic

**Test Steps**:
1. GET `/api/appointments/patient/upcoming`
2. Header: `Authorization: Bearer <clinic-token>`

**Expected Result**:
- HTTP 403 Forbidden

---

### TC-055: Patient Access Clinic Endpoint
**Feature**: Role Authorization  
**Scenario**: Patient blocked from clinic operations

**Preconditions**: Logged in as patient

**Test Steps**:
1. GET `/api/appointments/clinic/queue`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 403 Forbidden

---

### TC-056: Admin Access Admin Dashboard
**Feature**: Admin Role  
**Scenario**: Admin can access admin pages

**Preconditions**: Logged in as admin

**Test Steps**:
1. GET `/Admin/Index`
2. Header: `Authorization: Bearer <admin-token>`

**Expected Result**:
- HTTP 200 OK
- Admin dashboard rendered

---

### TC-057: Patient Access Admin Dashboard
**Feature**: Admin Authorization  
**Scenario**: Non-admin blocked from admin area

**Preconditions**: Logged in as patient

**Test Steps**:
1. GET `/Admin/Index`
2. Header: `Authorization: Bearer <patient-token>`

**Expected Result**:
- HTTP 403 Forbidden

---

## 8. DATA INTEGRITY TEST CASES

### TC-058: Concurrent Booking - Same Slot
**Feature**: Concurrency Control  
**Scenario**: Two patients book simultaneously

**Preconditions**: Clinic capacity = 2 for date

**Test Steps**:
1. Patient A starts booking request for clinic 1, date X
2. Patient B starts booking request for clinic 1, date X (simultaneously)
3. Both requests process concurrently

**Expected Result**:
- Both bookings succeed
- Patient A gets queue number 1
- Patient B gets queue number 2
- No duplicate queue numbers

**Validation Checks**:
- ✅ 2 appointments exist for same clinic/date
- ✅ Queue numbers are 1 and 2 (no duplicates)
- ✅ Database row locking worked correctly

---

### TC-059: Concurrent Booking - Capacity Limit
**Feature**: Capacity Enforcement  
**Scenario**: 3 patients book when capacity = 2

**Preconditions**: Clinic max capacity = 2

**Test Steps**:
1. Patient A, B, C all submit booking requests simultaneously
2. All for same clinic and date

**Expected Result**:
- 2 bookings succeed
- 1 booking fails with "capacity full" error
- No overbooking

**Validation Checks**:
- ✅ Exactly 2 appointments in database
- ✅ Total never exceeds maxPatients

---

### TC-060: Queue Number Consistency
**Feature**: Queue Numbering  
**Scenario**: Cancellations don't affect numbering

**Preconditions**: 
- 3 appointments: queue #1, #2, #3
- Appointment #2 gets canceled

**Test Steps**:
1. Cancel appointment with queue #2
2. New patient books appointment

**Expected Result**:
- Canceled appointment keeps queue #2
- New appointment gets queue #4 (not #2)

**Validation Checks**:
- ✅ Queue numbers: 1, 2 (canceled), 3, 4
- ✅ No queue number reuse

---

### TC-061: Rating Average Calculation
**Feature**: Rating Aggregation  
**Scenario**: Average updates correctly

**Preconditions**: Clinic has no ratings

**Test Steps**:
1. Patient A rates clinic: 5 stars
2. Check average = 5.0
3. Patient B rates clinic: 3 stars
4. Check average = 4.0
5. Patient C rates clinic: 4 stars
6. Check average = 4.0

**Expected Result**:
- After rating 1: average = 5.0, total = 1
- After rating 2: average = 4.0, total = 2
- After rating 3: average = 4.0, total = 3

**Validation Checks**:
- ✅ Calculation: (5+3+4)/3 = 4.0
- ✅ TotalRatings = 3

---

### TC-062: Cascade Delete - Clinic Profile
**Feature**: Data Integrity  
**Scenario**: Deleting clinic removes related data

**Preconditions**: 
- Clinic has appointments, ratings, working days

**Test Steps**:
1. Delete ClinicProfile record
2. Check related data

**Expected Result**:
- ClinicAddress deleted
- ClinicPhone records deleted
- ClinicWorkingDay records deleted
- ClinicException records deleted
- Appointments handled (based on cascade rules)
- Ratings handled (based on cascade rules)

---

## 9. ERROR HANDLING TEST CASES

### TC-063: Invalid JSON Request
**Feature**: Request Validation  
**Scenario**: Malformed request body

**Test Steps**:
1. POST `/api/account/register/patient`
2. Body: `{ "email": "test@email.com", invalid json }`

**Expected Result**:
- HTTP 400 Bad Request
- Error about JSON parsing

---

### TC-064: Missing Required Field
**Feature**: Validation  
**Scenario**: Registration without required field

**Test Steps**:
1. POST `/api/account/register/patient`
2. Body: Missing "email" field

**Expected Result**:
- HTTP 400 Bad Request
- Validation error: "Email is required"

---

### TC-065: Database Connection Failure
**Feature**: Error Handling  
**Scenario**: System handles database errors

**Preconditions**: Database temporarily unavailable

**Test Steps**:
1. Any database operation

**Expected Result**:
- HTTP 500 Internal Server Error
- Generic error message (no sensitive details)
- Error logged in system logs

---

### TC-066: Token Expiration
**Feature**: Authentication  
**Scenario**: Expired token rejected

**Preconditions**: Token issued >2 days ago

**Test Steps**:
1. GET any protected endpoint
2. Header: `Authorization: Bearer <expired-token>`

**Expected Result**:
- HTTP 401 Unauthorized
- Response: Token expired

---

### TC-067: Token Signature Invalid
**Feature**: Security  
**Scenario**: Tampered token rejected

**Preconditions**: Token signature modified

**Test Steps**:
1. Modify token signature
2. Send request with tampered token

**Expected Result**:
- HTTP 401 Unauthorized
- Token validation fails

---

## 10. INTEGRATION TEST SCENARIOS

### TC-068: Complete Patient Journey
**Feature**: End-to-End Flow  
**Scenario**: Patient full experience

**Test Steps**:
1. Register as patient
2. Verify email with OTP
3. Login and receive token
4. Search for cardiologist clinics
5. Select clinic with high rating
6. Check clinic availability for Monday
7. Book appointment
8. View upcoming appointments
9. Check queue position
10. Complete appointment (simulated)
11. Submit 5-star rating with review

**Expected Result**:
- All operations succeed
- Data consistency maintained
- Patient appears in clinic queue
- Rating updates clinic average

---

### TC-069: Complete Clinic Journey
**Feature**: End-to-End Flow  
**Scenario**: Clinic full experience

**Test Steps**:
1. Register as clinic with complete profile
2. Verify email
3. Login
4. View clinic profile
5. Update working hours (Mon-Fri 9-5)
6. Add Christmas holiday exception
7. Receive patient booking
8. View today's queue
9. Mark first patient as InProgress
10. Complete appointment
11. View received ratings

**Expected Result**:
- All operations succeed
- Schedule properly configured
- Queue management works
- Ratings visible

---

### TC-070: Multi-Patient Queue Flow
**Feature**: Queue Management  
**Scenario**: Multiple patients same day

**Test Steps**:
1. Patient A books appointment for Monday (queue #1)
2. Patient B books appointment for Monday (queue #2)
3. Patient C books appointment for Monday (queue #3)
4. Patient B cancels appointment
5. Clinic views queue (sees #1, #2 canceled, #3)
6. Clinic starts appointment #1
7. Clinic completes appointment #1
8. Clinic starts appointment #3
9. Clinic completes appointment #3

**Expected Result**:
- Queue numbers: 1, 2, 3 assigned correctly
- Cancellation doesn't affect others
- Clinic can manage in any order
- Final state: #1 completed, #2 canceled, #3 completed

---

## TEST EXECUTION SUMMARY

### Test Coverage Overview

**Authentication & Registration**: 14 test cases
- ✅ Patient registration flow
- ✅ Clinic registration flow
- ✅ Email verification
- ✅ OTP validation and lockout
- ✅ Login and token issuance

**Clinic Management**: 7 test cases
- ✅ Profile viewing and updates
- ✅ Search and filtering
- ✅ Authorization checks

**Working Schedule**: 7 test cases
- ✅ Schedule configuration
- ✅ Exception handling
- ✅ Availability checking

**Appointment Booking**: 18 test cases
- ✅ Booking validation
- ✅ Capacity management
- ✅ Queue numbering
- ✅ Cancellation rules
- ✅ History tracking

**Queue Management**: 5 test cases
- ✅ Queue viewing
- ✅ Status updates
- ✅ Authorization

**Ratings**: 6 test cases
- ✅ Rating submission
- ✅ Average calculation
- ✅ Public viewing

**Authorization**: 5 test cases
- ✅ Role-based access
- ✅ Admin restrictions

**Data Integrity**: 5 test cases
- ✅ Concurrency control
- ✅ Referential integrity

**Error Handling**: 5 test cases
- ✅ Validation errors
- ✅ System errors

**Integration**: 3 test cases
- ✅ End-to-end flows

**Total Test Cases**: 70

### Testing Tools Required

**API Testing**:
- Postman or similar REST client
- JWT decoder (jwt.io)
- JSON validator

**Database Testing**:
- SQL Server Management Studio
- Database query tools

**Performance Testing**:
- Apache JMeter (for concurrency tests)
- Load testing tools

**Security Testing**:
- Token manipulation tools
- OWASP ZAP (optional)

### Test Execution Recommendations

1. **Run in Order**: Execute authentication tests first, then dependent features
2. **Clean State**: Reset database between major test suites
3. **Parallel Testing**: Avoid parallel execution for concurrency tests
4. **Token Management**: Store valid tokens for reuse across related tests
5. **Time Sensitivity**: Account for OTP expiration during test execution
6. **Data Validation**: Always verify database state matches expected results
7. **Error Logging**: Monitor application logs during test execution
8. **Performance Baseline**: Record response times for performance comparison

### Success Criteria

**System passes testing if**:
- All authentication flows work correctly
- Role-based authorization properly enforced
- Appointment booking handles capacity and validation
- Queue numbers assigned without duplicates
- Concurrent bookings handled correctly
- Rating calculations accurate
- Error responses appropriate and secure
- Data integrity maintained across all operations

---

## APPENDIX: Test Data Setup

### Initial Test Data

**Admin User** (Seeded):
- Email: admin@mediqueue.com
- Password: Admin@123
- Role: Admin

**Test Clinic 1**:
- Email: clinic1@test.com
- Doctor: Dr. Ahmed Hassan
- Specialty: Cardiology
- City: Cairo
- Working: Mon-Fri 9:00-17:00
- MaxPatients: 20

**Test Clinic 2**:
- Email: clinic2@test.com
- Doctor: Dr. Sara Mohamed
- Specialty: Pediatrics
- City: Alexandria
- Working: Sun-Thu 10:00-16:00
- MaxPatients: 15

**Test Patient 1**:
- Email: patient1@test.com
- Name: John Doe
- Phone: +201234567890
- Blood Type: O+

**Test Patient 2**:
- Email: patient2@test.com
- Name: Jane Smith
- Phone: +201987654321
- Blood Type: A+

### SQL Scripts for Test Data

```sql
-- Check appointment counts
SELECT ClinicId, AppointmentDate, COUNT(*) as Total
FROM Appointments
GROUP BY ClinicId, AppointmentDate;

-- View queue for specific date
SELECT QueueNumber, PatientId, Status, CreatedAt
FROM Appointments
WHERE ClinicId = 1 AND AppointmentDate = '2026-02-02'
ORDER BY QueueNumber;

-- Check clinic ratings
SELECT c.DoctorName, c.AverageRating, c.TotalRatings
FROM ClinicProfiles c
ORDER BY c.AverageRating DESC;

-- Verify OTP status
SELECT Email, Code, ExpirationDate, IsUsed, FailedAttempts
FROM Otps
WHERE Email = 'test@email.com'
ORDER BY ExpirationDate DESC;
```
