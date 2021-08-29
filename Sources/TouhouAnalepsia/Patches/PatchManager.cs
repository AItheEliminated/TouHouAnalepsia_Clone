using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Encyclopedia;
using TaleWorlds.CampaignSystem.Encyclopedia.Pages;
using TaleWorlds.CampaignSystem.ViewModelCollection.CharacterCreation;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.ObjectSystem;
using static TouhouAnalepsia.HarmonyHandler;

namespace TouhouAnalepsia.Patches {
	class PatchManager {
		public static void Prepatch() {

		}
		public static void DefaultPatches() {
			StoryModePatches.Patch();
			MapPatches.Patch();
			SandBoxPatches.Patch();

			//Various deletions
			//HarmonyHandler.patchPrefix("TaleWorlds.CampaignSystem.SandBox.CampaignBehaviors.WorkshopsCampaignBehavior", "BuildWorkshopsAtGameStart", "TouhouAnalepsia.Patches", "RemoveMethod");


			patchPrefix("ManagedCallbacks.ScriptingInterfaceOfIScene", "GetNavMeshFaceCenterPosition", "TouhouAnalepsia.Patches", "Prefix");

			patchReturn("StoryMode.CharacterCreationContent.StoryModeCharacterCreationContent", "ChangeParentsOutfit");
			patchReturn("StoryMode.CharacterCreationContent.StoryModeCharacterCreationContent", "ChangePlayerOutfit");
			patchReturn("StoryMode.CharacterCreationContent.StoryModeCharacterCreationContent", "RefreshPropsAndClothing");
			patchReturn("StoryMode.CharacterCreationContent.StoryModeCharacterCreationContent", "ApplyEquipments");
			patchReturn("StoryMode.CharacterCreationContent.StoryModeCharacterCreationContent", "ApplyBrotherClothingAndEquipment");
		}
		static bool Prefix(UIntPtr scenePointer, int navMeshFace, ref Vec3 centerPos) {
			if (navMeshFace>=0) return true;
			Logger.Log("Begin Log for ManagedCallbacks.ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPosition");

			Logger.Log(Convert.ToString(navMeshFace));
			Logger.Log(Convert.ToString(centerPos));

			Logger.Log("End Log for ManagedCallbacks.ScriptingInterfaceOfIScene.GetNavMeshFaceCenterPosition");

			throw new Exception("Something is present on an invalid navmesh face.");
		}

		//Removes try-catch block found in LoadXML, which made debugging, at times, quite difficult.
		[HarmonyPatch(typeof(MBObjectManager), "LoadXML", new Type[] { typeof(string), typeof(bool), typeof(string), typeof(Type), typeof(bool)})]
		public static class LoadXMLPatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				foreach(CodeInstruction instruction in instructions) {
					if (instruction.opcode.Equals(OpCodes.Leave_S)) break;

					ExceptionBlock block = instruction.blocks.Find((ExceptionBlock x) => true);
					if(block?.blockType == ExceptionBlockType.BeginExceptionBlock) instruction.blocks.Remove(block);

					yield return instruction;
				}
				yield return new CodeInstruction(OpCodes.Ret);
			}
		}

		[HarmonyPatch(typeof(CharacterCreationCultureStageVM), "SortCultureList", new Type[] { typeof(MBBindingList<CharacterCreationCultureVM>) })]//TODO
		public static class SortCultureListPatch {
			static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
				yield return new CodeInstruction(OpCodes.Ret);
				foreach (CodeInstruction instruction in instructions) {


				}
			}
		}

		/*//
		[HarmonyPatch(typeof(DefaultEncyclopediaHeroPage))]
		[HarmonyPostfix()]
		public static void DefaultEncyclopediaHeroPagePatch(ref List<EncyclopediaFilterGroup> ____filters) {
			____filters.RemoveAll((EncyclopediaFilterGroup x) => x.Name.Equals(new TextObject("{=PUjDWe5j}Culture", null)));

			List <EncyclopediaFilterItem> items = new List<EncyclopediaFilterItem>();

			List<CultureObject> cultures = new List<CultureObject>();
			Game.Current.ObjectManager.GetAllInstancesOfObjectType<CultureObject>(ref cultures);

			foreach(CultureObject culture in cultures) {
				if(culture.StringId != "neutral_culture" && !culture.IsBandit) {
					if (culture.Equals("empire") || culture.Equals("aserai") || culture.Equals("sturgia") || culture.Equals("vlandia") || culture.Equals("battania") || culture.Equals("khuzait") || 
						culture.Equals("nord") || culture.Equals("vakken") || culture.Equals("darshi") || culture.Equals("looters") || culture.Equals("sea_raiders") || culture.Equals("mountain_bandits") || 
						culture.Equals("forrest_bandits") || culture.Equals("desert_bandits") || culture.Equals("steppe_bandits")) continue; //Removes vanilla factions from list
					if(FactionCulture.IsFactionCulture(culture.StringId)) {
						items.Add(new EncyclopediaFilterItem(culture.Name, (object c) => FactionCulture.getFactionCultureByID(culture.StringId).IsAcceptedCulture(((Hero)c).Culture)));
					} else {
						items.Add(new EncyclopediaFilterItem(culture.Name, (object c) => ((Hero)c).Culture == culture));
					}
				}
			}

			____filters.Add(new EncyclopediaFilterGroup(items, new TextObject("{=PUjDWe5j}Culture", null)));
		}
		*/

		/*[HarmonyPatch(typeof(MobileParty), "DailyTick")]
		class DebugPatch25 {
			static Exception Finalizer(MobileParty __instance, Exception __exception) {
				if (__exception?.Message == null) return __exception;
				Logger.Log("Begin Exception Log for MobileParty.DailyTick");
				if (__exception?.Message != null) {
					Logger.Log(__exception.Message);
					Logger.Log(__exception.StackTrace);
				} else Logger.Log("No exception message");
				Logger.Log(__instance.StringId);
				foreach (TroopRosterElement element in __instance.MemberRoster.GetTroopRoster()) {
					if (element.Character == null) {
						Logger.Log("A character object is null!");
					} else {
						if (element.Character.Culture != null) Logger.Log(element.ToString() + ": This character's culture is " + element.Character.Culture.StringId);
						//else Logger.Log(element.ToString() + ": This character's culture is null.");
						else {
							Logger.Log(element.Character?.StringId + ": This character's culture is null.");
						}
					}
				}
				Logger.Log("End Exception Log for MobileParty.DailyTick");

				return __exception;
			}
		}*/

		// sz: I disabled this for bannerlord 1.6.0 will read a localized text from GameTexts
		// while it's still not initialized yet, and a NRE will happen.
		/*[HarmonyPatch(typeof(DefaultPartyWageModel), "GetTotalWage")]
		class DebugPatch26 {
			static Exception Finalizer(MobileParty mobileParty, Exception __exception) {
				if (__exception?.Message == null) return __exception;
				Logger.Log("Begin Exception Log for DefaultPartyWageModel.GetTotalWage");
				if (__exception?.Message != null) {
					Logger.Log(__exception.Message);
					Logger.Log(__exception.StackTrace);
				} else Logger.Log("No exception message");
				Logger.Log(mobileParty.StringId);
				foreach (TroopRosterElement element in mobileParty.MemberRoster.GetTroopRoster()) {
					if (element.Character == null) {
						Logger.Log("A character object is null!");
					} else {
						if (element.Character.Culture != null) Logger.Log(element.ToString() + ": This character's culture is " + element.Character.Culture.StringId);
						//else Logger.Log(element.ToString() + ": This character's culture is null.");
						else {
							Logger.Log(element.Character?.StringId + ": This character's culture is null.");
						}
					}
				}
				Logger.Log("End Exception Log for DefaultPartyWageModel.GetTotalWage");

				return __exception;
			}
		}*/
	}
}
