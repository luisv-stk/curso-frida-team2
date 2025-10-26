# Requirements Specification

## 1. Overview  
The Image Tagging Service is a micro-service within the Frida API suite. It exposes a single REST endpoint that accepts an image file, forwards it to a specialized LLM (gpt-5) for visual analysis, and returns a set of descriptive, comma-separated tags. The service performs file validation, base64 encoding, external API integration, result parsing, and structured error handling, while emitting operational logs.

---

## 2. Functional Requirements  

### 2.1 Core Functionality  
- **MUST** accept an HTTP POST at `/api/imageTagging/generate-tags` with a single image file.  
- **MUST** validate that the uploaded file is non-empty and one of: JPEG, JPG, PNG, GIF, BMP.  
- **MUST** convert the image to a base64 data URI.  
- **MUST** construct and send a JSON payload to the external LLM API (`https://frida-llm-api.azurewebsites.net/v1/chat/completions`) with:  
  - A system message defining tagging rules.  
  - A user message carrying the image data URI and analysis instructions.  
- **MUST** include a Bearer token header for authentication when calling the LLM API.  
- **MUST** parse the LLM response, extract the comma-separated tags, trim and de-duplicate them.  
- **MUST** return a JSON response containing:  
  - An array of tags.  
  - Original image byte size.  
  - UTC timestamp when processing completed.  
- **SHOULD** log key events: validation failures, payload size, external API invocation, errors, and successful tag counts.  
- **COULD** support pagination or batch processing in the future.  
- **WON’T** store raw images or tags permanently.

### 2.2 User Interactions  
- Client performs a `multipart/form-data` POST with a field named `image`.  
- Responses:  
  - `200 OK` with `{ tags: string[], imageSize: long, processedAt: datetime }` on success.  
  - `400 Bad Request` for missing/invalid image or empty LLM response.  
  - `500 Internal Server Error` for unhandled exceptions or downstream failures.  

### 2.3 Data Management  
- **In-Memory Processing**: No persistent storage; image bytes live in memory only during request processing.  
- **DTOs**:  
  - `LlmApiRequest` / `LlmRequestMessage` / `LlmContentItem` for outbound.  
  - `LlmApiResponse` for inbound.  
  - `ImageTagsResponse` for client response.  
- **Logging**: Structured logs via `ILogger`, including event identifiers and error stacks.

---

## 3. Non-Functional Requirements  

### 3.1 Performance  
- **Response Time**:  
  - ≤ 2 seconds for images ≤ 1 MB.  
  - ≤ 5 seconds under typical network/LLM latency.  
- **Throughput**:  
  - Support at least 100 requests/minute per instance.  
- **Scalability**:  
  - Stateless design to allow horizontal scaling behind a load balancer.

### 3.2 Security  
- **Transport**: HTTPS only.  
- **Authentication**:  
  - Outbound: Bearer token for LLM API stored securely (e.g., Azure Key Vault).  
  - Inbound: (Optional future) API key or OAuth for clients.  
- **Input Validation**:  
  - Restrict file types and sizes.  
  - Reject content-type spoofing.  
- **Data Protection**:  
  - No logging of raw image data.  
  - Sanitize all logs for PII.

### 3.3 Usability  
- **API Documentation**: Swagger/OpenAPI with example requests/responses.  
- **Error Messages**: Clear, actionable JSON payloads.  
- **Validation Feedback**: Explicit reasons for rejection (e.g., “Unsupported format”).

### 3.4 Reliability  
- **Availability**: 99.9% SLA.  
- **Error Handling**:  
  - Catch and log all exceptions.  
  - Return meaningful HTTP status codes.  
- **Retries**: Exponential back-off for transient LLM API failures (e.g., HTTP 429, 5xx).  
- **Monitoring & Alerting**: Health checks and metrics (response times, error rates).  
- **Backup & Recovery**: Not applicable (stateless).

---

## 4. User Stories  
1. As a **developer**, I want to POST an image to `/generate-tags` so that I receive relevant descriptive tags.  
2. As a **QA engineer**, I want clear error responses for invalid file types so I can verify validation logic.  
3. As an **operations engineer**, I want structured logs for each request so I can monitor service health.  
4. As a **product owner**, I want the service to return tags within 5 seconds so that user experience remains snappy.  
5. As a **security auditor**, I want HTTPS enforcement and token-based outbound calls so that data remains secure in transit.  
6. As a **support engineer**, I want retries on LLM timeouts so transient errors don’t impact customers.  
7. As a **client application**, I want the response schema (tags, imageSize, processedAt) to be stable so I can integrate reliably.  

---

## 5. Constraints and Assumptions  

### 5.1 Technical Constraints  
- Framework: .NET 6+ / ASP.NET Core.  
- HTTP client must be injected (no static instantiation).  
- Must use the existing LLM API endpoint and token.  
- No relational or NoSQL database in v1.  

### 5.2 Business Constraints  
- **Timeline**: 4 weeks from project kickoff.  
- **Budget**: Fixed-cost engagement; no major scope expansions.  
- **Team**: 2 full-stack developers, 1 QA engineer.  

### 5.3 Assumptions  
- LLM API (gpt-5) is stable and available at least 99% of the time.  
- Typical image uploads will not exceed 5 MB.  
- Clients will handle their own authentication when invoking the API (future scope).  
- Network latency to LLM API averages under 200 ms.

---

## 6. Acceptance Criteria  
- GIVEN a valid JPEG/PNG/GIF/BMP ≤ 5 MB, WHEN POST to `/api/imageTagging/generate-tags`, THEN respond `200 OK` with a non-empty `tags[]`, correct `imageSize`, and valid `processedAt` timestamp.  
- GIVEN no file or unsupported file type, WHEN POST, THEN respond `400 Bad Request` with a descriptive error message.  
- GIVEN the LLM API returns a non-200 or empty payload, WHEN invoking external service, THEN retry up to 2 times, then return `502 Bad Gateway` or `400 Bad Request` as appropriate, with logs.  
- All log entries must include a correlation ID and at minimum: request start, external call start/end, and request outcome.  
- Security: service only listens on HTTPS. Outbound header must contain a valid Bearer token.  

---

## 7. Out of Scope  
- Persisting images or tagging history.  
- Client-side SDKs or UI components.  
- User authentication/authorization for the inbound API (to be introduced in v2).  
- Support for videos or audio inputs.  
- Multi-lingual tag generation (current prompts assume English).  

---

*End of Requirements Specification*