# Copilot Instructions for jayreservices-dotnet

## Architecture & Design Patterns

### Project Structure
- **Always use Controllers** for API endpoints
- **Use Vertical Slice Architecture** - organize code by features/use cases, not by technical layers
- **Do NOT use CQRS pattern** - keep command and query logic together within feature slices
- **Do NOT use Repository Pattern** - use EF Core DbContext directly in services
- **Always use Service Layer Pattern** - business logic should be encapsulated in service classes

### Code Organization
- **Use individual files** for each class, interface, DTO, enum, etc.
- Organize files by feature/slice within the project structure
- Each feature should contain its own controllers, services, DTOs, validators, and related code

## Design Principles

### SOLID Principles
- **Single Responsibility**: Each class should have one reason to change
- **Open/Closed**: Open for extension, closed for modification
- **Liskov Substitution**: Derived classes must be substitutable for their base classes
- **Interface Segregation**: Many client-specific interfaces are better than one general-purpose interface
- **Dependency Inversion**: Depend on abstractions, not concretions

### Additional Principles
- **DRY (Don't Repeat Yourself)**: Eliminate code duplication
- **KISS (Keep It Simple, Stupid)**: Favor simplicity over complexity
- **YAGNI (You Aren't Gonna Need It)**: Don't add functionality until it's necessary

## Error Handling & Validation

### Result Pattern
- **Always use Result Pattern** for business logic error handling
- Return Result<T> or Result from service methods to represent success or failure
- Avoid throwing exceptions for expected business rule violations
- Use exceptions only for truly exceptional circumstances

### Validation
- **Always use FluentValidation** for input validation
- Create validators for each request DTO
- Register validators with dependency injection
- Validate at the controller level before passing to services

## Data Transfer Objects (DTOs)

- **Use DTOs where it makes sense**:
  - API request models
  - API response models
  - Data transfer between layers when domain models shouldn't be exposed
- Don't create DTOs unnecessarily if the domain model is appropriate

## Dependency Injection

- **Always use Dependency Injection**
- Register all services, validators, and dependencies in `Program.cs`
- Use constructor injection
- Prefer interface-based registration for testability
- Use appropriate service lifetimes (Scoped, Transient, Singleton)

## Entity Framework Core

- Use EF Core DbContext directly in service classes
- No repository pattern abstraction needed
- Leverage EF Core features like:
  - Change tracking
  - LINQ queries
  - Include/ThenInclude for eager loading
  - AsNoTracking for read-only queries

## Best Practices

- Write clean, readable, and maintainable code
- Use meaningful names for variables, methods, and classes
- Follow C# naming conventions
- Keep methods small and focused
- Add XML documentation comments for public APIs
- Use async/await for I/O operations
- Handle cancellation tokens appropriately
- Follow RESTful conventions for API design

## Workflow

### Before Implementation
- **Always show your plan first** before implementing any changes
- Outline:
  - What files will be created or modified
  - What classes/interfaces will be added
  - How the solution fits the architecture
  - Any dependencies or packages needed
- Wait for approval before proceeding with implementation

### Implementation Order
1. Present the plan
2. Create/modify DTOs
3. Create FluentValidation validators
4. Create service interfaces
5. Implement service classes
6. Create/modify controllers
7. Register dependencies in `Program.cs`
8. Verify and test

## Code Review Checklist

Before completing any task, verify:
- [ ] Follows vertical slice architecture
- [ ] Uses service layer pattern
- [ ] Uses Result pattern for business errors
- [ ] Uses FluentValidation for input validation
- [ ] Uses DTOs appropriately
- [ ] All dependencies are injected
- [ ] Each class is in its own file
- [ ] Follows SOLID, DRY, KISS, and YAGNI principles
- [ ] No repository pattern used
- [ ] EF Core used directly in services
- [ ] Plan was presented before implementation
