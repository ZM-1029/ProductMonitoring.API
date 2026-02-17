using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class BitAddressMaster
{
    public long Id { get; set; }
    public string Code { get; set; } = null!;
    public string? PartNumber { get; set; } = null!;
    public string? Location { get; set; } = null!;
    public string? Section { get; set; } = null!;
    public string Message { get; set; } = null!;
    public long BitCategoryId { get; set; }
}
     