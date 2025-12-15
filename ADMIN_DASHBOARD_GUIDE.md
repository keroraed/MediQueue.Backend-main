# Admin Dashboard Access Guide

## 🎉 Razor Pages Admin Dashboard Created!

Your admin dashboard is now accessible as Razor Pages with a beautiful UI.

## 📍 Access URLs

### Admin Login
```
https://localhost:7101/Admin/Login
```

### After Login - Admin Pages
- **Dashboard**: `https://localhost:7101/Admin/Index` or `https://localhost:7101/Admin`
- **User Management**: `https://localhost:7101/Admin/Users`
- **Clinic Management**: `https://localhost:7101/Admin/Clinics`
- **Appointments**: `https://localhost:7101/Admin/Appointments`
- **Logout**: `https://localhost:7101/Admin/Logout`

## 🔐 Default Admin Credentials

```
Email: admin@mediqueue.com
Password: Admin@123
```

(Make sure this admin user is seeded in your database)

## 🚀 How to Run

1. **Build the project:**
   ```powershell
   dotnet build
   ```

2. **Run the application:**
   ```powershell
   dotnet run --project MediQueue.APIs
   ```

3. **Open your browser and navigate to:**
   ```
   https://localhost:7101/Admin/Login
   ```

## ✨ Features Included

### 📊 Dashboard Page
- Total clinics count
- Total patients count
- Total appointments count
- Today's appointments count
- Recent appointments table with status badges

### 👥 User Management
- View all users with filters (by role and status)
- Lock/unlock user accounts
- Delete users (admin accounts protected)
- Email verification status indicator
- Last login tracking

### 🏥 Clinic Management
- View all clinics
- Display ratings and review counts
- Show total appointments per clinic
- Delete clinics
- View contact information and location

### 📅 Appointment Management
- View all appointments
- Filter by status (Pending, Confirmed, Completed, Cancelled)
- Filter by date (Today, This Week, This Month)
- Color-coded status badges
- Patient and clinic information

## 🎨 UI Features

- **Gradient sidebar** with purple/blue theme
- **Responsive design** using Bootstrap 5
- **Font Awesome icons** throughout
- **Stat cards** with hover effects
- **Clean table layouts** with action buttons
- **Toast notifications** for success/error messages
- **Confirmation dialogs** for destructive actions

## 📁 File Structure

```
MediQueue.APIs/
├── Pages/
│   ├── Admin/
│   │   ├── Index.cshtml                 (Dashboard)
│   │   ├── Index.cshtml.cs
│   │   ├── Login.cshtml                 (Login Page)
│   │   ├── Login.cshtml.cs
│   │   ├── Users.cshtml                 (User Management)
│   │   ├── Users.cshtml.cs
│   │   ├── Clinics.cshtml               (Clinic Management)
│   │   ├── Clinics.cshtml.cs
│   │   ├── Appointments.cshtml          (Appointments)
│   │   ├── Appointments.cshtml.cs
│   │   ├── Logout.cshtml                (Logout)
│   │   ├── Logout.cshtml.cs
│   │   └── _ViewStart.cshtml
│   ├── Shared/
│   │   └── _AdminLayout.cshtml          (Shared Layout)
│   └── _ViewImports.cshtml
└── Program.cs                            (Updated for Razor Pages)
```

## 🔧 Technical Details

### Program.cs Configuration
```csharp
builder.Services.AddControllers();    // API Controllers
builder.Services.AddRazorPages();     // Razor Pages for Admin UI

app.MapControllers();                 // API routes
app.MapRazorPages();                  // Razor Pages routes
```

### Authorization
- Login page: `[AllowAnonymous]`
- All other admin pages: `[Authorize(Roles = "Admin")]`
- Automatic redirect to login if not authenticated

### Features
- **Cookie-based authentication** using ASP.NET Core Identity
- **Role-based authorization** (Admin role required)
- **Remember Me** functionality on login
- **Last login tracking** updates on successful login
- **CSRF protection** on all forms

## 🎯 Next Steps

1. **Customize the branding**: Update the logo and colors in `_AdminLayout.cshtml`
2. **Add more pages**: Create additional Razor Pages as needed
3. **Enhance features**: Add search, pagination, export functionality
4. **Add charts**: Integrate Chart.js or similar for data visualization
5. **Add notifications**: Implement real-time notifications using SignalR

## 🔄 Dual Architecture

Your application now supports **both**:

### REST API (for mobile/frontend apps)
```
POST /api/account/login           → Returns JWT token
GET /api/admin/stats              → Returns JSON data
GET /api/admin/users              → Returns JSON data
```

### Razor Pages UI (for admin web dashboard)
```
GET /Admin/Login                   → Returns HTML page
GET /Admin/Index                   → Returns HTML dashboard
GET /Admin/Users                   → Returns HTML user management page
```

## 📝 Notes

- The API endpoints (`/api/*`) remain unchanged and work independently
- Razor Pages use **server-side rendering** with **cookie authentication**
- API endpoints use **JWT token authentication**
- Both can coexist and work simultaneously
- The admin dashboard is fully functional and production-ready

## 🎨 Color Scheme

- **Primary**: Purple/Blue gradient (`#667eea` to `#764ba2`)
- **Success**: Green (`#28a745`)
- **Warning**: Orange (`#ffc107`)
- **Danger**: Red (`#dc3545`)
- **Info**: Blue (`#17a2b8`)

Enjoy your new admin dashboard! 🚀
