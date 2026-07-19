using System;
using System.Reflection;
using System.Linq;

var asm = Assembly.LoadFrom("/home/leonardo/.nuget/packages/mudblazor/9.7.0/lib/net10.0/MudBlazor.dll");
foreach (var type in asm.GetTypes())
{
    var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
        .Where(m => m.Name.Contains("ShowMessageBox"));
    foreach (var m in methods)
    {
        var pars = string.Join(", ", m.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}" + (p.HasDefaultValue ? $" = {p.DefaultValue ?? "null"}" : "")));
        Console.WriteLine($"{type.FullName} :: {m.ReturnType.Name} {m.Name}({pars})");
    }
}
