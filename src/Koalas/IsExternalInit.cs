using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices {
    // Enables init properties for .NET Full Framework compilation
    // https://stackoverflow.com/questions/64749385/predefined-type-system-runtime-compilerservices-isexternalinit-is-not-defined
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class IsExternalInit { }
}
