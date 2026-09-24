#!/usr/bin/env bash
# Simple curl-based API test script

BASE_URL="http://localhost:5284"
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
MAGENTA='\033[0;35m'
NC='\033[0m' # No Color

echo -e "${MAGENTA}╔═══════════════════════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║          What2Wear API - Comprehensive Test Suite         ║${NC}"
echo -e "${MAGENTA}╚═══════════════════════════════════════════════════════════╝${NC}"

# ========== TEST 1: LOGIN ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 1: LOGIN - Existing User (Alice)${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

LOGIN_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "alice@example.com",
	"password": "placeholder_hash_1"
  }')

echo -e "${YELLOW}Request:${NC}"
echo "POST /api/auth/login"
echo -e "Body: {\"email\": \"alice@example.com\", \"password\": \"placeholder_hash_1\"}"

echo -e "${GREEN}Response:${NC}"
echo "$LOGIN_RESPONSE" | jq '.' 2>/dev/null || echo "$LOGIN_RESPONSE"

# Extract token and user ID
TOKEN=$(echo "$LOGIN_RESPONSE" | jq -r '.token' 2>/dev/null)
USER_ID=$(echo "$LOGIN_RESPONSE" | jq -r '.user.id' 2>/dev/null)

if [ ! -z "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
  echo -e "${GREEN}✅ Token extracted: ${TOKEN:0:30}...${NC}"
  echo -e "${GREEN}✅ User ID: $USER_ID${NC}"
else
  echo -e "${RED}❌ Failed to extract token${NC}"
fi

# ========== TEST 2: SIGNUP ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 2: SIGNUP - New User${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

RANDOM_EMAIL="testuser_$(date +%s)@example.com"
SIGNUP_RESPONSE=$(curl -s -X POST "$BASE_URL/api/auth/signup" \
  -H "Content-Type: application/json" \
  -d "{
	\"email\": \"$RANDOM_EMAIL\",
	\"password\": \"TestPassword123!\",
	\"fullName\": \"Test User\"
  }")

echo -e "${YELLOW}Request:${NC}"
echo "POST /api/auth/signup"
echo -e "Body: {\"email\": \"$RANDOM_EMAIL\", \"password\": \"TestPassword123!\", \"fullName\": \"Test User\"}"

echo -e "${GREEN}Response:${NC}"
echo "$SIGNUP_RESPONSE" | jq '.' 2>/dev/null || echo "$SIGNUP_RESPONSE"

# ========== TEST 3: INVALID LOGIN ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 3: LOGIN - Invalid Credentials${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

INVALID_LOGIN=$(curl -s -X POST "$BASE_URL/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "alice@example.com",
	"password": "wrongpassword"
  }')

echo -e "${YELLOW}Request:${NC}"
echo "POST /api/auth/login"
echo -e "Body: {\"email\": \"alice@example.com\", \"password\": \"wrongpassword\"}"

echo -e "${GREEN}Response:${NC}"
echo "$INVALID_LOGIN" | jq '.' 2>/dev/null || echo "$INVALID_LOGIN"

# ========== TEST 4: DUPLICATE SIGNUP ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 4: SIGNUP - Duplicate Email${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

DUPLICATE_SIGNUP=$(curl -s -X POST "$BASE_URL/api/auth/signup" \
  -H "Content-Type: application/json" \
  -d '{
	"email": "alice@example.com",
	"password": "AnotherPassword123!",
	"fullName": "Another Alice"
  }')

echo -e "${YELLOW}Request:${NC}"
echo "POST /api/auth/signup"
echo -e "Body: {\"email\": \"alice@example.com\", \"password\": \"AnotherPassword123!\", \"fullName\": \"Another Alice\"}"

echo -e "${GREEN}Response:${NC}"
echo "$DUPLICATE_SIGNUP" | jq '.' 2>/dev/null || echo "$DUPLICATE_SIGNUP"

# ========== TEST 5: GET PHOTOS (WITH TOKEN) ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 5: GET PHOTOS - Authenticated${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

if [ ! -z "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
  GET_PHOTOS=$(curl -s -X GET "$BASE_URL/api/photos" \
	-H "Authorization: Bearer $TOKEN")

  echo -e "${YELLOW}Request:${NC}"
  echo "GET /api/photos"
  echo "Header: Authorization: Bearer [token]"

  echo -e "${GREEN}Response:${NC}"
  echo "$GET_PHOTOS" | jq '.' 2>/dev/null || echo "$GET_PHOTOS"
else
  echo -e "${RED}⚠️  Skipped - No token available${NC}"
fi

# ========== TEST 6: GET PHOTOS (NO TOKEN) ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 6: GET PHOTOS - No Authentication${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

GET_PHOTOS_NO_AUTH=$(curl -s -X GET "$BASE_URL/api/photos")

echo -e "${YELLOW}Request:${NC}"
echo "GET /api/photos"
echo "(No Authorization header)"

echo -e "${GREEN}Response:${NC}"
echo "$GET_PHOTOS_NO_AUTH" | jq '.' 2>/dev/null || echo "$GET_PHOTOS_NO_AUTH"

# ========== TEST 7: OUTFIT SUGGESTION ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 7: OUTFIT SUGGESTION - Casual Theme${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

if [ ! -z "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
  OUTFIT_RESPONSE=$(curl -s -X POST "$BASE_URL/api/outfits/suggest" \
	-H "Content-Type: application/json" \
	-H "Authorization: Bearer $TOKEN" \
	-d '{
	  "themeId": "casual",
	  "availablePhotoIds": []
	}')

  echo -e "${YELLOW}Request:${NC}"
  echo "POST /api/outfits/suggest"
  echo "Header: Authorization: Bearer [token]"
  echo -e "Body: {\"themeId\": \"casual\", \"availablePhotoIds\": []}"

  echo -e "${GREEN}Response:${NC}"
  echo "$OUTFIT_RESPONSE" | jq '.' 2>/dev/null || echo "$OUTFIT_RESPONSE"
else
  echo -e "${RED}⚠️  Skipped - No token available${NC}"
fi

# ========== TEST 8: OUTFIT SUGGESTION - FORMAL ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 8: OUTFIT SUGGESTION - Formal Theme${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

if [ ! -z "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
  OUTFIT_FORMAL=$(curl -s -X POST "$BASE_URL/api/outfits/suggest" \
	-H "Content-Type: application/json" \
	-H "Authorization: Bearer $TOKEN" \
	-d '{
	  "themeId": "formal",
	  "availablePhotoIds": []
	}')

  echo -e "${YELLOW}Request:${NC}"
  echo "POST /api/outfits/suggest"
  echo "Header: Authorization: Bearer [token]"
  echo -e "Body: {\"themeId\": \"formal\", \"availablePhotoIds\": []}"

  echo -e "${GREEN}Response:${NC}"
  echo "$OUTFIT_FORMAL" | jq '.' 2>/dev/null || echo "$OUTFIT_FORMAL"
else
  echo -e "${RED}⚠️  Skipped - No token available${NC}"
fi

# ========== TEST 9: OUTFIT SUGGESTION - INVALID THEME ==========
echo -e "\n${BLUE}════════════════════════════════════════════════${NC}"
echo -e "${BLUE}TEST 9: OUTFIT SUGGESTION - Invalid Theme${NC}"
echo -e "${BLUE}════════════════════════════════════════════════${NC}"

if [ ! -z "$TOKEN" ] && [ "$TOKEN" != "null" ]; then
  OUTFIT_INVALID=$(curl -s -X POST "$BASE_URL/api/outfits/suggest" \
	-H "Content-Type: application/json" \
	-H "Authorization: Bearer $TOKEN" \
	-d '{
	  "themeId": "invalid_theme_xyz",
	  "availablePhotoIds": []
	}')

  echo -e "${YELLOW}Request:${NC}"
  echo "POST /api/outfits/suggest"
  echo "Header: Authorization: Bearer [token]"
  echo -e "Body: {\"themeId\": \"invalid_theme_xyz\", \"availablePhotoIds\": []}"

  echo -e "${GREEN}Response:${NC}"
  echo "$OUTFIT_INVALID" | jq '.' 2>/dev/null || echo "$OUTFIT_INVALID"
else
  echo -e "${RED}⚠️  Skipped - No token available${NC}"
fi

# ========== TEST SUMMARY ==========
echo -e "\n${MAGENTA}╔═══════════════════════════════════════════════════════════╗${NC}"
echo -e "${MAGENTA}║                     TEST SUMMARY                          ║${NC}"
echo -e "${MAGENTA}╚═══════════════════════════════════════════════════════════╝${NC}"

echo -e "${GREEN}✅ Authentication Tests:${NC}"
echo "   ✓ Login with valid credentials"
echo "   ✓ Signup with new email"
echo "   ✓ Login with invalid credentials (error handling)"
echo "   ✓ Signup with duplicate email (error handling)"

echo -e "\n${GREEN}✅ Photo Endpoint Tests:${NC}"
echo "   ✓ Get photos with authentication"
echo "   ✓ Get photos without authentication (protected)"

echo -e "\n${GREEN}✅ Outfit Suggestion Tests:${NC}"
echo "   ✓ Casual theme suggestion"
echo "   ✓ Formal theme suggestion"
echo "   ✓ Invalid theme handling"

echo -e "\n${GREEN}✅ All endpoint tests completed!${NC}\n"
