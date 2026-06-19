# ⚡ Quick Start Guide

Get your What2Wear app running in **10 minutes**!

---

## 🎯 5-Minute Setup

### Step 1: Open Solution (1 min)
```bash
cd C:\Users\VelocityM\source\repos\What2Wear
start What2Wear.sln
```

### Step 2: Configure Database (2 min)
1. Open `What2Wear.Web/appsettings.json`
2. Verify connection string looks correct:
   ```json
   "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=What2Wear;Trusted_Connection=true;"
   ```
3. Save file

### Step 3: Create Database (1 min)
1. **Tools** → **NuGet Package Manager** → **Package Manager Console**
2. Set default project to: **What2Wear.Web**
3. Run these commands:
   ```powershell
   Add-Migration InitialCreate
   Update-Database
   ```

### Step 4: Start Web API (1 min)
1. Right-click **What2Wear.Web** → **Set as Startup Project**
2. Press **Ctrl+F5** (or F5)
3. Wait for: `Application started. Press Ctrl+C to shut down.`

### Step 5: Run Mobile App (1 min)
1. Right-click **What2Wear** → **Set as Startup Project**
2. Select Android Emulator or Device from dropdown
3. Press **Ctrl+F5**
4. App launches!

---

## ✅ Verify Everything Works

### Check API is Running
Open browser: `https://localhost:7118`
- Should see an API response (not an error page)

### Test Mobile App
1. Click **Sign Up**
2. Enter details:
   - Full Name: John Doe
   - Email: john@example.com
   - Password: password123
3. Click **Sign Up**
4. You should see the photo gallery (empty)
5. Click **📷 Take Photo**
6. Take or select a photo
7. Select clothing type and upload
8. Photo should appear in gallery!

---

## 🔧 Common Issues & Quick Fixes

### "Database not found"
```powershell
# In Package Manager Console:
Update-Database
```

### "Port already in use"
```powershell
# Find process using port 7118:
netstat -ano | findstr :7118
# Kill it:
taskkill /PID <PID> /F
```

### "API connection failed"
- Verify `BaseAddress` in `What2Wear/MauiProgram.cs` is: `https://localhost:7118`
- Verify Web API is running
- Check firewall isn't blocking port 7118

### "XAML errors"
- Clean solution: **Build** → **Clean Solution**
- Rebuild: **Build** → **Rebuild Solution**

---

## 📁 What Gets Created

After running, you'll have:
- ✅ Database: `C:\Users\[username]\AppData\Local\Microsoft\Microsoft SQL Server Local DB\Instances\`
- ✅ Photos: `What2Wear\What2Wear.Web\wwwroot\uploads\[userId]\`
- ✅ Logs: Visual Studio Output window

---

## 🎮 Test the Full Flow

### Signup & Login Test
1. **Signup** → Create account
2. **Logout** → Clear session
3. **Login** → Sign back in (token persists!)

### Photo Upload Test
1. **Capture Photo** → Take/select image
2. **Set Type** → Choose clothing type
3. **Add Tags** → "casual, summer, blue"
4. **Add Notes** → "My favorite shirt"
5. **Upload** → Should succeed instantly
6. **View Gallery** → Photo appears!

### Offline Test (Optional)
1. Stop Web API
2. Go to Photos page
3. No error - app handles gracefully

---

## 📱 Run on Different Platforms

### Android Emulator
- Already selected by default
- First run may take 1-2 minutes

### iOS Simulator (macOS only)
1. Device dropdown → iPhone Simulator
2. Run (Ctrl+F5)
3. Simulator launches

### Windows Desktop
1. Device dropdown → Windows Machine
2. Run (Ctrl+F5)
3. Desktop app launches

### Physical Device
1. Connect device via USB
2. Enable Developer Mode
3. Device appears in dropdown
4. Run (Ctrl+F5)

---

## 🚀 What's Next?

### Now You Have:
- ✅ Full authentication system
- ✅ Cross-platform mobile app
- ✅ Backend API
- ✅ Photo management system

### Next Steps:
1. **Test thoroughly** - Try uploading many photos
2. **Read docs** - See `README.md` for full feature list
3. **Customize** - Modify colors, text, features
4. **Deploy** - See deployment guide in docs
5. **Share** - Invite others to test!

---

## 📖 Detailed Guides

For more information, see:
- **README.md** - Full feature overview
- **ENVIRONMENT_SETUP.md** - Detailed configuration
- **DATABASE_SETUP.md** - Database management
- **API_TESTING.md** - API endpoint testing

---

## ⚙️ Configuration Checklist

Before sharing/deploying:

- [ ] Change JWT SecretKey in `appsettings.json` (⚠️ IMPORTANT)
- [ ] Test on real device (not just emulator)
- [ ] Verify photos upload correctly
- [ ] Test login persistence
- [ ] Test logout works
- [ ] Verify image file limits
- [ ] Check permissions on Android/iOS
- [ ] Test with multiple users

---

## 💡 Pro Tips

1. **Faster Rebuilds**: Close other Visual Studio instances
2. **Storage Space**: Photos stored in `wwwroot/uploads/` - can grow large
3. **Token Issues**: Clear app data and log in again
4. **Performance**: Use Release configuration for testing on device
5. **Debugging**: F11 to step through code, F12 for breakpoints

---

## 🆘 Need Help?

1. **Build errors?** → Clean and Rebuild
2. **Database errors?** → Run `Update-Database` in Package Manager Console
3. **API not responding?** → Verify Web API is running (should show in VS title bar)
4. **Mobile app crash?** → Check Output window for error messages
5. **Still stuck?** → See detailed guides listed above

---

## ✨ You're All Set!

**Time to launch**: ~5-10 minutes ⏱️

Your What2Wear mobile app is ready to use! 🎉

**Next: Follow step 1 above or jump to the full README.md for advanced features.**

---

**Happy coding!** 💪
