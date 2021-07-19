using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TouhouAnalepsia.Utils.Inits;

namespace TouhouAnalepsia.Utils
{
	public static class AssemblyUtil
	{
		private static HashSet<Assembly> _assemblies = null;

		/// <summary>
		/// Get all assemblies (.dll) of this mod and any submod.
		/// </summary>
		public static HashSet<Assembly> ModuleAssemblies
		{
			get
			{
				if (_assemblies == null)
				{
					_assemblies = new HashSet<Assembly>();
					var currentAssembly = typeof(InitManager).Assembly;
					_assemblies.Add(currentAssembly);

					foreach (var subModule in TaleWorlds.MountAndBlade.Module.CurrentModule.SubModules)
					{
						var assembly = subModule.GetType().Assembly;
						if (assembly != currentAssembly)
						{
							foreach (var referencedAssembly in assembly.GetReferencedAssemblies())
							{
								if (referencedAssembly.FullName == currentAssembly.FullName)
								{
									_assemblies.Add(assembly);
									break;
								}
							}
						}
					}
				}

				return _assemblies;
			}
		}

		public static List<(Type, T)> FindTypesWithAttribute<T>() where T : Attribute
		{
			var result = new List<(Type, T)>();
			foreach (var assembly in ModuleAssemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					T attr = null;
					if ((attr = type.GetCustomAttribute<T>()) != null)
					{
						result.Add((type, attr));
					}
				}
			}
			return result;
		}

		public static List<(MethodInfo, T)> FindMethodsWithAttribute<T>() where T : Attribute
		{
			var result = new List<(MethodInfo, T)>();
			foreach (var assembly in ModuleAssemblies)
			{
				foreach (var type in assembly.GetTypes())
				{
					foreach (var method in type.GetMethods(
						BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
					{
						T attr = null;
						if ((attr = method.GetCustomAttribute<T>()) != null)
						{
							result.Add((method, attr));
						}
					}
				}
			}
			return result;
		}
	}
}
