using System;
using System.Collections.Generic;

namespace AttAssApplication.Model;

public partial class Solution
{
    public int SolutionId { get; set; }

    public int SessionId { get; set; }

    public int No { get; set; }

    public int TaskAssigned { get; set; }

    public int SlotCompability { get; set; }

    public int SubjectDiversity { get; set; }

    public int QuotaAvalable { get; set; }

    public int WalkingDistance { get; set; }

    public int SubjectPreference { get; set; }

    public int SlotPreference { get; set; }

    public virtual ICollection<Result> Results { get; } = new List<Result>();

    public virtual Session Session { get; set; }
}
