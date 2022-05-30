using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace App
{
    public static class DllHelper
    {
        private static AssemblyLoadContext assemblyLoadContext;

        public static Assembly GetHotfixAssembly()
        {
            assemblyLoadContext?.Unload();
            System.GC.Collect();
            assemblyLoadContext = new AssemblyLoadContext("LibControllers", true);
            byte[] dllBytes = File.ReadAllBytes("./LibControllers.dll");
            byte[] pdbBytes = File.ReadAllBytes("./LibControllers.pdb");
            Assembly assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(dllBytes), new MemoryStream(pdbBytes));
            return assembly;
        }
    }
}
