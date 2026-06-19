# What2Wear - Mobile Clothing Photography App

A cross-platform mobile application built with .NET MAUI that allows users to capture, organize, and manage photos of their clothing items. Users can sign up, log in, take photos of their clothes, categorize them, add tags and notes, and view their collection.

## Features

### Authentication
- User signup with email and password
- Secure JWT-based login
- Password hashing with BCrypt
- Persistent authentication tokens via secure storage

### Photo Management
- Capture photos using device camera
- Select photos from device gallery
- Organize photos by clothing type (Shirt, Pants, Dress, Skirt, Jacket, Shoes, Accessories, Other)
- Add tags and notes to each photo
- Browse photo gallery
- Delete photos
- Responsive image display

### Cross-Platform Support
- **Android** (API 24+)
- **iOS** (15.0+)
- **macOS Catalyst** (15.0+)
- **Windows** (10.0.17763.0+)

## Architecture

### Project Structure

```
What2Wear/
├── What2Wear/                    # MAUI Client Application
│   ├── Services/
│   │   ├── ApiClient.cs         # HTTP client for API communication
│   ├── LoginPage.xaml/cs        # Login UI
│   ├── SignupPage.xaml/cs       # Signup UI
│   ├── PhotosPage.xaml/cs       # Photo gallery UI
│   ├── CapturePhotoPage.xaml/cs # Photo capture UI
│   ├── AppShell.xaml/cs         # Navigation shell
│   └── MauiProgram.cs           # Dependency injection & configuration
├── What2Wear.Web/                # ASP.NET Core Web API
│   ├── Controllers/
│   │   ├── AuthController.cs    # Authentication endpoints
│   │   └── PhotosController.cs  # Photo management endpoints
│   ├── Services/
│   │   ├── AuthService.cs       # Authentication business logic
│   │   ├── PhotoService.cs      # Photo business logic
│   │   └── JwtBearerHandler.cs  # JWT authentication handler
│   ├── Data/
│   │   └── What2WearDbContext.cs # EF Core database context
│   ├── Models/
│   │   └── DomainModels.cs      # User and Photo entities
│   └── Program.cs               # API configuration
├── What2Wear.Shared/            # Shared Models & Services
│   ├── Models/
│   │   ├── AuthModels.cs        # Auth DTOs
│   │   └── PhotoModels.cs       # Photo DTOs
│   └── Services/
│       └── IFormFactor.cs       # Platform detection interface
└── What2Wear.Web.Client/        # Blazor WebAssembly Client (optional)
```

## API Endpoints

### Authentication
- `POST /api/auth/signup` - User registration
- `POST /api/auth/login` - User login

### Photos
- `POST /api/photos/upload` - Upload photo (requires auth)
- `GET /api/photos` - Get user's photos (requires auth)
- `GET /api/photos/{photoId}` - Get specific photo (requires auth)
- `DELETE /api/photos/{photoId}` - Delete photo (requires auth)

## Setup Instructions

### Prerequisites
- .NET 10.0 SDK
- Visual Studio 2026 Community or Professional
- SQL Server LocalDB or SQL Server instance
- Android SDK (for Android development)
- iOS SDK (for iOS development on macOS)

### Backend Setup

1. **Configure Database Connection**
   - Open `What2Wear.Web/appsettings.json`
   - Update `ConnectionStrings.DefaultConnection` with your SQL Server connection string

2. **Configure JWT Settings**
   - In `appsettings.json`, update `JwtSettings`:
     ```json
     "JwtSettings": {
       "SecretKey": "your-super-secret-key-at-least-32-characters-long",
       "Issuer": "What2Wear.Web",
       "Audience": "What2Wear.Client",
       "ExpirationHours": "24"
     }
     ```
   - ⚠️ **IMPORTANT**: Change the SecretKey to a strong, random value in production

3. **Create Database**
   - Open Package Manager Console in Visual Studio
   - Select `What2Wear.Web` as default project
   - Run migrations:
     ```powershell
     Add-Migration InitialCreate
     Update-Database
     ```

4. **Run Web API**
   - Set `What2Wear.Web` as startup project
   - Run the application (default: `https://localhost:7118`)

### Mobile Client Setup

1. **Update API Base Address**
   - Open `What2Wear/MauiProgram.cs`
   - Update `BaseAddress` to match your Web API URL:
     ```csharp
     BaseAddress = new Uri("https://localhost:7118")
     ```

2. **Android Setup**
   - Ensure Android SDK (API 24+) is installed
   - Open `What2Wear/Platforms/Android/AndroidManifest.xml`
   - Verify camera and storage permissions are declared

3. **iOS Setup**
   - Ensure iOS SDK (15.0+) is installed
   - Camera and photo library permissions will be requested at runtime

4. **Run Mobile App**
   - Select target platform (Android, iOS, macOS, or Windows)
   - Run the application from Visual Studio

## Usage

### First Time User

1. **Launch App** → See Login page
2. **Sign Up** → Enter full name, email, and password
3. **Login** → Use credentials to sign in
4. **Photo Gallery** → See empty gallery initially

### Taking Photos

1. **On Photos Page** → Click "📷 Take Photo"
2. **On Capture Page**:
   - Click "📷 Take Photo" to use camera OR
   - Click "🖼️ Gallery" to select existing photo
3. **Select Clothing Type** from dropdown
4. **Add Tags** (optional, comma-separated)
5. **Add Notes** (optional)
6. **Click "Upload Photo"** to save

### Managing Photos

1. **View Collection** → Photos display in gallery grid
2. **Photo Details** → Each tile shows clothing type
3. **Delete Photo** → Long-press (or right-click) on photo
4. **Logout** → Click "Logout" button to sign out

## Security Considerations

### Production Deployment

1. **API Security**
   - Change JWT SecretKey to strong random value
   - Use HTTPS only (enforce with `UseHsts()`)
   - Validate file uploads (type, size, content)
   - Implement CORS appropriately
   - Add rate limiting
   - Use API keys for additional protection

2. **Database**
   - Use strong SQL Server passwords
   - Enable encryption at rest
   - Implement regular backups
   - Use parameterized queries (EF Core does this automatically)

3. **Mobile App**
   - Validate all user input
   - Don't hardcode API URLs; use configuration
   - Implement certificate pinning
   - Handle sensitive data securely (SecureStorage)

4. **User Data**
   - Implement data deletion on account removal
   - Comply with GDPR/privacy regulations
   - Log security events
   - Monitor for suspicious activity

## Troubleshooting

### Build Issues
- Clear NuGet cache: `nuget locals all -clear`
- Restore packages: `dotnet restore`
- Clean solution: `Build` → `Clean Solution`

### API Connection Issues
- Verify API is running on correct URL and port
- Check firewall allows local communication
- Verify `BaseAddress` in `MauiProgram.cs` is correct
- Check JWT token isn't expired

### Database Issues
- Verify SQL Server instance is running
- Check connection string in `appsettings.json`
- Ensure migrations have been applied: `Update-Database`
- Check SQL Server logs for errors

### Mobile App Crashes
- Check permissions are granted on device
- Verify network connectivity
- Check device has sufficient storage
- Monitor device logs via IDE debugger

## Dependencies

### NuGet Packages
- `Microsoft.Maui.Controls` - UI framework
- `Microsoft.EntityFrameworkCore` - ORM
- `Microsoft.EntityFrameworkCore.SqlServer` - SQL Server provider
- `System.IdentityModel.Tokens.Jwt` - JWT handling
- `BCrypt.Net-Next` - Password hashing

## Future Enhancements

- Photo filters and editing
- Outfit combinations/recommendations
- Cloud backup and sync
- Social sharing
- AI-powered clothing recognition
- Offline-first sync when online
- Dark mode optimization
- Multilingual support

## License

This project is for educational purposes. Modify as needed for your use case.

## Support

For issues or questions:
1. Check troubleshooting section above
2. Review API response messages
3. Enable logging in debug builds
4. Check platform-specific logs

## Contributors

Developed for What2Wear mobile application.
