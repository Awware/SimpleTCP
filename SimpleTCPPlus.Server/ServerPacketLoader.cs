using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SimpleTCPPlus.Server
{
    public class ServerPacketLoader
    {
        private Assembly asm { get; }
        public ServerPacketLoader(Assembly asm)
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(ASM_RESOLVE);
            this.asm = asm;
        }
        public List<IServerPacket> LoadPackets()
        {
            List<IServerPacket> packets = new List<IServerPacket>();
            foreach (var type in asm.GetTypes())
                if (typeof(IServerPacket).IsAssignableFrom(type) && type != typeof(IServerPacket))
                    packets.Add(Activator.CreateInstance(type) as IServerPacket);
            return packets;
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
