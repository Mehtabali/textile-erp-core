namespace ArunVastra.Domain.Enums;

public enum UserRole
{
    Admin = 0,
    Supplier = 1,
    Transport = 2,
    Customer = 3,
    Agency = 4,
    // Legacy Admin role value was 5. New and updated admins use ROLE = 0.
    FloorManager = 6,
    Accounts = 7
}
