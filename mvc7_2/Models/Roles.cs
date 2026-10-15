namespace mvc7_2.Models;

/// <summary>
/// Các vai trò cố định theo thứ tự phân cấp quyền: NORMAL &lt; STAFF &lt; MANAGER.
/// </summary>
public static class Roles
{
    public const string Normal = "NORMAL";
    public const string Staff = "STAFF";
    public const string Manager = "MANAGER";

    // Dùng cho [Authorize(Roles = ...)]: từ STAFF trở lên
    public const string StaffOrAbove = Staff + "," + Manager;

    // Thứ tự từ thấp đến cao
    public static readonly string[] All = { Normal, Staff, Manager };

    public static int Level(string roleName) => Array.IndexOf(All, roleName);

    /// <summary>
    /// Vai trò cao nhất trong danh sách vai trò của người dùng (mặc định NORMAL).
    /// </summary>
    public static string Highest(IEnumerable<string> roleNames) =>
        roleNames.Where(r => Level(r) >= 0).OrderByDescending(Level).FirstOrDefault() ?? Normal;
}
