using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using TaleWorlds.Engine;
using TaleWorlds.ModuleManager;
using TouhouAnalepsia.Utils.Inits;
using Path = System.IO.Path;

// ReSharper disable ConditionIsAlwaysTrueOrFalse
namespace TouhouAnalepsia.Utils
{
	public static class ShaderSetup
	{
		[InitOnSubModuleLoad]
		public static void Init()
		{
			try
			{
				var bannerlordShaderPath = Utilities.GetExecutableWorkingDirectory() + "../../Shaders/Sources";
				var tgwShaderHeaderPath = ModuleHelper.GetModuleFullPath(SubModule.MODULE_ID) + "/Shaders";
				var tgwShaderSourcePath = tgwShaderHeaderPath + "/Sources";

				var headerFiles = Directory.EnumerateFiles(tgwShaderHeaderPath, "*.rsh", SearchOption.TopDirectoryOnly);
				var sourceFiles = Directory.EnumerateFiles(tgwShaderSourcePath, "*.rs", SearchOption.TopDirectoryOnly);

				foreach (var file in headerFiles)
				{
					UpdateShader(file, bannerlordShaderPath, false);
				}

				foreach (var file in sourceFiles)
				{
					UpdateShader(file, bannerlordShaderPath, true);
				}
			}
			catch (Exception e)
			{
				DebugUtil.ShowErrorMsg("Failed to update shaders. " + e.Message);
				throw;
			}
		}

		private static void UpdateShader(string originFile, string exportDir, bool isSource)
		{
			var fileName = Path.GetFileName(originFile);
			var exportFile = Path.Combine(exportDir, fileName);
			var originTime = File.GetLastWriteTimeUtc(originFile);

			var updateFile = !File.Exists(exportFile);

			if (!updateFile)
			{
				var exportTime = File.GetLastWriteTimeUtc(exportFile);
				var ot = originTime.ToFileTimeUtc();
				var et = exportTime.ToFileTimeUtc();
				// if exportTime < originTime (the export file is out-of-date), then copy origin to export
				// if exportTime == originTime (the export file is up-to-date), then do nothing
				// if exportTime > originTime (the origin file is out-of-date) and it's in shader dev mode, then copy export to origin
				if (ot - et > 0)
				{
					updateFile = true;
				}
				else if (et - ot > 0)
				{
					/* This is inverse shader-copying (copy generated shader codes from export dir to origin dir), which only used for shader developing.
					 * To enable this, add those to your Sources\TouhouAnalepsia\TouhouAnalepsia.csproj.user
					 *
					 * <PropertyGroup>
					 *   <DefineConstants>$(DefineConstants);SHADER_DEV</DefineConstants>
					 * </PropertyGroup>
					 */
					// only copy generated codes
					if (isSource)
					{
						// TODO: log?
#if SHADER_DEV
						var content = File.ReadAllText(exportFile, Encoding.UTF8);
						content = content.Replace("../../Modules/TouhouAnalepsia/Shaders/", "");
						File.WriteAllText(originFile, content, new UTF8Encoding(false));
						File.SetLastWriteTimeUtc(originFile, exportTime);
#endif
					}
				}
			}

			if (updateFile)
			{
				File.Copy(originFile, exportFile, true);
				File.SetLastWriteTimeUtc(exportFile, originTime);
			}
		}
	}
}