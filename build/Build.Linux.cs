

using System.Runtime.InteropServices;

public partial class Build
{

    public bool IsLinux() => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

    
}

