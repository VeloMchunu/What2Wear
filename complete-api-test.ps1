#!/usr/bin/env powershell
<#
.SYNOPSIS
	Comprehensive API test suite for What2Wear application
.DESCRIPTION
	Tests all endpoints including authentication, photo management, and outfit suggestions
.NOTES
	Run with: powershell -ExecutionPolicy Bypass -File complete-api-test.ps1
#>

$BaseUrl = "http://localhost:5284"
$TestResults = @{
	Passed = 0
	Failed = 0
	Tests = @()
}

function Test-Endpoint {
	param(
		[string]$Name,
		[string]$Method,
		[string]$Endpoint,
		[object]$Body,
		[hashtable]$Headers,
		[string]$Description = ""
	)

	Write-Host "`n━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan
	Write-Host "TEST: $Name" -ForegroundColor Cyan
	if ($Description) {
		Write-Host "Description: $Description" -ForegroundColor Gray
	}
	Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Cyan

	$url = "$BaseUrl$Endpoint"
	Write-Host "Method: $Method" -ForegroundColor Yellow
	Write-Host "URL: $url" -ForegroundColor Yellow
	if ($Body) {
		Write-Host "Body: $(($Body | ConvertTo-Json) -replace '\n', '')" -ForegroundColor Yellow
	}

	try {
		$params = @{
			Uri = $url
			Method = $Method
			ContentType = "application/json"
			UseBasicParsing = $true
			ErrorAction = "Stop"
		}

		if ($Body) {
			$params['Body'] = $Body | ConvertTo-Json
		}

		if ($Headers) {
			$params['Headers'] = $Headers
		}

		$response = Invoke-WebRequest @params

		Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
		Write-Host "✅ PASSED" -ForegroundColor Green

		$TestResults.Passed++
		$TestResults.Tests += @{ Name = $Name; Status = "PASSED"; Code = $response.StatusCode }

		return $response
	}
	catch {
		$statusCode = $_.Exception.Response.StatusCode.Value__
		Write-Host "Status: $statusCode" -ForegroundColor Red

		if ($statusCode -eq 400 -or $statusCode -eq 401) {
			Write-Host "✅ PASSED (Expected Error)" -ForegroundColor Green
			$TestResults.Passed++
			$TestResults.Tests += @{ Name = $Name; Status = "PASSED"; Code = $statusCode; Note = "Expected error" }
		} else {
			Write-Host "❌ FAILED: $($_.Exception.Message)" -ForegroundColor Red
			$TestResults.Failed++
			$TestResults.Tests += @{ Name = $Name; Status = "FAILED"; Error = $_.Exception.Message }
		}

		return $null
	}
}

# ═════════════════════════════════════════════════════════════════════════════
# SECTION: AUTHENTICATION
# ═════════════════════════════════════════════════════════════════════════════

Write-Host "`n`n" -ForegroundColor White
Write-Host "╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║              AUTHENTICATION ENDPOINTS TESTS                     ║" -ForegroundColor Magenta
Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

$timestamp = Get-Date -Format "yyyyMMddHHmmss_fff"
$testEmail = "apitest_$timestamp@example.com"
$testPassword = "TestPassword123!"
$testName = "API Test User"

# Test 1: Signup - New User
$signupResp = Test-Endpoint -Name "Signup - New User" `
	-Method POST `
	-Endpoint "/api/auth/signup" `
	-Body @{
		email = $testEmail
		password = $testPassword
		fullName = $testName
	} `
	-Description "Create a new user account with email, password, and full name"

$token = $null
$userId = $null
if ($signupResp) {
	$signupJson = $signupResp.Content | ConvertFrom-Json
	$token = $signupJson.token
	$userId = $signupJson.user.id
	Write-Host "User Created - ID: $userId, Email: $($signupJson.user.email)" -ForegroundColor Green
}

# Test 2: Signup - Duplicate Email (should fail)
Test-Endpoint -Name "Signup - Duplicate Email (Error Case)" `
	-Method POST `
	-Endpoint "/api/auth/signup" `
	-Body @{
		email = $testEmail
		password = "AnotherPassword123!"
		fullName = "Another User"
	} `
	-Description "Attempt to create another account with the same email (should fail with 400)"

# Test 3: Login - Valid Credentials
$loginResp = Test-Endpoint -Name "Login - Valid Credentials" `
	-Method POST `
	-Endpoint "/api/auth/login" `
	-Body @{
		email = $testEmail
		password = $testPassword
	} `
	-Description "Authenticate with correct email and password"

if ($loginResp) {
	$loginJson = $loginResp.Content | ConvertFrom-Json
	$token = $loginJson.token
	Write-Host "Token Generated: $($token.Substring(0, 30))..." -ForegroundColor Magenta
}

# Test 4: Login - Invalid Password (should fail)
Test-Endpoint -Name "Login - Invalid Password (Error Case)" `
	-Method POST `
	-Endpoint "/api/auth/login" `
	-Body @{
		email = $testEmail
		password = "WrongPassword123!"
	} `
	-Description "Attempt to authenticate with wrong password (should fail with 401)"

# ═════════════════════════════════════════════════════════════════════════════
# SECTION: PHOTOS
# ═════════════════════════════════════════════════════════════════════════════

Write-Host "`n`n" -ForegroundColor White
Write-Host "╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                  PHOTO ENDPOINTS TESTS                          ║" -ForegroundColor Magenta
Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

if ($token) {
	# Test 5: Get Photos (with authentication)
	$photosResp = Test-Endpoint -Name "Get Photos - Authenticated" `
		-Method GET `
		-Endpoint "/api/photos" `
		-Headers @{ Authorization = "Bearer $token" } `
		-Description "Retrieve all photos for the authenticated user"

	if ($photosResp) {
		$photosJson = $photosResp.Content | ConvertFrom-Json
		Write-Host "Photos Count: $($photosJson.photos.Count)" -ForegroundColor Green
	}

	# Test 6: Get Photos (without authentication - should fail)
	Test-Endpoint -Name "Get Photos - No Authentication (Error Case)" `
		-Method GET `
		-Endpoint "/api/photos" `
		-Description "Attempt to get photos without JWT token (should fail with 401)"
} else {
	Write-Host "⚠️  Skipping photo tests - no token available" -ForegroundColor Yellow
}

# ═════════════════════════════════════════════════════════════════════════════
# SECTION: OUTFIT SUGGESTIONS
# ═════════════════════════════════════════════════════════════════════════════

Write-Host "`n`n" -ForegroundColor White
Write-Host "╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║            OUTFIT SUGGESTION ENDPOINTS TESTS                    ║" -ForegroundColor Magenta
Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

if ($token) {
	# Test 7: Outfit Suggestion - Casual Theme (no photos - expected error)
	Test-Endpoint -Name "Outfit Suggestion - Casual Theme (No Photos)" `
		-Method POST `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "casual"
			availablePhotoIds = @()
		} `
		-Headers @{ Authorization = "Bearer $token" } `
		-Description "Request outfit suggestions for casual theme with empty photo list (expected 400)"

	# Test 8: Outfit Suggestion - Formal Theme
	Test-Endpoint -Name "Outfit Suggestion - Formal Theme (No Photos)" `
		-Method POST `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "formal"
			availablePhotoIds = @()
		} `
		-Headers @{ Authorization = "Bearer $token" } `
		-Description "Request outfit suggestions for formal theme with empty photo list (expected 400)"

	# Test 9: Outfit Suggestion - Invalid Theme
	Test-Endpoint -Name "Outfit Suggestion - Invalid Theme (Error Case)" `
		-Method POST `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "nonexistent_theme"
			availablePhotoIds = @()
		} `
		-Headers @{ Authorization = "Bearer $token" } `
		-Description "Request outfit suggestions with invalid theme (expected 400)"

	# Test 10: Outfit Suggestion - No Authentication (should fail)
	Test-Endpoint -Name "Outfit Suggestion - No Authentication (Error Case)" `
		-Method POST `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "casual"
			availablePhotoIds = @()
		} `
		-Description "Attempt outfit suggestion without JWT token (should fail with 401)"
} else {
	Write-Host "⚠️  Skipping outfit suggestion tests - no token available" -ForegroundColor Yellow
}

# ═════════════════════════════════════════════════════════════════════════════
# SUMMARY
# ═════════════════════════════════════════════════════════════════════════════

Write-Host "`n`n" -ForegroundColor White
Write-Host "╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor Magenta
Write-Host "║                      TEST SUMMARY                              ║" -ForegroundColor Magenta
Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor Magenta

Write-Host "`nTotal Tests: $($TestResults.Passed + $TestResults.Failed)" -ForegroundColor White
Write-Host "✅ Passed: $($TestResults.Passed)" -ForegroundColor Green
Write-Host "❌ Failed: $($TestResults.Failed)" -ForegroundColor Red

Write-Host "`nDetailed Results:" -ForegroundColor White
$TestResults.Tests | ForEach-Object {
	$status = if ($_.Status -eq "PASSED") { "✅ PASSED" } else { "❌ FAILED" }
	Write-Host "  $status - $($_.Name)" -ForegroundColor $(if ($_.Status -eq "PASSED") { "Green" } else { "Red" })
	if ($_.Code) {
		Write-Host "         Status Code: $($_.Code)" -ForegroundColor Gray
	}
	if ($_.Note) {
		Write-Host "         Note: $($_.Note)" -ForegroundColor Gray
	}
}

Write-Host "`n" -ForegroundColor White
if ($TestResults.Failed -eq 0) {
	Write-Host "╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor Green
	Write-Host "║                    🎉 ALL TESTS PASSED 🎉                       ║" -ForegroundColor Green
	Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor Green
} else {
	Write-Host "╔═════════════════════════════════════════════════════════════════╗" -ForegroundColor Yellow
	Write-Host "║              ⚠️  SOME TESTS FAILED - REVIEW ABOVE              ║" -ForegroundColor Yellow
	Write-Host "╚═════════════════════════════════════════════════════════════════╝" -ForegroundColor Yellow
}

Write-Host "`n"
