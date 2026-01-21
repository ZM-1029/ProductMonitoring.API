using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class BitCategory
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }
}
