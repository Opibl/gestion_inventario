using MiApp.Domain;

namespace MiApp.Business;

public static class PermissionService
{
    public static bool CanCreateProduct(User user)
        => user.Role == UserRole.Admin;

    public static bool CanDeleteProduct(User user)
        => user.Role == UserRole.Admin;

    public static bool CanManageUsers(User user)
        => user.Role == UserRole.Admin;

    public static bool CanViewAudit(User user)
        => user.Role == UserRole.Admin;
}