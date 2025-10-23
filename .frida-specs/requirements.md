# Requirements Specification

## 1. Overview  
The Image Tagging Service is a .NET Core Web API that accepts an image file, sends it to an external LLM (Large Language Model) API for analysis, and returns a comma-separated list of tags. The user has requested a change in how the JSON payload is constructed: instead of serializing request classes, the API should build a raw string containing only the required values. This document captures the functional and non-functional requirements, user stories, constraints, assumptions, success criteria, and out-of-scope items for implementing that change and ensuring overall system quality.

---

## 2. Functional Requirements

### 2.1 Core Functionality  
Priority | ID   | Requirement  
-------- | ---- | ---------------------------------------------------------------  
Must     | FR-1 | Accept an image upload (`IFormFile`) via `POST /api/imageTagging/generate-tags`.  
Must     | FR-2 | Validate file presence and supported content types (`jpeg, jpg, png, gif, bmp`).  
Must     | FR-3 | Convert the image to a Base64 string.  
Must     | FR-4 | Construct the LLM API request payload as a single raw JSON string with inlined Base64, model name, prompt text, and other parameters—no intermediary request DTO classes.  
Should   | FR-5 | Send the payload to `https://frida-llm-api.azurewebsites.net/v1/chat/completions` with a Bearer token.  
Must     | FR-6 | Parse the LLM response payload to extract the “choices[0].message.content” string.  
Must     | FR-7 | Split returned comma-separated text into tags, trim whitespace, and exclude empty entries.  
Must     | FR-8 | Return a JSON response containing:  
         |      | • `Tags`: array of strings  
         |      | • `ImageSize`: long  
         |      | • `ProcessedAt`: UTC timestamp  

### 2.2 User Interactions  
- A client application (web/Mobile/Postman) issues a `POST` request with `multipart/form-data` containing the image.  
- The API responds with HTTP 200 and a JSON body of tags or an error status code with message.  
- Clients display or consume the tag list.

### 2.3 Data Management  
- No persistent storage required for images or results.  
- Transient in-memory Base64 conversion and tag processing.  
- Logs persisted via ASP.NET Core’s logging infrastructure.  

---

## 3. Non-Functional Requirements

### 3.1 Performance  
- FR-P1: ≤ 300 ms server-side processing (image conversion + JSON build) for images ≤ 1 MB.  
- FR-P2: Handle 50 concurrent requests with acceptable latency (< 1 s).  
- FR-P3: Scale out via Kubernetes or auto-scaling group if average CPU > 60%.

### 3.2 Security  
- FR-S1: Use HTTPS for all inbound and outbound calls.  
- FR-S2: Authenticate outbound LLM API calls with the provided Bearer token.  
- FR-S3: Sanitize and validate inputs to prevent injection.  
- FR-S4: Do not log full Base64 content—only log image size.  

### 3.3 Usability  
- FR-U1: Clear error messages for invalid file types or missing image.  
- FR-U2: API documentation via Swagger / OpenAPI.  
- FR-U3: Support .NET SDK consumers and standard REST clients.  

### 3.4 Reliability  
- FR-R1: 99.9% uptime SLAs.  
- FR-R2: Graceful error handling—catch exceptions and return 500 with a generic message.  
- FR-R3: Retry outbound HTTP calls up to 2 times on transient failures (HTTP 5xx or timeouts).  
- FR-R4: Daily backup of configuration and logs.

---

## 4. User Stories

1. As an API consumer, I want to upload an image and receive tags so that I can categorize my image automatically.  
2. As a developer, I want the request to the LLM API to be a simple JSON string so that I avoid maintaining multiple DTO classes.  
3. As a system operator, I want clear logs of requests and errors so that I can diagnose issues quickly.  
4. As a security officer, I want all external calls to be authenticated so that the service cannot be abused.  
5. As a performance engineer, I want the service to handle 50 concurrent uploads without degradation so that SLAs are met.  
6. As an API consumer, I want descriptive error messages for unsupported file types so that I can correct my request.  
7. As a QA engineer, I want retry logic on outbound calls so that transient LLM API errors don’t break the user experience.  

---

## 5. Constraints and Assumptions

### 5.1 Technical Constraints  
- Technology Stack: .NET 6+, C#, ASP.NET Core Web API.  
- Outbound HTTP client must use `HttpClientFactory`.  
- No ORM or database; stateless service.  
- Target environment: Azure App Service or Kubernetes.

### 5.2 Business Constraints  
- Timeline: 2 sprints (4 weeks) for implementation, testing, and rollout.  
- Budget: Existing cloud and personnel—no additional licenses.  
- Resources: 1 backend developer, 1 QA, 1 DevOps engineer.

### 5.3 Assumptions  
- Users are familiar with REST and `multipart/form-data`.  
- Bearer token is managed by an Ops team and rotates monthly.  
- LLM API contract (fields and URLs) remains stable during implementation.

---

## 6. Acceptance Criteria

- AC-1: Uploading a valid image returns HTTP 200 with a JSON body:  
  `{ "tags": ["tag1","tag2"], "imageSize": 12345, "processedAt": "2024-07-01T12:00:00Z" }`.  
- AC-2: The JSON payload sent to the LLM API is a single string built via interpolation—no use of request model classes or `JsonSerializer.Serialize`.  
- AC-3: Invalid file types (`.exe`, `.txt`) return HTTP 400 with message “Invalid image format…”.  
- AC-4: If the LLM API returns non-2xx, the API returns the same status code and logs the error.  
- AC-5: Service handles 50 concurrent uploads within 1 s average response time (verified via performance test).  
- AC-6: Unit tests cover:  
   • JSON payload composition logic  
   • Base64 conversion  
   • Tag parsing  
   • Error flows  
- AC-7: Swagger documentation updated to reflect new payload-building approach.

---

## 7. Out of Scope

- Persisting images or tags to a database.  
- User authentication or multi-tenant support.  
- Generating HTML or UI components—API-only.  
- Support for chunked or streaming image uploads.  
- On-premise deployment scenarios (only Azure-hosted).

---

End of Specification.