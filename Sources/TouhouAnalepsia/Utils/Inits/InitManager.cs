using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.MountAndBlade;

namespace TouhouAnalepsia.Utils.Inits
{
	internal static class InitManager
	{

		private static List<MethodInfo> FindAndSortMethodsWithAttribute<T>() where T : AbstractInitAttribute
		{
			var initMethods = AssemblyUtil.FindMethodsWithAttribute<T>();
			var initSeq = new List<MethodInfo>();
			int countOfLastPass = 0;

			while (initMethods.Count > 0)
			{
				countOfLastPass = initMethods.Count;

				for (int i = initMethods.Count - 1; i >= 0; i--)
				{
					var pair = initMethods[i];
					var method = pair.Item1;
					var attr = pair.Item2;
					bool dependenciesReady = true;

					if (attr.After.Length > 0)
					{
						dependenciesReady = attr.After.All(
							type => initSeq.Any(
								method2 => method2.DeclaringType.FullName == type.FullName));
					}

					if (dependenciesReady)
					{
						initMethods.RemoveAt(i);
						if (!method.IsStatic)
						{
							DebugUtil.ShowErrorMsg(
								$"The init method '{method.Name}' of type '{method.DeclaringType.FullName}' is not static. This method will be ignored.");
						}
						else
						{
							initSeq.Add(method);
						}
					}
				}

				// if no method was added to the sequence in this pass,
				// then there is a circular dependency. (A inits after B. B inits after A.)
				if (initMethods.Count == countOfLastPass)
				{
					var sb = new StringBuilder("Circular dependency detected in these init methods: ");
					bool isFirst = true;
					foreach (var method in initMethods)
					{
						if (isFirst)
							isFirst = false;
						else
							sb.Append(", ");
						sb.Append(method.Item1.Name);
						sb.Append(" of ");
						sb.Append(method.Item1.DeclaringType.FullName);
					}
					sb.Append(". The behaviour could be unexpected.");
					DebugUtil.ShowErrorMsg(sb.ToString());
					initSeq.AddRange(initMethods.Select(obj => obj.Item1));
					break;
				}
			}

			return initSeq;
		}


		private static void InvokeInitMethods<T>(params object[] param) where T : AbstractInitAttribute
		{
			var initMethods = FindAndSortMethodsWithAttribute<T>();
			for (int i = 0; i < initMethods.Count; i++)
			{
				if (initMethods[i].GetParameters().Length == 0)
					initMethods[i].Invoke(null, null);
				else
					initMethods[i].Invoke(null, param?.Length > 0 ? param : null);
			}
		}

		public static void OnSubModuleLoad()
		{
			InvokeInitMethods<InitOnSubModuleLoadAttribute>();
		}

		public static void OnBeforeInitialModuleScreenSetAsRoot()
		{
			InvokeInitMethods<InitOnBeforeInitialModuleScreenSetAsRootAttribute>();
		}

		public static void OnGameRegisterTypes<T>() where T : GameType
		{
			InvokeInitMethods<InitOnGameRegisterTypesAttribute>(typeof(T));
		}

		public static void OnGameCreate(bool isLoadingSave)
		{
			InvokeInitMethods<InitOnGameCreateAttribute>(isLoadingSave);
		}

		public static IEnumerable<MissionBehaviour> OnMissionOpenNew(
			IEnumerable<MissionBehaviour> missionBehaviours)
		{
			var list = missionBehaviours.ToList();
			InvokeInitMethods<InitOnMissionOpenNewAttribute>(list);
			return list;
		}
	}
}
