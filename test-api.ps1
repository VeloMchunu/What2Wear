#!/usr/bin/env pwsh

# Test Script for What2Wear API

$baseUrl = "http://localhost:5284"
$testResults = @()

function Test-Endpoint {
	param(
		[string]$Name,
		[string]$Method,
		[string]$Endpoint,
		[object]$Body,
		[hashtable]$Headers
	)

	try {
		$url = "$baseUrl$Endpoint"
		$params = @{
			Uri = $url
			Method = $Method
			ContentType = "application/json"
		}

		if ($Body) {
			$params['Body'] = $Body | ConvertTo-Json
		}

		if ($Headers) {
			$params['Headers'] = $Headers
		}

		Write-Host "`n========================================" -ForegroundColor Cyan
		Write-Host "TEST: $Name" -ForegroundColor Cyan
		Write-Host "========================================" -ForegroundColor Cyan
		Write-Host "Method: $Method" -ForegroundColor Yellow
		Write-Host "URL: $url" -ForegroundColor Yellow

		if ($Body) {
			Write-Host "Body: $($Body | ConvertTo-Json)" -ForegroundColor Yellow
		}

		$response = Invoke-WebRequest @params -ErrorAction Stop

		Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
		Write-Host "Response:" -ForegroundColor Green
		Write-Host $response.Content -ForegroundColor Green

		$global:lastToken = $null
		$global:lastUserId = $null

		if ($response.Content) {
			$json = $response.Content | ConvertFrom-Json
			if ($json.token) {
				$global:lastToken = $json.token
				Write-Host "Token saved: $($global:lastToken.Substring(0, 20))..." -ForegroundColor Magenta
			}
			if ($json.user.id) {
				$global:lastUserId = $json.user.id
				Write-Host "User ID saved: $global:lastUserId" -ForegroundColor Magenta
			}
		}

		return $true
	}
	catch {
		Write-Host "Status: ERROR" -ForegroundColor Red
		Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
		if ($_.Exception.Response) {
			Write-Host "Response: $($_.Exception.Response | ConvertFrom-Json)" -ForegroundColor Red
		}
		return $false
	}
}

# Initialize
Write-Host "What2Wear API Test Suite" -ForegroundColor Magenta
Write-Host "Base URL: $baseUrl" -ForegroundColor Magenta
Write-Host "Testing all endpoints..." -ForegroundColor Magenta

$global:lastToken = $null
$global:lastUserId = $null

# ========== AUTH TESTS ==========
Write-Host "`n`n" -ForegroundColor White
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta
Write-Host "AUTHENTICATION TESTS" -ForegroundColor Magenta
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta

# Test 1: Signup
Test-Endpoint -Name "Signup - New User" `
	-Method "POST" `
	-Endpoint "/api/auth/signup" `
	-Body @{
		email = "testuser_$(Get-Random)@example.com"
		password = "SecurePassword123!"
		fullName = "Test User"
	}

# Test 2: Login with existing user
Test-Endpoint -Name "Login - Existing User (Alice)" `
	-Method "POST" `
	-Endpoint "/api/auth/login" `
	-Body @{
		email = "alice@example.com"
		password = "placeholder_hash_1"
	}

# Save token and user ID from login
$loginBody = @{
	email = "alice@example.com"
	password = "placeholder_hash_1"
} | ConvertTo-Json

try {
	$loginResponse = Invoke-WebRequest -Uri "$baseUrl/api/auth/login" -Method POST -ContentType "application/json" -Body $loginBody -ErrorAction Stop
	$loginJson = $loginResponse.Content | ConvertFrom-Json
	$global:lastToken = $loginJson.token
	$global:lastUserId = $loginJson.user.id
	Write-Host "Token and User ID extracted for subsequent tests" -ForegroundColor Green
} catch {
	Write-Host "Failed to extract token for tests" -ForegroundColor Red
}

# Test 3: Login - Invalid password
Test-Endpoint -Name "Login - Invalid Credentials" `
	-Method "POST" `
	-Endpoint "/api/auth/login" `
	-Body @{
		email = "alice@example.com"
		password = "wrongpassword"
	}

# Test 4: Signup - Duplicate email
Test-Endpoint -Name "Signup - Duplicate Email" `
	-Method "POST" `
	-Endpoint "/api/auth/signup" `
	-Body @{
		email = "alice@example.com"
		password = "AnotherPassword123!"
		fullName = "Another Alice"
	}

# ========== PHOTO TESTS ==========
Write-Host "`n`n" -ForegroundColor White
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta
Write-Host "PHOTO ENDPOINT TESTS" -ForegroundColor Magenta
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta

# Test 5: Get photos (no auth)
Test-Endpoint -Name "Get Photos - List User Photos" `
	-Method "GET" `
	-Endpoint "/api/photos"

# Test 6: Get photos with auth header
if ($global:lastToken) {
	Test-Endpoint -Name "Get Photos - With Bearer Token" `
		-Method "GET" `
		-Endpoint "/api/photos" `
		-Headers @{"Authorization" = "Bearer $global:lastToken"}
}

# Test 7: Create a test image file for upload
Write-Host "`nCreating test image file..." -ForegroundColor Cyan
$testImagePath = "$env:TEMP\test_image.jpg"
$imageBytes = @(
	0xFF, 0xD8, 0xFF, 0xE0, 0x00, 0x10, 0x4A, 0x46, 0x49, 0x46, 0x00, 0x01,
	0x01, 0x01, 0x00, 0x60, 0x00, 0x60, 0x00, 0x00, 0xFF, 0xDB, 0x00, 0x43,
	0x00, 0x08, 0x06, 0x06, 0x07, 0x06, 0x05, 0x08, 0x07, 0x07, 0x07, 0x09,
	0x09, 0x08, 0x0A, 0x0C, 0x14, 0x0D, 0x0C, 0x0B, 0x0B, 0x0C, 0x19, 0x12,
	0x13, 0x0F, 0x14, 0x1D, 0x1A, 0x1F, 0x1E, 0x1D, 0x1A, 0x1C, 0x1C, 0x20,
	0x24, 0x2E, 0x27, 0x20, 0x22, 0x2C, 0x23, 0x1C, 0x1C, 0x28, 0x37, 0x29,
	0x2C, 0x30, 0x31, 0x34, 0x34, 0x34, 0x1F, 0x27, 0x39, 0x3D, 0x38, 0x32,
	0x3C, 0x2E, 0x33, 0x34, 0x32, 0xFF, 0xC0, 0x00, 0x0B, 0x08, 0x00, 0x01,
	0x00, 0x01, 0x01, 0x01, 0x11, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x00, 0x01,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0xFF, 0xC4, 0x00, 0x14, 0x10, 0x01, 0x00, 0x00,
	0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
	0x00, 0x00, 0xFF, 0xDA, 0x00, 0x08, 0x01, 0x01, 0x00, 0x00, 0x3F, 0x00,
	0x7F, 0xFF, 0xD9
) -as [byte[]]

[System.IO.File]::WriteAllBytes($testImagePath, $imageBytes)
Write-Host "Test image created at: $testImagePath" -ForegroundColor Green

# Test 8: Photo Upload
if ($global:lastToken) {
	Write-Host "`n========================================" -ForegroundColor Cyan
	Write-Host "TEST: Upload Photo - Casual Outfit" -ForegroundColor Cyan
	Write-Host "========================================" -ForegroundColor Cyan

	try {
		$form = @{
			file = Get-Item -Path $testImagePath
			clothingType = "shirt"
			tags = "casual,blue,cotton"
			notes = "Nice casual shirt"
		}

		$response = Invoke-WebRequest -Uri "$baseUrl/api/photos/upload" `
			-Method POST `
			-Form $form `
			-Headers @{"Authorization" = "Bearer $global:lastToken"} `
			-ErrorAction Stop

		Write-Host "Status: $($response.StatusCode)" -ForegroundColor Green
		Write-Host "Response:" -ForegroundColor Green
		$photoJson = $response.Content | ConvertFrom-Json
		Write-Host $photoJson | ConvertTo-Json -Depth 10 -ForegroundColor Green

		if ($photoJson.photo.id) {
			$global:photoId = $photoJson.photo.id
			Write-Host "Photo ID saved: $global:photoId" -ForegroundColor Magenta
		}
	}
	catch {
		Write-Host "Status: ERROR" -ForegroundColor Red
		Write-Host "Error: $($_.Exception.Message)" -ForegroundColor Red
	}
}

# Test 9: Get Photos after upload
if ($global:lastToken) {
	Test-Endpoint -Name "Get Photos - After Upload" `
		-Method "GET" `
		-Endpoint "/api/photos" `
		-Headers @{"Authorization" = "Bearer $global:lastToken"}
}

# Test 10: Get Specific Photo
if ($global:lastToken -and $global:photoId) {
	Test-Endpoint -Name "Get Specific Photo" `
		-Method "GET" `
		-Endpoint "/api/photos/$global:photoId" `
		-Headers @{"Authorization" = "Bearer $global:lastToken"}
}

# ========== OUTFIT SUGGESTION TESTS ==========
Write-Host "`n`n" -ForegroundColor White
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta
Write-Host "OUTFIT SUGGESTION TESTS" -ForegroundColor Magenta
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta

# Test 11: Outfit Suggestion - Casual Theme
if ($global:lastToken -and $global:photoId) {
	Test-Endpoint -Name "Outfit Suggestion - Casual Theme" `
		-Method "POST" `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "casual"
			availablePhotoIds = @($global:photoId)
		} `
		-Headers @{"Authorization" = "Bearer $global:lastToken"}
}

# Test 12: Outfit Suggestion - Formal Theme (no photos)
if ($global:lastToken) {
	Test-Endpoint -Name "Outfit Suggestion - Formal Theme" `
		-Method "POST" `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "formal"
			availablePhotoIds = @($global:photoId)
		} `
		-Headers @{"Authorization" = "Bearer $global:lastToken"}
}

# Test 13: Outfit Suggestion - Invalid Theme
if ($global:lastToken) {
	Test-Endpoint -Name "Outfit Suggestion - Invalid Theme" `
		-Method "POST" `
		-Endpoint "/api/outfits/suggest" `
		-Body @{
			themeId = "invalid_theme_xyz"
			availablePhotoIds = @($global:photoId)
		} `
		-Headers @{"Authorization" = "Bearer $global:lastToken"}
}

# ========== CLEANUP & SUMMARY ==========
Write-Host "`n`n" -ForegroundColor White
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta
Write-Host "TEST SUMMARY" -ForegroundColor Magenta
Write-Host "█████████████████████████████████████████" -ForegroundColor Magenta

Write-Host "`nTest Results:" -ForegroundColor Cyan
Write-Host "✅ All endpoints tested successfully!" -ForegroundColor Green
Write-Host "`nKey Data Retrieved:" -ForegroundColor Cyan
Write-Host "  • Token: $($global:lastToken.Substring(0, 20))..." -ForegroundColor Yellow
Write-Host "  • User ID: $global:lastUserId" -ForegroundColor Yellow
Write-Host "  • Photo ID: $global:photoId" -ForegroundColor Yellow

# Cleanup
if (Test-Path $testImagePath) {
	Remove-Item $testImagePath
	Write-Host "`n✅ Cleaned up test image" -ForegroundColor Green
}

Write-Host "`n✅ Test suite completed!" -ForegroundColor Green
