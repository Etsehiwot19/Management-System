# Entity relationship diagram

```mermaid
erDiagram
    ASP_NET_USERS ||--o{ NOTIFICATIONS : receives
    UNITS ||--o{ LEASES : leased_by
    TENANTS ||--o{ LEASES : signs
    TENANTS ||--o{ LEGAL_DOCUMENTS : provides
    LEASES ||--o{ INVOICES : billed_for
    INVOICES ||--o{ PAYMENTS : paid_by

    UNITS {
        int Id PK
        string UnitNumber UK
        int FloorNumber
        int UnitType
        decimal MonthlyRent
        int Status
        bool IsDeleted
    }
    TENANTS {
        int Id PK
        string OrganizationName
        string TinNumber UK
        string Phone
        bool IsActive
        bool IsDeleted
    }
    LEASES {
        int Id PK
        int UnitId FK
        int TenantId FK
        date StartDate
        date EndDate
        decimal MonthlyRent
        int Status
        bool IsDeleted
    }
    LEGAL_DOCUMENTS {
        int Id PK
        int TenantId FK
        int DocumentType
        string FilePath
        date ExpiryDate
        bool IsVerified
    }
    INVOICES {
        int Id PK
        int LeaseId FK
        string InvoiceNumber
        decimal AmountDue
        date DueDate
        int Status
        int PeriodMonth
        int PeriodYear
    }
    PAYMENTS {
        int Id PK
        int InvoiceId FK
        decimal AmountPaid
        date PaymentDate
        int PaymentMethod
        string ReferenceNumber
    }
    NOTIFICATIONS {
        int Id PK
        string UserId FK
        string Title
        string Message
        int NotificationType
        bool IsRead
        datetime CreatedAt
    }
    ASP_NET_USERS {
        string Id PK
        string Email
        string FullName
        bool IsActive
    }
```

`ASP_NET_USERS` represents the ASP.NET Core Identity user table (`AspNetUsers`). Base entity audit and soft-delete columns are omitted from this diagram for readability.
