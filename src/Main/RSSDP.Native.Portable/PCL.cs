using System;

namespace Rssdp
{
    internal static class PCL
    {
        public static Exception StubException => new NotImplementedException
            (
            "The empty PCL implementation for Rssdp was loaded. Ensure you have added the Rssdp nuget package to each of your platform projects."
            );
    }
}
