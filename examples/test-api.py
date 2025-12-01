#!/usr/bin/env python3
"""
Example Python script to test the Linkzip API.
"""

import requests
import json
import os

API_URL = os.environ.get('API_URL', 'http://localhost:5000')

def test_zip(text):
    """Test the zip endpoint."""
    response = requests.post(
        f"{API_URL}/api/v1/zipper/zip",
        json={"text": text}
    )
    response.raise_for_status()
    return response.json()

def test_unzip(links_notation):
    """Test the unzip endpoint."""
    response = requests.post(
        f"{API_URL}/api/v1/zipper/unzip",
        json={"linksNotation": links_notation}
    )
    response.raise_for_status()
    return response.json()

def main():
    print(f"Testing Linkzip API at {API_URL}")
    print("=" * 50)

    # Test 1: Simple zip
    print("\n1. Testing /api/v1/zipper/zip with simple text:")
    result = test_zip("hello world test")
    print(json.dumps(result, indent=2))

    # Test 2: Zip with repeated patterns
    print("\n2. Testing /api/v1/zipper/zip with repeated patterns:")
    result = test_zip("papa loves mama son loves mama daughter loves mama")
    print(json.dumps(result, indent=2))
    print(f"Patterns applied: {result['patternsApplied']}")

    # Test 3: Unzip
    print("\n3. Testing /api/v1/zipper/unzip:")
    result = test_unzip("hello world")
    print(json.dumps(result, indent=2))

    # Test 4: Round trip
    print("\n4. Testing zip/unzip round trip:")
    original = "test data compression with links notation"
    zipped = test_zip(original)
    print(f"Original: {original}")
    print(f"Zipped: {zipped['linksNotation']}")
    print(f"Patterns: {zipped['patternsApplied']}")

    unzipped = test_unzip(zipped['linksNotation'])
    print(f"Unzipped: {unzipped['text']}")

    print("\nAll tests completed!")

if __name__ == "__main__":
    main()
