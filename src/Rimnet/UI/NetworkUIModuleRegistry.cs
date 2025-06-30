using System;
using System.Collections.Generic;
using Verse;

namespace RimNet
{


    public static class NetworkUIModuleRegistry
    {
        private static Dictionary<Type, NetworkUIModule> registeredModules = new Dictionary<Type, NetworkUIModule>();

        static NetworkUIModuleRegistry()
        {
            foreach (var moduleType in GenTypes.AllSubclasses(typeof(NetworkUIModule)))
            {
                var instance = Activator.CreateInstance(moduleType) as NetworkUIModule;
                if (instance != null)
                {
                    registeredModules[moduleType] = instance;
                }
            }
            //RegisterModule<Module_CompPowerTransmitter>();
            //RegisterModule<Module_CompPowerBattery>();
            //RegisterModule<Module_CompTempControl>();
            //RegisterModule<Module_Building_Turret>();
            //RegisterModule<Module_Building_SolarGenerator>();
        }

        public static void RegisterModule<T>() where T : NetworkUIModule, new()
        {
            registeredModules[typeof(T)] = new T();
        }

        public static void RegisterModule(NetworkUIModule type)
        {
            registeredModules[type.GetType()] = type;
        }

        public static NetworkUIModule GetModule(Type moduleType)
        {
            if (registeredModules.TryGetValue(moduleType, out NetworkUIModule module))
            {
                return Activator.CreateInstance(module.GetType()) as NetworkUIModule;
            }
            return null;
        }

        public static List<NetworkUIModule> GetAllCompatibleModules(ThingWithComps thing)
        {
            var modules = new List<NetworkUIModule>();

            foreach (var kvp in registeredModules)
            {
                var instance = Activator.CreateInstance(kvp.Key) as NetworkUIModule;
                if (instance.CanHandleComponent(thing))
                {
                    modules.Add(instance);
                }
            }

            return modules;
        }
    }
}