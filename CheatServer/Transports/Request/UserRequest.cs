namespace CheatServer.Transports;

public enum UserCommand
{
    Create,
    Authenticate
}

public sealed class UserRequest
{
    public UserCommand UserCommand { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public bool Admin { get; set; } = false;

    public string RegistrationIp { get; set; } = string.Empty;

    public string? RecentIp { get; set; }

    public DateTime CreationDate { get; set; } = DateTime.Now;

    public string HardwareId { get; set; } = string.Empty;

    public bool Active { get; set; } = true;
}
