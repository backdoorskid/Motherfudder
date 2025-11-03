using dnlib.DotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MfObfDotNet
{
    public class RandomizeOrder
    {
        public static void Execute(ModuleDefMD module)
        {
            foreach (var type in module.GetTypes())
            {
                if (type.IsGlobalModuleType) continue;

                var methodsList = type.Methods.ToList();
                type.Methods.Clear();
                Random.Shuffle(methodsList);
                foreach (var m in methodsList) {
                    type.Methods.Add(m);
                }

                var propertiesList = type.Properties.ToList();
                type.Properties.Clear();
                Random.Shuffle(propertiesList);
                foreach (var p in propertiesList)
                {
                    type.Properties.Add(p);
                }

                var fieldsList = type.Fields.ToList();
                type.Fields.Clear();
                Random.Shuffle(methodsList);
                foreach (var f in fieldsList)
                {
                    type.Fields.Add(f);
                }

                var eventsList = type.Events.ToList();
                type.Events.Clear();
                Random.Shuffle(eventsList);
                foreach (var e in eventsList)
                {
                    type.Events.Add(e);
                }
            }
        }
    }
}
