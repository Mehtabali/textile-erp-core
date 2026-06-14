namespace ArunVastra.Infrastructure.Data.Entities;

public sealed class UserView
{
    public int Userid { get; set; }

    public string? Usercode { get; set; }

    public string Firstname { get; set; } = string.Empty;

    public string? Lastname { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public string? Pwhash { get; set; }

    public int Role { get; set; }

    public int? Profit { get; set; }

    public bool Locked { get; set; }

    public string? Gstin { get; set; }

    public string? Brandname { get; set; }

    public string? Agentname { get; set; }

    public string? Cityname { get; set; }

    public DateTime? Lastaccess { get; set; }
}
