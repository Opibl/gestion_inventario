using Xunit;
using MiApp.Business;
using MiApp.Domain;

namespace MiApp.Tests;

public class PermissionServiceTests
{
    [Fact]
    public void Admin_Should_Create_Product()
    {
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Role = UserRole.Admin
        };

        var result = PermissionService.CanCreateProduct(user);

        Assert.True(result);
    }

    [Fact]
    public void Operador_Should_Not_Delete_Product()
    {
        var user = new User
        {
            Id = 2,
            Username = "operador",
            Role = UserRole.Operador
        };

        var result = PermissionService.CanDeleteProduct(user);

        Assert.False(result);
    }

    [Fact]
    public void Admin_Should_View_Audit()
    {
        var user = new User
        {
            Id = 1,
            Username = "admin",
            Role = UserRole.Admin
        };

        var result = PermissionService.CanViewAudit(user);

        Assert.True(result);
    }

    [Fact]
    public void Operador_Should_Not_View_Audit()
    {
        var user = new User
        {
            Id = 2,
            Username = "operador",
            Role = UserRole.Operador
        };

        var result = PermissionService.CanViewAudit(user);

        Assert.False(result);
    }
}