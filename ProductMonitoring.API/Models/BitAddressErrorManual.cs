using System;
using System.Collections.Generic;

namespace ProductMonitoring.API.Models;

public partial class BitAddressErrorManual
{
    public long Id { get; set; }
    public long BitAddressId { get; set; }
    public string? ManualUrl { get; set; }
    public bool IsAdditionalManual { get; set; } = false;

}
