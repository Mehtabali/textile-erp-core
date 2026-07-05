using System;
using System.Collections.Generic;

namespace ArunVastra.Infrastructure.Data.Entities;

public partial class User
{
    public int Userid { get; set; }

    public int? Cityid { get; set; }

    public string? Usercode { get; set; }

    public string Firstname { get; set; } = null!;

    public string? Lastname { get; set; }

    public string Email { get; set; } = null!;

    public string? Phone { get; set; }

    public string? Mobile { get; set; }

    public short? Gender { get; set; }

    public string Pwhash { get; set; } = null!;

    public int Role { get; set; }

    public int? Profit { get; set; }

    public bool Locked { get; set; }

    public string? Description { get; set; }

    public DateTime? Created { get; set; }

    public bool Btuser { get; set; }

    public string? Gstin { get; set; }

    public string? Brandname { get; set; }

    public string? Useraddress { get; set; }

    public string? Isgstupdate { get; set; }

    public int? Agentid { get; set; }

    public string? Agentname { get; set; }

    public decimal? Extracharges { get; set; }

    public decimal? Discount { get; set; }

    public string? Passwordhash { get; set; }

    public bool Passwordmigrated { get; set; }

    public DateTime? Lastloginat { get; set; }

    public bool Passwordresetrequired { get; set; }

    public DateTime? Updatedat { get; set; }

    public virtual ICollection<UserRefreshToken> UserRefreshTokens { get; set; } = new List<UserRefreshToken>();
}
