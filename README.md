# linkzip

A .NET 8 Web API that converts text to compressed links notation and back.

## Overview

Linkzip provides two main endpoints:
- `/api/v1/zipper/zip` - Converts text to links notation with compression
- `/api/v1/zipper/unzip` - Converts links notation back to text

The API uses [Link.Foundation.Links.Notation](https://github.com/link-foundation/links-notation) for parsing and formatting, with compression logic ported from [deduplino](https://github.com/link-foundation/deduplino).

## Requirements

- .NET 8.0 SDK or later

## Getting Started

### Build and Run

```bash
cd Linkzip
dotnet restore
dotnet build
dotnet run
```

The API will be available at `http://localhost:5151` (or the port configured in your launch settings).

### Run Tests

```bash
cd Linkzip.Tests
dotnet test
```

## API Documentation

### POST /api/v1/zipper/zip

Converts text to compressed links notation.

**Request Body:**
```json
{
  "text": "hello world test"
}
```

**Response:**
```json
{
  "linksNotation": "(hello world test)",
  "patternsApplied": 0
}
```

**Example with compression:**
```bash
curl -X POST "http://localhost:5151/api/v1/zipper/zip" \
  -H "Content-Type: application/json" \
  -d '{"text": "papa loves mama son loves mama daughter loves mama"}'
```

### POST /api/v1/zipper/unzip

Converts links notation back to text.

**Request Body:**
```json
{
  "linksNotation": "hello world test"
}
```

**Response:**
```json
{
  "text": "hello world test"
}
```

**Example:**
```bash
curl -X POST "http://localhost:5151/api/v1/zipper/unzip" \
  -H "Content-Type: application/json" \
  -d '{"linksNotation": "hello world test"}'
```

## Examples

The `examples/` directory contains example scripts for testing the API:

- `test-api.sh` - Bash script for testing endpoints
- `test-api.py` - Python script for testing endpoints

To use the examples:

```bash
# Using bash
./examples/test-api.sh

# Using Python
pip install requests
./examples/test-api.py
```

## Swagger UI

When running in development mode, Swagger UI is available at:
`http://localhost:5151/swagger`

## Project Structure

```
Linkzip/
├── Controllers/
│   └── ZipperController.cs      # API endpoints
├── Models/
│   ├── ZipRequest.cs
│   ├── ZipResponse.cs
│   ├── UnzipRequest.cs
│   └── UnzipResponse.cs
├── Services/
│   ├── DeduplicationService.cs  # Compression logic
│   └── ZipperService.cs         # Main service
└── Program.cs                   # Application entry point

Linkzip.Tests/
└── ZipperServiceTests.cs        # Unit tests

examples/
├── test-api.sh                  # Bash test script
└── test-api.py                  # Python test script
```

## Technologies Used

- .NET 8.0
- ASP.NET Core Web API
- Link.Foundation.Links.Notation (0.12.0)
- Swashbuckle (Swagger/OpenAPI)
- xUnit (Testing)

## License

This project uses the Unlicense.