namespace backend.Models;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string FullName { get; set; } = "";

    // LabHead, Engineer, EcoResponsible
    public string Role { get; set; } = "";

    // заполним в ЛР11
    public string? PasswordHash { get; set; }
}
