using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public class AttributesModifier
    {
        public static void Execute(ModuleDefMD module)
        {
            int v1 = Math.Abs((int)Random.U32()) % 10;
            int v2 = Math.Abs((int)Random.U32()) % 10;
            int v3 = Math.Abs((int)Random.U32()) % 10;
            int v4 = Math.Abs((int)Random.U32()) % 10;

            module.Assembly.Version = new Version(v1, v2, v3, v4);
            module.Assembly.Name = Random.Variable();
            module.Name = module.Assembly.Name + ".exe";
            
            foreach (var attribute in module.Assembly.CustomAttributes)
            {
                if (attribute.TypeFullName == "System.Runtime.InteropServices.GuidAttribute")
                {
                    if (attribute.ConstructorArguments.Count == 0) continue;

                    foreach (var arg in attribute.ConstructorArguments.ToList())
                    {
                        if (arg.Type.ToString() == "System.String")
                        {
                            attribute.ConstructorArguments.Clear();
                            CAArgument guidArgument = new CAArgument();
                            guidArgument.Type = arg.Type;
                            guidArgument.Value = Guid.NewGuid().ToString();
                            attribute.ConstructorArguments.Add(guidArgument);
                            break;
                        }
                    }
                }
                if (attribute.TypeFullName == "System.Reflection.AssemblyFileVersionAttribute")
                {
                    if (attribute.ConstructorArguments.Count == 0) continue;

                    foreach (var arg in attribute.ConstructorArguments.ToList())
                    {
                        if (arg.Type.ToString() == "System.String")
                        {
                            attribute.ConstructorArguments.Clear();
                            CAArgument guidArgument = new CAArgument();
                            guidArgument.Type = arg.Type;
                            guidArgument.Value = v1.ToString() + "." + v2.ToString() + "." + v3.ToString() + "." + v4.ToString();
                            attribute.ConstructorArguments.Add(guidArgument);
                            break;
                        }
                    }
                }
                if (attribute.TypeFullName == "System.Reflection.AssemblyTitleAttribute")
                {
                    if (attribute.ConstructorArguments.Count == 0) continue;

                    foreach (var arg in attribute.ConstructorArguments.ToList())
                    {
                        if (arg.Type.ToString() == "System.String")
                        {
                            attribute.ConstructorArguments.Clear();
                            CAArgument guidArgument = new CAArgument();
                            guidArgument.Type = arg.Type;
                            guidArgument.Value = module.Assembly.Name;
                            attribute.ConstructorArguments.Add(guidArgument);
                            break;
                        }
                    }
                }
                if (attribute.TypeFullName == "System.Reflection.AssemblyProductAttribute")
                {
                    if (attribute.ConstructorArguments.Count == 0) continue;

                    foreach (var arg in attribute.ConstructorArguments.ToList())
                    {
                        if (arg.Type.ToString() == "System.String")
                        {
                            attribute.ConstructorArguments.Clear();
                            CAArgument guidArgument = new CAArgument();
                            guidArgument.Type = arg.Type;
                            guidArgument.Value = module.Assembly.Name;
                            attribute.ConstructorArguments.Add(guidArgument);
                            break;
                        }
                    }
                }
            }

            foreach (var attribute in module.Assembly.CustomAttributes.ToList())
            {
                if (attribute.TypeFullName == "System.Reflection.AssemblyCopyrightAttribute")
                {
                    module.Assembly.CustomAttributes.Remove(attribute);
                }
            }
        }
    }
}
