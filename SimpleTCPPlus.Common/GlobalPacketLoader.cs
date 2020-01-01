using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace SimpleTCPPlus.Common
{
    public class GlobalPacketLoader
    {
        private Assembly asm { get; }
        public GlobalPacketLoader(Assembly asm)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ASM_RESOLVE);
            this.asm = asm;
        }
        public List<T> LoadPackets<T>(params object[] args) where T : class
        {
            List<T> Packets = new List<T>();
            foreach (var type in asm.GetTypes())
                if (typeof(T).IsAssignableFrom(type) && type != typeof(T))
                    Packets.Add(Activator.CreateInstance(type, args) as T);
            return Packets;
        }
        private Assembly ASM_RESOLVE(object sender, ResolveEventArgs args)
        {
            Assembly MyAssembly, objExecutingAssemblies;
            string strTempAssmbPath = "";
            objExecutingAssemblies = args.RequestingAssembly;
            AssemblyName[] arrReferencedAssmbNames = objExecutingAssemblies.GetReferencedAssemblies();
            foreach (AssemblyName strAssmbName in arrReferencedAssmbNames)
            {
                if (strAssmbName.FullName.Substring(0, strAssmbName.FullName.IndexOf(",")) == args.Name.Substring(0, args.Name.IndexOf(",")))
                {
                    strTempAssmbPath = $"{Path.GetDirectoryName(asm.Location)}\\{args.Name.Substring(0, args.Name.IndexOf(","))}.dll";
                    break;
                }
            }
            MyAssembly = Assembly.Load(File.ReadAllBytes(strTempAssmbPath));
            return MyAssembly;
        }
    }
}
