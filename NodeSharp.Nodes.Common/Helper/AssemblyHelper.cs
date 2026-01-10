using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace NodeSharp.Nodes.Common.Helper;

public class AssemblyHelper
{
    public static IEnumerable<Type> FindImplementations<TInterface>(string folder)
    {
        var dlls = Directory.GetFiles(folder, "*.dll", SearchOption.AllDirectories);
        var interfaceType = typeof(TInterface);

        foreach (var dll in dlls)
        {
            Assembly asm;
            try { asm = Assembly.LoadFrom(dll); }
            catch { continue; }

            foreach (var type in asm.GetTypes())
            {
                if (interfaceType.IsAssignableFrom(type) && type.IsClass && !type.IsAbstract)
                    yield return type;
            }
        }
    }
}