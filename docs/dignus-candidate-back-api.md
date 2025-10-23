# API - Dignus Candidate Backend (`dignus-candidate-back`)

## 1. Início da Jornada do Candidato

**POST** `/api/candidate/start`

### Payload

```json
{
  "cpf": "123.456.789-00",
  "email": "joao@email.com"
}
```

### Resposta

```json
200 OK
{
  "candidateId": "cad-789",
  "name": "João Silva",
  "status": "IN_PROGRESS"
}
```

---

## 2. Envio da Prova de Português (com áudio)

**POST** `/api/candidate/portuguese`  
**Content-Type:** `multipart/form-data`

### Form Data

- `candidateId`: `cad-789`
- `questionId`: `q1`
- `audioFile`: `[arquivo .mp3 ou .wav]`

### Resposta

```json
201 Created
{
  "message": "Áudio enviado com sucesso"
}
```

---

## 3. Envio da Prova de Matemática

**POST** `/api/candidate/math`

### Payload

```json
{
  "candidateId": "cad-789",
  "answers": [
    { "questionId": "m1", "answer": "C" },
    { "questionId": "m2", "answer": "B" }
  ]
}
```

---

## 4. Envio do Questionário Psicológico

**POST** `/api/candidate/psychology`

### Payload

```json
{
  "candidateId": "cad-789",
  "responses": [
    { "questionId": "p1", "score": 5 },
    { "questionId": "p2", "score": 2 }
  ]
}
```

---

## 5. Envio do Vídeo de Apresentação

**POST** `/api/candidate/video`  
**Content-Type:** `multipart/form-data`

### Form Data

- `candidateId`: `cad-789`
- `videoFile`: `[arquivo .mp4]`

### Resposta

```json
201 Created
{
  "message": "Vídeo enviado com sucesso"
}
```

---

## 6. Finalização da Inscrição

**POST** `/api/candidate/submit`

### Payload

```json
{
  "candidateId": "cad-789"
}
```

---

## 7. Recebimento de Resultados do Agente

**POST** `/api/agent/results`

### Payload

```json
{
  "candidateId": "cad-789",
  "portugueseScore": 87,
  "psychologySummary": "Perfil analítico, estável",
  "videoSummary": "Comunicativo, linguagem corporal positiva",
  "finalScore": 8.9
}
```

---

## 8. Integração Gupy - Buscar candidatos periodicamente

**GET** `/api/sync/gupy`

### Resposta

```json
200 OK
[
  {
    "candidateId": "cad-789",
    "fullName": "João Silva",
    "vacancyId": "1234-abc",
    "additionalInfo": {...}
  },
  {
    "candidateId": "cad-790",
    "fullName": "Maria Souza",
    "vacancyId": "1234-def",
    "additionalInfo": {...}
  }
]
```
