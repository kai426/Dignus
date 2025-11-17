# 📚 Documentação Completa - API Dignus Candidate

> **Documentação técnica completa dos workflows da plataforma Dignus**
> Incluindo exemplos de requisições cURL, Postman e código

---

## 📑 Índice

1. [Informações Gerais](#informações-gerais)
2. [Workflow de Autenticação](#workflow-de-autenticação)
3. [Workflow de Consentimento LGPD](#workflow-de-consentimento-lgpd)
4. [Progresso do Candidato](#progresso-do-candidato)
5. [Workflow de Testes - Português](#workflow-de-testes---português)
6. [Workflow de Testes - Matemática](#workflow-de-testes---matemática)
7. [Workflow de Testes - Psicologia](#workflow-de-testes---psicologia)
8. [Workflow de Testes - Retenção Visual](#workflow-de-testes---retenção-visual)
9. [Workflow de Testes - Entrevista](#workflow-de-testes---entrevista)
10. [Tratamento de Erros](#tratamento-de-erros)
11. [Códigos de Status HTTP](#códigos-de-status-http)

---

## 📋 Informações Gerais

### Ambiente de Desenvolvimento

**URL Base**: `http://localhost:5076`
**URL Base (HTTPS)**: `https://localhost:7214`
**Swagger UI**: `https://localhost:7214/swagger`

**Banco de Dados**: PostgreSQL (Azure)
**Autenticação**: JWT Bearer Token
**Formato de Dados**: JSON

### Candidato de Teste

| Campo | Valor |
|-------|-------|
| **Nome** | Maria Oliveira Costa |
| **CPF** | `07766468000` |
| **Email** | `maria.oliveira@example.com` |
| **Telefone** | `11912345678` |
| **Data Nascimento** | `1998-07-22` |
| **CandidateId** | `0acf8567-0a49-4504-b275-11c346a08a13` |
| **Status** | InProcess |

### Tipos de Teste

| ID | Nome | Tipo de Questões | Auto-corrigido |
|----|------|------------------|----------------|
| `1` | Português | Vídeo + Leitura | ❌ (IA/Manual) |
| `2` | Matemática | Vídeo | ❌ (IA/Manual) |
| `3` | Psicologia | Múltipla Escolha (49 questões) | ✅ Sim |
| `4` | Retenção Visual | Múltipla Escolha (15 questões) | ✅ Sim |
| `5` | Entrevista | Vídeo (5 questões) | ❌ (IA/Manual) |

---

## 🔐 Workflow de Autenticação

### Fluxo Completo

```
┌─────────────────────────────────────────────────────────┐
│                  WORKFLOW DE AUTENTICAÇÃO               │
└─────────────────────────────────────────────────────────┘

1. Solicitar Token por Email
   ↓
2. Receber Código de 6 Dígitos
   ↓
3. Validar Token e Receber JWT
   ↓
4. Usar JWT em Requisições Protegidas
```

---

### Passo 1: Solicitar Token de Autenticação

**Endpoint**: `POST /api/candidate-auth/request-token`

**Descrição**: Candidato solicita um código de 6 dígitos que será enviado por email (em desenvolvimento, aparece nos logs).

#### cURL

```bash
curl -X POST "http://localhost:5076/api/candidate-auth/request-token" \
  -H "Content-Type: application/json" \
  -d '{
    "CPF": "07766468000",
    "Email": "maria.oliveira@example.com"
  }'
```

#### PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5076/api/candidate-auth/request-token" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{
    "CPF": "07766468000",
    "Email": "maria.oliveira@example.com"
  }'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post('/api/candidate-auth/request-token', {
  CPF: '07766468000',
  Email: 'maria.oliveira@example.com'
});
```

#### Resposta de Sucesso (200 OK)

```json
{
  "message": "Código de verificação enviado para mar***@example.com",
  "expirationMinutes": 15
}
```

#### Possíveis Erros

**404 Not Found** - Candidato não encontrado:
```json
{
  "error": "CANDIDATE_NOT_FOUND",
  "message": "Candidato não encontrado no sistema Gupy"
}
```

**400 Bad Request** - Conta bloqueada:
```json
{
  "error": "ACCOUNT_LOCKED",
  "message": "Conta bloqueada devido a múltiplas tentativas falhas. Tente novamente após 13:45",
  "lockedUntil": "2025-11-10T13:45:00Z"
}
```

---

### Passo 2: Obter o Token (Desenvolvimento)

**⚠️ Apenas em Desenvolvimento**: O token é exibido nos logs do servidor.

#### Buscar Token no Banco de Dados

```sql
SELECT "TokenCode", "ExpiresAt", "IsUsed"
FROM "CandidateAuthTokens"
WHERE "CPF" = '07766468000'
ORDER BY "CreatedAt" DESC
LIMIT 1;
```

**Exemplo de Token**: `294595`

**🚀 Em Produção**: O token é enviado via SendGrid para o email do candidato.

---

### Passo 3: Validar Token e Receber JWT

**Endpoint**: `POST /api/candidate-auth/validate-token`

**Descrição**: Valida o código de 6 dígitos e retorna um JWT para autenticação.

#### cURL

```bash
curl -X POST "http://localhost:5076/api/candidate-auth/validate-token" \
  -H "Content-Type: application/json" \
  -d '{
    "CPF": "07766468000",
    "TokenCode": "294595"
  }'
```

#### PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5076/api/candidate-auth/validate-token" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{
    "CPF": "07766468000",
    "TokenCode": "294595"
  }'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post('/api/candidate-auth/validate-token', {
  CPF: '07766468000',
  TokenCode: '294595'
});

// Armazenar tokens
localStorage.setItem('accessToken', response.data.accessToken);
localStorage.setItem('refreshToken', response.data.refreshToken);
localStorage.setItem('candidateId', response.data.candidateId);
```

#### Resposta de Sucesso (200 OK)

```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "unETU4ueROEmB3D0Zi4fk22wd3+HaWHs...",
  "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
  "requiresLGPDConsent": true,
  "message": "Autenticação realizada com sucesso"
}
```

**Campos Importantes**:
- `accessToken`: Use em todas as requisições protegidas
- `requiresLGPDConsent`: Se `true`, redirecionar para tela de consentimento
- `candidateId`: ID único do candidato

#### Possíveis Erros

**400 Bad Request** - Token inválido ou expirado:
```json
{
  "error": "INVALID_TOKEN",
  "message": "Código inválido ou expirado"
}
```

**400 Bad Request** - Email não corresponde:
```json
{
  "error": "EMAIL_MISMATCH",
  "message": "O e-mail fornecido não corresponde ao cadastrado no sistema"
}
```

---

### Passo 4: Verificar Status de Bloqueio (Opcional)

**Endpoint**: `GET /api/candidate-auth/lockout-status/{cpf}`

#### cURL

```bash
curl -X GET "http://localhost:5076/api/candidate-auth/lockout-status/07766468000"
```

#### Resposta (200 OK)

```json
{
  "isLockedOut": false,
  "lockedUntil": null,
  "remainingMinutes": 0
}
```

---

## 📝 Workflow de Consentimento LGPD

### Fluxo Completo

```
┌─────────────────────────────────────────────────────────┐
│              WORKFLOW DE CONSENTIMENTO LGPD             │
└─────────────────────────────────────────────────────────┘

1. Verificar Status de Consentimento
   ↓
2. Obter Política de Privacidade
   ↓
3. Apresentar Formulário (3 checkboxes)
   ↓
4. Enviar Consentimento
   ↓
5. Liberar Acesso ao Dashboard
```

---

### Passo 1: Verificar Status de Consentimento

**Endpoint**: `GET /api/consent/status/{cpf}`

#### cURL

```bash
curl -X GET "http://localhost:5076/api/consent/status/07766468000"
```

#### PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5076/api/consent/status/07766468000" `
  -Method Get
```

#### JavaScript (Axios)

```javascript
const response = await axios.get(`/api/consent/status/${cpf}`);
if (!response.data.hasAccepted) {
  // Redirecionar para página de consentimento
  router.push('/consent');
}
```

#### Resposta (200 OK)

```json
{
  "hasAccepted": false,
  "acceptedAt": null,
  "privacyPolicyVersion": null
}
```

---

### Passo 2: Obter Informações da Política de Privacidade

**Endpoint**: `GET /api/consent/privacy-policy`

#### cURL

```bash
curl -X GET "http://localhost:5076/api/consent/privacy-policy"
```

#### Resposta (200 OK)

```json
{
  "version": "v1.0",
  "url": "/docs/privacy-policy.pdf",
  "lastUpdated": "2025-01-01T00:00:00+00:00"
}
```

---

### Passo 3: Enviar Consentimento LGPD

**Endpoint**: `POST /api/consent`

**⚠️ Importante**: Todos os três consentimentos devem ser `true`.

#### cURL

```bash
curl -X POST "http://localhost:5076/api/consent" \
  -H "Content-Type: application/json" \
  -d '{
    "CPF": "07766468000",
    "AcceptPrivacyPolicy": true,
    "AcceptDataSharing": true,
    "AcceptCreditAnalysis": true
  }'
```

#### PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5076/api/consent" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{
    "CPF": "07766468000",
    "AcceptPrivacyPolicy": true,
    "AcceptDataSharing": true,
    "AcceptCreditAnalysis": true
  }'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post('/api/consent', {
  CPF: cpf,
  AcceptPrivacyPolicy: true,
  AcceptDataSharing: true,
  AcceptCreditAnalysis: true
});
```

#### Resposta de Sucesso (200 OK)

```json
{
  "success": true,
  "message": "Consentimento registrado com sucesso"
}
```

#### Consentimentos Obrigatórios

| Campo | Descrição | Obrigatório |
|-------|-----------|-------------|
| `AcceptPrivacyPolicy` | Aceita política de privacidade | ✅ Sim |
| `AcceptDataSharing` | Aceita compartilhamento de dados | ✅ Sim |
| `AcceptCreditAnalysis` | Aceita análise de crédito | ✅ Sim |

---

## 📊 Workflow de Testes - Português

### Características

- **Tipo**: Português (TestType = 1)
- **Questões**: 3 vídeos + 1 texto de leitura
- **Correção**: Manual/IA
- **Tempo Limite**: Sem limite
- **Dificuldade**: Fácil, Médio, Difícil

### Fluxo Completo

```
┌─────────────────────────────────────────────────────────┐
│           WORKFLOW DE TESTE - PORTUGUÊS                 │
└─────────────────────────────────────────────────────────┘

1. Criar Teste de Português
   ↓
2. Iniciar Teste
   ↓
3. Obter Questões (3 vídeo + 1 leitura)
   ↓
4. Upload de Vídeos (3x)
   ↓
5. Responder Questão de Leitura
   ↓
6. Submeter Teste
   ↓
7. Aguardar Avaliação da IA/Recrutador
```

---

### Passo 1: Criar Teste de Português

**Endpoint**: `POST /api/v2/tests`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "testType": 1,
    "difficultyLevel": "medium"
  }'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post('/api/v2/tests', {
  candidateId: candidateId,
  testType: 1, // Português
  difficultyLevel: 'medium'
}, {
  headers: {
    'Authorization': `Bearer ${accessToken}`
  }
});

const testId = response.data.id;
```

#### Resposta de Sucesso (201 Created)

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
  "testType": "Portuguese",
  "status": "NotStarted",
  "score": null,
  "rawScore": null,
  "maxPossibleScore": null,
  "startedAt": null,
  "completedAt": null,
  "durationSeconds": null,
  "timeLimitSeconds": null,
  "createdAt": "2025-11-10T13:30:00Z"
}
```

---

### Passo 2: Iniciar Teste

**Endpoint**: `POST /api/v2/tests/{testId}/start`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/start?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

#### Resposta (200 OK)

```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "status": "InProgress",
  "startedAt": "2025-11-10T13:31:00Z",
  ...
}
```

---

### Passo 3: Obter Questões do Teste

**Endpoint**: `GET /api/v2/tests/{testId}/questions`

#### cURL

```bash
curl -X GET "http://localhost:5076/api/v2/tests/{testId}/questions?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

#### Resposta (200 OK)

```json
[
  {
    "id": "question-snapshot-guid-1",
    "questionText": "Qual é a ideia principal do texto apresentado?",
    "optionsJson": null,
    "allowMultipleAnswers": false,
    "maxAnswersAllowed": null,
    "questionOrder": 1,
    "pointValue": 10.0,
    "estimatedTimeSeconds": null
  },
  {
    "id": "question-snapshot-guid-2",
    "questionText": "Descreva o personagem principal da história.",
    "optionsJson": null,
    "allowMultipleAnswers": false,
    "maxAnswersAllowed": null,
    "questionOrder": 2,
    "pointValue": 10.0,
    "estimatedTimeSeconds": null
  },
  {
    "id": "question-snapshot-guid-3",
    "questionText": "Qual é sua opinião sobre o tema abordado?",
    "optionsJson": null,
    "allowMultipleAnswers": false,
    "maxAnswersAllowed": null,
    "questionOrder": 3,
    "pointValue": 15.0,
    "estimatedTimeSeconds": null
  },
  {
    "id": "question-snapshot-guid-4",
    "questionText": "Após ler o texto, qual alternativa melhor resume o conteúdo?",
    "optionsJson": "[{\"id\":\"A\",\"text\":\"O texto fala sobre...\"},{\"id\":\"B\",\"text\":\"O autor argumenta que...\"}]",
    "allowMultipleAnswers": false,
    "maxAnswersAllowed": 1,
    "questionOrder": 4,
    "pointValue": 10.0,
    "estimatedTimeSeconds": 120
  }
]
```

---

### Passo 4: Upload de Resposta em Vídeo

**Endpoint**: `POST /api/v2/tests/{testId}/videos?candidateId={candidateId}`

**Content-Type**: `multipart/form-data`

**Query Parameters**:
- `candidateId` (obrigatório): ID do candidato

**Form Data**:
- `questionSnapshotId` (opcional): ID da questão sendo respondida
- `videoFile` (obrigatório): Arquivo de vídeo
- `questionNumber` (opcional, recomendado): Número da questão (1-100). Se não fornecido, será derivado automaticamente do `questionSnapshotId` via consulta ao banco de dados. **Recomendação**: Enviar o valor de `questionOrder` recebido ao obter as questões para melhor performance.
- `responseType` (opcional): Tipo de resposta (para Português: Reading ou QuestionAnswer)

---

**⚠️ IMPORTANTE - Testes de Português: 2 Tipos de Upload de Vídeo**

Os testes de Português possuem **4 vídeos no total**:
- **3 vídeos para questões** (COM `questionSnapshotId`)
- **1 vídeo para leitura do texto** (SEM `questionSnapshotId`)

#### Tipo 1: Upload de Vídeo para Questões (3x)

**Use este formato para responder as 3 questões:**

- ✅ **DEVE** incluir `questionSnapshotId` (obtido do endpoint `/questions`)
- ✅ **DEVE** incluir `responseType = 2` (QuestionAnswer)
- ✅ **DEVE** incluir `questionNumber` = 1, 2 ou 3

**Exemplo cURL - Questão 1:**
```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/videos?candidateId={candidateId}" \
  -H "Authorization: Bearer {accessToken}" \
  -F "questionSnapshotId=8a12adce-fa42-4e44-afce-3881e006d409" \
  -F "questionNumber=1" \
  -F "responseType=2" \
  -F "videoFile=@questao1.mp4"
```

#### Tipo 2: Upload de Vídeo para Leitura do Texto (1x)

**Use este formato para o vídeo de leitura:**

- ❌ **NÃO** incluir `questionSnapshotId` (deixar vazio/omitir)
- ✅ **DEVE** incluir `responseType = 1` (Reading)
- ✅ **DEVE** incluir `questionNumber = 4` (ou próximo número sequencial)

**Exemplo cURL - Leitura:**
```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/videos?candidateId={candidateId}" \
  -H "Authorization: Bearer {accessToken}" \
  -F "questionNumber=4" \
  -F "responseType=1" \
  -F "videoFile=@leitura.mp4"
```

**Nota:** Observe que o vídeo de leitura **NÃO possui o campo `questionSnapshotId`**!

---

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/videos?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}" \
  -F "questionSnapshotId=question-snapshot-guid-1" \
  -F "videoFile=@/path/to/video.mp4"
```

#### JavaScript (Axios com FormData)

**Para Questões (3x) - COM questionSnapshotId:**
```javascript
// Upload de vídeo para uma questão (questões 1, 2, 3)
const uploadQuestionVideo = async (question, videoBlob) => {
  const formData = new FormData();
  formData.append('questionSnapshotId', question.id);  // ✅ Incluir para questões
  formData.append('questionNumber', question.questionOrder);
  formData.append('responseType', '2');  // QuestionAnswer
  formData.append('videoFile', videoBlob, 'questao.mp4');

  const response = await axios.post(
    `/api/v2/tests/${testId}/videos?candidateId=${candidateId}`,
    formData,
    {
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        console.log(`Upload: ${percentCompleted}%`);
      }
    }
  );

  return response.data;
};
```

**Para Leitura do Texto (1x) - SEM questionSnapshotId:**
```javascript
// Upload de vídeo para leitura do texto
const uploadReadingVideo = async (videoBlob) => {
  const formData = new FormData();
  // ❌ NÃO incluir questionSnapshotId para leitura!
  formData.append('questionNumber', '4');
  formData.append('responseType', '1');  // Reading
  formData.append('videoFile', videoBlob, 'leitura.mp4');

  const response = await axios.post(
    `/api/v2/tests/${testId}/videos?candidateId=${candidateId}`,
    formData,
    {
      headers: {
        'Authorization': `Bearer ${accessToken}`,
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round((progressEvent.loaded * 100) / progressEvent.total);
        console.log(`Upload leitura: ${percentCompleted}%`);
      }
    }
  );

  return response.data;
};
```

#### Resposta de Sucesso (200 OK)

```json
{
  "id": "video-response-guid",
  "questionSnapshotId": "question-snapshot-guid-1",
  "questionNumber": 1,
  "responseType": null,
  "blobUrl": "https://dignusstorage.blob.core.windows.net/test-videos/video-response-guid.mp4",
  "fileSizeBytes": 10485760,
  "uploadedAt": "2025-11-10T13:35:00Z",
  "score": null,
  "feedback": null,
  "verdict": null,
  "analyzedAt": null
}
```

**Restrições**:
- Tamanho máximo: 500 MB
- Formatos aceitos: `.mp4`, `.mov`, `.avi`, `.wmv`
- MIME types: `video/mp4`, `video/quicktime`, `video/x-msvideo`, `video/x-ms-wmv`

---

### Passo 5: Responder Questão de Múltipla Escolha

**Endpoint**: `POST /api/v2/tests/{testId}/answers?candidateId={candidateId}`

**Query Parameters**:
- `candidateId` (obrigatório): ID do candidato

**Request Body**: Array de respostas (não wrapper object)

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/answers?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '[
    {
      "questionSnapshotId": "question-snapshot-guid-4",
      "selectedAnswers": ["A"],
      "responseTimeMs": 5000
    }
  ]'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post(
  `/api/v2/tests/${testId}/answers?candidateId=${candidateId}`,
  [
    {
      questionSnapshotId: 'question-snapshot-guid-4',
      selectedAnswers: ['A'],
      responseTimeMs: 5000
    }
  ],
  {
    headers: {
      'Authorization': `Bearer ${accessToken}`
    }
  }
);
```

#### Resposta (200 OK)

**Nota**: A resposta é um array direto, não um objeto com wrapper.

```json
[
  {
    "id": "response-guid",
    "questionSnapshotId": "question-snapshot-guid-4",
    "selectedAnswers": ["A"],
    "responseTimeMs": 5000,
    "answeredAt": "2025-11-10T13:40:00Z",
    "isCorrect": null,
    "pointsEarned": null
  }
]
```

---

### Passo 6: Submeter Teste

**Endpoint**: `POST /api/v2/tests/{testId}/submit`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/submit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "testId": "550e8400-e29b-41d4-a716-446655440000",
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "answers": []
  }'
```

**Nota Importante sobre o campo `answers`**:
- Para testes de múltipla escolha (Psicologia, Retenção Visual), o array `answers` deve conter as respostas das questões
- Para testes baseados em vídeo (Português, Matemática, Entrevista), o array `answers` deve estar **vazio** `[]`, pois as respostas já foram enviadas via upload de vídeo no endpoint `/api/v2/tests/{testId}/videos`

#### Resposta (200 OK)

```json
{
  "success": true,
  "testId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Submitted",
  "score": null,
  "completedAt": "2025-11-10T13:45:00Z",
  "autoGraded": false,
  "needsManualReview": true,
  "message": "Teste submetido com sucesso. Aguarde avaliação."
}
```

**Nota**: Testes de vídeo requerem avaliação manual ou por IA. O `score` será `null` até a avaliação.

---

### Passo 7: Verificar Status do Teste

**Endpoint**: `GET /api/v2/tests/{testId}/status`

#### cURL

```bash
curl -X GET "http://localhost:5076/api/v2/tests/{testId}/status?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

#### Resposta (200 OK)

```json
{
  "testId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "Submitted",
  "progress": 100,
  "answeredQuestions": 4,
  "totalQuestions": 4,
  "videoUploads": 3,
  "requiredVideos": 3,
  "timeElapsed": 900,
  "timeRemaining": null,
  "canSubmit": false,
  "isSubmitted": true
}
```

---

## 🔢 Workflow de Testes - Matemática

### Características

- **Tipo**: Matemática (TestType = 2)
- **Questões**: 2 vídeos de raciocínio lógico
- **Correção**: Manual/IA
- **Tempo Limite**: Sem limite
- **Foco**: Resolução de problemas, cálculos, raciocínio

### Fluxo Completo

```
Criar Teste (type=2) → Iniciar → Obter Questões →
Upload Vídeo 1 → Upload Vídeo 2 → Submeter → Aguardar Avaliação
```

---

### Criar Teste de Matemática

**Endpoint**: `POST /api/v2/tests`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "testType": 2,
    "difficultyLevel": "medium"
  }'
```

**Exemplo de Questões**:

1. **Vídeo 1** - "Uma fábrica produz 1.200 peças por dia. Se a produção aumentar 15%, quantas peças serão produzidas? Explique seu raciocínio em vídeo."

2. **Vídeo 2** - "Três máquinas produzem 450 unidades em 6 horas. Quantas máquinas são necessárias para produzir 750 unidades em 5 horas? Resolva em vídeo mostrando os cálculos."

*O restante do workflow é idêntico ao teste de Português (upload de vídeos + submissão).*

---

## 🧠 Workflow de Testes - Psicologia

### Características

- **Tipo**: Psicologia (TestType = 3)
- **Questões**: 49 questões de múltipla escolha
- **Correção**: ⚠️ **Não há correção automática** (teste de perfil comportamental, sem respostas certas/erradas)
- **Tempo Limite**: 60 minutos
- **Seleção**: ⚠️ **TODAS as 49 questões são entregues em ordem cronológica** (não aleatória)

### Fluxo Completo

```
┌─────────────────────────────────────────────────────────┐
│           WORKFLOW DE TESTE - PSICOLOGIA                │
└─────────────────────────────────────────────────────────┘

1. Criar Teste (TODAS as 49 questões em ordem cronológica)
   ↓
2. Obter Questões (49x múltipla escolha em ordem)
   ↓
3. Responder Questões (envio em lote - array direto)
   ↓
4. Verificar Progresso (endpoint /status)
   ↓
5. Iniciar Teste (começa timer de 60 min)
   ↓
6. Submeter Teste (com answers: [])
   ↓
7. Teste Finalizado (sem nota - perfil comportamental)
```

---

### Criar Teste de Psicologia

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "testType": 3,
    "difficultyLevel": null
  }'
```

#### Resposta (201 Created)

```json
{
  "id": "psych-test-guid",
  "testType": "Psychology",
  "status": "NotStarted",
  "timeLimitSeconds": 3600,
  "totalQuestions": 49,
  "questions": [
    // Array com TODAS as 49 questões em ordem cronológica
    // (não há seleção aleatória para testes de Psicologia)
  ]
}
```

**⚠️ IMPORTANTE**:
- O teste de Psicologia entrega **TODAS as 49 questões** disponíveis no banco
- As questões são ordenadas cronologicamente (por `CreatedAt`)
- **NÃO há seleção aleatória** para este tipo de teste

---

### Iniciar Teste de Psicologia

**⚠️ Importante**: O timer de 60 minutos começa neste momento!

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/psych-test-guid/start?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

---

### Obter Questões (49 questões)

**⚠️ Nota**: As questões já vêm no response da criação do teste. Este endpoint é opcional para recarregar questões.

#### cURL

```bash
curl -X GET "http://localhost:5076/api/v2/tests/psych-test-guid?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

#### Exemplo de Resposta (amostra de 3 das 49 questões)

```json
[
  {
    "id": "q1-guid",
    "questionText": "Como você prefere trabalhar?",
    "optionsJson": "[{\"id\":\"A\",\"text\":\"Sozinho, focado\"},{\"id\":\"B\",\"text\":\"Em equipe, colaborando\"},{\"id\":\"C\",\"text\":\"Depende da tarefa\"},{\"id\":\"D\",\"text\":\"Com supervisão próxima\"}]",
    "allowMultipleAnswers": false,
    "maxAnswersAllowed": 1,
    "questionOrder": 1,
    "pointValue": 2.0,
    "estimatedTimeSeconds": 30
  },
  {
    "id": "q2-guid",
    "questionText": "Diante de um problema complexo, você:",
    "optionsJson": "[{\"id\":\"A\",\"text\":\"Analisa todas as variáveis\"},{\"id\":\"B\",\"text\":\"Toma decisão rápida\"},{\"id\":\"C\",\"text\":\"Consulta colegas\"},{\"id\":\"D\",\"text\":\"Busca exemplos similares\"}]",
    "allowMultipleAnswers": false,
    "maxAnswersAllowed": 1,
    "questionOrder": 2,
    "pointValue": 2.0,
    "estimatedTimeSeconds": 30
  },
  // ... mais 47 questões (total de 49)
]
```

---

### Responder Questões (Envio em Lote)

#### cURL - Enviando 5 Respostas

```bash
curl -X POST "http://localhost:5076/api/v2/tests/psych-test-guid/answers?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '[
    {"questionSnapshotId": "q1-guid", "selectedAnswers": ["B"], "responseTimeMs": 15000},
    {"questionSnapshotId": "q2-guid", "selectedAnswers": ["A"], "responseTimeMs": 18000},
    {"questionSnapshotId": "q3-guid", "selectedAnswers": ["C"], "responseTimeMs": 12000},
    {"questionSnapshotId": "q4-guid", "selectedAnswers": ["A"], "responseTimeMs": 20000},
    {"questionSnapshotId": "q5-guid", "selectedAnswers": ["D"], "responseTimeMs": 16000}
  ]'
```

**⚠️ FORMATO CRÍTICO DO BODY**:
- O body **DEVE SER UM ARRAY DIRETO** de respostas
- **NÃO envolva** o array em um objeto com `testId`, `candidateId`, `answers`
- O `candidateId` vai na **query string**, não no body

**❌ ERRADO** (não funciona):
```json
{
  "testId": "...",
  "candidateId": "...",
  "answers": [...]
}
```

**✅ CORRETO** (funciona):
```json
[
  {"questionSnapshotId": "...", "selectedAnswers": ["A"], "responseTimeMs": 5000}
]
```

**Dica**: Envie respostas em lotes de 10-20 para melhor performance.

---

### Verificar Progresso do Teste

**⚠️ NOVO**: Endpoint para verificar quantas questões foram respondidas.

#### cURL

```bash
curl -X GET "http://localhost:5076/api/v2/tests/psych-test-guid/status?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

#### Resposta (200 OK)

```json
{
  "testId": "psych-test-guid",
  "status": "NotStarted",
  "totalQuestions": 49,
  "questionsAnswered": 35,
  "videosUploaded": 0,
  "videosRequired": 0,
  "canStart": true,
  "canSubmit": false,
  "startedAt": null,
  "remainingTimeSeconds": null
}
```

---

### Submeter Teste de Psicologia

**⚠️ IMPORTANTE**: Para testes de Psicologia, o array `answers` deve estar **vazio** `[]` porque as respostas já foram enviadas via endpoint `/answers`.

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/psych-test-guid/submit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "testId": "psych-test-guid",
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "answers": []
  }'
```

#### Resposta (200 OK) - Teste de Perfil Comportamental

```json
{
  "testId": "psych-test-guid",
  "status": "Submitted",
  "score": 0,
  "rawScore": 0,
  "maxPossibleScore": 0,
  "correctAnswers": 0,
  "totalQuestions": 49,
  "durationSeconds": 1850,
  "message": "Test submitted successfully"
}
```

**⚠️ Notas Importantes**:
- Testes de Psicologia **NÃO têm correção automática** (não há respostas certas ou erradas)
- Os scores são sempre 0 (é um teste de perfil comportamental)
- O status final é `Submitted` (não `Approved` ou `Rejected`)
- As respostas são analisadas posteriormente para traçar o perfil do candidato

---

## 👁️ Workflow de Testes - Retenção Visual

### Características

- **Tipo**: Retenção Visual (TestType = 4)
- **Questões**: 15 questões de múltipla escolha (aleatórias)
- **Correção**: ✅ **Automática**
- **Tempo Limite**: 20 minutos
- **Foco**: Memória visual, padrões, sequências

### Criar Teste de Retenção Visual

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "testType": 4,
    "difficultyLevel": null
  }'
```

#### Resposta (201 Created)

```json
{
  "id": "visual-test-guid",
  "testType": "VisualRetention",
  "status": "NotStarted",
  "timeLimitSeconds": 1200,
  "totalQuestions": 15,
  ...
}
```

**Diferença dos outros testes**:
- Questões incluem imagens/padrões visuais
- Timer de apenas 20 minutos
- Apenas 15 questões
- Teste de memória e atenção visual

*O workflow de resposta e submissão é idêntico ao teste de Psicologia.*

---

## 🎤 Workflow de Testes - Entrevista

### Características

- **Tipo**: Entrevista (TestType = 5)
- **Questões**: 5 vídeos comportamentais
- **Correção**: Manual/IA
- **Tempo Limite**: Sem limite
- **Foco**: Comportamento, experiência, soft skills

### Criar Teste de Entrevista

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "testType": 5,
    "difficultyLevel": null
  }'
```

**Exemplo de Questões de Entrevista**:

1. **Apresentação**: "Fale sobre você, sua formação e experiência profissional."
2. **Situação de Pressão**: "Descreva uma situação em que você teve que trabalhar sob pressão. Como lidou?"
3. **Resolução de Conflitos**: "Conte sobre um conflito com um colega de trabalho e como foi resolvido."
4. **Conquistas**: "Qual foi sua maior conquista profissional? Por quê?"
5. **Objetivos**: "Onde você se vê daqui a 5 anos? Quais são seus objetivos?"

*O workflow é idêntico ao teste de Português (5 uploads de vídeo + submissão).*

---

## ⚠️ Tratamento de Erros

### Erros de Autenticação

#### 401 Unauthorized - Token Ausente ou Inválido

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Token de autenticação inválido ou expirado"
}
```

**Ação**: Redirecionar para tela de login.

---

#### 403 Forbidden - Sem Permissão

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Forbidden",
  "status": 403,
  "detail": "Você não tem permissão para acessar este recurso"
}
```

**Ação**: Verificar se o candidateId do JWT corresponde ao recurso solicitado.

---

### Erros de Validação

#### 400 Bad Request - Dados Inválidos

```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Bad Request",
  "status": 400,
  "errors": {
    "CPF": ["CPF inválido"],
    "Email": ["Email inválido"]
  }
}
```

**Ação**: Mostrar erros específicos nos campos do formulário.

---

### Erros de Teste

#### 409 Conflict - Teste Já Existe

```json
{
  "error": "Test already exists for this candidate and type",
  "existingTestId": "existing-test-guid",
  "status": "InProgress"
}
```

**Ação**: Redirecionar para o teste existente em vez de criar novo.

---

#### 400 Bad Request - Teste Não Iniciado

```json
{
  "error": "Test must be started before answering"
}
```

**Ação**: Chamar `POST /api/v2/tests/{testId}/start` antes de responder questões.

---

#### 400 Bad Request - Teste Já Submetido

```json
{
  "error": "Test has already been submitted"
}
```

**Ação**: Não permitir edição. Mostrar resultados ou status atual.

---

#### 400 Bad Request - Tempo Esgotado

```json
{
  "error": "Test time limit exceeded",
  "timeLimitSeconds": 3600,
  "elapsedSeconds": 3750
}
```

**Ação**: Submeter automaticamente ou informar que o tempo acabou.

---

### Erros de Upload

#### 400 Bad Request - Arquivo Muito Grande

```json
{
  "error": "File size exceeds maximum allowed (500 MB)",
  "fileSize": 524288000,
  "maxSize": 524288000
}
```

**Ação**: Comprimir vídeo ou usar menor resolução.

---

#### 400 Bad Request - Formato Inválido

```json
{
  "error": "Invalid file format. Allowed: .mp4, .mov, .avi, .wmv",
  "receivedFormat": ".mpeg"
}
```

**Ação**: Converter vídeo para formato aceito.

---

## 📊 Códigos de Status HTTP

### Códigos de Sucesso

| Código | Nome | Uso |
|--------|------|-----|
| `200` | OK | Operação bem-sucedida |
| `201` | Created | Recurso criado (teste, resposta) |
| `204` | No Content | Operação bem-sucedida sem retorno |

### Códigos de Erro do Cliente

| Código | Nome | Uso |
|--------|------|-----|
| `400` | Bad Request | Dados inválidos ou erro de validação |
| `401` | Unauthorized | Token ausente ou inválido |
| `403` | Forbidden | Sem permissão para o recurso |
| `404` | Not Found | Recurso não encontrado |
| `409` | Conflict | Conflito (teste já existe) |
| `422` | Unprocessable Entity | Entidade não processável |

### Códigos de Erro do Servidor

| Código | Nome | Uso |
|--------|------|-----|
| `500` | Internal Server Error | Erro interno do servidor |
| `503` | Service Unavailable | Serviço temporariamente indisponível |

---

## 🔧 Utilitários e Snippets

### JavaScript: Formatação de CPF

```javascript
function formatCPF(cpf) {
  const cleaned = cpf.replace(/\D/g, '');
  return cleaned.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, '$1.$2.$3-$4');
}

// Uso
formatCPF('07766468000'); // "077.664.680-00"
```

### JavaScript: Validação de CPF

```javascript
function isValidCPF(cpf) {
  cpf = cpf.replace(/\D/g, '');

  if (cpf.length !== 11 || /^(\d)\1{10}$/.test(cpf)) {
    return false;
  }

  let sum = 0;
  let remainder;

  for (let i = 1; i <= 9; i++) {
    sum += parseInt(cpf.substring(i - 1, i)) * (11 - i);
  }

  remainder = (sum * 10) % 11;
  if (remainder === 10 || remainder === 11) remainder = 0;
  if (remainder !== parseInt(cpf.substring(9, 10))) return false;

  sum = 0;
  for (let i = 1; i <= 10; i++) {
    sum += parseInt(cpf.substring(i - 1, i)) * (12 - i);
  }

  remainder = (sum * 10) % 11;
  if (remainder === 10 || remainder === 11) remainder = 0;
  if (remainder !== parseInt(cpf.substring(10, 11))) return false;

  return true;
}

// Uso
isValidCPF('077.664.680-00'); // true
```

### JavaScript: Timer de Teste

```javascript
function startTestTimer(timeLimitSeconds, onTick, onExpire) {
  const startTime = Date.now();
  const endTime = startTime + (timeLimitSeconds * 1000);

  const interval = setInterval(() => {
    const now = Date.now();
    const remaining = Math.floor((endTime - now) / 1000);

    if (remaining <= 0) {
      clearInterval(interval);
      onExpire();
    } else {
      onTick(remaining);
    }
  }, 1000);

  return () => clearInterval(interval); // cleanup function
}

// Uso
const cleanup = startTestTimer(
  3600, // 60 minutos
  (remaining) => {
    const minutes = Math.floor(remaining / 60);
    const seconds = remaining % 60;
    console.log(`Tempo restante: ${minutes}:${seconds.toString().padStart(2, '0')}`);
  },
  () => {
    alert('Tempo esgotado!');
    submitTest();
  }
);
```

### JavaScript: Progress Bar

```javascript
function calculateProgress(answeredQuestions, totalQuestions) {
  return Math.round((answeredQuestions / totalQuestions) * 100);
}

// Uso em React
<ProgressBar
  value={calculateProgress(currentQuestion, totalQuestions)}
  max={100}
/>
```

---

## 📞 Suporte e Contato

**Documentação Oficial**: https://docs.dignus.com
**Swagger UI**: https://localhost:7214/swagger
**Repositório**: https://github.com/bemol/dignus

**Time de Desenvolvimento**:
- Backend: Dignus Backend Team
- Frontend: Bemol Digital Team

---

## 📝 Notas de Versão

**Versão**: 1.0.0
**Data**: 2025-11-10
**Última Atualização**: 2025-11-10

### Changelog

#### v1.0.0 (2025-11-10)
- ✅ Documentação inicial completa
- ✅ Workflow de autenticação testado
- ✅ Workflow de consentimento LGPD testado
- ✅ Workflows de todos os 5 tipos de testes documentados
- ✅ Exemplos cURL para todas as APIs
- ✅ Tratamento de erros documentado
- ✅ Utilitários JavaScript incluídos

### Próximas Versões

- [ ] Adicionar exemplos em Python
- [ ] Adicionar exemplos em C#
- [ ] Documentar API de avaliação de testes
- [ ] Documentar webhooks (se implementado)
- [ ] Adicionar diagramas de sequência
- [ ] Documentar integração com Gupy/Databricks

---

**Fim da Documentação v1.0.0**

*Gerado automaticamente por Claude Code - Dignus Platform*
