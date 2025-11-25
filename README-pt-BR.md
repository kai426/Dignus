# Documentação da API Dignus Candidate

**Responsáveis:** Bruno, Vitor

## Visão Geral

A API Dignus Candidate é o sistema backend central para gerenciar processos de seleção de candidatos, incluindo submissões de testes, análise baseada em IA e integração com recrutadores. Ela fornece endpoints abrangentes para lidar com avaliações de candidatos, uploads de mídia, candidaturas a vagas e fluxos de trabalho de avaliação.

## Início Rápido

### Pré-requisitos

- SDK .NET 9.0
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

A aplicação aplica automaticamente as migrações do banco de dados na inicialização. Certifique-se de que sua string de conexão PostgreSQL está configurada em `appsettings.json`.

Para aplicar migrações manualmente:

```bash
cd Dignus.Data
dotnet ef database update --startup-project ../Dignus.Candidate.Back --context DignusContextNew
```

### Requisitos de Configuração

Antes de executar a aplicação, certifique-se de que as seguintes configurações estejam devidamente definidas:

1. **String de Conexão PostgreSQL** - Necessária para operações de banco de dados
2. **String de Conexão Azure Storage** - Necessária para endpoints de upload de vídeo
3. **Configurações JWT** - Necessárias para autenticação
4. **Configurações de Email** - Necessárias para entrega de tokens de autenticação
5. **Configurações do Agente IA Externo** - Necessárias para análise de vídeo (testes de Português, Matemática, Entrevista)

---

## Documentação Completa da API

Para a documentação completa e detalhada da API incluindo todos os exemplos de requisição/resposta, autenticação e informações de segurança, consulte:

**[Documentação Completa da API](../docs/API_DOCUMENTATION.md)**

A documentação cobre todos os 11 controladores com **70+ endpoints** incluindo:

- ✅ **NOVO: Autenticação por Token CPF + Email** (CandidateAuthController)
- ✅ **NOVO: Gerenciamento de Consentimento LGPD** (ConsentController)
- ✅ **NOVO: Gerenciamento Administrativo de Questões** (AdminQuestionGroupsController)
- ✅ **NOVO: Gerenciamento de Templates de Questões** (QuestionTemplatesController)
- Gerenciamento de Candidatos
- Gerenciamento de Vagas
- API Unificada de Testes v2
- Avaliação e Relatórios
- Verificações de Status e Saúde
- Integração Databricks

---

## Referência Rápida da API

### 1. Autenticação e Autorização

#### Autenticação por Token CPF + Email (NOVO)

**Rota Base:** `/api/candidate-auth`

| Método | Endpoint                                   | Descrição                              |
| ------ | ------------------------------------------ | -------------------------------------- |
| POST   | `/api/candidate-auth/request-token`        | Solicitar token de 6 dígitos via email |
| POST   | `/api/candidate-auth/validate-token`       | Validar token e receber JWT            |
| GET    | `/api/candidate-auth/lockout-status/{cpf}` | Verificar se CPF está bloqueado        |

**Principais Recursos:**

- Token de 6 dígitos enviado via email
- Expiração do token em 15 minutos
- Limitação de taxa: 5 tentativas por 15 minutos
- Bloqueio de 30 minutos após 5 tentativas falhadas
- Retorna JWT com expiração de 24 horas

#### Autenticação Legada

**Rota Base:** `/api/auth`

| Método | Endpoint                           | Descrição                     |
| ------ | ---------------------------------- | ----------------------------- |
| POST   | `/api/auth/login`                  | Login apenas com CPF (legado) |
| GET    | `/api/auth/progress/{candidateId}` | Obter progresso do candidato  |

---

### 2. Gerenciamento de Consentimento LGPD (NOVO)

**Rota Base:** `/api/consent`

| Método | Endpoint                      | Descrição                                    | Auth    |
| ------ | ----------------------------- | -------------------------------------------- | ------- |
| GET    | `/api/consent/status/{cpf}`   | Verificar status de consentimento            | Público |
| POST   | `/api/consent`                | Submeter consentimento LGPD                  | JWT     |
| GET    | `/api/consent/privacy-policy` | Obter informações da política de privacidade | Público |

**Requisitos LGPD:**

- Todos os três consentimentos são obrigatórios:
  - Aceitação da Política de Privacidade
  - Consentimento para Compartilhamento de Dados
  - Consentimento para Análise de Crédito
- Registra endereço IP, User-Agent e timestamp
- Consentimento único (não pode ser revertido)

---

### 3. Gerenciamento Administrativo de Questões (NOVO)

#### Gerenciamento de Grupos de Questões

**Rota Base:** `/api/admin/question-groups`
**Autorização:** Requer papéis de Admin/Recrutador

| Método | Endpoint                                                      | Descrição                          |
| ------ | ------------------------------------------------------------- | ---------------------------------- |
| GET    | `/api/admin/question-groups`                                  | Listar todos os grupos de questões |
| GET    | `/api/admin/question-groups/{groupId}`                        | Obter detalhes do grupo            |
| POST   | `/api/admin/question-groups`                                  | Criar novo grupo                   |
| PUT    | `/api/admin/question-groups/{groupId}`                        | Atualizar metadados do grupo       |
| PUT    | `/api/admin/question-groups/{groupId}/questions/{questionId}` | Atualizar questão específica       |
| PATCH  | `/api/admin/question-groups/{groupId}/questions/reorder`      | Reordenar questões                 |
| PATCH  | `/api/admin/question-groups/{groupId}/activate`               | Ativar grupo                       |
| PATCH  | `/api/admin/question-groups/{groupId}/deactivate`             | Desativar grupo                    |
| DELETE | `/api/admin/question-groups/{groupId}`                        | Excluir grupo                      |

**Principais Recursos:**

- Agrupar questões por tipo de teste
- Apenas UM grupo ativo por tipo de teste
- Questões têm ordem de exibição
- Suporte a exclusão lógica

**Uso:**

- **Português, Matemática, Entrevista:** Use Grupos de Questões
- Questões aparecem na ordem exata especificada por `groupOrder`
- Crie um grupo com o número exato de questões necessárias
- Ative o grupo para torná-lo disponível para testes

#### Gerenciamento de Templates de Questões

**Rota Base:** `/api/admin/question-templates`
**Autorização:** Requer papéis de Admin/Recrutador

| Método | Endpoint                                         | Descrição                               |
| ------ | ------------------------------------------------ | --------------------------------------- |
| POST   | `/api/admin/question-templates`                  | Criar questão com resposta              |
| PUT    | `/api/admin/question-templates/{id}`             | Atualizar questão                       |
| GET    | `/api/admin/question-templates/{id}`             | Obter questão (sem resposta)            |
| GET    | `/api/admin/question-templates/{id}/with-answer` | Obter com resposta correta              |
| GET    | `/api/admin/question-templates`                  | Listar questões (paginado)              |
| GET    | `/api/admin/question-templates/by-difficulty`    | Filtrar por dificuldade                 |
| GET    | `/api/admin/question-templates/by-category`      | Filtrar por categoria                   |
| DELETE | `/api/admin/question-templates/{id}`             | Desativar questão                       |
| POST   | `/api/admin/question-templates/{id}/reactivate`  | Reativar questão                        |
| GET    | `/api/admin/question-templates/statistics`       | Obter estatísticas do banco de questões |

**Principais Recursos:**

- Seleção aleatória de questões do pool de templates
- Suporte para múltipla escolha (única ou múltiplas respostas)
- Correção automática baseada na resposta correta
- Níveis de dificuldade e marcação por categoria
- Suporte a exclusão lógica

**Uso:**

- **Psicologia, Retenção Visual:** Use Templates de Questões
- Sistema seleciona aleatoriamente questões de todos os templates ativos
- Recomendado: Criar 60-100+ templates para boa variedade
- Cada template pode ter respostas corretas para correção automática

#### Resumo do Gerenciamento de Questões

| Tipo de Teste   | Sistema               | Quantidade | Método de Seleção     | Correção Automática |
| --------------- | --------------------- | ---------- | --------------------- | ------------------- |
| Português       | Grupos de Questões    | 3          | Ordenado (GroupOrder) | ❌ (Agente IA)      |
| Matemática      | Grupos de Questões    | 2          | Ordenado (GroupOrder) | ❌ (Agente IA)      |
| Entrevista      | Grupos de Questões    | 5          | Ordenado (GroupOrder) | ❌ (Agente IA)      |
| Psicologia      | Templates de Questões | 52         | Seleção aleatória     | ✅ Lado do servidor |
| Retenção Visual | Templates de Questões | 29         | Seleção aleatória     | ✅ Lado do servidor |

---

### 4. Gerenciamento de Candidatos

**Rota Base:** `/api/candidate`
**Autorização:** JWT necessário

| Método | Endpoint                      | Descrição                              |
| ------ | ----------------------------- | -------------------------------------- |
| GET    | `/api/candidate/{id}`         | Obter candidato por ID                 |
| POST   | `/api/candidate`              | Criar novo candidato                   |
| PUT    | `/api/candidate/{id}`         | Atualizar candidato                    |
| GET    | `/api/candidate/search`       | Buscar candidatos (paginado)           |
| GET    | `/api/candidate/{id}/profile` | Obter perfil simplificado              |
| PATCH  | `/api/candidate/{id}/status`  | Atualizar apenas status                |
| GET    | `/api/candidate/{id}/job`     | Obter informações da vaga do candidato |

---

### 5. Gerenciamento de Vagas

**Rota Base:** `/api/job`
**Autorização:** JWT necessário (opcional para busca)

| Método | Endpoint                   | Descrição                                    |
| ------ | -------------------------- | -------------------------------------------- |
| GET    | `/api/job`                 | Buscar vagas com filtros                     |
| GET    | `/api/job/{id}`            | Obter detalhes da vaga                       |
| GET    | `/api/job/statistics`      | Obter estatísticas de vagas                  |
| POST   | `/api/job/{id}/apply`      | Candidatar-se à vaga                         |
| GET    | `/api/job/{id}/candidates` | Obter candidatos da vaga (apenas recrutador) |

---

### 6. Testes (API Unificada v2)

**Rota Base:** `/api/v2/tests`
**Autorização:** JWT necessário com proteção IDOR

#### Ciclo de Vida do Teste

| Método | Endpoint                                                     | Descrição                          |
| ------ | ------------------------------------------------------------ | ---------------------------------- |
| POST   | `/api/v2/tests`                                              | Criar novo teste                   |
| GET    | `/api/v2/tests/{testId}?candidateId={id}`                    | Obter teste por ID                 |
| GET    | `/api/v2/tests/candidate/{candidateId}`                      | Obter todos os testes do candidato |
| POST   | `/api/v2/tests/{testId}/start?candidateId={id}`              | Iniciar teste                      |
| POST   | `/api/v2/tests/{testId}/submit`                              | Submeter teste para correção       |
| GET    | `/api/v2/tests/{testId}/questions?candidateId={id}`          | Obter questões do teste            |
| GET    | `/api/v2/tests/{testId}/status?candidateId={id}`             | Obter status do teste              |
| GET    | `/api/v2/tests/candidate/{candidateId}/can-start/{testType}` | Verificar se pode iniciar          |

#### Respostas em Vídeo (Português/Entrevista)

| Método | Endpoint                                                       | Descrição                   |
| ------ | -------------------------------------------------------------- | --------------------------- |
| POST   | `/api/v2/tests/{testId}/videos`                                | Upload de resposta em vídeo |
| GET    | `/api/v2/tests/{testId}/videos?candidateId={id}`               | Obter todos os vídeos       |
| GET    | `/api/v2/tests/{testId}/videos/{videoId}?candidateId={id}`     | Obter vídeo específico      |
| GET    | `/api/v2/tests/{testId}/videos/{videoId}/url?candidateId={id}` | Obter URL segura do vídeo   |
| DELETE | `/api/v2/tests/{testId}/videos/{videoId}?candidateId={id}`     | Excluir vídeo               |

#### Respostas a Questões (Múltipla Escolha)

| Método | Endpoint                                                       | Descrição                           |
| ------ | -------------------------------------------------------------- | ----------------------------------- |
| POST   | `/api/v2/tests/{testId}/answers?candidateId={id}`              | Submeter/atualizar respostas (lote) |
| GET    | `/api/v2/tests/{testId}/answers?candidateId={id}`              | Obter todas as respostas            |
| PUT    | `/api/v2/tests/{testId}/answers/{responseId}?candidateId={id}` | Atualizar resposta específica       |
| DELETE | `/api/v2/tests/{testId}/answers/{responseId}?candidateId={id}` | Excluir resposta                    |

**Tipos de Teste:**

- 1: Português (3 questões em vídeo + 1 texto de leitura = 4 vídeos total)
- 2: Matemática (2 questões em vídeo)
- 3: Psicologia (52 questões de múltipla escolha)
- 4: Retenção Visual (29 questões de múltipla escolha, 6 opções A-F)
- 5: Entrevista (5 questões em vídeo)

**Status do Teste:**

- 0: Não Iniciado
- 1: Em Progresso
- 2: Submetido
- 3: Aprovado
- 4: Rejeitado

#### Fluxos de Trabalho de Teste por Tipo

##### Teste de Português (Baseado em Vídeo)

**Fluxo:**

1. Candidato recebe 1 texto de leitura + 3 questões
2. Grava 1 vídeo lendo o texto em voz alta
3. Grava 3 vídeos separados respondendo cada questão
4. **Total:** 4 vídeos necessários
5. Cada upload de vídeo automaticamente aciona análise do agente IA externo
6. Agente IA preenche campos Score, Feedback, Verdict, AnalyzedAt
7. Teste pode ser submetido após todos os 4 vídeos serem enviados

**Fonte das Questões:** TestQuestionGroup ativo (ordenado por GroupOrder)

##### Teste de Matemática (Baseado em Vídeo)

**Fluxo:**

1. Candidato recebe 2 problemas de matemática
2. Grava 2 vídeos separados explicando as soluções
3. **Total:** 2 vídeos necessários
4. Cada upload de vídeo automaticamente aciona análise do agente IA externo
5. Agente IA preenche campos Score, Feedback, Verdict, AnalyzedAt
6. Teste pode ser submetido após todos os 2 vídeos serem enviados

**Fonte das Questões:** TestQuestionGroup ativo (ordenado por GroupOrder)

##### Teste de Entrevista (Baseado em Vídeo)

**Fluxo:**

1. Candidato recebe 5 questões de entrevista
2. Grava 5 respostas em vídeo separadas
3. **Total:** 5 vídeos necessários
4. Cada upload de vídeo automaticamente aciona análise do agente IA externo
5. Agente IA preenche campos Score, Feedback, Verdict, AnalyzedAt
6. Teste pode ser submetido após todos os 5 vídeos serem enviados

**Fonte das Questões:** TestQuestionGroup ativo (ordenado por GroupOrder)

##### Teste de Psicologia (Múltipla Escolha)

**Fluxo:**

1. Candidato recebe 52 questões selecionadas aleatoriamente
2. Responde todas as questões (única ou múltipla escolha por questão)
3. Submete todas as respostas em um lote
4. **Correção Automática:** Backend calcula automaticamente a pontuação
5. **Sem limite de tempo**

**Fonte das Questões:** Seleção aleatória do pool de QuestionTemplate ativo

##### Teste de Retenção Visual (Múltipla Escolha)

**Fluxo:**

1. Candidato recebe 29 questões selecionadas aleatoriamente
2. Responde todas as questões (única ou múltipla escolha por questão)
3. Submete todas as respostas em um lote
4. **Correção Automática:** Backend calcula automaticamente a pontuação
5. **Sem limite de tempo**

**Fonte das Questões:** Seleção aleatória do pool de QuestionTemplate ativo

---

### 7. Avaliação e Relatórios

**Rota Base:** `/api/evaluation`
**Autorização:** JWT necessário com proteção IDOR

| Método | Endpoint                                           | Descrição                  |
| ------ | -------------------------------------------------- | -------------------------- |
| GET    | `/api/evaluation/candidates/{candidateId}`         | Obter avaliação abrangente |
| POST   | `/api/evaluation/candidates/{candidateId}/refresh` | Atualizar avaliação        |
| GET    | `/api/evaluation/candidates/{candidateId}/status`  | Obter status da avaliação  |
| GET    | `/api/evaluation/health`                           | Verificação de saúde       |

---

### 8. Status e Saúde

**Rota Base:** `/api/status`

| Método | Endpoint                | Descrição           | Auth    |
| ------ | ----------------------- | ------------------- | ------- |
| GET    | `/api/status`           | Obter status da API | Público |
| GET    | `/api/status/protected` | Testar autenticação | JWT     |

**Endpoints Adicionais de Saúde:**

- `GET /health` - Saúde completa com verificação do BD
- `GET /health/ready` - Sonda de prontidão
- `GET /health/live` - Sonda de vida

---

### 9. Integração Databricks

**Rota Base:** `/api/databricks`
**Autorização:** JWT necessário

| Método | Endpoint                                | Descrição                 |
| ------ | --------------------------------------- | ------------------------- |
| POST   | `/api/databricks/sync?batchSize={size}` | Sincronizar dados do Gupy |

---

## Fluxo de Autenticação

### NOVO: Autenticação por Token CPF + Email (Recomendado)

1. **Solicitar Token**

   ```http
   POST /api/candidate-auth/request-token
   Content-Type: application/json

   {
     "cpf": "12345678901",
     "email": "candidato@exemplo.com"
   }
   ```

   Resposta: Token enviado para o email do candidato

2. **Validar Token e Obter JWT**

   ```http
   POST /api/candidate-auth/validate-token
   Content-Type: application/json

   {
     "cpf": "12345678901",
     "tokenCode": "123456"
   }
   ```

   Resposta:

   ```json
   {
     "accessToken": "eyJhbGciOiJIUzI1...",
     "candidateId": "guid",
     "requiresLGPDConsent": true
   }
   ```

3. **Usar JWT nas Requisições**
   ```http
   GET /api/candidate/{id}
   Authorization: Bearer eyJhbGciOiJIUzI1...
   ```

### Legado: Autenticação Apenas por CPF

1. **Login**

   ```http
   POST /api/auth/login
   Content-Type: application/json

   {
     "cpf": "12345678901"
   }
   ```

2. **Usar token nas requisições subsequentes**

---

## Fluxo de Consentimento LGPD

Após a autenticação, verifique se o candidato precisa aceitar o consentimento LGPD:

1. **Verificar Status**

   ```http
   GET /api/consent/status/12345678901
   ```

2. **Se consentimento não foi aceito, mostrar formulário de consentimento com:**

   - Checkbox da Política de Privacidade
   - Checkbox de Compartilhamento de Dados
   - Checkbox de Análise de Crédito

3. **Submeter Consentimento** (todos devem ser verdadeiros)

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

## Recursos de Segurança

### Proteção IDOR

Todos os endpoints específicos de candidatos aplicam proteção contra Referência Direta Insegura de Objetos:

- ID do usuário autenticado deve corresponder ao `candidateId` solicitado
- Papéis Admin/Recrutador podem acessar dados de qualquer candidato
- Violações são registradas para auditoria de segurança

### Limitação de Taxa

- Solicitações de token: Máx 5 tentativas por 15 minutos por CPF
- Validações falhadas: Bloqueio de 30 minutos após 5 tentativas falhadas

### Claims JWT

- `sub`: ID do Candidato
- `candidateId`: ID do Candidato (GUID)
- `name`: Nome do candidato
- `cpf`: CPF do candidato
- `role`: Papel do usuário ("candidate", "Admin", "Recruiter")
- `exp`: Expiração (24 horas)

---

## Tratamento de Erros

Todos os endpoints retornam respostas de erro consistentes:

### Códigos de Status HTTP

- `200 OK` - Sucesso
- `201 Created` - Recurso criado
- `204 No Content` - Sucesso sem corpo de resposta
- `400 Bad Request` - Dados de requisição inválidos
- `401 Unauthorized` - JWT ausente/inválido
- `403 Forbidden` - Violação IDOR ou permissões insuficientes
- `404 Not Found` - Recurso não encontrado
- `409 Conflict` - Conflito de recurso
- `500 Internal Server Error` - Erro do servidor

### Formato de Resposta de Erro

```json
{
  "error": "CODIGO_ERRO",
  "message": "Mensagem de erro legível em português",
  "details": {
    "field": "Contexto adicional"
  }
}
```

---

## Configuração

### Estrutura do appsettings.json

```json
{
  "Jwt": {
    "SecretKey": "sua-chave-secreta-min-32-caracteres",
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
    "SmtpServer": "smtp.exemplo.com",
    "SmtpPort": 587,
    "Username": "naoresponder@exemplo.com",
    "Password": "senha",
    "FromEmail": "naoresponder@dignus.com",
    "FromName": "Plataforma Dignus"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=https;..."
  },
  "MediaUploadSettings": {
    "MaxFileSizeMB": 100,
    "AllowedExtensions": [".mp4", ".webm", ".mov", ".avi"],
    "AllowedMimeTypes": [
      "video/mp4",
      "video/webm",
      "video/quicktime",
      "video/x-msvideo"
    ]
  },
  "ExternalAIAgent": {
    "BaseUrl": "https://sua-api-agente-ia.com",
    "ApiKey": "sua-chave-api",
    "TimeoutSeconds": 30
  }
}
```

---

## Notas de Desenvolvimento

### Arquitetura

- **Framework:** ASP.NET Core 9.0
- **Banco de Dados:** PostgreSQL com Entity Framework Core
- **Autenticação:** JWT Bearer Tokens + Sistema de Token por Email
- **Armazenamento de Arquivos:** Azure Blob Storage
- **Logging:** Serilog com logging estruturado
- **Documentação da API:** Swagger/OpenAPI

### Principais Recursos

- ✅ **NOVO** Autenticação por token CPF + Email com limitação de taxa
- ✅ **NOVO** Gerenciamento de consentimento LGPD com trilha de auditoria
- ✅ **NOVO** Gerenciamento administrativo de questões com versionamento
- ✅ **NOVO** Grupos de questões configuráveis por tipo de teste
- ✅ **NOVO** Integração com agente IA externo para análise de vídeo
- ✅ **NOVO** Sistema unificado de testes suportando 5 tipos de teste
- Fluxo abrangente de avaliação de candidatos
- Sistema de testes multi-tipo:
  - **Português:** 3 questões em vídeo + leitura (corrigido por IA)
  - **Matemática:** 2 questões em vídeo (corrigido por IA)
  - **Entrevista:** 5 questões em vídeo (corrigido por IA)
  - **Psicologia:** 52 questões de múltipla escolha (correção automática)
  - **Retenção Visual:** 29 questões de múltipla escolha (correção automática)
- Análise de vídeo baseada em IA com pontuação, feedback e veredicto
- Correção automática para testes objetivos
- Upload seguro de mídia para Azure Blob Storage
- Candidatura e gerenciamento de vagas
- Integração com dashboard de recrutador
- Monitoramento de saúde e observabilidade
- Proteção IDOR em toda a API

---

## Testes

### Usando Swagger UI

Visite `https://localhost:7214/swagger` para explorar e testar a API interativamente.

### Exemplo de Fluxo de Teste

1. **Autenticar**

   - Solicitar token: POST `/api/candidate-auth/request-token`
   - Validar token: POST `/api/candidate-auth/validate-token`
   - Copiar token JWT

2. **Submeter Consentimento LGPD**

   - POST `/api/consent` com todos os três consentimentos = true

3. **Criar Teste**

   - POST `/api/v2/tests` com candidateId e testType

4. **Iniciar Teste**

   - POST `/api/v2/tests/{testId}/start`

5. **Submeter Respostas**

   - POST `/api/v2/tests/{testId}/answers` (lote)

6. **Submeter Teste**

   - POST `/api/v2/tests/{testId}/submit`

7. **Obter Avaliação**
   - GET `/api/evaluation/candidates/{candidateId}`

---

## Paginação

Endpoints paginados seguem este formato:

**Requisição:**

```
GET /api/endpoint?page=1&pageSize=20
```

**Resposta:**

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

## Suporte e Manutenção

Para questões ou problemas relacionados à API Dignus Candidate:

1. Verifique os logs na saída da aplicação
2. Verifique a conectividade do banco de dados
3. Certifique-se da configuração adequada em `appsettings.json`
4. Revise tokens de autenticação e claims
5. Consulte a [Documentação Completa da API](../docs/API_DOCUMENTATION.md)
6. Contate a equipe de desenvolvimento: Bruno, Vitor

### Monitoramento

- Logs da aplicação via Serilog
- Endpoints de verificação de saúde para monitoramento de tempo de atividade
- Métricas de desempenho através de logging estruturado
- Monitoramento de desempenho de consultas do banco de dados via Entity Framework

---

## Recursos Adicionais

- **Documentação Completa da API:** [API_DOCUMENTATION.md](../docs/API_DOCUMENTATION.md)
- **Swagger UI:** `/swagger` (apenas desenvolvimento)
- **Especificação OpenAPI:** `/swagger/v1/swagger.json`
- **Verificação de Saúde:** `/api/status`

---

**Última Atualização:** 22 de outubro de 2025
**Versão da API:** 2.0 (Testes Unificados)
**Versão do Documento:** 2.1

**Atualizações Recentes:**

- Configurações de teste atualizadas (Português: 3 questões, Matemática: 2 questões, Entrevista: 5 questões, Psicologia: 52 questões, Retenção Visual: 29 questões)
- Documentação abrangente de fluxo de trabalho de teste adicionada para todos os 5 tipos de teste
- Integração com agente IA externo documentada para testes baseados em vídeo
- Documentação de gerenciamento de questões atualizada com diretrizes de uso
- Distinção clarificada entre correção automática vs correção por IA
