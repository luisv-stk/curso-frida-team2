Below are two parts:  
1. Suggested refinements for the “system” and “user” prompts you send to the LLM.  
2. A complete Markdown‐formatted System Design Specification for the Image Tagging service.

---

# 1. Improved LLM Prompts

**System Prompt (refined):**  
```text
You are an expert image‐tagging assistant specialized in visual analysis and metadata extraction. 
Your job:
  • Identify both obvious and subtle objects, actions, environments, and stylistic elements.
  • Produce tags in lowercase, hyphenated multi‐word format (e.g. “golden-retriever”, “rule-of-thirds”).
  • Prefer specific labels (“golden-retriever” over “dog”).
  • Include technical aspects where relevant (“bokeh-effect”, “high-contrast”).
  • Reflect cultural or contextual cues when present.
  • Balance descriptive and interpretive tags.
Output requirements:
  • Strictly a comma-separated list of tags.
  • No explanations, no additional text.
```

**User Prompt (refined):**  
```text
Analyze the following image (provided as base64). Generate tags in these five categories, but return them as a single comma-separated list:

1. Subject Classification:
   – Primary subjects (specific objects/animals/people)
   – Secondary elements and backgrounds
   – Interactions or relationships

2. Technical Details:
   – Image type (photograph, illustration, etc.)
   – Quality traits (high-resolution, motion-blur, grainy)
   – Techniques (macro, aerial-view, long-exposure)

3. Artistic Composition:
   – Color palette (vibrant, pastel, monochrome)
   – Lighting (natural-light, backlit, studio-lighting)
   – Composition (rule-of-thirds, symmetrical, leading-lines)

4. Contextual & Cultural:
   – Setting, time of day, era
   – Notable cultural or stylistic references

5. Emotional & Abstract:
   – Mood or atmosphere
   – Style influences or thematic elements

Image (base64):  
data:{contentType};base64,{base64String}

Return only:  
tag1,tag2,tag3,…  
```

---

# System Design Specification

## 1. Architecture Overview

### 1.1 High-Level Architecture
We will build a modular microservice architecture:

• Frontend: Single‐Page Application (React)  
• Backend API: ASP.NET Core Web API (C#)  
• LLM Integration: External Azure‐hosted LLM service (via REST)  
• Storage:
  – Blob Storage for images (Azure Blob Storage)  
  – Relational DB (PostgreSQL) for metadata  
• Authentication/Authorization: JWT + RBAC  
• CI/CD & Hosting: Azure DevOps / GitHub Actions → Azure App Services or AKS  

### 1.2 Architecture Diagram (Mermaid)
```mermaid
graph TD
  Client[Web or Mobile Client]
  API[ASP.NET Core API]
  LLM[Azure LLM Service]
  Blob[Azure Blob Storage]
  DB[(PostgreSQL)]
  Auth[Identity Provider (Azure AD B2C)]
  CDN[CDN]

  Client -->|HTTPS + JWT| API
  API -->|REST| LLM
  API -->|Store/Retrieve| Blob
  API -->|CRUD| DB
  Client -->|Static Assets| CDN
  Client -->|Auth Redirect| Auth
  API -->|Token Validation| Auth
```

### 1.3 Technology Stack
- Frontend:
  • React (TypeScript), Redux or Context API  
  • UI library: Material-UI or TailwindCSS  
  • Build: Vite or Webpack  
- Backend:
  • .NET 7+ ASP.NET Core Web API  
  • HTTP Client for LLM calls  
  • FluentValidation / DataAnnotations  
- Database:
  • PostgreSQL (hosted on Azure Database for PostgreSQL)  
  • Entity Framework Core ORM  
- Storage & Services:
  • Azure Blob Storage for raw images  
  • Azure AD B2C for authentication  
  • Azure Monitor / Application Insights  
- Dev Tools:
  • Git, GitHub Actions or Azure Pipelines  
  • Docker for local development  
  • Swagger / OpenAPI  

## 2. Component Design

### 2.1 Frontend Components
- ImageUploader
  • Handles file selection, previews, and sends to `/api/image-tagging/generate-tags`.
- TagResults
  • Displays generated tags in a stylized tag cloud or list.
- AuthWrapper
  • Manages login/logout via Azure AD B2C and injects JWT into requests.

### 2.2 Backend Services
- ImageTaggingController
  • Endpoint: `POST /api/image-tagging/generate-tags`
  • Validates image, converts to base64, invokes `LlmClient`.
- LlmClient (Service)
  • Wraps HTTP interactions with the external LLM API.
- ImageStorageService
  • Uploads raw image to Blob Storage, returns URL or keeps binary in DB.
- TagRepository
  • Persists tag results, associates them with image metadata.
- AuthMiddleware
  • Validates incoming JWT, enforces RBAC policies.

### 2.3 Database Layer
- Use EF Core with Repository/Unit‐of‐Work patterns.
- Data access through async methods.
- Leverage connection pooling, retry policies.

## 3. Data Models

### 3.1 Database Schema

Images table:
- id (UUID, PK)  
- user_id (UUID, FK → Users)  
- blob_url (string)  
- original_filename (string)  
- content_type (string)  
- size_bytes (int)  
- uploaded_at (timestamp)  

Tags table:
- id (UUID, PK)  
- image_id (UUID, FK → Images)  
- tag_text (string)  
- category (enum: Subject, Technical, Artistic, Contextual, Emotional)  
- confidence (float, optional)  

Users table:
- id (UUID, PK)  
- email (string, unique)  
- password_hash (string)  
- role (enum: Admin, User)  
- created_at, updated_at (timestamps)

### 3.2 Data Flow
1. Client posts image → API.  
2. Controller validates, stores image in Blob, writes record to Images table.  
3. Controller calls LLM service, receives comma‐separated tags.  
4. Tags are parsed, each saved to Tags table with reference to image.  
5. Response (tags + image metadata) returned to client.

## 4. API Design

### 4.1 Endpoints

| Method | Path                                        | Request Body / Params                   | Response                                | Auth          |
|--------|---------------------------------------------|-----------------------------------------|-----------------------------------------|---------------|
| POST   | /api/image-tagging/generate-tags            | formData: image file                    | { imageId, tags[], imageUrl, timestamp} | Bearer JWT    |
| GET    | /api/image-tagging/{imageId}/tags           | path: imageId                           | { imageId, tags[], imageUrl, uploadedAt}| Bearer JWT    |
| GET    | /api/images                                 | query: page, pageSize                   | [{ imageId, url, tags[] }]              | Bearer JWT    |
| POST   | /api/auth/register                          | { email, password }                     | { userId, token }                       | Public        |
| POST   | /api/auth/login                             | { email, password }                     | { token }                               | Public        |

### 4.2 API Patterns
- RESTful routes with clear noun-based endpoints.
- JSON:API–style envelope if needed.
- All endpoints versioned in URL: `/v1/...` (future proofing).
- WebSocket not required—long polling for status if LLM is slow.

## 5. Security Design

### 5.1 Authentication Strategy
- OAuth 2.0 implicit / authorization code flow via Azure AD B2C.  
- JWT tokens issued on login/registration.  
- Tokens stored in `httpOnly` cookies or secure local storage.

### 5.2 Authorization
- Role-based access control (RBAC):
  • Admin can view all images and tags.  
  • Users only access their own records.

### 5.3 Data Protection
- HTTPS everywhere (TLS 1.2+).  
- AES-256 encryption at rest for blob storage and DB.  
- Sanitize file uploads: whitelist MIME types.  
- Input validation on all fields.

## 6. Integration Points

### 6.1 External Services
- Azure Blob Storage (image hosting).  
- Azure AD B2C (authentication).  
- Azure LLM API (image analysis).  
- SendGrid or Azure Communication Services (optional email notifications).

### 6.2 Internal Integrations
- Services communicate via DI‐injected interfaces.  
- Use `HttpClientFactory` for LLM calls with named clients and retry policies.

## 7. Performance Considerations

### 7.1 Optimization Strategies
- Caching:
  • CDN for static assets and blob images.  
  • In-memory cache (Redis) for frequent DB queries (recent images).  
- DB Indexing:
  • Index on `Images(user_id)` and `Tags(image_id)`.  
- Asynchronous processing:
  • Consider offloading LLM calls to a background queue (Azure Queue + Function) if latency high.

### 7.2 Scalability
- API as stateless microservices → scale horizontally behind Azure Load Balancer.  
- Blob Storage and PostgreSQL managed services scale easily.  
- Use Kubernetes (AKS) or Azure App Service autoscale.

## 8. Error Handling and Logging

### 8.1 Error Handling Strategy
- Global exception middleware to catch unhandled errors.  
- Return structured error objects: `{ code, message }`.  
- Validation errors return `400` with details.

### 8.2 Logging and Monitoring
- Structured logging with Serilog → Application Insights.  
- Log:
  • Request/response times.  
  • External call statuses.  
  • Exceptions with stack traces.  
- Alerts on error thresholds and performance degradation.

## 9. Development Workflow

### 9.1 Project Structure
```
/src
  /api
    Controllers/
    Services/
    Models/
    Repositories/
    DTOs/
    Program.cs
  /client
    src/
      components/
      pages/
      services/
      utils/
.gitignore
README.md
```

### 9.2 Development Environment
- .env for local secrets (DB conn, blob keys, AD B2C config).  
- Docker Compose for local PostgreSQL + Redis.  
- Scripts: `npm start`, `dotnet run`, `docker-compose up`.

### 9.3 Testing Strategy
- Unit Tests: xUnit (backend), Jest+React Testing Library (frontend).  
- Integration Tests: in-memory DB or test container.  
- E2E Tests: Playwright or Cypress.  
- Aim ≥ 80% coverage.

## 10. Deployment Architecture

### 10.1 Deployment Strategy
- CI/CD:
  • On push to `main` → build/test → deploy to `staging`.  
  • Manual approval → deploy to `production`.  
- Use GitHub Actions or Azure Pipelines with environment-specific variables.

### 10.2 Infrastructure
- Hosting:
  • API & frontend on Azure App Service or AKS.  
  • PostgreSQL Managed.  
  • Blob Storage.  
- Containerization:
  • Docker images for API and client.  
  • Helm charts or Terraform for infra as code.

---

This specification provides a clear, actionable blueprint to implement, test, secure, and deploy your Image Tagging service end‐to‐end.