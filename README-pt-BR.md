# Documentação da API Dignus Candidate

**Responsáveis:** Bruno, Vitor

## Visão Geral

A API Dignus Candidate é o sistema central backend para gerenciar processos de seleção de candidatos, incluindo submissão de testes, análise com IA e integração com recrutadores. Fornece endpoints abrangentes para lidar com avaliações de candidatos, uploads de mídia, candidaturas a vagas e fluxos de trabalho de avaliação.

## Início Rápido

### Pré-requisitos
- .NET 9.0 SDK
- Banco de dados PostgreSQL
- Conta do Azure Storage (para armazenamento de arquivos)

### Executando a Aplicação

```bash
cd Dignus.Candidate.Back
dotnet restore
dotnet run
```

A API estará disponível em:
- **HTTPS:** https://localhost:7214
- **HTTP:** http://localhost:5076
- **Swagger:** https://localhost:7214/swagger

### Configuração do Banco de Dados
A aplicação aplica automaticamente as migrações do banco de dados na inicialização. Certifique-se de que sua string de conexão PostgreSQL esteja configurada no `appsettings.json`.

### Requisitos de Configuração

Antes de executar a aplicação, certifique-se de que as seguintes configurações estejam definidas adequadamente:

1. **String de Conexão PostgreSQL** - Obrigatório para operações de banco de dados
2. **String de Conexão Azure Storage** - Obrigatório para endpoints de upload de arquivos
3. **Configurações JWT** - Obrigatório para autenticação (modo desenvolvimento)
4. **Configurações Azure AD** - Obrigatório para autenticação em produção

---

## Status dos Testes

### ✅ Endpoints Funcionando
- **Verificações de Saúde:** `/health`, `/health/ready`, `/health/live`
- **Status:** `/api/status`
- **Perguntas da Entrevista:** `/api/interview/questions`
- **Configuração da Entrevista:** `/api/interview/config`
- **Tratamento de Erro 404:** Endpoints inexistentes retornam respostas 404 adequadas

### ⚠️ Endpoints Dependentes de Configuração
Os seguintes endpoints requerem configuração adequada do Azure Storage:
- **Upload de Mídia:** Endpoints `/api/media/*`
- **Perguntas de Testes:** Endpoints `/api/test/questions/*`
- **Perguntas de Português:** Endpoints `/api/portuguese-questions/*`

### 🔐 Endpoints que Requerem Autenticação
Estes endpoints requerem autenticação por token JWT:
- **Questionário:** Endpoints `/api/tests/questionnaire/*`
- **Gerenciamento de Candidatos:** Maioria dos endpoints `/api/candidate/*`
- **Candidaturas a Vagas:** Endpoints `/api/job/*`

### 📋 Problemas de Dependência do Banco de Dados
- **Autenticação:** Requer registros de candidatos existentes no banco de dados
- **Criação de Candidatos:** Requer relacionamentos de chaves estrangeiras válidos para Job e Recruiter

---

## Autenticação e Autorização

### Métodos de Autenticação

#### Autenticação por Token JWT (Desenvolvimento)
- **Tipo:** Bearer Token
- **Cabeçalho:** `Authorization: Bearer <token>`
- **Duração do Token:** 24 horas (configurável)

#### Azure AD (Produção)
- Usa Microsoft Identity Web para autenticação empresarial
- Configurado no `appsettings.json` na seção `AzureAd`

### Políticas de Autorização
- **RequireAuthentication:** Requisito básico de autenticação
- **CandidateAccess:** Requer claim `role: candidate`
- **RecruiterAccess:** Requer claim `role: recruiter`

### Começando com Autenticação

1. **Login para obter token JWT:**
```http
POST /api/auth/login
Content-Type: application/json

{
  "cpf": "12345678901"
}
```

2. **Use o token em requisições subsequentes:**
```http
GET /api/candidate/{id}
Authorization: Bearer <seu-jwt-token>
```

---

## Endpoints da API

### 🔐 Controller de Autenticação
**Rota Base:** `/api/auth`

#### POST /api/auth/login
Autenticar candidato usando CPF e obter token JWT.

**Requisição:**
```json
{
  "cpf": "12345678901"
}
```

**Resposta:**
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
Obter informações de progresso do candidato.

**Resposta:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "overallProgress": 65.5,
  "testsCompleted": 2,
  "totalTests": 4,
  "currentStage": "Teste de Psicologia"
}
```

---

### 👤 Controller de Candidato
**Rota Base:** `/api/candidate`
**Autenticação:** Obrigatória (temporariamente desabilitada)

#### GET /api/candidate/{id}
Obter detalhes do candidato por ID.

**Resposta:**
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
Criar um novo candidato.

**Requisição:**
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
Atualizar candidato existente.

**Requisição:**
```json
{
  "name": "Maria Santos Silva",
  "phone": "+5511777777777",
  "status": "Approved"
}
```

#### GET /api/candidate/search
Buscar candidatos com paginação.

**Parâmetros de Query:**
- `pageNumber` (int, padrão: 1): Número da página
- `pageSize` (int, padrão: 10): Itens por página
- `searchTerm` (string, opcional): Termo de busca para nome ou email

**Resposta:**
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
Obter perfil simplificado do candidato para frontend.

#### PATCH /api/candidate/{id}/status
Atualizar status do candidato.

**Requisição:**
```json
{
  "status": "Approved"
}
```

#### GET /api/candidate/{id}/job
Obter informações da vaga associada ao candidato.

---

### 💼 Controller de Vaga
**Rota Base:** `/api/job`
**Autenticação:** Obrigatória

#### GET /api/job
Buscar e filtrar vagas com paginação.

**Parâmetros de Query:**
- `pageNumber` (int): Número da página
- `pageSize` (int): Itens por página
- `searchTerm` (string): Termo de busca
- `location` (string): Filtrar por localização
- `company` (string): Filtrar por empresa
- `status` (string): Filtrar por status

**Resposta:**
```json
{
  "data": [
    {
      "id": "job-123",
      "name": "Desenvolvedor de Software",
      "description": "Posição de desenvolvedor full-stack",
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
Obter detalhes específicos da vaga.

#### GET /api/job/statistics
Obter estatísticas do dashboard de vagas.

**Resposta:**
```json
{
  "totalJobs": 150,
  "activeJobs": 45,
  "totalApplications": 2340,
  "averageApplicationsPerJob": 15.6
}
```

#### POST /api/job/{id}/apply
Candidatar-se a uma vaga.

**Requisição:**
```json
{
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "coverLetter": "Estou interessado nesta posição porque..."
}
```

#### GET /api/job/{id}/candidates
Obter candidatos que se candidataram à vaga (acesso apenas para recrutadores).

---

### 🎥 Controller de Entrevista
**Rota Base:** `/api/interview`
**Autenticação:** Obrigatória (temporariamente desabilitada)

#### GET /api/interview/questions
Obter perguntas padrão da entrevista para gravação de vídeo.

**Resposta:**
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
Obter configurações da entrevista.

**Resposta:**
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

### 📊 Controller de Avaliação
**Rota Base:** `/api/evaluation`
**Autenticação:** Obrigatória

#### GET /api/evaluation/candidates/{candidateId}
Obter avaliação abrangente de um candidato.

**Resposta:**
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
  "strengths": ["Pensamento analítico", "Comunicação"],
  "areasForImprovement": ["Conhecimento técnico"]
}
```

#### POST /api/evaluation/candidates/{candidateId}/refresh
Atualizar avaliação de um candidato.

#### GET /api/evaluation/candidates/{candidateId}/status
Obter status da avaliação.

**Resposta:**
```json
{
  "status": "Completed",
  "lastUpdated": "2024-09-20T10:00:00Z",
  "progress": 100.0
}
```

#### GET /api/evaluation/health
Endpoint de verificação de saúde para o serviço de avaliação.

---

### 📁 Controller de Mídia
**Rota Base:** `/api/media`
**Autenticação:** Obrigatória (temporariamente desabilitada)

#### POST /api/media/audio
Fazer upload de arquivo de áudio para testes.

**Requisição:** Form-data
- `audioFile`: Arquivo de áudio (tamanho máximo configurado nas configurações)
- `candidateId`: ID do Candidato (GUID)
- `testType`: Tipo de teste (string)
- `questionId`: ID da Pergunta (GUID)

**Resposta:**
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
Fazer upload de arquivo de vídeo para testes.

**Requisição:** Form-data
- `videoFile`: Arquivo de vídeo (tamanho máximo configurado nas configurações)
- `candidateId`: ID do Candidato (GUID)
- `questionId`: ID da Pergunta (GUID, opcional)

#### GET /api/media/audio/candidate/{candidateId}
Obter submissões de áudio de um candidato.

#### GET /api/media/video/candidate/{candidateId}
Obter entrevistas em vídeo de um candidato.

#### GET /api/media/audio/{audioId}
Obter submissão de áudio específica.

#### GET /api/media/video/{videoId}
Obter entrevista em vídeo específica.

#### DELETE /api/media/audio/{audioId}
Excluir submissão de áudio.

#### DELETE /api/media/video/{videoId}
Excluir entrevista em vídeo.

#### GET /api/media/upload-limits
Obter limites de configuração de upload.

**Resposta:**
```json
{
  "maxAudioSizeBytes": 52428800,
  "maxVideoSizeBytes": 209715200
}
```

---

### 📝 Controller de Teste
**Rota Base:** `/api/test`
**Autenticação:** Obrigatória (temporariamente desabilitada)

#### GET /api/test/{testId}
Obter teste por ID para um candidato específico.

**Parâmetros de Query:**
- `candidateId` (GUID): ID do Candidato

#### GET /api/test/{testType}/candidate/{candidateId}
Obter ou criar tipo de teste específico para candidato.

**Tipos de Teste:** `portuguese`, `math`, `psychology`, `visualretention`

**Resposta:**
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
Obter todos os testes de um candidato.

#### POST /api/test
Criar um novo teste.

**Requisição:**
```json
{
  "testType": "Portuguese",
  "candidateId": "11111111-1111-1111-1111-111111111111",
  "timeLimit": 30
}
```

#### POST /api/test/{testId}/start
Iniciar um teste.

**Requisição:**
```json
"11111111-1111-1111-1111-111111111111"
```

#### POST /api/test/{testId}/submit
Submeter respostas do teste.

**Requisição:**
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
Obter perguntas aleatórias para um tipo de teste.

**Parâmetros de Query:**
- `questionCount` (int, padrão: 10): Número de perguntas

#### GET /api/test/candidate/{candidateId}/status
Obter status de conclusão do teste.

**Resposta:**
```json
{
  "portuguese": true,
  "math": false,
  "psychology": false,
  "visualretention": false
}
```

#### GET /api/test/candidate/{candidateId}/can-start/{testType}
Verificar se o candidato pode iniciar um teste específico.

#### GET /api/test/{testId}/timeout
Verificar se o teste expirou.

#### GET /api/test/{testId}/remaining-time
Obter tempo restante para um teste.

**Resposta:**
```json
{
  "remainingMinutes": 15.5
}
```

#### POST /api/test/{testId}/math/question/{questionNumber}/video
Fazer upload de resposta em vídeo para questão do teste de Matemática.

**Parâmetros:**
- `testId` (GUID): ID do teste de Matemática
- `questionNumber` (int): Número da pergunta (1 ou 2)
- `candidateId` (GUID): ID do Candidato (parâmetro de query)
- `videoFile`: Arquivo de vídeo (form-data)

---

### 📋 Controller de Questionário
**Rota Base:** `/api/tests/questionnaire`
**Autenticação:** Obrigatória

#### GET /api/tests/questionnaire
Obter estrutura completa do questionário com todas as perguntas de psicologia.

**Resposta:**
```json
{
  "id": "questionnaire-psychology",
  "title": "Avaliação Psicológica",
  "description": "Avaliação psicológica completa",
  "sections": [
    {
      "id": 0,
      "title": "Avaliação de Personalidade",
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
Obter progresso do questionário para candidato.

**Resposta:**
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
Inicializar questionário para candidato.

#### GET /api/tests/questionnaire/sections/{sectionId}
Obter detalhes de seção específica.

**Parâmetros:**
- `sectionId` (int): ID da Seção (0-8)

#### POST /api/tests/questionnaire/sections/{sectionId}/responses
Salvar respostas da seção.

**Requisição:**
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
Submeter questionário completo.

**Requisição:**
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
Verificar se o candidato pode iniciar o questionário.

#### POST /api/tests/questionnaire/submit-psychology-responses
Submeter todas as respostas do teste de psicologia de uma vez.

**Requisição:**
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

### 🧠 Controller de Teste de Retenção Visual
**Rota Base:** `/api/tests/visual-retention`
**Autenticação:** Obrigatória (temporariamente desabilitada)

#### GET /api/tests/visual-retention/candidate/{candidateId}
Obter ou criar teste de retenção visual para candidato.

**Resposta:**
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
Obter teste de retenção visual por ID.

**Parâmetros de Query:**
- `candidateId` (GUID): ID do Candidato

#### POST /api/tests/visual-retention/{testId}/start
Iniciar teste de retenção visual.

**Requisição:**
```json
"11111111-1111-1111-1111-111111111111"
```

#### POST /api/tests/visual-retention/submit
Submeter respostas do teste de retenção visual.

**Requisição:**
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
Obter status do teste para candidato.

#### GET /api/tests/visual-retention/candidate/{candidateId}/can-start
Verificar se o candidato pode iniciar o teste.

---

### 📚 Controller de Perguntas de Português
**Rota Base:** `/api/portuguese-questions`
**Autenticação:** Obrigatória

#### GET /api/portuguese-questions
Obter todas as perguntas de Português com paginação.

**Parâmetros de Query:**
- `pageNumber` (int, padrão: 1): Número da página
- `pageSize` (int, padrão: 20): Itens por página
- `searchTerm` (string, opcional): Termo de busca para texto da pergunta

#### GET /api/portuguese-questions/{id}
Obter pergunta de Português por ID.

#### POST /api/portuguese-questions
Criar nova pergunta de Português.

**Requisição:**
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
Atualizar pergunta de Português.

#### DELETE /api/portuguese-questions/{id}
Excluir pergunta de Português.

#### POST /api/portuguese-questions/bulk-import
Importar perguntas de Português em massa.

**Requisição:**
```json
[
  {
    "text": "Pergunta 1...",
    "options": ["A", "B", "C", "D"],
    "correctAnswer": "A"
  },
  {
    "text": "Pergunta 2...",
    "options": ["A", "B", "C", "D"],
    "correctAnswer": "B"
  }
]
```

---

### 🔗 Controller do Databricks
**Rota Base:** `/api/databricks`
**Autenticação:** Obrigatória (temporariamente desabilitada)

#### POST /api/databricks/sync
Iniciar sincronização de dados do Gupy via Databricks.

**Parâmetros de Query:**
- `batchSize` (int, padrão: 1000): Número de registros para processar em cada lote

**Resposta:**
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

### ✅ Controller de Status
**Rota Base:** `/api/status`
**Autenticação:** Não obrigatória

#### GET /api/status
Obter status da API e informações de versão.

**Resposta:**
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
Testar autenticação (endpoint protegido).

**Resposta:**
```json
{
  "message": "Você está autenticado",
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

## Verificações de Saúde

A API fornece endpoints de verificação de saúde para monitoramento:

- **GET /health** - Status de saúde completo incluindo conectividade do banco de dados
- **GET /health/ready** - Sonda de prontidão para orquestração de containers
- **GET /health/live** - Sonda de vida para disponibilidade básica da API

---

## Tratamento de Erros

Todos os endpoints retornam respostas de erro consistentes:

### Códigos de Status HTTP
- **200 OK** - Sucesso
- **201 Created** - Recurso criado com sucesso
- **204 No Content** - Sucesso sem corpo de resposta
- **400 Bad Request** - Dados de requisição inválidos
- **401 Unauthorized** - Autenticação necessária
- **403 Forbidden** - Acesso negado
- **404 Not Found** - Recurso não encontrado
- **409 Conflict** - Conflito de recurso
- **413 Payload Too Large** - Tamanho do arquivo excede os limites
- **500 Internal Server Error** - Erro do servidor

### Formato de Resposta de Erro
```json
{
  "error": "ValidationError",
  "message": "Os dados fornecidos são inválidos",
  "details": {
    "field": "email",
    "issue": "Formato de email inválido"
  }
}
```

---

## Configuração

### Variáveis de Ambiente
- **ASPNETCORE_ENVIRONMENT** - Ambiente (Development/Production)
- **ConnectionStrings__DefaultConnection** - String de conexão PostgreSQL

### Estrutura do appsettings.json
```json
{
  "Jwt": {
    "SecretKey": "sua-chave-secreta",
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

## Testes

### Usando Swagger UI
Visite `https://localhost:7214/swagger` para explorar e testar a API interativamente.

### Testando Autenticação
1. Use `/api/auth/login` para obter um token JWT
2. Clique em "Authorize" no Swagger e digite: `Bearer <seu-token>`
3. Teste endpoints protegidos

### Dados de Teste Exemplo
O sistema inclui dados de teste exemplo:
- **CPF de Candidato de Teste:** `12345678901`
- **ID de Vaga Exemplo:** `22222222-2222-2222-2222-222222222222`

---

## Diretrizes de Integração

### Integração Frontend
1. **Fluxo de Autenticação:**
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

2. **Requisições Autenticadas:**
   ```javascript
   const token = localStorage.getItem('authToken');
   const response = await fetch('/api/candidate/123', {
     headers: { 'Authorization': `Bearer ${token}` }
   });
   ```

3. **Upload de Arquivo:**
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

### Esquema do Banco de Dados
O sistema usa Entity Framework Core com PostgreSQL. Entidades principais:
- **Candidate** - Informações principais do candidato
- **Job** - Posições de trabalho disponíveis
- **Tests** - Vários tipos de teste (Português, Matemática, Psicologia, Retenção Visual)
- **Media** - Submissões de áudio/vídeo
- **Evaluations** - Avaliações de candidatos com IA

---

## Notas de Desenvolvimento

### Arquitetura
- **Framework:** ASP.NET Core 9.0
- **Banco de Dados:** PostgreSQL com Entity Framework Core
- **Autenticação:** Tokens JWT Bearer / Azure AD
- **Armazenamento de Arquivos:** Azure Blob Storage
- **Logging:** Serilog com logging estruturado
- **Documentação da API:** Swagger/OpenAPI

### Recursos Principais
- Fluxo de trabalho abrangente de avaliação de candidatos
- Sistema de testes multi-tipo (Português, Matemática, Psicologia, Retenção Visual)
- Avaliação e pontuação com IA
- Upload e armazenamento seguro de mídia
- Candidatura e gerenciamento de vagas
- Integração de dashboard de recrutador
- Monitoramento de saúde e observabilidade

### Recursos de Segurança
- Autenticação por token JWT
- Proteção IDOR (Referência Direta a Objeto Insegura)
- Validação e sanitização de entrada
- Upload seguro de arquivos com restrições de tamanho e tipo
- Configuração CORS
- Middleware de cabeçalhos de segurança

---

## Suporte e Manutenção

Para problemas ou questões relacionadas à API Dignus Candidate:

1. Verifique os logs na saída da aplicação
2. Verifique a conectividade do banco de dados
3. Certifique-se da configuração adequada no `appsettings.json`
4. Revise tokens de autenticação e claims
5. Contate a equipe de desenvolvimento: Bruno, Vitor

### Monitoramento
- Logs de aplicação via Serilog
- Endpoints de verificação de saúde para monitoramento de tempo de atividade
- Métricas de performance através de logging estruturado
- Monitoramento de performance de consultas do banco de dados via Entity Framework