using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using mvc7_2.Models;

namespace mvc7_2.Authorization;

/// <summary>
/// Yêu cầu quyền xóa một sản phẩm cụ thể (resource-based authorization).
/// </summary>
public class ProductDeleteRequirement : IAuthorizationRequirement
{
}

/// <summary>
/// MANAGER: xóa được mọi sản phẩm.
/// STAFF: chỉ xóa được sản phẩm do chính mình tạo.
/// NORMAL / chưa đăng nhập: không được xóa.
/// </summary>
public class ProductDeleteHandler : AuthorizationHandler<ProductDeleteRequirement, Product>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, ProductDeleteRequirement requirement, Product product)
    {
        var user = context.User;

        if (user.IsInRole(Roles.Manager))
        {
            context.Succeed(requirement);
        }
        else if (user.IsInRole(Roles.Staff)
                 && int.TryParse(user.FindFirstValue(ClaimTypes.NameIdentifier), out int userId)
                 && product.CreatedByUserId == userId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
