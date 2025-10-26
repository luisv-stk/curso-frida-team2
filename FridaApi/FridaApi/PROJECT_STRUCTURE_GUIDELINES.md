# ASP.NET Core Project Structure Guidelines

## Standard Folder Conventions

### Core Application Structure

```
FridaApi/
├── Controllers/           # API Controllers - Handle HTTP requests
├── Services/             # Business logic services and interfaces
├── Models/               # Data models organized by purpose
│   ├── DTOs/            # Data Transfer Objects for API communication
│   ├── Entities/        # Database entity models
│   └── ViewModels/      # Models for view representation
├── Middleware/          # Custom middleware components
├── Configuration/       # Configuration classes and extensions
├── Extensions/          # Extension methods and helpers
├── Validators/          # Input validation classes
├── Exceptions/          # Custom exception classes
├── Constants/           # Application constants
├── Properties/          # Launch settings and assembly info
├── wwwroot/             # Static files (if serving web content)
└── Program.cs           # Application entry point
```

### Current Assessment vs. Best Practices

#### ✅ Correctly Structured

- `Controllers/` - Properly contains API controllers
- `Properties/` - Contains launch settings
- `Program.cs` - Entry point is correctly placed

#### ⚠️ Needs Improvement

- `Models/` - Currently has mixed purposes:
  - `LlmApi/` - These are DTOs for external API communication
  - `Responses/` - These are response DTOs for our API
  - Empty `Llm/` folder - Should be removed

#### 🔧 Recommended Refactoring

1. **Reorganize Models folder:**

   - Move `Models/LlmApi/*` → `Models/DTOs/External/LlmApi/`
   - Move `Models/Responses/*` → `Models/DTOs/Responses/`
   - Remove empty `Models/Llm/` folder

2. **Add missing folders:**

   - `Services/` - For business logic (image processing service)
   - `Configuration/` - For configuration classes
   - `Extensions/` - For extension methods

3. **Future scalability:**
   - `Validators/` - For input validation
   - `Exceptions/` - For custom exceptions
   - `Constants/` - For application constants

## Namespace Conventions

### Current Structure

```
FridaApi.Controllers
FridaApi.Models.LlmApi
FridaApi.Models.Responses
```

### Recommended Structure

```
FridaApi.Controllers
FridaApi.Services
FridaApi.Models.DTOs.External.LlmApi
FridaApi.Models.DTOs.Responses
FridaApi.Configuration
FridaApi.Extensions
```

## Benefits of Proper Structure

1. **Maintainability** - Clear separation of concerns
2. **Scalability** - Easy to add new features
3. **Team Collaboration** - Consistent organization
4. **Testing** - Easier to locate and test components
5. **Documentation** - Self-documenting structure
