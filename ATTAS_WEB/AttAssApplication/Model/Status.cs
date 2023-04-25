using System;
using System.Collections.Generic;

namespace AttAssApplication.Model;

public partial class Status
{
    public int StatusId { get; set; }

    public string Name { get; set; }

    public virtual ICollection<Session> Sessions { get; } = new List<Session>();
}
