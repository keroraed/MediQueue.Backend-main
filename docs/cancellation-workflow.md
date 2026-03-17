# Cancellation and Queue Workflow (Clinic / Doctor)

## Overview
- Two clinic-driven cancellation modes:
  - **Single appointment cancel** (status update to `Canceled`)
  - **Bulk day cancel** (cancel all active appointments for a given date)
- Patient-driven cancel is separate and does **not** send email.
- Clinic cancellations **require a reason**; the reason is sent in the cancellation email to patients.

## API Endpoints (clinic role)

### Single appointment status update (including cancel)
- **Endpoint:** `PUT /api/appointments/{id}/status`
- **Payload:**
  ```json
  {
    "status": "Canceled" | "InProgress" | "Completed" | "Delayed",
    "reason": "string (required when status = Canceled)"
  }
  ```
- **Behavior:**
  - Validates clinic ownership and allowed status transitions.
  - When `status = Canceled`, `reason` is required. An email is sent using the shared cancellation template (includes reason).
  - When `status = InProgress`, triggers queue reminder (emails the patient now with exactly 2 people ahead).

### Bulk day cancel
- **Endpoint:** `POST /api/appointments/clinic/cancel-day?date=YYYY-MM-DD&reason=...`
- **Query params:**
  - `date` (required)
  - `reason` (required)
- **Behavior:**
  - Cancels only active appointments (`Booked` or `Delayed`) for that date.
  - Skips `InProgress` and `Completed` (already being seen or finished stay untouched).
  - Sends cancellation email (with reason) to each affected patient.
  - Returns count of canceled appointments.

### Call next patient
- **Endpoint:** `POST /api/appointments/clinic/call-next?date=YYYY-MM-DD`
- **Behavior:** Picks earliest `Booked` for that date, sets `InProgress`, sends queue reminder to the patient now with 2 ahead.

### Other clinic actions
- `POST /api/appointments/{id}/start` ? mark `InProgress`
- `POST /api/appointments/{id}/complete` ? mark `Completed`
- `POST /api/appointments/{id}/delay` ? mark `Delayed`

## Emails
- **Cancellation email:** Shared template for single and bulk cancellations. Parameters: patient name, doctor name, date, time, queue number, **reason**. Refund note included.
- **Queue reminder email:** Sent when an appointment transitions to `InProgress` (via update or call-next). Targets the patient with exactly 2 people ahead.

## DTO changes (backend contract)
- `UpdateAppointmentStatusDto` now includes `Reason` (string?). Required when `status = Canceled` for clinic-driven changes.
- `IAppointmentService.CancelClinicDayAsync` signature includes `reason`.
- `IEmailService.SendAppointmentCancellationEmailAsync` signature includes `reason`.

## Workflow Scenarios

### Clinic cancels a single appointment
1) Frontend calls `PUT /api/appointments/{id}/status` with `{ "status": "Canceled", "reason": "..." }`.
2) Backend validates ownership and transition; rejects if reason is missing.
3) Appointment status set to `Canceled` (queue number remains for history).
4) Cancellation email sent to patient with the provided reason.
5) Queue continues; next `Booked` will be served when clinic triggers call/start.

### Clinic cancels the full day (remaining patients)
1) Frontend calls `POST /api/appointments/clinic/cancel-day?date=...&reason=...`.
2) Backend cancels only `Booked` / `Delayed` for that date; leaves `InProgress` / `Completed` untouched.
3) Sends cancellation email (with reason) to each affected patient.
4) Returns count of canceled appointments.

### Clinic has already finished some appointments and wants to cancel the rest
- Use the bulk endpoint. Completed or currently InProgress appointments remain unchanged; only pending (`Booked`/`Delayed`) are canceled and notified.

### Patient cancels their own appointment
- Endpoint: `POST /api/appointments/{id}/cancel` (patient role).
- No email is sent; reason not required.

## Frontend Requirements / Notes
- When clinic selects `Canceled` status, always supply a non-empty `reason` in the payload.
- For bulk cancel UI, enforce required reason alongside the date.
- Show the returned message/count from bulk cancel.
- No change needed for patient cancel flow (still no email/reason).
- Be aware the cancellation email template now displays the provided reason; ensure the UI collects meaningful text.

## Current State Summary (doctor capabilities)
- Call next patient, mark in progress, complete, delay.
- Cancel single appointment with required reason ? email goes out.
- Cancel rest of day (pending patients) with required reason ? emails go out; finished/active visits remain.
- Queue reminders continue to work when appointments enter `InProgress`.
