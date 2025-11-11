# Dignus Candidate API Documentation

**Responsáveis:** Bruno, Vitor

## Overview

The Dignus Candidate API is the backend core system for managing candidate selection processes, including test submissions, AI-powered analysis, and recruiter integration. It provides comprehensive endpoints for handling candidate assessments, media uploads, job applications, and evaluation workflows.

## Quick Start

### Prerequisites
- .NET 9.0 SDK
- PostgreSQL database
- Azure Storage Account (for file storage)

### Running the Application

```bash
cd Dignus.Candidate.Back
dotnet restore
dotnet run
```

The API will be available at:
- **HTTPS:** https://localhost:7214
- **HTTP:** http://localhost:5076
- **Swagger:** https://localhost:7214/swagger

### Database Setup
The application automatically applies database migrations on startup. Ensure your PostgreSQL connection string is configured in `appsettings.json`.

To manually apply migrations:
```bash
cd Dignus.Data
dotnet ef database update --startup-project ../Dignus.Candidate.Back --context DignusContextNew
```

### Configuration Requirements

Before running the application, ensure the following configurations are properly set:

1. **PostgreSQL Connection String** - Required for database operations
2. **Azure Storage Connection String** - Required for video upload endpoints
3. **JWT Settings** - Required for authentication
4. **Email Settings** - Required for authentication token delivery
5. **External AI Agent Settings** - Required for video analysis (Portuguese, Math, Interview tests)

---

## Complete API Documentation

For the complete, detailed API documentation including all request/response examples, authentication, and security information, please see:

**[Complete API Documentation](../docs/API_DOCUMENTATION.md)**

The documentation covers all 11 controllers with **70+ endpoints** including:
- ✅ **NEW: CPF + Email Token Authentication** (CandidateAuthController)
- ✅ **NEW: LGPD Consent Management** (ConsentController)
- ✅ **NEW: Admin Question Management** (AdminQuestionGroupsController)
- ✅ **NEW: Question Templates Management** (QuestionTemplatesController)
- Candidate Management
- Job Management
- Unified Tests v2 API
- Evaluation & Reporting
- Status & Health Checks
- Databricks Integration

---

## Quick API Reference

### 1. Authentication & Authorization

#### CPF + Email Token Authentication (NEW)
**Base Route:** `/api/candidate-auth`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/candidate-auth/request-token` | Request 6-digit token via email |
| POST | `/api/candidate-auth/validate-token` | Validate token and receive JWT |
| GET | `/api/candidate-auth/lockout-status/{cpf}` | Check if CPF is locked out |

**Key Features:**
- 6-digit token sent via email
- 15-minute token expiration
- Rate limiting: 5 attempts per 15 minutes
- 30-minute lockout after 5 failed attempts
- Returns JWT with 24-hour expiration

#### Legacy Authentication
**Base Route:** `/api/auth`

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/login` | Login with CPF only (legacy) |
| GET | `/api/auth/progress/{candidateId}` | Get candidate progress |

---

### 2. LGPD Consent Management (NEW)

**Base Route:** `/api/consent`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/consent/status/{cpf}` | Check consent status | Public |
| POST | `/api/consent` | Submit LGPD consent | JWT |
| GET | `/api/consent/privacy-policy` | Get privacy policy info | Public |

**LGPD Requirements:**
- All three consents are mandatory:
  - Privacy Policy acceptance
  - Data Sharing consent
  - Credit Analysis consent
- Logs IP address, User-Agent, and timestamp
- One-time consent (cannot revert)

---

### 3. Admin Question Management (NEW)

#### Question Groups Management
**Base Route:** `/api/admin/question-groups`
**Authorization:** Admin/Recruiter roles required

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/admin/question-groups` | List all question groups |
| GET | `/api/admin/question-groups/{groupId}` | Get group details |
| POST | `/api/admin/question-groups` | Create new group |
| PUT | `/api/admin/question-groups/{groupId}` | Update group metadata |
| PUT | `/api/admin/question-groups/{groupId}/questions/{questionId}` | Update specific question |
| PATCH | `/api/admin/question-groups/{groupId}/questions/reorder` | Reorder questions |
| PATCH | `/api/admin/question-groups/{groupId}/activate` | Activate group |
| PATCH | `/api/admin/question-groups/{groupId}/deactivate` | Deactivate group |
| DELETE | `/api/admin/question-groups/{groupId}` | Delete group |

**Key Features:**
- Group questions by test type
- Only ONE active group per test type
- Questions have display order
- Soft delete support

**Usage:**
- **Portuguese, Math, Interview:** Use Question Groups
- Questions appear in the exact order specified by `groupOrder`
- Create one group with the exact number of questions needed
- Activate the group to make it available for tests

#### Question Templates Management
**Base Route:** `/api/admin/question-templates`
**Authorization:** Admin/Recruiter roles required

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/admin/question-templates` | Create question with answer |
| PUT | `/api/admin/question-templates/{id}` | Update question |
| GET | `/api/admin/question-templates/{id}` | Get question (no answer) |
| GET | `/api/admin/question-templates/{id}/with-answer` | Get with correct answer |
| GET | `/api/admin/question-templates` | List questions (paginated) |
| GET | `/api/admin/question-templates/by-difficulty` | Filter by difficulty |
| GET | `/api/admin/question-templates/by-category` | Filter by category |
| DELETE | `/api/admin/question-templates/{id}` | Deactivate question |
| POST | `/api/admin/question-templates/{id}/reactivate` | Reactivate question |
| GET | `/api/admin/question-templates/statistics` | Get question bank stats |

**Key Features:**
- Random question selection from template pool
- Support for multiple choice (single or multiple answers)
- Automatic grading based on correct answer
- Difficulty levels and category tagging
- Soft delete support

**Usage:**
- **Psychology, Visual Retention:** Use Question Templates
- System randomly selects questions from all active templates
- Recommended: Create 60-100+ templates for good variety
- Each template can have correct answers for auto-grading

#### Question Management Summary

| Test Type | System | Count | Selection Method | Auto-Graded |
|-----------|--------|-------|------------------|-------------|
| Portuguese | Question Groups | 3 | Ordered (GroupOrder) | ❌ (AI Agent) |
| Math | Question Groups | 2 | Ordered (GroupOrder) | ❌ (AI Agent) |
| Interview | Question Groups | 5 | Ordered (GroupOrder) | ❌ (AI Agent) |
| Psychology | Question Templates | 50 | Random selection | ✅ Server-side |
| Visual Retention | Question Templates | 15 | Random selection | ✅ Server-side |

---

### 4. Candidate Management

**Base Route:** `/api/candidate`
**Authorization:** JWT required

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/candidate/{id}` | Get candidate by ID |
| POST | `/api/candidate` | Create new candidate |
| PUT | `/api/candidate/{id}` | Update candidate |
| GET | `/api/candidate/search` | Search candidates (paginated) |
| GET | `/api/candidate/{id}/profile` | Get simplified profile |
| PATCH | `/api/candidate/{id}/status` | Update status only |
| GET | `/api/candidate/{id}/job` | Get candidate's job info |

---

### 5. Job Management

**Base Route:** `/api/job`
**Authorization:** JWT required (optional for search)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/job` | Search jobs with filters |
| GET | `/api/job/{id}` | Get job details |
| GET | `/api/job/statistics` | Get job statistics |
| POST | `/api/job/{id}/apply` | Apply to job |
| GET | `/api/job/{id}/candidates` | Get job applicants (recruiter only) |

---

### 6. Tests (Unified v2 API)

**Base Route:** `/api/v2/tests`
**Authorization:** JWT required with IDOR protection

#### Test Lifecycle

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v2/tests` | Create new test |
| GET | `/api/v2/tests/{testId}?candidateId={id}` | Get test by ID |
| GET | `/api/v2/tests/candidate/{candidateId}` | Get all candidate tests |
| POST | `/api/v2/tests/{testId}/start?candidateId={id}` | Start test |
| POST | `/api/v2/tests/{testId}/submit` | Submit test for grading |
| GET | `/api/v2/tests/{testId}/questions?candidateId={id}` | Get test questions |
| GET | `/api/v2/tests/{testId}/status?candidateId={id}` | Get test status |
| GET | `/api/v2/tests/candidate/{candidateId}/can-start/{testType}` | Check if can start |

#### Video Responses (Portuguese/Interview)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v2/tests/{testId}/videos` | Upload video response |
| GET | `/api/v2/tests/{testId}/videos?candidateId={id}` | Get all videos |
| GET | `/api/v2/tests/{testId}/videos/{videoId}?candidateId={id}` | Get specific video |
| GET | `/api/v2/tests/{testId}/videos/{videoId}/url?candidateId={id}` | Get secure video URL |
| DELETE | `/api/v2/tests/{testId}/videos/{videoId}?candidateId={id}` | Delete video |

#### Question Responses (Multiple Choice)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/v2/tests/{testId}/answers?candidateId={id}` | Submit/update answers (batch) |
| GET | `/api/v2/tests/{testId}/answers?candidateId={id}` | Get all answers |
| PUT | `/api/v2/tests/{testId}/answers/{responseId}?candidateId={id}` | Update specific answer |
| DELETE | `/api/v2/tests/{testId}/answers/{responseId}?candidateId={id}` | Delete answer |

**Test Types:**
- 1: Portuguese (3 video questions + 1 reading text = 4 videos total)
- 2: Math (2 video questions)
- 3: Psychology (50 multiple choice questions)
- 4: VisualRetention (15 multiple choice questions)
- 5: Interview (5 video questions)

**Test Status:**
- 0: NotStarted
- 1: InProgress
- 2: Submitted
- 3: Approved
- 4: Rejected

#### Test Workflows by Type

##### Portuguese Test (Video-Based)
**Flow:**
1. Candidate receives 1 reading text + 3 questions
2. Records 1 video reading the text aloud
3. Records 3 separate videos answering each question
4. **Total:** 4 videos required
5. Each video upload automatically triggers external AI agent analysis
6. AI agent populates Score, Feedback, Verdict, AnalyzedAt fields
7. Test can be submitted after all 4 videos uploaded

**Question Source:** Active TestQuestionGroup (ordered by GroupOrder)

##### Math Test (Video-Based)
**Flow:**
1. Candidate receives 2 math problems
2. Records 2 separate videos explaining solutions
3. **Total:** 2 videos required
4. Each video upload automatically triggers external AI agent analysis
5. AI agent populates Score, Feedback, Verdict, AnalyzedAt fields
6. Test can be submitted after all 2 videos uploaded

**Question Source:** Active TestQuestionGroup (ordered by GroupOrder)

##### Interview Test (Video-Based)
**Flow:**
1. Candidate receives 5 interview questions
2. Records 5 separate video responses
3. **Total:** 5 videos required
4. Each video upload automatically triggers external AI agent analysis
5. AI agent populates Score, Feedback, Verdict, AnalyzedAt fields
6. Test can be submitted after all 5 videos uploaded

**Question Source:** Active TestQuestionGroup (ordered by GroupOrder)

##### Psychology Test (Multiple Choice)
**Flow:**
1. Candidate receives 50 randomly selected questions
2. Answers all questions (single or multiple choice per question)
3. Submits all answers in one batch
4. **Auto-graded:** Backend automatically calculates score
5. **Time Limit:** 60 minutes (3600 seconds)

**Question Source:** Random selection from active QuestionTemplate pool

##### Visual Retention Test (Multiple Choice)
**Flow:**
1. Candidate receives 15 randomly selected questions
2. Answers all questions (single or multiple choice per question)
3. Submits all answers in one batch
4. **Auto-graded:** Backend automatically calculates score
5. **Time Limit:** 20 minutes (1200 seconds)

**Question Source:** Random selection from active QuestionTemplate pool

---

### 7. Evaluation & Reporting

**Base Route:** `/api/evaluation`
**Authorization:** JWT required with IDOR protection

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/evaluation/candidates/{candidateId}` | Get comprehensive evaluation |
| POST | `/api/evaluation/candidates/{candidateId}/refresh` | Refresh evaluation |
| GET | `/api/evaluation/candidates/{candidateId}/status` | Get evaluation status |
| GET | `/api/evaluation/health` | Health check |

---

### 8. Status & Health

**Base Route:** `/api/status`

| Method | Endpoint | Description | Auth |
|--------|----------|-------------|------|
| GET | `/api/status` | Get API status | Public |
| GET | `/api/status/protected` | Test authentication | JWT |

**Additional Health Endpoints:**
- `GET /health` - Complete health with DB check
- `GET /health/ready` - Readiness probe
- `GET /health/live` - Liveness probe

---

### 9. Databricks Integration

**Base Route:** `/api/databricks`
**Authorization:** JWT required

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/databricks/sync?batchSize={size}` | Sync data from Gupy |

---

## Authentication Flow

### NEW: CPF + Email Token Authentication (Recommended)

1. **Request Token**
   ```http
   POST /api/candidate-auth/request-token
   Content-Type: application/json

   {
     "cpf": "12345678901",
     "email": "candidate@example.com"
   }
   ```

   Response: Token sent to candidate's email

2. **Validate Token & Get JWT**
   ```http
   POST /api/candidate-auth/validate-token
   Content-Type: application/json

   {
     "cpf": "12345678901",
     "tokenCode": "123456"
   }
   ```

   Response:
   ```json
   {
     "accessToken": "eyJhbGciOiJIUzI1...",
     "candidateId": "guid",
     "requiresLGPDConsent": true
   }
   ```

3. **Use JWT in Requests**
   ```http
   GET /api/candidate/{id}
   Authorization: Bearer eyJhbGciOiJIUzI1...
   ```

### Legacy: CPF-Only Authentication

1. **Login**
   ```http
   POST /api/auth/login
   Content-Type: application/json

   {
     "cpf": "12345678901"
   }
   ```

2. **Use token in subsequent requests**

---

## LGPD Consent Flow

After authentication, check if candidate needs to accept LGPD consent:

1. **Check Status**
   ```http
   GET /api/consent/status/12345678901
   ```

2. **If consent not accepted, show consent form with:**
   - Privacy Policy checkbox
   - Data Sharing checkbox
   - Credit Analysis checkbox

3. **Submit Consent** (all must be true)
   ```http
   POST /api/consent
   Authorization: Bearer {jwt-token}
   Content-Type: application/json

   {
     "cpf": "12345678901",
     "acceptPrivacyPolicy": true,
     "acceptDataSharing": true,
     "acceptCreditAnalysis": true
   }
   ```

---

## Security Features

### IDOR Protection
All candidate-specific endpoints enforce Insecure Direct Object Reference protection:
- Authenticated user ID must match requested `candidateId`
- Admin/Recruiter roles can access any candidate data
- Violations are logged for security auditing

### Rate Limiting
- Token requests: Max 5 attempts per 15 minutes per CPF
- Failed validations: 30-minute lockout after 5 failed attempts

### JWT Claims
- `sub`: Candidate ID
- `candidateId`: Candidate ID (GUID)
- `name`: Candidate name
- `cpf`: Candidate CPF
- `role`: User role ("candidate", "Admin", "Recruiter")
- `exp`: Expiration (24 hours)

---

## Error Handling

All endpoints return consistent error responses:

### HTTP Status Codes
- `200 OK` - Success
- `201 Created` - Resource created
- `204 No Content` - Success with no response body
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing/invalid JWT
- `403 Forbidden` - IDOR violation or insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Resource conflict
- `500 Internal Server Error` - Server error

### Error Response Format
```json
{
  "error": "ERROR_CODE",
  "message": "Human-readable error message in Portuguese",
  "details": {
    "field": "Additional context"
  }
}
```

---

## Configuration

### appsettings.json Structure
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key-min-32-characters",
    "Issuer": "Dignus.API",
    "Audience": "Dignus.Candidates",
    "ExpirationMinutes": 1440
  },
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=dignusdb;Username=user;Password=pass"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;..."
  },
  "Email": {
    "SmtpServer": "smtp.example.com",
    "SmtpPort": 587,
    "Username": "noreply@example.com",
    "Password": "password",
    "FromEmail": "noreply@dignus.com",
    "FromName": "Dignus Platform"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;..."
  },
  "MediaUploadSettings": {
    "MaxFileSizeMB": 100,
    "AllowedExtensions": [".mp4", ".webm", ".mov", ".avi"],
    "AllowedMimeTypes": ["video/mp4", "video/webm", "video/quicktime", "video/x-msvideo"]
  },
  "ExternalAIAgent": {
    "BaseUrl": "https://your-ai-agent-api.com",
    "ApiKey": "your-api-key",
    "TimeoutSeconds": 30
  }
}
```

---

## Development Notes

### Architecture
- **Framework:** ASP.NET Core 9.0
- **Database:** PostgreSQL with Entity Framework Core
- **Authentication:** JWT Bearer Tokens + Email Token System
- **File Storage:** Azure Blob Storage
- **Logging:** Serilog with structured logging
- **API Documentation:** Swagger/OpenAPI

### Key Features
- ✅ **NEW** CPF + Email token authentication with rate limiting
- ✅ **NEW** LGPD consent management with audit trail
- ✅ **NEW** Admin question management with versioning
- ✅ **NEW** Configurable question groups by test type
- ✅ **NEW** External AI agent integration for video analysis
- ✅ **NEW** Unified test system supporting 5 test types
- Comprehensive candidate assessment workflow
- Multi-type testing system:
  - **Portuguese:** 3 video questions + reading (AI-graded)
  - **Math:** 2 video questions (AI-graded)
  - **Interview:** 5 video questions (AI-graded)
  - **Psychology:** 50 multiple choice questions (auto-graded)
  - **Visual Retention:** 15 multiple choice questions (auto-graded)
- AI-powered video analysis with score, feedback, and verdict
- Automatic grading for objective tests
- Secure media upload to Azure Blob Storage
- Job application and management
- Recruiter dashboard integration
- Health monitoring and observability
- IDOR protection throughout API

---

## Testing

### Using Swagger UI
Visit `https://localhost:7214/swagger` to explore and test the API interactively.

### Sample Test Flow

1. **Authenticate**
   - Request token: POST `/api/candidate-auth/request-token`
   - Validate token: POST `/api/candidate-auth/validate-token`
   - Copy JWT token

2. **Submit LGPD Consent**
   - POST `/api/consent` with all three consents = true

3. **Create Test**
   - POST `/api/v2/tests` with candidateId and testType

   **Example Request:**
   ```json
   {
     "candidateId": "60fd75e6-1225-44e0-a4a1-c1aa7d82b6a8",
     "testType": "Portuguese"
   }
   ```

   **Note:** The `testType` field accepts both string and integer formats:
   - **String format (recommended):** `"Portuguese"`, `"Math"`, `"Psychology"`, `"VisualRetention"`, `"Interview"`
   - **Integer format (legacy):** `1` (Portuguese), `2` (Math), `3` (Psychology), `4` (VisualRetention), `5` (Interview)

   Both formats are supported for backward compatibility and developer convenience.

4. **Start Test**
   - POST `/api/v2/tests/{testId}/start`

5. **Submit Answers**
   - POST `/api/v2/tests/{testId}/answers` (batch)

6. **Submit Test**
   - POST `/api/v2/tests/{testId}/submit`

7. **Get Evaluation**
   - GET `/api/evaluation/candidates/{candidateId}`

---

## Pagination

Paginated endpoints follow this format:

**Request:**
```
GET /api/endpoint?page=1&pageSize=20
```

**Response:**
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalPages": 5,
    "totalCount": 100,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

---

## Support & Maintenance

For issues or questions regarding the Dignus Candidate API:

1. Check the logs in the application output
2. Verify database connectivity
3. Ensure proper configuration in `appsettings.json`
4. Review authentication tokens and claims
5. Check [Complete API Documentation](../docs/API_DOCUMENTATION.md)
6. Contact the development team: Bruno, Vitor

### Monitoring
- Application logs via Serilog
- Health check endpoints for uptime monitoring
- Performance metrics through structured logging
- Database query performance monitoring via Entity Framework

---

## Additional Resources

- **Complete API Documentation:** [API_DOCUMENTATION.md](../docs/API_DOCUMENTATION.md)
- **Swagger UI:** `/swagger` (development only)
- **OpenAPI Spec:** `/swagger/v1/swagger.json`
- **Health Check:** `/api/status`

---

**Last Updated:** October 21, 2025
**API Version:** 2.0 (Unified Tests)
**Document Version:** 2.1

**Recent Updates:**
- Updated test configurations (Portuguese: 3 questions, Math: 2 questions, Interview: 5 questions, Psychology: 50 questions)
- Added comprehensive test workflow documentation for all 5 test types
- Documented external AI agent integration for video-based tests
- Updated question management documentation with usage guidelines
- Clarified auto-grading vs AI-grading distinction
