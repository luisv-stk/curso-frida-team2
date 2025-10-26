# Requirements Specification

## 1. Overview  
The Image Tagging API (“FridaApi”) provides an HTTP endpoint to analyze an image and return a set of descriptive tags. Internally, it forwards the image to an external Large Language Model (LLM) API, parses the comma-separated tag list, filters and trims tags, and returns metadata including image size and processing timestamp. This document captures all functional and non-functional requirements, user stories, constraints, assumptions, acceptance criteria, and out-of-scope items for the project.

---

## 2. Functional Requirements

### 2.1 Core Functionality  
Priority (MoSCoW in brackets)

1. FR1 [MUST] – Accept an HTTP POST request containing a single image file (`IFormFile`).  
2. FR2 [MUST] – Validate that the file is provided and non-empty.  
3. FR3 [MUST] – Validate that the file content type is one of: JPEG, JPG, PNG, GIF, BMP (case-insensitive).  
4. FR4 [MUST] – Read the image bytes, compute the length, and convert to Base64.  
5. FR5 [MUST] – Construct an `LlmApiRequest` with:
   - Model = `"gpt-5"`  
   - Messages array containing:
     1. A text prompt with instructions  
     2. An image_url object with `data:{contentType};base64,{base64Image}`  
6. FR6 [MUST] – Send the request to the external LLM API via `HttpClient` and await the response.  
7. FR7 [MUST] – Handle non-200 HTTP status codes from the LLM API by returning an `ObjectResult` with the same status code and a generic API‐failure error message.  
8. FR8 [MUST] – Deserialize a successful (200) JSON response into an `LlmApiResponse` model.  
9. FR9 [MUST] – Validate that `Choices`, the first choice’s `Message`, and its `Content` are non-null and non-empty.  
10. FR10 [MUST] – Split the content string by commas, trim whitespace, filter out empty entries, and return the resulting tag array.  
11. FR11 [MUST] – Return an `OkObjectResult` with an `ImageTagsResponse` containing:
    - `Tags[]`  
    - `ImageSize` (in bytes)  
    - `ProcessedAt` (UTC timestamp)  
12. FR12 [SHOULD] – Log key events, including start/end of processing, validation failures, exceptions, and upstream API errors.  

### 2.2 User Interactions  
- HTTP POST `/image/tagging`  
  - Request: multipart/form-data with field `file`  
  - Responses:
    - 200 OK + JSON body on success  
    - 400 Bad Request + error message for missing/invalid file or failed tag extraction  
    - 401/403/4xx/5xx from LLM API mapped to the same status code + generic LLM API error message  
    - 500 Internal Server Error + generic internal‐error message on unexpected exceptions  

### 2.3 Data Management  
- No persistent storage  
- In-memory handling of `IFormFile` stream  
- Transient storage of LLM API request/response models  
- JSON serialization/deserialization via `System.Text.Json`  

---

## 3. Non-Functional Requirements

### 3.1 Performance  
- Response time ≤ 2 seconds under nominal load (single request).  
- Must support at least 100 concurrent requests per instance.  
- Should scale horizontally behind a load balancer.

### 3.2 Security  
- API served over HTTPS only.  
- Optional API key or OAuth 2.0 bearer token authentication on the endpoint.  
- Sanitize and limit maximum image size (e.g., 5 MB).  
- Do not log raw image bytes or full base64 strings.  
- Handle and mask sensitive LLM API credentials.  

### 3.3 Usability  
- Clear, human-readable error messages:
  - “No image file provided.”
  - “Invalid image format. Supported formats: JPEG, PNG, GIF, BMP.”
  - “Failed to extract tags from LLM response.”
  - “Failed to analyze image with LLM API.”
  - “An internal server error occurred while processing the image.”
- Accessibility: adhere to REST best practices.  
- API documentation (Swagger/OpenAPI) describing schema and response codes.  
- Compatible with modern browsers and HTTP clients.

### 3.4 Reliability  
- Availability target: 99.9% uptime.  
- Graceful error handling: no unhandled exceptions.  
- Retry logic or exponential back-off for transient LLM API failures (future enhancement).  
- Daily backups of configuration and credentials.  

---

## 4. User Stories

1. As a **developer**, I want to POST an image so that I can get descriptive tags for downstream processing.  
2. As a **client application**, I want clear 4xx and 5xx error messages so that I can handle failures gracefully.  
3. As an **API consumer**, I want responses under 2 seconds so that my user interface remains responsive.  
4. As a **DevOps engineer**, I want the service to be stateless so that I can scale it horizontally.  
5. As a **security auditor**, I want all traffic over HTTPS and no sensitive data in logs so that compliance is maintained.  
6. As a **QA engineer**, I want high unit‐test coverage and no redundant tests so that the codebase quality remains high.  
7. As a **product owner**, I want the service to validate input formats strictly so that we avoid abuse.  
8. As a **support engineer**, I want meaningful logs for all failures so that troubleshooting is faster.  

---

## 5. Constraints and Assumptions

### 5.1 Technical Constraints  
- .NET 7 / ASP.NET Core Web API  
- Use `HttpClient` for external calls; no third-party REST clients.  
- JSON serialization using `System.Text.Json`.  
- Unit tests via xUnit + Moq.  
- LLM API endpoint and contract pre-defined; model = “gpt-5”.

### 5.2 Business Constraints  
- Initial delivery in 12 weeks.  
- Budget capped at 3 full-time developer months.  
- SLA: 99.9% uptime, support business hours bug-fix turnaround ≤ 4 hours.

### 5.3 Assumptions  
- LLM API credentials and endpoint URL will be provided before development.  
- Maximum image size ≤ 5 MB.  
- Consumers will handle authentication if enabled.  
- Network latency to LLM API is < 500 ms.  

---

## 6. Acceptance Criteria

- All **Must Have** functional requirements (FR1–FR11) are implemented and verified with automated tests.  
- 90%+ unit test coverage of the controller code, including success, validation, error, edge and integration‐style scenarios (mocked).  
- No duplicate or redundant tests; each test targets a unique code path or behavior.  
- API responds within 2 seconds for typical workloads.  
- All error conditions return the correct HTTP status code and message as specified.  
- LLM request payload matches the schema (`model`, `messages`) and contains correct Base64 image data.  
- ProcessedAt timestamp is within the request handling window.  
- API documented in Swagger with examples for request and response.  

---

## 7. Out of Scope

- Persistent storage of images or tags.  
- Complex image analysis (face detection, object bounding boxes).  
- Automatic retry policy or circuit breaker for LLM API (to be considered in v2).  
- Front-end/UI development.  
- Multi‐tenant or per-user quota enforcement.  
- Real‐time streaming or WebSocket support.  
- Additional AI features (e.g., sentiment analysis, OCR).