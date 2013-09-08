using System;
using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyProduct("iiBee")]
[assembly: AssemblyCopyright("Copyright 2013 Armin Kogler")]

[assembly: ComVisible(false)]
[assembly: CLSCompliant(false)]