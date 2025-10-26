# System Design Specification

## 1. Architecture Overview

### 1.1 High-Level Architecture  
- Monolithic ASP.NET Core Web API for image tagging  
- Stateless HTTP API that accepts image uploads, forwards them to an LLM-based tagging service, and returns tags  
- Optionally persists tagging requests/results in a relational database for audit and analytics  
- Deployed in containers behind a load-balanced ingress  

### 1.2 Architecture Diagram  
```mermaid
flowchart LR
  Client[Client Application] -->|HTTP POST /api/image-tags| API[FridaApi Web API]
  API -->|Validate & Preprocess Image| ImageService[ImageTaggingService]
  ImageService -->|Call| LLMClient[LLM API Client]
  LLMClient -->|HTTP(s) Request| LLM[Third-Party LLM Service]
  LLM -->|HTTP(s) Response| LLMClient
  ImageService -->|Parse & Filter Tags| TagProcessor[TagProcessor]
  TagProcessor -->|Optional Persist| Database[(SQL Database)]
  TagProcessor -->|Return| API
  API -->|HTTP 200/4xx/5xx| Client
```

### 1.3 Technology Stack  
- Frontend technologies  
  - Any HTTP client (JavaScript/TypeScript, .NET, mobile)  
  - HTML form or multipart upload widget  
- Backend technologies  
  - .NET 6+ / ASP.NET Core Web API  
  - C# 10+  
  - Dependency Injection  
  - HttpClientFactory  
  - JSON serialization via System.Text.Json  
- Database systems  
  - PostgreSQL or SQL Server (Entity Framework Core)  
  - Redis for caching (optional)  
- Third-party services and APIs  
  - LLM Provider API (e.g., OpenAI, Azure OpenAI)  
  - Cloud Storage (S3/Blob) if persisting raw images  
  - Email/Notification service (optional)  
  - Application Monitoring (AppInsights, Datadog)  
- Development tools  
  - xUnit, Moq for unit/integration tests  
  - Docker, Kubernetes (Helm)  
  - GitHub Actions / Azure DevOps / Jenkins for CI/CD  
  - SonarQube for code quality  

## 2. Component Design

### 2.1 Frontend Components  
Though primary focus is the API, a minimal frontend might include:  
- ImageUploadForm  
  - Renders `<input type="file">`, validates file presence and size client-side  
  - Submits multipart/form-data to POST `/api/image-tags`  
- TagResultsDisplay  
  - Shows returned tags and metadata (image size, processed timestamp)  
- ErrorBanner  
  - Displays user-friendly error messages on 4xx/5xx responses  

### 2.2 Backend Services  
- Controllers  
  - `ImageTaggingController`  
    - Endpoint: `POST /api/image-tags`  
    - Accepts `IFormFile`, runs validation, calls `IImageTaggingService`  
- Services  
  - `IImageTaggingService`  
    - `Task<ImageTagsResponse> GenerateTagsAsync(IFormFile imageFile)`  
    - Coordinates: validation, base64 encoding, LLM call, tag parsing  
  - `ILlmApiClient`  
    - `Task<LlmApiResponse> AnalyzeImageAsync(LlmApiRequest request)`  
    - Encapsulates HTTP calls, error handling, deserialization  
  - `ITagRepository` (optional)  
    - `Task SaveRequestAsync(TagRequestEntity entity)`  
    - `Task SaveResultAsync(TagResultEntity entity)`  
- Processors  
  - `TagProcessor`  
    - Splits comma-separated string, trims white space, filters empty entries  
- Infrastructure  
  - `HttpClient` configured via `IHttpClientFactory` for resilience (retries, timeouts)  
  - Global exception filter / middleware for consistent error responses  

### 2.3 Database Layer  
- Interaction via Entity Framework Core  
- Entities: `TagRequest`, `TagResult`  
- Context: `FridaDbContext`  
- Patterns: repository or generic repository + unit of work  

## 3. Data Models

### 3.1 Database Schema

Users Table (if authentication is required):
- id: UUID (PK)  
- username: VARCHAR(100), Unique  
- password_hash: VARCHAR(200)  
- role: VARCHAR(50)  
- created_at: TIMESTAMP  
- updated_at: TIMESTAMP  

TagRequest Table:
- id: UUID (PK)  
- user_id: UUID (FK to Users)  
- image_size: INT  
- content_type: VARCHAR(50)  
- created_at: TIMESTAMP  

TagResult Table:
- id: UUID (PK)  
- request_id: UUID (FK to TagRequest)  
- tags: TEXT[] or JSONB  
- processed_at: TIMESTAMP  

### 3.2 Data Flow  
1. Client uploads image → Controller  
2. Controller validates file → Service  
3. Service streams image into memory, computes size, encodes to base64  
4. Service builds `LlmApiRequest` → `LlmApiClient.AnalyzeImageAsync`  
5. LLM response → Service → `TagProcessor` extracts clean tag array  
6. (Optional) Persist request & result  
7. Controller returns `ImageTagsResponse`  

## 4. API Design

### 4.1 Endpoints

#### POST /api/image-tags
- HTTP Method: POST  
- URL Path: `/api/image-tags`  
- Authentication: Bearer JWT (optional)  
- Request:  
  - Content-Type: `multipart/form-data`  
  - Body: Field `image` (file)  
- Responses:  
  - 200 OK  
    ```json
    {
      "tags": ["nature","landscape","mountains","sky"],
      "imageSize": 12345,
      "processedAt": "2024-07-01T12:34:56.789Z"
    }
    ```  
  - 400 Bad Request  
    - Missing file → `"No image file provided."`  
    - Invalid format → `"Invalid image format. Supported formats: JPEG, PNG, GIF, BMP."`  
    - LLM returned empty/invalid content → `"Failed to extract tags from LLM response."`  
  - 4xx from LLM API  
    - Pass-through status code, body `"Failed to analyze image with LLM API."`  
  - 500 Internal Server Error  
    - JSON parsing / network errors → `"An internal server error occurred while processing the image."`  

### 4.2 API Patterns  
- RESTful POST for creation of a tagging job  
- All responses JSON with consistent shape  
- Could evolve to GraphQL or WebSocket for real-time progress (future)  

## 5. Security Design

### 5.1 Authentication Strategy  
- JWT Bearer tokens issued by Identity Provider (Auth0, Azure AD B2C)  
- Middleware: `AddJwtBearer` in ASP.NET Core  

### 5.2 Authorization  
- Role-based access control (Admin, User)  
- Policies for rate-limiting endpoint usage  

### 5.3 Data Protection  
- HTTPS/TLS for all transport  
- Encryption at rest for DB data  
- Validate and sanitize `IFormFile` uploads to prevent malicious content  

## 6. Integration Points

### 6.1 External Services  
- LLM API (OpenAI, Azure)  
  - Auth via API key in header  
  - Rate limit handling (429) with retry/backoff  
- (Optional) Cloud Storage  
  - Store raw images in S3/Blob after tagging  
- (Optional) Analytics  
  - Send usage events to telemetry (e.g., Application Insights)  

### 6.2 Internal Integrations  
- HTTP Client Factory for typed `ILlmApiClient`  
- EF Core for DB  
- Logging via `ILogger<T>`  

## 7. Performance Considerations

### 7.1 Optimization Strategies  
- Caching  
  - Redis cache for repeated image URLs → cached tags  
- Database Indexing  
  - Index on `TagRequest.created_at`  
- Query Optimization  
- Lazy loading disabled by default  
- Stream image bytes rather than fully buffering (for very large files)  

### 7.2 Scalability  
- Containerized horizontally (Kubernetes/HPA)  
- Load balancing via ingress controller  
- Database vertical scaling; consider read replicas  
- Circuit Breaker/Retry on LLM API calls  

## 8. Error Handling and Logging

### 8.1 Error Handling Strategy  
- Use ASP.NET Core middleware to catch unhandled exceptions → standardized 500 JSON response  
- Throw `BadRequestException`, `UpstreamApiException` to produce 400/4xx  

### 8.2 Logging and Monitoring  
- Log at `Information` for request start/end, `Warning` for recoverable errors, `Error` for exceptions  
- Correlation ID per request  
- Centralized log aggregation (ELK/Datadog)  
- Metrics: request rate, latency, error rate  

## 9. Development Workflow

### 9.1 Project Structure  
```
/src
  /FridaApi
    /Controllers
    /Services
    /Clients
    /Models
    /Data
    /Middleware
    Program.cs
  /FridaApi.Tests
    /Unit
    /Integration
    ImageTaggingControllerTests.cs
/docker
  Dockerfile
/helm
  Chart.yaml
  templates/
.gitignore
README.md
```

### 9.2 Development Environment  
- .NET 6 SDK  
- Local PostgreSQL (Docker)  
- Environment variables:
  - `LLM_API_KEY`
  - `ConnectionStrings__Default`
  - `ASPNETCORE_ENVIRONMENT`  
- Launch settings: Kestrel HTTPS + Swagger  

### 9.3 Testing Strategy  
- Unit tests (xUnit + Moq) for controllers and services  
- Integration tests using TestServer (in-memory ASP.NET Core)  
- E2E tests with a real LLM mock or stub  
- Target ≥ 80% coverage for business logic  

## 10. Deployment Architecture

### 10.1 Deployment Strategy  
- CI/CD pipeline (GitHub Actions)  
  - Build → Run unit tests → Build Docker image → Push to registry → Helm deploy to dev  
  - Manual promotion to staging/production  

### 10.2 Infrastructure  
- Hosting on AWS EKS / Azure Kubernetes Service  
- Docker containers  
- Managed PostgreSQL (RDS / Azure Database)  
- Secrets in AWS Secrets Manager / Azure Key Vault  
- Ingress via NGINX or managed load balancer  
- Horizontal Pod Autoscaler based on CPU and custom metrics (request latency)  

---

This specification provides a concrete blueprint for implementing, testing, and deploying the Image Tagging API (“FridaApi”). It balances simplicity (monolithic service) with extensibility (clear service boundaries, database persistence, caching, and CI/CD), follows industry best practices, and is ready for iteration as requirements evolve.