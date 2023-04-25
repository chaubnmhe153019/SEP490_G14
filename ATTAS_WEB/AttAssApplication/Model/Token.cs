using System;
using System.Collections.Generic;

namespace AttAssApplication.Model;

public partial class Token
{
    public int TokenId { get; set; }

    public string TokenHash { get; set; }

    public string User { get; set; }
}
