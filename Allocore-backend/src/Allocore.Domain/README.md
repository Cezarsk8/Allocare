# Allocore.Domain

This layer contains the core business logic and domain entities.

## Structure

```
Allocore.Domain/
├── Common/
│   ├── Entity.cs      # Base entity with Id, CreatedAt, UpdatedAt
│   └── Result.cs      # Result pattern for operation outcomes
├── Entities/
│   └── Users/
│       ├── User.cs    # User entity (placeholder for US002)
│       └── Role.cs    # User roles enum (placeholder for US002)
└── README.md
```

## TODO (Future User Stories)

- **US002**: Implement full User entity with authentication fields
- **US002**: Add RefreshToken entity
- **US003**: Add Company and UserCompany entities
