namespace mvc7_2.Authorization;

public static class Policies
{
    // Policy kiểm tra quyền xóa trên từng sản phẩm
    public const string CanDeleteProduct = "CanDeleteProduct";
}
