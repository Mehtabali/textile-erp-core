using System;
using System.Collections.Generic;

namespace ArunVastra.Infrastructure.Data.Entities;

public partial class UserRefreshToken
{
    public long Id { get; set; }

    public int Userid { get; set; }

    public string Tokenhash { get; set; } = null!;

    public DateTime Expiresat { get; set; }

    public DateTime Createdat { get; set; }

    public DateTime? Revokedat { get; set; }

    public virtual User User { get; set; } = null!;
}
