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
   - [Atualizar Status PCD](#atualizar-status-pcd-do-candidato)
   - [Upload Documento PCD](#upload-de-documento-comprobatório-pcd)
   - [Atualizar Status Estrangeiro](#atualizar-status-de-estrangeiro-foreigner-do-candidato)
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

| Campo               | Valor                                  |
| ------------------- | -------------------------------------- |
| **Nome**            | Maria Oliveira Costa                   |
| **CPF**             | `07766468000`                          |
| **Email**           | `maria.oliveira@example.com`           |
| **Telefone**        | `11912345678`                          |
| **Data Nascimento** | `1998-07-22`                           |
| **CandidateId**     | `0acf8567-0a49-4504-b275-11c346a08a13` |
| **Status**          | InProcess                              |

### Tipos de Teste

| ID  | Nome            | Tipo de Questões                             | Auto-corrigido |
| --- | --------------- | -------------------------------------------- | -------------- |
| `1` | Português       | Vídeo + Leitura                              | ❌ (IA/Manual) |
| `2` | Matemática      | Vídeo                                        | ❌ (IA/Manual) |
| `3` | Psicologia      | Múltipla Escolha (52 questões)               | ✅ Sim         |
| `4` | Retenção Visual | Múltipla Escolha (29 questões, 6 opções A-F) | ✅ Sim         |
| `5` | Entrevista      | Vídeo (5 questões)                           | ❌ (IA/Manual) |

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
const response = await axios.post("/api/candidate-auth/request-token", {
  CPF: "07766468000",
  Email: "maria.oliveira@example.com",
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
const response = await axios.post("/api/candidate-auth/validate-token", {
  CPF: "07766468000",
  TokenCode: "294595",
});

// Armazenar tokens
localStorage.setItem("accessToken", response.data.accessToken);
localStorage.setItem("refreshToken", response.data.refreshToken);
localStorage.setItem("candidateId", response.data.candidateId);
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
  router.push("/consent");
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
const response = await axios.post("/api/consent", {
  CPF: cpf,
  AcceptPrivacyPolicy: true,
  AcceptDataSharing: true,
  AcceptCreditAnalysis: true,
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

| Campo                  | Descrição                        | Obrigatório |
| ---------------------- | -------------------------------- | ----------- |
| `AcceptPrivacyPolicy`  | Aceita política de privacidade   | ✅ Sim      |
| `AcceptDataSharing`    | Aceita compartilhamento de dados | ✅ Sim      |
| `AcceptCreditAnalysis` | Aceita análise de crédito        | ✅ Sim      |

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
const response = await axios.post(
  "/api/v2/tests",
  {
    candidateId: candidateId,
    testType: 1, // Português
    difficultyLevel: "medium",
  },
  {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  }
);

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
  formData.append("questionSnapshotId", question.id); // ✅ Incluir para questões
  formData.append("questionNumber", question.questionOrder);
  formData.append("responseType", "2"); // QuestionAnswer
  formData.append("videoFile", videoBlob, "questao.mp4");

  const response = await axios.post(
    `/api/v2/tests/${testId}/videos?candidateId=${candidateId}`,
    formData,
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "multipart/form-data",
      },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round(
          (progressEvent.loaded * 100) / progressEvent.total
        );
        console.log(`Upload: ${percentCompleted}%`);
      },
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
  formData.append("questionNumber", "4");
  formData.append("responseType", "1"); // Reading
  formData.append("videoFile", videoBlob, "leitura.mp4");

  const response = await axios.post(
    `/api/v2/tests/${testId}/videos?candidateId=${candidateId}`,
    formData,
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "multipart/form-data",
      },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round(
          (progressEvent.loaded * 100) / progressEvent.total
        );
        console.log(`Upload leitura: ${percentCompleted}%`);
      },
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
      questionSnapshotId: "question-snapshot-guid-4",
      selectedAnswers: ["A"],
      responseTimeMs: 5000,
    },
  ],
  {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
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

_O restante do workflow é idêntico ao teste de Português (upload de vídeos + submissão)._

---

## 🧠 Workflow de Testes - Psicologia

### Características

- **Tipo**: Psicologia (TestType = 3)
- **Questões**: 52 questões de múltipla escolha
- **Correção**: ⚠️ **Não há correção automática** (teste de perfil comportamental, sem respostas certas/erradas)
- **Tempo Limite**: Sem limite de tempo
- **Seleção**: ⚠️ **TODAS as 52 questões são entregues em ordem cronológica** (não aleatória)
- **⚠️ IMPORTANTE**: Durante o teste de Psicologia (seção Diversidade e Inclusão), o candidato responde se é PCD. Use o endpoint de atualização de status PCD para salvar essa informação.

### Fluxo Completo

```
┌─────────────────────────────────────────────────────────┐
│           WORKFLOW DE TESTE - PSICOLOGIA                │
└─────────────────────────────────────────────────────────┘

1. Criar Teste (TODAS as 52 questões em ordem cronológica)
   ↓
2. Obter Questões (52x múltipla escolha em ordem)
   ↓
3. Responder Questões (envio em lote - array direto)
   ↓
4. **Atualizar Status PCD** (quando candidato responde questão 47 - Diversidade e Inclusão)
   ↓
5. **Upload Documento PCD** (se isPCD = true, upload de laudo/certificado)
   ↓
6. Verificar Progresso (endpoint /status)
   ↓
7. Iniciar Teste (começa timer de 60 min)
   ↓
8. Submeter Teste (com answers: [])
   ↓
9. Teste Finalizado (sem nota - perfil comportamental)
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
  "timeLimitSeconds": null,
  "totalQuestions": 52,
  "questions": [
    // Array com TODAS as 52 questões em ordem cronológica
    // (não há seleção aleatória para testes de Psicologia)
  ]
}
```

**⚠️ IMPORTANTE**:

- O teste de Psicologia entrega **TODAS as 52 questões** disponíveis no banco
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

### Obter Questões (52 questões)

**⚠️ Nota**: As questões já vêm no response da criação do teste. Este endpoint é opcional para recarregar questões.

#### cURL

```bash
curl -X GET "http://localhost:5076/api/v2/tests/psych-test-guid?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Authorization: Bearer {accessToken}"
```

#### Exemplo de Resposta (amostra de 3 das 52 questões)

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
  }
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
  {
    "questionSnapshotId": "...",
    "selectedAnswers": ["A"],
    "responseTimeMs": 5000
  }
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

### Atualizar Status PCD do Candidato

**⚠️ IMPORTANTE**: Este endpoint deve ser chamado quando o candidato responder a Questão 47 do teste de Psicologia (Seção Diversidade e Inclusão): "Você se enquadra como uma Pessoa PCD?"

**Endpoint**: `PATCH /api/Candidate/{candidateId}/pcd`

**Descrição**: Atualiza o status PCD (Pessoa com Deficiência) do candidato.

#### cURL

```bash
curl -X PATCH "http://localhost:5076/api/Candidate/0acf8567-0a49-4504-b275-11c346a08a13/pcd" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "isPCD": true
  }'
```

#### PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5076/api/Candidate/0acf8567-0a49-4504-b275-11c346a08a13/pcd" `
  -Method Patch `
  -ContentType "application/json" `
  -Headers @{Authorization="Bearer {accessToken}"} `
  -Body '{"isPCD": true}'
```

#### JavaScript (Axios)

```javascript
// Quando o candidato responder "Sim" na questão 47
const updatePCDStatus = async (candidateId, isPCD) => {
  const response = await axios.patch(
    `/api/Candidate/${candidateId}/pcd`,
    {
      isPCD: isPCD, // true se "Sim", false se "Não"
    },
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    }
  );

  return response.data;
};

// Exemplo de uso ao processar a resposta da questão 47
if (question.id === "q-47") {
  // "Você se enquadra como uma Pessoa PCD?"
  const isPCD = answer === "A"; // 'A' = Sim, 'B' = Não
  await updatePCDStatus(candidateId, isPCD);
}
```

#### Resposta de Sucesso (200 OK)

```json
{
  "id": "0acf8567-0a49-4504-b275-11c346a08a13",
  "name": "Maria Oliveira Costa",
  "cpf": "07766468000",
  "email": "maria.oliveira@example.com",
  "phone": "11912345678",
  "birthDate": "1998-07-22T00:00:00Z",
  "status": "InProcess",
  "createdAt": "2025-11-10T12:00:00Z",
  "isPCD": true
}
```

#### Possíveis Erros

**404 Not Found** - Candidato não encontrado:

```json
{
  "error": "Candidate with ID 0acf8567-0a49-4504-b275-11c346a08a13 not found"
}
```

**400 Bad Request** - Request body inválido:

```json
{
  "error": "Request body is required"
}
```

**401 Unauthorized** - Token ausente ou inválido:

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Token de autenticação inválido ou expirado"
}
```

#### Quando Chamar Este Endpoint

**Durante o Teste de Psicologia:**

1. O candidato responde a **Questão 47** (Seção 9: Diversidade e Inclusão)
2. Questão: "Você se enquadra como uma Pessoa PCD?"
3. Opções: "Sim" ou "Não"
4. **Imediatamente após** o candidato selecionar a resposta, chame este endpoint
5. Envie `isPCD: true` se resposta for "Sim", ou `isPCD: false` se resposta for "Não"

**Fluxo Recomendado:**

```javascript
// Ao salvar resposta da questão 47 (PCD)
const handlePsychologyAnswer = async (question, answer) => {
  // 1. Salvar resposta normalmente via /api/v2/tests/{testId}/answers
  await saveAnswer(testId, questionId, answer);

  // 2. Se for a questão 47 (PCD), atualizar status PCD separadamente
  if (question.id === "q-47") {
    const isPCD = answer === "A"; // A = Sim, B = Não
    await updatePCDStatus(candidateId, isPCD);
  }
};
```

---

### Atualizar Status de Estrangeiro (Foreigner) do Candidato

**⚠️ IMPORTANTE**: Este endpoint deve ser chamado quando o candidato responder a questão sobre ser estrangeiro no teste de Psicologia (Seção Diversidade e Inclusão).

**Endpoint**: `PATCH /api/Candidate/{candidateId}/foreigner`

**Descrição**: Atualiza o status de estrangeiro do candidato e o país de origem.

#### cURL

```bash
curl -X PATCH "http://localhost:5076/api/Candidate/0acf8567-0a49-4504-b275-11c346a08a13/foreigner" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "isForeigner": true,
    "countryOfOrigin": "Argentina"
  }'
```

#### PowerShell

```powershell
Invoke-RestMethod -Uri "http://localhost:5076/api/Candidate/0acf8567-0a49-4504-b275-11c346a08a13/foreigner" `
  -Method Patch `
  -ContentType "application/json" `
  -Headers @{Authorization="Bearer {accessToken}"} `
  -Body '{
    "isForeigner": true,
    "countryOfOrigin": "Argentina"
  }'
```

#### JavaScript (Axios)

```javascript
// Quando o candidato responder sobre ser estrangeiro
const updateForeignerStatus = async (
  candidateId,
  isForeigner,
  countryOfOrigin
) => {
  const response = await axios.patch(
    `/api/Candidate/${candidateId}/foreigner`,
    {
      isForeigner: isForeigner,
      countryOfOrigin: isForeigner ? countryOfOrigin : null,
    },
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
      },
    }
  );

  return response.data;
};

// Exemplo de uso ao processar a resposta sobre ser estrangeiro
if (question.text.includes("estrangeiro")) {
  const isForeigner = answer === "A"; // 'A' = Sim, 'B' = Não
  const country = isForeigner ? await showCountrySelector() : null;
  await updateForeignerStatus(candidateId, isForeigner, country);
}
```

#### Resposta de Sucesso (200 OK)

```json
{
  "id": "0acf8567-0a49-4504-b275-11c346a08a13",
  "name": "Maria Oliveira Costa",
  "cpf": "07766468000",
  "email": "maria.oliveira@example.com",
  "phone": "11912345678",
  "birthDate": "1998-07-22T00:00:00Z",
  "status": "InProcess",
  "createdAt": "2025-11-10T12:00:00Z",
  "isForeigner": true,
  "countryOfOrigin": "Argentina"
}
```

#### Possíveis Erros

**400 Bad Request** - Request body inválido:

```json
{
  "error": "Request body is required"
}
```

**400 Bad Request** - País de origem não informado quando estrangeiro:

```json
{
  "error": "CountryOfOrigin is required when IsForeigner is true"
}
```

**404 Not Found** - Candidato não encontrado:

```json
{
  "error": "Candidate with ID 0acf8567-0a49-4504-b275-11c346a08a13 not found"
}
```

**401 Unauthorized** - Token ausente ou inválido:

```json
{
  "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
  "title": "Unauthorized",
  "status": 401,
  "detail": "Token de autenticação inválido ou expirado"
}
```

#### Quando Chamar Este Endpoint

**Durante o Teste de Psicologia:**

1. O candidato responde à questão sobre ser estrangeiro (Seção 9: Diversidade e Inclusão)
2. Questão: "Você é estrangeiro?" ou similar
3. Opções: "Sim" ou "Não"
4. Se "Sim", solicitar o país de origem
5. **Imediatamente após** o candidato informar, chame este endpoint
6. Envie `isForeigner: true` e `countryOfOrigin` se resposta for "Sim"
7. Envie `isForeigner: false` se resposta for "Não" (countryOfOrigin será limpo automaticamente)

**Fluxo Recomendado:**

```javascript
// Ao salvar resposta sobre ser estrangeiro
const handleForeignerQuestion = async (question, answer) => {
  // 1. Salvar resposta normalmente via /api/v2/tests/{testId}/answers
  await saveAnswer(testId, questionId, answer);

  // 2. Se a resposta for sobre ser estrangeiro
  const isForeigner = answer === "A"; // A = Sim, B = Não

  if (isForeigner) {
    // 3. Mostrar seletor de país
    const country = await showCountrySelector();

    // 4. Atualizar status de estrangeiro com país
    await updateForeignerStatus(candidateId, true, country);
  } else {
    // 5. Atualizar status como não estrangeiro
    await updateForeignerStatus(candidateId, false, null);
  }
};
```

**Importante**:

- O campo `countryOfOrigin` é **obrigatório** quando `isForeigner` é `true`
- O campo `countryOfOrigin` aceita código ISO 3166-1 alpha-2 (ex: "AR", "US", "CO") ou nome completo do país
- Tamanho máximo: 100 caracteres
- Quando `isForeigner` é `false`, o `countryOfOrigin` é automaticamente limpo no banco de dados

---

### Upload de Documento Comprobatório PCD

**⚠️ IMPORTANTE**: Este endpoint deve ser chamado quando o candidato informar que é PCD (`isPCD: true`) para fazer upload do documento comprobatório.

**Endpoint**: `POST /api/Candidate/{candidateId}/pcd-document`

**Descrição**: Faz upload do documento comprobatório de PCD (laudo médico, certificado, etc.) para o Azure Blob Storage.

**Content-Type**: `multipart/form-data`

#### Especificações do Arquivo

- **Formatos aceitos**: PDF (`.pdf`) e DOCX (`.docx`)
- **Tamanho máximo**: 10 MB
- **MIME Types**: `application/pdf`, `application/vnd.openxmlformats-officedocument.wordprocessingml.document`
- **Organização no Blob Storage**:
  - Container: `pcd-documents`
  - Path: `candidate-{candidateId}/pcd-document-{timestamp}.{ext}`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/Candidate/0acf8567-0a49-4504-b275-11c346a08a13/pcd-document" \
  -H "Authorization: Bearer {accessToken}" \
  -F "document=@/path/to/laudo_medico.pdf"
```

#### PowerShell

```powershell
$form = @{
    document = Get-Item -Path "C:\Documents\laudo_medico.pdf"
}

Invoke-RestMethod -Uri "http://localhost:5076/api/Candidate/0acf8567-0a49-4504-b275-11c346a08a13/pcd-document" `
  -Method Post `
  -Headers @{Authorization="Bearer {accessToken}"} `
  -Form $form
```

#### JavaScript (Axios)

```javascript
// Upload de documento PCD
const uploadPCDDocument = async (candidateId, file) => {
  const formData = new FormData();
  formData.append("document", file);

  const response = await axios.post(
    `/api/Candidate/${candidateId}/pcd-document`,
    formData,
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "multipart/form-data",
      },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round(
          (progressEvent.loaded * 100) / progressEvent.total
        );
        console.log(`Upload: ${percentCompleted}%`);
      },
    }
  );

  return response.data;
};

// Exemplo de uso com input file
const handlePCDDocumentUpload = async (event) => {
  const file = event.target.files[0];

  // Validar tipo de arquivo
  const allowedTypes = [
    "application/pdf",
    "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
  ];
  if (!allowedTypes.includes(file.type)) {
    alert("Apenas arquivos PDF e DOCX são permitidos");
    return;
  }

  // Validar tamanho (10MB)
  const maxSize = 10 * 1024 * 1024;
  if (file.size > maxSize) {
    alert("O arquivo não pode exceder 10MB");
    return;
  }

  try {
    const result = await uploadPCDDocument(candidateId, file);
    console.log("Documento enviado com sucesso:", result);
  } catch (error) {
    console.error("Erro ao enviar documento:", error);
  }
};
```

#### Resposta de Sucesso (200 OK)

```json
{
  "id": "0acf8567-0a49-4504-b275-11c346a08a13",
  "name": "Maria Oliveira Costa",
  "cpf": "07766468000",
  "email": "maria.oliveira@example.com",
  "phone": "11912345678",
  "birthDate": "1998-07-22T00:00:00Z",
  "status": "InProcess",
  "createdAt": "2025-11-10T12:00:00Z",
  "isPCD": true,
  "pcdDocumentUrl": "http://127.0.0.1:10000/devstoreaccount1/pcd-documents/candidate-0acf8567-0a49-4504-b275-11c346a08a13/pcd-document-1731595200.pdf",
  "pcdDocumentFileName": "laudo_medico.pdf",
  "pcdDocumentUploadedAt": "2025-11-24T15:00:00Z"
}
```

#### Possíveis Erros

**400 Bad Request** - Arquivo não fornecido:

```json
{
  "error": "Document file is required"
}
```

**400 Bad Request** - Tamanho excedido:

```json
{
  "error": "File size exceeds maximum allowed size of 10MB"
}
```

**400 Bad Request** - Formato inválido:

```json
{
  "error": "Invalid file format. Only PDF and DOCX files are allowed"
}
```

**400 Bad Request** - MIME type inválido:

```json
{
  "error": "Invalid file type. Only PDF and DOCX files are allowed"
}
```

**404 Not Found** - Candidato não encontrado:

```json
{
  "error": "Candidate with ID 0acf8567-0a49-4504-b275-11c346a08a13 not found"
}
```

#### Quando Chamar Este Endpoint

**Fluxo Recomendado - Durante o Teste de Psicologia:**

```javascript
// Passo 1: Candidato responde questão 47 (PCD)
const handlePsychologyAnswer = async (question, answer) => {
  // Salvar resposta normalmente
  await saveAnswer(testId, questionId, answer);

  // Se for a questão 47 (PCD)
  if (question.id === "q-47") {
    const isPCD = answer === "A"; // A = Sim, B = Não

    // Atualizar status PCD
    await updatePCDStatus(candidateId, isPCD);

    // Se o candidato é PCD, solicitar upload do documento
    if (isPCD) {
      // Mostrar modal ou tela de upload
      showPCDDocumentUploadModal();
    }
  }
};

// Passo 2: Upload do documento
const showPCDDocumentUploadModal = () => {
  // Renderizar componente de upload
  return (
    <div>
      <h3>Upload de Documento Comprobatório PCD</h3>
      <p>
        Por favor, envie seu laudo médico ou certificado que comprove sua
        condição de PCD.
      </p>
      <p>
        <strong>Formatos aceitos:</strong> PDF, DOCX (máximo 10MB)
      </p>
      <input
        type="file"
        accept=".pdf,.docx"
        onChange={handlePCDDocumentUpload}
      />
    </div>
  );
};
```

**Importante**:

- O documento deve ser enviado **imediatamente após** o candidato informar que é PCD
- O upload é **obrigatório** para candidatos PCD
- O documento será armazenado de forma segura no Azure Blob Storage
- O candidato pode fazer um novo upload para substituir um documento anterior (o blob é sobrescrito)

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
- **Questões**: 29 questões de múltipla escolha (6 opções: A-F)
- **Correção**: ✅ **Automática**
- **Tempo Limite**: Sem limite de tempo
- **Foco**: Memória visual, padrões, sequências, reconhecimento espacial

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
  "timeLimitSeconds": null,
  "totalQuestions": 29,
  ...
}
```

**Diferença dos outros testes**:

- 29 questões com 6 opções cada (A-F)
- Sem limite de tempo
- Teste de memória e atenção visual
- Auto-graded (correção automática ao submeter)

### Workflow de Submissão - Retenção Visual

**⚠️ IMPORTANTE**: O workflow é idêntico ao teste de Psicologia e requer **DUAS chamadas de API**:

#### Passo 1: Submeter Respostas Individuais

**Endpoint**: `POST /api/v2/tests/{testId}/answers?candidateId={candidateId}`

```bash
curl -X POST "http://localhost:5076/api/v2/tests/visual-test-guid/answers?candidateId=0acf8567-0a49-4504-b275-11c346a08a13" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '[
    {"questionSnapshotId": "q1-guid", "selectedAnswers": ["E"], "responseTimeMs": 0},
    {"questionSnapshotId": "q2-guid", "selectedAnswers": ["C"], "responseTimeMs": 0},
    {"questionSnapshotId": "q3-guid", "selectedAnswers": ["B"], "responseTimeMs": 0}
    // ... todas as 29 respostas
  ]'
```

**⚠️ FORMATO CRÍTICO**: Payload deve ser **ARRAY DIRETO** (não envolva em objeto).

#### Passo 2: Finalizar Teste

**Endpoint**: `POST /api/v2/tests/{testId}/submit`

```bash
curl -X POST "http://localhost:5076/api/v2/tests/visual-test-guid/submit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "0acf8567-0a49-4504-b275-11c346a08a13",
    "answers": []
  }'
```

**Resposta Esperada** (200 OK):

```json
{
  "testId": "visual-test-guid",
  "status": "Submitted",
  "score": 29,
  "rawScore": 29,
  "maxPossibleScore": 29,
  "correctAnswers": 29,
  "totalQuestions": 29,
  "durationSeconds": 300,
  "message": "Test submitted successfully"
}
```

---

## 🎤 Workflow de Testes - Entrevista

### Características

- **Tipo**: Entrevista (TestType = 5)
- **Questões**: 5 vídeos comportamentais (gerados automaticamente)
- **Correção**: Manual/IA
- **Tempo Limite**: Sem limite total (sugestão: 180s por questão)
- **Foco**: Comportamento, experiência, soft skills
- **Tamanho Máximo do Vídeo**: 500 MB por vídeo
- **Formatos Aceitos**: `.mp4`, `.mov`, `.avi`, `.wmv`
- **MIME Types**: `video/mp4`, `video/quicktime`, `video/x-msvideo`, `video/x-ms-wmv`

### Fluxo Completo

```
┌─────────────────────────────────────────────────────────┐
│           WORKFLOW DE TESTE - ENTREVISTA                │
└─────────────────────────────────────────────────────────┘

1. Criar Teste de Entrevista (5 questões geradas)
   ↓
2. Iniciar Teste
   ↓
3. Obter Questões (5x vídeo comportamental)
   ↓
4. Upload de Vídeo para Questão 1
   ↓
5. Upload de Vídeo para Questão 2
   ↓
6. Upload de Vídeo para Questão 3
   ↓
7. Upload de Vídeo para Questão 4
   ↓
8. Upload de Vídeo para Questão 5
   ↓
9. Submeter Teste (com answers: [])
   ↓
10. Aguardar Avaliação da IA/Recrutador
```

---

### Passo 1: Criar Teste de Entrevista

**Endpoint**: `POST /api/v2/tests`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "candidateId": "55205319-c6e9-49ca-bd06-2f323d218f2f",
    "testType": 5
  }'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post(
  "/api/v2/tests",
  {
    candidateId: candidateId,
    testType: 5, // Interview
  },
  {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  }
);

const testId = response.data.id;
const questions = response.data.questions; // 5 questões já vêm no response
```

#### Resposta de Sucesso (201 Created)

```json
{
  "id": "f55dbdc2-adab-4edb-8490-44a3156c94c2",
  "testType": "Interview",
  "candidateId": "55205319-c6e9-49ca-bd06-2f323d218f2f",
  "status": "NotStarted",
  "questions": [
    {
      "id": "04bb670f-c9d3-451a-afdb-252498cd9a2b",
      "questionText": "Conte-me sobre você: sua formação acadêmica, experiência profissional e o que o motiva nesta candidatura.",
      "questionOrder": 1,
      "pointValue": 1.0,
      "estimatedTimeSeconds": 180
    },
    {
      "id": "0441df4a-1781-4f8e-a398-c866969d04d6",
      "questionText": "Descreva uma situação em que você teve que trabalhar sob pressão. Como você lidou com essa situação e qual foi o resultado?",
      "questionOrder": 2,
      "pointValue": 1.0,
      "estimatedTimeSeconds": 180
    },
    {
      "id": "fbf67f96-1d4a-4a97-af27-8fe0996bf211",
      "questionText": "Conte sobre uma vez em que você teve um conflito com um colega de trabalho. Como você resolveu essa situação?",
      "questionOrder": 3,
      "pointValue": 1.0,
      "estimatedTimeSeconds": 180
    },
    {
      "id": "e7837461-b2f8-4c55-8c08-58899803a574",
      "questionText": "Qual é a sua maior conquista profissional até o momento? Por que você considera isso uma conquista?",
      "questionOrder": 4,
      "pointValue": 1.0,
      "estimatedTimeSeconds": 180
    },
    {
      "id": "5d39c0a9-b09e-46e7-aadd-a0e16fd8bdc9",
      "questionText": "Onde você se vê daqui a 5 anos? Quais são seus objetivos de carreira e como esta posição se alinha com esses objetivos?",
      "questionOrder": 5,
      "pointValue": 1.0,
      "estimatedTimeSeconds": 180
    }
  ]
}
```

---

### Passo 2: Iniciar Teste de Entrevista

**Endpoint**: `POST /api/v2/tests/{testId}/start`

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/start?candidateId={candidateId}" \
  -H "Authorization: Bearer {accessToken}"
```

#### Resposta (200 OK)

```json
{
  "id": "f55dbdc2-adab-4edb-8490-44a3156c94c2",
  "status": "InProgress",
  "startedAt": "2025-11-17T13:43:21.755Z",
  ...
}
```

---

### Passo 3: Upload de Vídeo para Cada Questão

**Endpoint**: `POST /api/v2/tests/{testId}/videos?candidateId={candidateId}`

**⚠️ IMPORTANTE - Requisitos do Azure Blob Storage**:

- Em **desenvolvimento**: Azurite deve estar rodando na porta 10000
  - Instalar: `npm install -g azurite`
  - Executar: `azurite-blob --location c:\azurite --debug c:\azurite\debug.log`
- Em **produção**: Azure Blob Storage configurado

**Content-Type**: `multipart/form-data`

**Form Data**:

- `questionSnapshotId` (obrigatório): ID da questão
- `questionNumber` (obrigatório): Número da questão (1-5)
- `videoFile` (obrigatório): Arquivo de vídeo com MIME type correto

#### cURL - Upload para Questão 1

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/videos?candidateId={candidateId}" \
  -H "Authorization: Bearer {accessToken}" \
  -F "questionSnapshotId=04bb670f-c9d3-451a-afdb-252498cd9a2b" \
  -F "questionNumber=1" \
  -F "videoFile=@/caminho/para/video.mp4;type=video/mp4"
```

**⚠️ Nota Crítica**: O parâmetro `;type=video/mp4` é **obrigatório** no cURL para especificar o MIME type correto. Sem ele, o arquivo será enviado como `application/octet-stream` e será rejeitado.

#### JavaScript (Axios) - Upload com Progress

```javascript
// Upload de vídeo para uma questão
const uploadInterviewVideo = async (questionId, questionNumber, videoBlob) => {
  const formData = new FormData();
  formData.append("questionSnapshotId", questionId);
  formData.append("questionNumber", questionNumber);
  formData.append("videoFile", videoBlob, "interview-video.mp4");

  const response = await axios.post(
    `/api/v2/tests/${testId}/videos?candidateId=${candidateId}`,
    formData,
    {
      headers: {
        Authorization: `Bearer ${accessToken}`,
        "Content-Type": "multipart/form-data",
      },
      onUploadProgress: (progressEvent) => {
        const percentCompleted = Math.round(
          (progressEvent.loaded * 100) / progressEvent.total
        );
        console.log(`Upload Q${questionNumber}: ${percentCompleted}%`);
        // Atualizar UI com progresso
        updateProgressBar(questionNumber, percentCompleted);
      },
    }
  );

  return response.data;
};

// Upload de todos os 5 vídeos
const uploadAllVideos = async (questions, videos) => {
  for (let i = 0; i < questions.length; i++) {
    const question = questions[i];
    const videoBlob = videos[i];

    console.log(`Uploading video for question ${i + 1}...`);
    const result = await uploadInterviewVideo(
      question.id,
      question.questionOrder,
      videoBlob
    );

    console.log(`Question ${i + 1} uploaded:`, result.blobUrl);
  }
};
```

#### Resposta de Sucesso (200 OK)

```json
{
  "id": "1906d514-5650-4a75-b7c6-b50dfe9ef74b",
  "questionSnapshotId": "04bb670f-c9d3-451a-afdb-252498cd9a2b",
  "questionNumber": 1,
  "responseType": null,
  "blobUrl": "http://127.0.0.1:10000/devstoreaccount1/test-videos-.../q1_1763387172.mp4",
  "fileSizeBytes": 6226075,
  "uploadedAt": "2025-11-17T13:46:12.706Z",
  "score": null,
  "feedback": null,
  "verdict": null,
  "analyzedAt": null
}
```

**Repita o processo para as questões 2, 3, 4 e 5.**

---

### Passo 4: Submeter Teste de Entrevista

**Endpoint**: `POST /api/v2/tests/{testId}/submit`

**⚠️ IMPORTANTE**: Para testes de Entrevista, o array `answers` deve estar **vazio** `[]` porque as respostas já foram enviadas via upload de vídeo.

#### cURL

```bash
curl -X POST "http://localhost:5076/api/v2/tests/{testId}/submit" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {accessToken}" \
  -d '{
    "testId": "f55dbdc2-adab-4edb-8490-44a3156c94c2",
    "candidateId": "55205319-c6e9-49ca-bd06-2f323d218f2f",
    "answers": []
  }'
```

#### JavaScript (Axios)

```javascript
const response = await axios.post(
  `/api/v2/tests/${testId}/submit`,
  {
    testId: testId,
    candidateId: candidateId,
    answers: [], // Vazio para testes de vídeo
  },
  {
    headers: {
      Authorization: `Bearer ${accessToken}`,
    },
  }
);
```

#### Resposta de Sucesso (200 OK)

```json
{
  "testId": "f55dbdc2-adab-4edb-8490-44a3156c94c2",
  "status": "Submitted",
  "score": 0,
  "rawScore": 0,
  "maxPossibleScore": 0,
  "correctAnswers": 0,
  "totalQuestions": 5,
  "durationSeconds": 242,
  "message": "Test submitted successfully"
}
```

**⚠️ Notas Importantes**:

- Testes de Entrevista **NÃO têm correção automática**
- O `score` será `0` até a avaliação manual/IA
- O status final é `Submitted` (aguardando avaliação)
- As respostas em vídeo são analisadas posteriormente por IA ou recrutador

---

### Verificar Progresso Atualizado

**Endpoint**: `GET /api/v2/tests/candidate/{candidateId}/progress`

#### cURL

```bash
curl -X GET "http://localhost:5076/api/v2/tests/candidate/{candidateId}/progress" \
  -H "Authorization: Bearer {accessToken}"
```

#### Resposta Após Conclusão da Entrevista (200 OK)

```json
{
  "candidateId": "55205319-c6e9-49ca-bd06-2f323d218f2f",
  "completionPercentage": 20.0,
  "completedTests": 1,
  "totalTests": 5,
  "testProgress": {
    "Interview": {
      "testType": "Interview",
      "status": "Submitted",
      "isCompleted": true,
      "score": 0.0,
      "completedAt": "2025-11-17T13:47:24.121258"
    },
    "Portuguese": {
      "status": "InProgress",
      "isCompleted": false
    },
    "Math": {
      "status": "NotStarted",
      "isCompleted": false
    },
    "Psychology": {
      "status": "NotStarted",
      "isCompleted": false
    },
    "VisualRetention": {
      "status": "NotStarted",
      "isCompleted": false
    }
  }
}
```

---

### ✅ Workflow Validado com Sucesso

**Teste realizado em**: 2025-11-17
**Status**: ✅ Todos os endpoints funcionando conforme documentado

**Resultados da Validação**:

- ✅ Criação de teste com 5 questões automáticas
- ✅ Início do teste (NotStarted → InProgress)
- ✅ Upload de 5 vídeos (~6MB cada) com sucesso
- ✅ Validação de MIME type funcionando
- ✅ Armazenamento em Azure Blob Storage (Azurite)
- ✅ Submissão do teste com sucesso
- ✅ Atualização de progresso (20% completado)
- ✅ Duração do teste registrada (242 segundos)

**Tempo Total do Workflow**: ~4 minutos (incluindo uploads)

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

| Código | Nome       | Uso                               |
| ------ | ---------- | --------------------------------- |
| `200`  | OK         | Operação bem-sucedida             |
| `201`  | Created    | Recurso criado (teste, resposta)  |
| `204`  | No Content | Operação bem-sucedida sem retorno |

### Códigos de Erro do Cliente

| Código | Nome                 | Uso                                  |
| ------ | -------------------- | ------------------------------------ |
| `400`  | Bad Request          | Dados inválidos ou erro de validação |
| `401`  | Unauthorized         | Token ausente ou inválido            |
| `403`  | Forbidden            | Sem permissão para o recurso         |
| `404`  | Not Found            | Recurso não encontrado               |
| `409`  | Conflict             | Conflito (teste já existe)           |
| `422`  | Unprocessable Entity | Entidade não processável             |

### Códigos de Erro do Servidor

| Código | Nome                  | Uso                                  |
| ------ | --------------------- | ------------------------------------ |
| `500`  | Internal Server Error | Erro interno do servidor             |
| `503`  | Service Unavailable   | Serviço temporariamente indisponível |

---

## 🔧 Utilitários e Snippets

### JavaScript: Formatação de CPF

```javascript
function formatCPF(cpf) {
  const cleaned = cpf.replace(/\D/g, "");
  return cleaned.replace(/(\d{3})(\d{3})(\d{3})(\d{2})/, "$1.$2.$3-$4");
}

// Uso
formatCPF("07766468000"); // "077.664.680-00"
```

### JavaScript: Validação de CPF

```javascript
function isValidCPF(cpf) {
  cpf = cpf.replace(/\D/g, "");

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
isValidCPF("077.664.680-00"); // true
```

### JavaScript: Timer de Teste

```javascript
function startTestTimer(timeLimitSeconds, onTick, onExpire) {
  const startTime = Date.now();
  const endTime = startTime + timeLimitSeconds * 1000;

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
    console.log(
      `Tempo restante: ${minutes}:${seconds.toString().padStart(2, "0")}`
    );
  },
  () => {
    alert("Tempo esgotado!");
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
/>;
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

_Gerado automaticamente por Claude Code - Dignus Platform_
