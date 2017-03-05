using System.Reflection;
using System.Runtime.InteropServices;

using SonarLogAPI.Localization;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle(ProjectDescriptions.Product)]
//[assembly: AssemblyDescription("")]
[assembly: AssemblyCompany(ProjectDescriptions.Company)]
[assembly: AssemblyProduct(ProjectDescriptions.Product)]
[assembly: AssemblyCopyright(ProjectDescriptions.Copyright)]
//[assembly: AssemblyTrademark("")]
//[assembly: AssemblyCulture("")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a7ef68a0-7a8d-404e-b3fe-4eabcfb26293")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion(ProjectDescriptions.Version)]
[assembly: AssemblyFileVersion(ProjectDescriptions.Version)]
