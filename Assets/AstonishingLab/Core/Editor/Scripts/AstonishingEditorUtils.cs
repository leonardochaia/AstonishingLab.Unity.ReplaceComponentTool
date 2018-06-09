using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace AstonishingLab.Core.Editor
{
    public static class AstonishingEditorUtils
    {
        public static IEnumerable<Type> GetValidSubClassesOf<T>(Assembly findOnlyInThisAssembly = null)
        {
            return GetValidSubClassesOf(typeof(T), findOnlyInThisAssembly);
        }

        public static IEnumerable<Type> GetValidSubClassesOf(Type targetType, Assembly findOnlyInThisAssembly = null)
        {
            IEnumerable<Assembly> assemblies;
            if (findOnlyInThisAssembly != null)
            {
                assemblies = new[] { findOnlyInThisAssembly };
            }
            else
            {
                assemblies = AppDomain.CurrentDomain.GetAssemblies();
            }
            return assemblies.SelectMany(assembly => assembly.GetTypes().Where(t => t.IsSubclassOf(targetType) && !t.IsAbstract && t.IsClass));
        }
    }
}