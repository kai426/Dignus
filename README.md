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

### Configuration Requirements

Before running the application, ensure the following configurations are properly set:

1. **PostgreSQL Connection String** - Required for database operations
2. **Azure Storage Connection String** - Required for file upload endpoints
3. **JWT Settings** - Required for authentication (development mode)
4. **Azure AD Settings** - Required for production authentication

---

## Testing Status

### ✅ Working Endpoints
- **Health Checks:** `/health`, `/health/ready`, `/health/live`
- **Status:** `/api/status`
- **Interview Questions:** `/api/interview/questions`
- **Interview Configuration:** `/api/interview/config`
- **404 Error Handling:** Non-existent endpoints return proper 404 responses

### ⚠️ Configuration-Dependent Endpoints
The following endpoints require proper Azure Storage configuration:
- **Media Upload:** `/api/media/*` endpoints
- **Test Questions:** `/api/test/questions/*` endpoints
- **Portuguese Questions:** `/api/portuguese-questions/*` endpoints

### 🔐 Authentication-Required Endpoints
These endpoints require JWT token authentication:
- **Questionnaire:** `/api/tests/questionnaire/*` endpoints
- **Candidate Management:** Most `/api/candidate/*` endpoints
- **Job Applications:** `/api/job/*` endpoints

### 📋 Database Dependency Issues
- **Authentication:** Requires existing candidate records in database
- **Candidate Creation:** Requires valid Job and Recruiter foreign key relationships

---

## Authentication & Authorization

### Authentication Methods

#### JWT Token Authentication (Development)
- **Type:** Bearer Token
- **Header:** `Authorization: Bearer <token>`
- **Token Lifetime:** 24 hours (configurable)

#### Azure AD (Production)
- Uses Microsoft Identity Web for enterprise authentication
- Configured in `appsettings.json` under `AzureAd` section

### Authorization Policies
- **RequireAuthentication:** Basic authentication requirement
- **CandidateAccess:** Requires `role: candidate` claim
- **RecruiterAccess:** Requires `role: recruiter` claim

### Getting Started with Authentication

1. **Login to get JWT token:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "cpf": "12345678901"
}
```

2. **Use token in subsequent requests:**
```http
GET /api/candidate/{id}
Authorization: Bearer <your-jwt-token>
```

---

## API Endpoints

### 🔐 Authentication Controller
**Base Route:** `/api/auth`

#### POST /api/auth/login
Authenticate candidate using CPF and get JWT token.

**Request:**
```json
{
  "cpf": "12345678901"
}
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "candidate": {
    "id": "11111111-1111-1111-1111-111111111111",
    "name": "João Silva",
    "email": "joao@email.com",
    "cpf": "12345678901",
    "status": "InProcess"
  },
  "expiresAt": "2024-09-24T12:00:00Z"
}
```

#### GET /api/auth/progress/{candidateId}
Get candidate progress information.

**Response:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "overallProgress": 65.5,
  "testsCompleted": 2,
  "totalTests": 4,
  "currentStage": "Psychology Test"
}
```

---

### 👤 Candidate Controller
**Base Route:** `/api/candidate`
**Authentication:** Required (temporarily disabled)

#### GET /api/candidate/{id}
Get candidate details by ID.

**Response:**
```json
{
  "id": "11111111-1111-1111-1111-111111111111",
  "name": "João Silva",
  "cpf": "12345678901",
  "email": "joao@email.com",
  "phone": "+5511999999999",
  "birthDate": "1990-01-01T00:00:00Z",
  "status": "InProcess",
  "createdAt": "2024-09-01T10:00:00Z",
  "jobId": "22222222-2222-2222-2222-222222222222",
  "recruiterId": "33333333-3333-3333-3333-333333333333"
}
```

#### POST /api/candidate
Create a new candidate.

**Request:**
```json
{
  "name": "Maria Santos",
  "cpf": "98765432100",
  "email": "maria@email.com",
  "phone": "+5511888888888",
  "birthDate": "1985-05-15T00:00:00Z",
  "jobId": "22222222-2222-2222-2222-222222222222",
  "recruiterId": "33333333-3333-3333-3333-333333333333"
}
```

#### PUT /api/candidate/{id}
Update existing candidate.

**Request:**
```json
{
  "name": "Maria Santos Silva",
  "phone": "+5511777777777",
  "status": "Approved"
}
```

#### GET /api/candidate/search
Search candidates with pagination.

**Query Parameters:**
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 10): Items per page
- `searchTerm` (string, optional): Search term for name or email

**Response:**
```json
{
  "data": [
    {
      "id": "11111111-1111-1111-1111-111111111111",
      "name": "João Silva",
      "email": "joao@email.com",
      "status": "InProcess"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

#### GET /api/candidate/{id}/profile
Get simplified candidate profile for frontend.

#### PATCH /api/candidate/{id}/status
Update candidate status.

**Request:**
```json
{
  "status": "Approved"
}
```

#### GET /api/candidate/{id}/job
Get job information associated with the candidate.

---

### 💼 Job Controller
**Base Route:** `/api/job`
**Authentication:** Required

#### GET /api/job
Search and filter jobs with pagination.

**Query Parameters:**
- `pageNumber` (int): Page number
- `pageSize` (int): Items per page
- `searchTerm` (string): Search term
- `location` (string): Filter by location
- `company` (string): Filter by company
- `status` (string): Filter by status

**Response:**
```json
{
  "data": [
    {
      "id": "job-123",
      "name": "Software Developer",
      "description": "Full-stack developer position",
      "requirements": ["C#", ".NET", "React"],
      "location": "São Paulo",
      "company": "Tech Corp",
      "status": "Publicado",
      "publishedAt": "2024-09-01T00:00:00Z"
    }
  ],
  "pageNumber": 1,
  "pageSize": 10,
  "totalCount": 1,
  "totalPages": 1
}
```

#### GET /api/job/{id}
Get specific job details.

#### GET /api/job/statistics
Get job dashboard statistics.

**Response:**
```json
{
  "totalJobs": 150,
  "activeJobs": 45,
  "totalApplications": 2340,
  "averageApplicationsPerJob": 15.6
}
```

#### POST /api/job/{id}/apply
Apply to a job.

**Request:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "coverLetter": "I am interested in this position because..."
}
```

#### GET /api/job/{id}/candidates
Get candidates who applied to a job (recruiter access only).

---

### 🎥 Interview Controller
**Base Route:** `/api/interview`
**Authentication:** Required (temporarily disabled)

#### GET /api/interview/questions
Get standard interview questions for video recording.

**Response:**
```json
[
  {
    "id": "44444444-4444-4444-4444-444444444444",
    "text": "Conte sobre você, sua experiência profissional e porque está interessado nesta posição.",
    "order": 1,
    "maxDurationSeconds": 180
  },
  {
    "id": "55555555-5555-5555-5555-555555555555",
    "text": "Descreva uma situação desafiadora que enfrentou no trabalho e como a resolveu.",
    "order": 2,
    "maxDurationSeconds": 180
  }
]
```

#### GET /api/interview/config
Get interview configuration settings.

**Response:**
```json
{
  "maxTotalDurationMinutes": 20,
  "maxQuestionDurationMinutes": 3,
  "minVideoQuality": "720p",
  "requiredQuestions": 5,
  "instructions": [
    "Clique em Iniciar gravação para iniciar sua câmera e microfone.",
    "Aguarde que começar as perguntas antes de gravação.",
    "Ao finalizar, clique em Encerrar gravação para salvar seu vídeo."
  ]
}
```

---

### 📊 Evaluation Controller
**Base Route:** `/api/evaluation`
**Authentication:** Required

#### GET /api/evaluation/candidates/{candidateId}
Get comprehensive evaluation for a candidate.

**Response:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "overallScore": 85.5,
  "testScores": {
    "portuguese": 90.0,
    "math": 82.0,
    "psychology": 88.0,
    "visualRetention": 81.0
  },
  "evaluationDate": "2024-09-20T10:00:00Z",
  "recommendation": "Approved",
  "strengths": ["Analytical thinking", "Communication"],
  "areasForImprovement": ["Technical knowledge"]
}
```

#### POST /api/evaluation/candidates/{candidateId}/refresh
Refresh evaluation for a candidate.

#### GET /api/evaluation/candidates/{candidateId}/status
Get evaluation status.

**Response:**
```json
{
  "status": "Completed",
  "lastUpdated": "2024-09-20T10:00:00Z",
  "progress": 100.0
}
```

#### GET /api/evaluation/health
Health check endpoint for evaluation service.

---

### 📁 Media Controller
**Base Route:** `/api/media`
**Authentication:** Required (temporarily disabled)

#### POST /api/media/audio
Upload audio file for tests.

**Request:** Form-data
- `audioFile`: Audio file (max size configured in settings)
- `candidateId`: Candidate ID (GUID)
- `testType`: Test type (string)
- `questionId`: Question ID (GUID)

**Response:**
```json
{
  "id": "66666666-6666-6666-6666-666666666666",
  "fileName": "audio_response.wav",
  "filePath": "https://storage.blob.core.windows.net/audio/...",
  "uploadedAt": "2024-09-20T10:00:00Z",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "testType": "Portuguese"
}
```

#### POST /api/media/video
Upload video file for tests.

**Request:** Form-data
- `videoFile`: Video file (max size configured in settings)
- `candidateId`: Candidate ID (GUID)
- `questionId`: Question ID (GUID, optional)

#### GET /api/media/audio/candidate/{candidateId}
Get audio submissions for a candidate.

#### GET /api/media/video/candidate/{candidateId}
Get video interviews for a candidate.

#### GET /api/media/audio/{audioId}
Get specific audio submission.

#### GET /api/media/video/{videoId}
Get specific video interview.

#### DELETE /api/media/audio/{audioId}
Delete audio submission.

#### DELETE /api/media/video/{videoId}
Delete video interview.

#### GET /api/media/upload-limits
Get upload configuration limits.

**Response:**
```json
{
  "maxAudioSizeBytes": 52428800,
  "maxVideoSizeBytes": 209715200
}
```

---

### 📝 Test Controller
**Base Route:** `/api/test`
**Authentication:** Required (temporarily disabled)

#### GET /api/test/{testId}
Get test by ID for a specific candidate.

**Query Parameters:**
- `candidateId` (GUID): Candidate ID

#### GET /api/test/{testType}/candidate/{candidateId}
Get or create specific test type for candidate.

**Test Types:** `portuguese`, `math`, `psychology`, `visualretention`

**Response:**
```json
{
  "id": "77777777-7777-7777-7777-777777777777",
  "testType": "Portuguese",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "status": "NotStarted",
  "questions": [
    {
      "id": "88888888-8888-8888-8888-888888888888",
      "text": "Qual é o plural de 'cidadão'?",
      "options": ["A) cidadãos", "B) cidadões", "C) cidadães", "D) cidadãs"],
      "correctAnswer": "A",
      "order": 1
    }
  ],
  "timeLimit": 30,
  "createdAt": "2024-09-20T10:00:00Z"
}
```

#### GET /api/test/candidate/{candidateId}
Get all tests for a candidate.

#### POST /api/test
Create a new test.

**Request:**
```json
{
  "testType": "Portuguese",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "timeLimit": 30
}
```

#### POST /api/test/{testId}/start
Start a test.

**Request:**
```json
"11111111-1111-1111-1111-111111111111"
```

#### POST /api/test/{testId}/submit
Submit test answers.

**Request:**
```json
{
  "testId": "77777777-7777-7777-7777-777777777777",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "answers": [
    {
      "questionId": "88888888-8888-8888-8888-888888888888",
      "selectedAnswer": "A"
    }
  ],
  "timeSpent": 1800
}
```

#### GET /api/test/questions/{testType}
Get random questions for a test type.

**Query Parameters:**
- `questionCount` (int, default: 10): Number of questions

#### GET /api/test/candidate/{candidateId}/status
Get test completion status.

**Response:**
```json
{
  "portuguese": true,
  "math": false,
  "psychology": false,
  "visualretention": false
}
```

#### GET /api/test/candidate/{candidateId}/can-start/{testType}
Check if candidate can start a specific test.

#### GET /api/test/{testId}/timeout
Check if test has timed out.

#### GET /api/test/{testId}/remaining-time
Get remaining time for a test.

**Response:**
```json
{
  "remainingMinutes": 15.5
}
```

#### POST /api/test/{testId}/math/question/{questionNumber}/video
Upload video answer for Math test question.

**Parameters:**
- `testId` (GUID): Math test ID
- `questionNumber` (int): Question number (1 or 2)
- `candidateId` (GUID): Candidate ID (query parameter)
- `videoFile`: Video file (form-data)

---

### 📋 Questionnaire Controller
**Base Route:** `/api/tests/questionnaire`
**Authentication:** Required

#### GET /api/tests/questionnaire
Get complete questionnaire structure with all psychology questions.

**Response:**
```json
{
  "id": "questionnaire-psychology",
  "title": "Psychology Assessment",
  "description": "Complete psychological evaluation",
  "sections": [
    {
      "id": 0,
      "title": "Personality Assessment",
      "questions": [
        {
          "id": 1,
          "text": "Você se considera uma pessoa extrovertida?",
          "options": ["A", "B", "C", "D", "E"],
          "type": "MultipleChoice"
        }
      ]
    }
  ],
  "totalQuestions": 180,
  "estimatedTimeMinutes": 45
}
```

#### GET /api/tests/questionnaire/candidate/{candidateId}/progress
Get questionnaire progress for candidate.

**Response:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "completedSections": [0, 1, 2],
  "currentSection": 3,
  "totalSections": 9,
  "overallProgress": 33.3,
  "isCompleted": false,
  "startedAt": "2024-09-20T10:00:00Z"
}
```

#### POST /api/tests/questionnaire/candidate/{candidateId}/initialize
Initialize questionnaire for candidate.

#### GET /api/tests/questionnaire/sections/{sectionId}
Get specific section details.

**Parameters:**
- `sectionId` (int): Section ID (0-8)

#### POST /api/tests/questionnaire/sections/{sectionId}/responses
Save section responses.

**Request:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "responses": [
    {
      "questionId": 1,
      "selectedOption": "A"
    },
    {
      "questionId": 2,
      "selectedOption": "C"
    }
  ]
}
```

#### POST /api/tests/questionnaire/submit
Submit completed questionnaire.

**Request:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "responses": [
    {
      "questionId": 1,
      "selectedOption": "A"
    }
  ],
  "completedAt": "2024-09-20T11:30:00Z"
}
```

#### GET /api/tests/questionnaire/candidate/{candidateId}/can-start
Check if candidate can start questionnaire.

#### POST /api/tests/questionnaire/submit-psychology-responses
Submit all psychology test responses at once.

**Request:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "responses": [
    {
      "questionId": 1,
      "selectedOption": "A"
    },
    {
      "questionId": 2,
      "selectedOption": "B"
    }
  ]
}
```

---

### 🧠 Visual Retention Test Controller
**Base Route:** `/api/tests/visual-retention`
**Authentication:** Required (temporarily disabled)

#### GET /api/tests/visual-retention/candidate/{candidateId}
Get or create visual retention test for candidate.

**Response:**
```json
{
  "id": "99999999-9999-9999-9999-999999999999",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "status": "NotStarted",
  "questions": [
    {
      "id": 1,
      "imageUrl": "https://storage.blob.core.windows.net/visual-tests/image1.png",
      "options": ["A", "B", "C", "D", "E"],
      "displayTimeSeconds": 5
    }
  ],
  "totalQuestions": 20,
  "createdAt": "2024-09-20T10:00:00Z"
}
```

#### GET /api/tests/visual-retention/{testId}
Get visual retention test by ID.

**Query Parameters:**
- `candidateId` (GUID): Candidate ID

#### POST /api/tests/visual-retention/{testId}/start
Start visual retention test.

**Request:**
```json
"11111111-1111-1111-1111-111111111111"
```

#### POST /api/tests/visual-retention/submit
Submit visual retention test responses.

**Request:**
```json
{
  "testId": "99999999-9999-9999-9999-999999999999",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "responses": [
    {
      "questionId": 1,
      "selectedOption": "A"
    },
    {
      "questionId": 2,
      "selectedOption": "C"
    }
  ]
}
```

#### GET /api/tests/visual-retention/candidate/{candidateId}/status
Get test status for candidate.

#### GET /api/tests/visual-retention/candidate/{candidateId}/can-start
Check if candidate can start test.

---

### 📚 Portuguese Questions Controller
**Base Route:** `/api/portuguese-questions`
**Authentication:** Required

#### GET /api/portuguese-questions
Get all Portuguese questions with pagination.

**Query Parameters:**
- `pageNumber` (int, default: 1): Page number
- `pageSize` (int, default: 20): Items per page
- `searchTerm` (string, optional): Search term for question text

#### GET /api/portuguese-questions/{id}
Get Portuguese question by ID.

#### POST /api/portuguese-questions
Create new Portuguese question.

**Request:**
```json
{
  "text": "Qual é o feminino de 'poeta'?",
  "options": ["A) poeta", "B) poetisa", "C) poetriz", "D) poetesa"],
  "correctAnswer": "B",
  "difficulty": "Medium",
  "category": "Grammar"
}
```

#### PUT /api/portuguese-questions/{id}
Update Portuguese question.

#### DELETE /api/portuguese-questions/{id}
Delete Portuguese question.

#### POST /api/portuguese-questions/bulk-import
Bulk import Portuguese questions.

**Request:**
```json
[
  {
    "text": "Question 1...",
    "options": ["A", "B", "C", "D"],
    "correctAnswer": "A"
  },
  {
    "text": "Question 2...",
    "options": ["A", "B", "C", "D"],
    "correctAnswer": "B"
  }
]
```

---

### 🔗 Databricks Controller
**Base Route:** `/api/databricks`
**Authentication:** Required (temporarily disabled)

#### POST /api/databricks/sync
Start synchronization of data from Gupy via Databricks.

**Query Parameters:**
- `batchSize` (int, default: 1000): Number of records to process in each batch

**Response:**
```json
{
  "syncId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "type": "Full",
  "status": "Running",
  "totalRecords": 5000,
  "processedRecords": 1000,
  "startedAt": "2024-09-20T10:00:00Z",
  "errorMessage": null
}
```

---

### ✅ Status Controller
**Base Route:** `/api/status`
**Authentication:** None required

#### GET /api/status
Get API status and version information.

**Response:**
```json
{
  "status": "Healthy",
  "version": "1.0.0",
  "environment": "Development",
  "timestamp": "2024-09-20T10:00:00Z",
  "service": "Dignus Candidate API"
}
```

#### GET /api/status/protected
Test authentication (protected endpoint).

**Response:**
```json
{
  "message": "You are authenticated",
  "user": "user@example.com",
  "claims": [
    {
      "type": "candidateId",
      "value": "11111111-1111-1111-1111-111111111111"
    }
  ],
  "timestamp": "2024-09-20T10:00:00Z"
}
```

---

## Health Checks

The API provides health check endpoints for monitoring:

- **GET /health** - Complete health status including database connectivity
- **GET /health/ready** - Readiness probe for container orchestration
- **GET /health/live** - Liveness probe for basic API availability

---

## Error Handling

All endpoints return consistent error responses:

### HTTP Status Codes
- **200 OK** - Success
- **201 Created** - Resource created successfully
- **204 No Content** - Success with no response body
- **400 Bad Request** - Invalid request data
- **401 Unauthorized** - Authentication required
- **403 Forbidden** - Access denied
- **404 Not Found** - Resource not found
- **409 Conflict** - Resource conflict
- **413 Payload Too Large** - File size exceeds limits
- **500 Internal Server Error** - Server error

### Error Response Format
```json
{
  "error": "ValidationError",
  "message": "The provided data is invalid",
  "details": {
    "field": "email",
    "issue": "Invalid email format"
  }
}
```

---

## Configuration

### Environment Variables
- **ASPNETCORE_ENVIRONMENT** - Environment (Development/Production)
- **ConnectionStrings__DefaultConnection** - PostgreSQL connection string

### appsettings.json Structure
```json
{
  "Jwt": {
    "SecretKey": "your-secret-key",
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
  "MediaUploadSettings": {
    "MaxAudioSizeMB": 50,
    "MaxVideoSizeMB": 200,
    "AllowedAudioFormats": ["wav", "mp3", "m4a"],
    "AllowedVideoFormats": ["mp4", "webm", "avi"]
  }
}
```

---

## Testing

### Using Swagger UI
Visit `https://localhost:7214/swagger` to explore and test the API interactively.

### Authentication Testing
1. Use `/api/auth/login` to get a JWT token
2. Click "Authorize" in Swagger and enter: `Bearer <your-token>`
3. Test protected endpoints

### Sample Test Data
The system includes sample test data:
- **Test Candidate CPF:** `12345678901`
- **Sample Job ID:** `22222222-2222-2222-2222-222222222222`

---

## Integration Guidelines

### Frontend Integration
1. **Authentication Flow:**
   ```javascript
   // Login
   const response = await fetch('/api/auth/login', {
     method: 'POST',
     headers: { 'Content-Type': 'application/json' },
     body: JSON.stringify({ cpf: '12345678901' })
   });

   const { token } = await response.json();
   localStorage.setItem('authToken', token);
   ```

2. **Authenticated Requests:**
   ```javascript
   const token = localStorage.getItem('authToken');
   const response = await fetch('/api/candidate/123', {
     headers: { 'Authorization': `Bearer ${token}` }
   });
   ```

3. **File Upload:**
   ```javascript
   const formData = new FormData();
   formData.append('audioFile', audioBlob);
   formData.append('candidateId', candidateId);
   formData.append('testType', 'Portuguese');

   const response = await fetch('/api/media/audio', {
     method: 'POST',
     headers: { 'Authorization': `Bearer ${token}` },
     body: formData
   });
   ```

### Database Schema
The system uses Entity Framework Core with PostgreSQL. Key entities:
- **Candidate** - Main candidate information
- **Job** - Available job positions
- **Tests** - Various test types (Portuguese, Math, Psychology, Visual Retention)
- **Media** - Audio/video submissions
- **Evaluations** - AI-powered candidate assessments

---

## Development Notes

### Architecture
- **Framework:** ASP.NET Core 9.0
- **Database:** PostgreSQL with Entity Framework Core
- **Authentication:** JWT Bearer Tokens / Azure AD
- **File Storage:** Azure Blob Storage
- **Logging:** Serilog with structured logging
- **API Documentation:** Swagger/OpenAPI

### Key Features
- Comprehensive candidate assessment workflow
- Multi-type testing system (Portuguese, Math, Psychology, Visual Retention)
- AI-powered evaluation and scoring
- Secure media upload and storage
- Job application and management
- Recruiter dashboard integration
- Health monitoring and observability

### Security Features
- JWT token authentication
- IDOR (Insecure Direct Object Reference) protection
- Input validation and sanitization
- Secure file upload with size and type restrictions
- CORS configuration
- Security headers middleware

---

## Support & Maintenance

For issues or questions regarding the Dignus Candidate API:

1. Check the logs in the application output
2. Verify database connectivity
3. Ensure proper configuration in `appsettings.json`
4. Review authentication tokens and claims
5. Contact the development team: Bruno, Vitor

### Monitoring
- Application logs via Serilog
- Health check endpoints for uptime monitoring
- Performance metrics through structured logging
- Database query performance monitoring via Entity Framework