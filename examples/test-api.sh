#!/bin/bash

# Example script to test the Linkzip API

API_URL="${API_URL:-http://localhost:5000}"

echo "Testing Linkzip API at $API_URL"
echo "================================"

# Test 1: Zip simple text
echo -e "\n1. Testing /api/v1/zipper/zip with simple text:"
curl -X POST "$API_URL/api/v1/zipper/zip" \
  -H "Content-Type: application/json" \
  -d '{"text": "hello world test"}' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 2: Zip with repeated patterns
echo -e "\n2. Testing /api/v1/zipper/zip with repeated patterns:"
curl -X POST "$API_URL/api/v1/zipper/zip" \
  -H "Content-Type: application/json" \
  -d '{"text": "papa loves mama son loves mama daughter loves mama"}' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 3: Unzip
echo -e "\n3. Testing /api/v1/zipper/unzip:"
curl -X POST "$API_URL/api/v1/zipper/unzip" \
  -H "Content-Type: application/json" \
  -d '{"linksNotation": "hello world"}' \
  -w "\nHTTP Status: %{http_code}\n"

# Test 4: Zip and Unzip round trip
echo -e "\n4. Testing zip/unzip round trip:"
ZIPPED=$(curl -s -X POST "$API_URL/api/v1/zipper/zip" \
  -H "Content-Type: application/json" \
  -d '{"text": "test data compression"}')

echo "Zipped result: $ZIPPED"

NOTATION=$(echo $ZIPPED | jq -r '.linksNotation')

echo -e "\nUnzipping: $NOTATION"
curl -X POST "$API_URL/api/v1/zipper/unzip" \
  -H "Content-Type: application/json" \
  -d "{\"linksNotation\": \"$NOTATION\"}" \
  -w "\nHTTP Status: %{http_code}\n"

echo -e "\nAll tests completed!"
