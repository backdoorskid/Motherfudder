using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public partial class Renamer
    {
        public static void Execute(ModuleDefMD module)
        {
            foreach (var type in module.Types)
            {
                type.Namespace = new UTF8String(Encoding.UTF8.GetBytes(Random.Variable()));
                if (CanRename(type))
                    type.Name = Random.Variable();

                foreach (var m in type.Methods)
                {
                    if (CanRename(m))
                        m.Name = Random.Variable();
                    foreach (var para in m.Parameters)
                    {
                        if (para.ParamDef == null) { para.CreateParamDef(); }
                        para.ParamDef.Name = Random.Variable();
                        para.Name = Random.Variable();
                    }
                }
                foreach (var p in type.Properties)
                {
                    if (CanRename(p))
                        p.Name = Random.Variable();
                }
                foreach (var f in type.Fields)
                {
                    if (CanRename(f))
                        f.Name = Random.Variable();
                }
                foreach (var n in type.NestedTypes)
                {
                    if (n.IsDelegate)
                    {
                        n.Name = Random.Variable();
                        var paramNames = new List<string>();
                        foreach (var m in n.Methods)
                        {
                            if (m.Name == ".ctor") continue;
                            if (m.Parameters.Count > paramNames.Count)
                            {
                                int z = paramNames.Count;
                                for (int i = 0; i < m.Parameters.Count - z; i++)
                                {
                                    paramNames.Add(Random.Variable());
                                }
                            }
                            for (var i = 0; i < m.Parameters.Count; i++)
                            {
                                m.Parameters[i].Name = paramNames[i];
                            }
                        }
                    }
                }

                foreach (EventDef e in type.Events)
                {
                    e.Name = Random.Variable();
                    foreach (PropertyDef property in type.Properties)
                    {
                        if (property.IsRuntimeSpecialName) continue;
                        property.Name = Random.Variable();
                    }
                }
            }
        }

        public static bool CanRename(object obj)
        {
            SimpleAnalyzer analyzer = null;
            if (obj is TypeDef)
                analyzer = new TypeDefAnalyzer();
            else if (obj is MethodDef)
                analyzer = new MethodDefAnalyzer();
            else if (obj is EventDef)
                analyzer = new EventDefAnalyzer();
            else if (obj is FieldDef)
                analyzer = new FieldDefAnalyzer();
            if (analyzer == null)
                return false;
            return analyzer.Execute(obj);
        }
    }
}
