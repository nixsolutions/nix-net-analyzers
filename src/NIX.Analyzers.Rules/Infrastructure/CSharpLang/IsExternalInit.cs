// this is workaround to get working init-only property accessor in a framework earlier than .NET 5
// more: https://www.mking.net/blog/error-cs0518-isexternalinit-not-defined
using System.ComponentModel;

namespace System.Runtime.CompilerServices
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal static class IsExternalInit { }
}