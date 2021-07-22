using System;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.MountAndBlade.View.Screen;
using TouhouAnalepsia.Items;
using TouhouAnalepsia.Items.Scripts;
using TouhouAnalepsia.Utils;

namespace TouhouAnalepsia.Visuals
{
	public static class AgentVisualsPatch
	{
		static void AddCustomSkinMeshes(MBAgentVisuals visuals, BodyProperties properties, Equipment equipment, SkinMask skinMask,
			bool prepareImmediately = false, float faceDirt = 0f, bool useTranslucencyData = false, bool useTesselationData = false)
		{
			ExtendedItemInfo bodyItemInfo = null;
			ExtendedItemInfo legsItemInfo = null;
			var showBody = skinMask.HasFlag(SkinMask.BodyVisible);
			var showHead = skinMask.HasFlag(SkinMask.HeadVisible);
			var showArms = skinMask.HasFlag(SkinMask.HandsVisible);
			var showLegs = skinMask.HasFlag(SkinMask.LegsVisible);
			skinMask &= ~(SkinMask.BodyVisible | SkinMask.LegsVisible);

			SkinGenerationParams skinParams;
			Equipment.UnderwearTypes underwearTypes;
			if (equipment != null)
			{
				var armor = equipment[EquipmentIndex.Body];
				if (!armor.IsEmpty && ExtendedItemInfo.TryGet(armor.Item, out bodyItemInfo))
				{
					showBody &= bodyItemInfo.BodyCover != ExtendedItemInfo.CoverType.Type3;
				}

				var boots = equipment[EquipmentIndex.Leg];
				if (!boots.IsEmpty)
				{
					legsItemInfo = ExtendedItemInfo.Get(boots.Item);
				}

				underwearTypes = equipment.GetUnderwearType(true);
				skinParams = new SkinGenerationParams((int)skinMask, Equipment.UnderwearTypes.NoUnderwear,
					(int) equipment.BodyMeshType,
					(int) equipment.HairCoverType,
					(int) equipment.BeardCoverType,
					(int) equipment.BodyDeformType,
					prepareImmediately,
					faceDirt,
					1, // female
					useTranslucencyData,
					useTesselationData);
			}
			else
			{
				underwearTypes = Equipment.UnderwearTypes.FullUnderwear;
				skinParams = new SkinGenerationParams((int)skinMask, Equipment.UnderwearTypes.NoUnderwear,
					0, // default body
					4, // show hair
					0, // show beard
					0, // no body deform
					prepareImmediately,
					0, // no face dirt
					1, // female
					useTranslucencyData,
					useTesselationData);
			}

			var mask = NativeUtil.GetLowerbodyEnforcementMasks(visuals.GetSkeleton());
			mask |= 0x7FFFFF000000000;
			NativeUtil.SetLowerbodyEnforcementMasks(visuals.GetSkeleton(), mask);

			visuals.AddSkinMeshes(skinParams, properties);
			var skinColor = properties.GetSkinColor();
			var outlineColor = new Color(249 / 255f, 194 / 255f, 43 / 255f).ToUnsignedInteger();

			var hideBottom = underwearTypes != Equipment.UnderwearTypes.FullUnderwear;
			if (bodyItemInfo != null && legsItemInfo != null)
				hideBottom = bodyItemInfo.HideUnderwearBottom | legsItemInfo.HideUnderwearBottom;
			else if (bodyItemInfo != null)
				hideBottom = bodyItemInfo.HideUnderwearBottom;
			else if (legsItemInfo != null)
				hideBottom = legsItemInfo.HideUnderwearBottom;

			var hideTop = underwearTypes == Equipment.UnderwearTypes.NoUnderwear;
			if (bodyItemInfo != null)
				hideTop = bodyItemInfo.HideUnderwearTop;

			if (showHead)
			{
				visuals.GetHeadMetaMeshOfVisuals(out var headMetaMesh,
					out var breadMetaMesh,
					out var hairMetaMesh,
					out var eyebrowMetaMesh);

				if (headMetaMesh != null)
				{
					headMetaMesh.ClearMeshes();
					headMetaMesh.AddMetaMesh(BodyMeshCache.GetHeadMesh(0));

					/*var faceMeshes = BodyMeshCache.GetHeadMeshes(0);
					foreach (var faceMesh in faceMeshes)
					{
						var mesh = faceMesh.Item1.CreateCopy();
						mesh.Color = skinColor;
						headMetaMesh.AddMesh(mesh, faceMesh.Item2);
						mesh.ManualInvalidate();
					}*/

					/*for (var enumerator = headMetaMesh.Enumerate(); enumerator.MoveNext();)
					{
						var mesh = enumerator.Current;
						if (mesh.GetMaterial().Name.Contains("eye_th"))
						{
							var mat = Material.GetFromResource("eye_th_1");
							mesh.SetMaterial(mat);
							mesh.Color = properties.GetEyeColor();
							//mesh.Color = new Color(1, 0, 0, 1).ToUnsignedInteger();
							mat.ManualInvalidate();
						}
					}*/

					BodyMeshCache.FixupHeadMesh(headMetaMesh, properties);
				}

				headMetaMesh?.ManualInvalidate();
				breadMetaMesh?.ManualInvalidate();
				hairMetaMesh?.ManualInvalidate();
				eyebrowMetaMesh?.ManualInvalidate();
			}

			if (showArms && 
			    bodyItemInfo?.IsEmpty == false &&
			    bodyItemInfo.ArmCover != ExtendedItemInfo.CoverType.None)
			{
				var skeleton = visuals.GetSkeleton();
				var num = skeleton.GetComponentCount(GameEntity.ComponentType.MetaMesh);
				for (int i = 0; i < num; i++)
				{
					var mesh = skeleton.GetComponentAtIndex(GameEntity.ComponentType.MetaMesh, i);
					if (mesh is MetaMesh metaMesh)
					{
						var name = metaMesh.GetName();
						if (name == "hands_female_th")
						{
							var vec3 = metaMesh.VectorUserData;
							metaMesh.SetVectorArgument(bodyItemInfo.GetArmCoverShaderParam(),
								vec3.Y, vec3.Z, 0);
						}
					}
				}
			}

			if (showBody)
			{
				var bodyMetaMesh = BodyMeshCache.CreateBodyCopy(properties.GetBreastSize());
				bodyMetaMesh.SetFactor1Linear(skinColor);
				if (bodyItemInfo?.IsEmpty == false)
				{
					var vec3 = bodyMetaMesh.VectorUserData;
					bodyMetaMesh.SetVectorArgument(bodyItemInfo.GetBodyCoverShaderParam(), 
						vec3.Y, vec3.Z, 0);
				}
				visuals.AddMultiMesh(bodyMetaMesh, BodyMeshTypes.Body);
				bodyMetaMesh.ManualInvalidate();
			}

			if (showLegs)
			{
				var legsMetaMesh = SkinManager.GetLegsMetaMeshCopy(0, false);
				legsMetaMesh.SetFactor1Linear(skinColor);
				if (legsItemInfo?.IsEmpty == false)
				{
					legsMetaMesh.SetVectorArgument(
						legsItemInfo.GetFootHorizontalCoverShaderParam(),
						legsItemInfo.GetFootVerticalCoverShaderParam(),
						legsItemInfo.GetLegCoverShaderParam(),
						0);
				}
				visuals.AddMultiMesh(legsMetaMesh, BodyMeshTypes.Legs);
				legsMetaMesh.ManualInvalidate();
			}

			if (!hideBottom)
			{
				var underwearBottomMetaMesh = MetaMesh.GetCopy("underwear_bottom_th_a");
				visuals.AddMultiMesh(underwearBottomMetaMesh, BodyMeshTypes.Underwear);
				underwearBottomMetaMesh.ManualInvalidate();
			}

			if (!hideTop)
			{
				var underwearTopMetaMesh = MorphedMeshCache.GetMorphedCopy("underwear_top_th_a", properties.GetBreastSize());
				visuals.AddMultiMesh(underwearTopMetaMesh, BodyMeshTypes.Underwear);
				underwearTopMetaMesh.ManualInvalidate();
			}

			/*int i1 = skeleton.GetComponentCount(GameEntity.ComponentType.ClothSimulator);
			int i2 = skeleton.GetComponentCount(GameEntity.ComponentType.MetaMesh);
			int i3 = skeleton.GetComponentCount(GameEntity.ComponentType.CustomType1);
			int i4 = skeleton.GetComponentCount(GameEntity.ComponentType.CompositeComponent);
			var comp = skeleton.GetComponentAtIndex(GameEntity.ComponentType.CustomType1, 0);
			var entity = comp.GetEntity();
			var mesh = comp.GetFirstMetaMesh();*/
			//skeleton.ClearComponents();

			/*var mm1 = CreateMorphedCopy(MetaMesh.GetCopy("body_female_th"), properties.GetHeight());
			var mm2 = MetaMesh.GetCopy("feet_female_th");
			mm1.SetFactor1Linear(properties.GetSkinColor());
			mm2.SetFactor1Linear(properties.GetSkinColor());
			visuals.AddMultiMesh(mm1, BodyMeshTypes.Body);
			visuals.AddMultiMesh(mm2, BodyMeshTypes.Legs);
			mm1.ManualInvalidate();
			mm2.ManualInvalidate();*/
			
			/*var skeleton = visuals.GetSkeleton();
			var boneNames = new string[skeleton.GetBoneCount()];
			var localRestFrames = new MatrixFrame[skeleton.GetBoneCount()];
			var EntitialRestFrames = new MatrixFrame[skeleton.GetBoneCount()];
			var EntitialFrames = new MatrixFrame[skeleton.GetBoneCount()];
			for (int i = 0, j = skeleton.GetBoneCount(); i < j; i++)
			{
				boneNames[i] = skeleton.GetBoneName((byte) i);
				localRestFrames[i] = skeleton.GetBoneLocalRestFrame((sbyte) i);
				EntitialRestFrames[i] = skeleton.GetBoneEntitialRestFrame((sbyte) i);
				EntitialFrames[i] = skeleton.GetBoneEntitialFrame((sbyte) i);
			}

			for (int i = 28, j = skeleton.GetBoneCount(); i < j; i++)
			{
				var rotMat = Mat3.Identity;
				rotMat.RotateAboutForward(0.78f);
				rotMat.RotateAboutSide(-0.78f);
				rotMat.RotateAboutUp(0.78f);
				var mat = new MatrixFrame(rotMat, Vec3.Zero);
				skeleton.SetBoneLocalFrame(i, mat);
			}

			skeleton.UpdateEntitialFramesFromLocalFrames();

			for (int i = 0, j = skeleton.GetBoneCount(); i < j; i++)
			{
				boneNames[i] = skeleton.GetBoneName((byte)i);
				localRestFrames[i] = skeleton.GetBoneLocalRestFrame((sbyte)i);
				EntitialRestFrames[i] = skeleton.GetBoneEntitialRestFrame((sbyte)i);
				EntitialFrames[i] = skeleton.GetBoneEntitialFrame((sbyte)i);
			}*/
		}

		[HarmonyPatch(typeof(TaleWorlds.MountAndBlade.View.AgentVisuals))]
		[HarmonyPatch("AddSkinMeshesToEntity")]
		static class AddSkinMeshesToEntityPatch
		{
			static bool Prefix(ref TaleWorlds.MountAndBlade.View.AgentVisuals __instance, AgentVisualsData ____data, int __0)
			{
				var properties = ____data.BodyPropertiesData;
				if (properties.Age <= 21f || !__instance.IsFemale)
					return true; // return to original method if the agent is either male or kid

				AddCustomSkinMeshes(____data.AgentVisuals, properties, ____data.EquipmentData, (SkinMask) __0,
					____data.PrepareImmediatelyData, 0f, ____data.UseTranslucencyData, ____data.UseTesselationData);
				return false; // skip original method
			}

			/*static void Postfix(ref AgentVisuals __instance)
			{
				if (!__instance.IsFemale)
				{
					__instance.GetVisuals().GetHeadMetaMeshOfVisuals(
						out var headMetaMesh,
						out var breadMetaMesh,
						out var hairMetaMesh,
						out var eyebrowMetaMesh);
					var breadName = breadMetaMesh?.GetName();
					var hairName = hairMetaMesh?.GetName();
					int daze = 1;
				}
			}*/
		}

		[HarmonyPatch(typeof(Agent))]
		[HarmonyPatch("AddSkinMeshes")]
		static class AddSkinMeshesPatch
		{
			static bool Prefix(ref Agent __instance)
			{
				var properties = __instance.BodyPropertiesValue;
				if (properties.Age <= 21f || !__instance.IsFemale)
					return true; // return to original method if the agent is either male or kid

				bool prepareImmediately = __instance == Agent.Main;
				SkinMask skinMeshesMask = __instance.SpawnEquipment.GetSkinMeshesMask();
				AddCustomSkinMeshes(__instance.AgentVisuals, properties, __instance.SpawnEquipment, 
					skinMeshesMask, prepareImmediately, __instance.Character.FaceDirtAmount);

				for (var equipment = EquipmentIndex.WeaponItemBeginSlot;
					equipment < EquipmentIndex.NumPrimaryWeaponSlots;
					equipment++)
				{
					if (!__instance.Equipment[equipment].IsEmpty)
					{
						if (ExtendedItemInfo.TryGet(__instance.Equipment[equipment].Item, out var itemInfo))
						{
							var va1 = itemInfo.VectorArgument1;
							if (va1.IsNonZero || va1.w != 0)
							{
								if (__instance.GetWeaponEntityFromEquipmentSlot(equipment) is GameEntity entity)
								{
									entity.SetVectorArgument(va1.x, va1.y, va1.z, va1.w);
								}
							}
							var va2 = itemInfo.VectorArgument2;
							if (va2.IsNonZero || va2.w != 0)
							{
								if (__instance.GetWeaponEntityFromEquipmentSlot(equipment) is GameEntity entity)
								{
									var count = entity.GetComponentCount(GameEntity.ComponentType.MetaMesh);
									for (int i = 0; i < count; i++)
									{
										((MetaMesh) entity.GetComponentAtIndex(i, GameEntity.ComponentType.MetaMesh))
											.SetVectorArgument2(va2.x, va2.y, va2.z, va2.w);
									}
								}
							}
						}
					}
				}
				return false; // skip original method
			}
		}

		// TODO: rewrite these shits with transpiler
		[HarmonyPatch(typeof(MissionScreen))]
		[HarmonyPatch("TaleWorlds.MountAndBlade.IMissionListener.OnEquipItemsFromSpawnEquipment")]
		static class OnEquipItemsFromSpawnEquipmentPatch
		{
			static bool Prefix(MissionScreen __instance, Agent __0, Agent.CreationType __1)
			{
				Agent agent = __0;
				Agent.CreationType creationType = __1;
				switch (creationType)
				{
					case Agent.CreationType.FromRoster:
					case Agent.CreationType.FromCharacterObj:
						{
							bool useTeamColor = false;
							Random randomGenerator = null;
							bool randomizeColors = agent.RandomizeColors;
							uint color3;
							uint color4;
							if (randomizeColors)
							{
								int bodyPropertiesSeed = agent.BodyPropertiesSeed;
								randomGenerator = new Random(bodyPropertiesSeed);
								Color color;
								Color color2;
								AgentVisuals.GetRandomClothingColors(bodyPropertiesSeed, Color.FromUint(agent.ClothingColor1), Color.FromUint(agent.ClothingColor2), out color, out color2);
								color3 = color.ToUnsignedInteger();
								color4 = color2.ToUnsignedInteger();
							}
							else
							{
								color3 = agent.ClothingColor1;
								color4 = agent.ClothingColor2;
							}
							for (EquipmentIndex equipmentIndex = EquipmentIndex.NumAllWeaponSlots; equipmentIndex < EquipmentIndex.ArmorItemEndSlot; equipmentIndex++)
							{
								if (!agent.SpawnEquipment[equipmentIndex].IsEmpty)
								{
									bool hasGloves = equipmentIndex == EquipmentIndex.Body && agent.SpawnEquipment[EquipmentIndex.Gloves].Item != null;
									bool isFemale = agent.Age >= 14f && agent.IsFemale;
									// --------------------------------------
									MetaMesh multiMesh = null;
									if (equipmentIndex == EquipmentIndex.Body)
									{
										string meshName = agent.SpawnEquipment[equipmentIndex].Item?.MultiMeshName;
										multiMesh = meshName != null ? MorphedMeshCache.GetMorphedCopy(meshName, agent.BodyPropertiesValue.GetBreastSize()) : null;
									}
									else
									{
										multiMesh = agent.SpawnEquipment[equipmentIndex].GetMultiMesh(isFemale, hasGloves, true);
									}

									/*if (multiMesh != null)
									{
										for (int i = 0, j = multiMesh.MeshCount; i < j; i++)
										{
											var mesh = multiMesh.GetMeshAtIndex(i);
											var lod = multiMesh.GetLodMaskForMeshAtIndex(i);
											if ((lod & 1) != 0)
											{
												var outlineMesh = mesh.CreateCopy();
												outlineMesh.SetMaterial(Material.GetFromResource("outline_body_female"));
												multiMesh.AddMesh(outlineMesh, 0);
												outlineMesh.ManualInvalidate();
											}
										}
									}*/
									// --------------------------------------
									if (multiMesh != null)
									{
										if (randomizeColors)
										{
											multiMesh.SetGlossMultiplier(AgentVisuals.GetRandomGlossFactor(randomGenerator));
										}
										if (agent.SpawnEquipment[equipmentIndex].Item.IsUsingTeamColor)
										{
											for (int i = 0; i < multiMesh.MeshCount; i++)
											{
												Mesh meshAtIndex = multiMesh.GetMeshAtIndex(i);
												if (!meshAtIndex.HasTag("no_team_color"))
												{
													meshAtIndex.Color = color3;
													meshAtIndex.Color2 = color4;
													Material material = meshAtIndex.GetMaterial().CreateCopy();
													material.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
													meshAtIndex.SetMaterial(material);
													useTeamColor = true;
												}
												meshAtIndex.ManualInvalidate();
											}
										}
										if (agent.SpawnEquipment[equipmentIndex].Item.UsingFacegenScaling)
										{
											multiMesh.UseHeadBoneFaceGenScaling(agent.AgentVisuals.GetSkeleton(), agent.Monster.HeadLookDirectionBoneIndex, agent.AgentVisuals.GetFacegenScalingMatrix());
										}
										Skeleton skeleton = agent.AgentVisuals.GetSkeleton();
										int num = (skeleton != null) ? skeleton.GetComponentCount(GameEntity.ComponentType.ClothSimulator) : -1;
										agent.AgentVisuals.AddMultiMesh(multiMesh, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
										multiMesh.ManualInvalidate();
										int num2 = (skeleton != null) ? skeleton.GetComponentCount(GameEntity.ComponentType.ClothSimulator) : -1;
										if (skeleton != null && num2 > num && 
										    (equipmentIndex == EquipmentIndex.Cape /*||
										     equipmentIndex == EquipmentIndex.Body*/))
										{
											GameEntityComponent componentAtIndex = skeleton.GetComponentAtIndex(GameEntity.ComponentType.ClothSimulator, num2 - 1);
											agent.SetCapeClothSimulator(componentAtIndex);
										}
									}
									if (equipmentIndex == EquipmentIndex.Body && !string.IsNullOrEmpty(agent.SpawnEquipment[equipmentIndex].Item.ArmBandMeshName))
									{
										MetaMesh copy = MetaMesh.GetCopy(agent.SpawnEquipment[equipmentIndex].Item.ArmBandMeshName, true, true);
										if (copy != null)
										{
											if (randomizeColors)
											{
												copy.SetGlossMultiplier(AgentVisuals.GetRandomGlossFactor(randomGenerator));
											}
											if (agent.SpawnEquipment[equipmentIndex].Item.IsUsingTeamColor)
											{
												for (int j = 0; j < copy.MeshCount; j++)
												{
													Mesh meshAtIndex2 = copy.GetMeshAtIndex(j);
													if (!meshAtIndex2.HasTag("no_team_color"))
													{
														meshAtIndex2.Color = color3;
														meshAtIndex2.Color2 = color4;
														Material material2 = meshAtIndex2.GetMaterial().CreateCopy();
														material2.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
														meshAtIndex2.SetMaterial(material2);
														useTeamColor = true;
													}
													meshAtIndex2.ManualInvalidate();
												}
											}
											agent.AgentVisuals.AddMultiMesh(copy, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
											copy.ManualInvalidate();
										}
									}
									// --------------------------------------
									if (ExtendedItemInfo.TryGet(
										agent.SpawnEquipment[equipmentIndex].Item, out var itemInfo))
									{
										if (!string.IsNullOrWhiteSpace(itemInfo.ItemScript))
										{
											var param = new AbstractItemScript.ItemScriptParam();
											param.UserParams = itemInfo.ItemScriptParams;
											AbstractItemScript.AddParamAndScript(
												agent, itemInfo.ItemScript, param);
										}
									}
									// --------------------------------------
								}
							}
							ItemObject item = agent.SpawnEquipment[EquipmentIndex.Body].Item;
							if (item != null)
							{
								int lodAtlasIndex = item.LodAtlasIndex;
								if (lodAtlasIndex != -1)
								{
									agent.AgentVisuals.SetLodAtlasShadingIndex(lodAtlasIndex, useTeamColor, agent.ClothingColor1, agent.ClothingColor2);
								}
							}
							break;
						}
					case Agent.CreationType.FromHorseObj:
						MountVisualCreator.AddMountMeshToAgentVisual(agent.AgentVisuals, agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].Item, agent.SpawnEquipment[EquipmentIndex.HorseHarness].Item, agent.HorseCreationKey, agent);
						break;
				}
				ArmorComponent.ArmorMaterialTypes bodyArmorMaterialType = ArmorComponent.ArmorMaterialTypes.None;
				ItemObject item2 = agent.SpawnEquipment[EquipmentIndex.Body].Item;
				if (item2 != null)
				{
					bodyArmorMaterialType = item2.ArmorComponent.MaterialType;
				}
				agent.SetBodyArmorMaterialType(bodyArmorMaterialType);
				/*
				switch (creationType)
				{
					case Agent.CreationType.FromRoster:
					case Agent.CreationType.FromCharacterObj:
						{
							bool useTeamColor = false;
							Random randomGenerator = null;
							bool randomizeColors = agent.RandomizeColors;
							uint color3;
							uint color4;
							if (randomizeColors)
							{
								int bodyPropertiesSeed = agent.BodyPropertiesSeed;
								randomGenerator = new Random(bodyPropertiesSeed);
								Color color;
								Color color2;
								TaleWorlds.MountAndBlade.View.AgentVisuals.GetRandomClothingColors(bodyPropertiesSeed, Color.FromUint(agent.ClothingColor1), Color.FromUint(agent.ClothingColor2), out color, out color2);
								color3 = color.ToUnsignedInteger();
								color4 = color2.ToUnsignedInteger();
							}
							else
							{
								color3 = agent.ClothingColor1;
								color4 = agent.ClothingColor2;
							}
							for (EquipmentIndex equipmentIndex = EquipmentIndex.NumAllWeaponSlots; equipmentIndex < EquipmentIndex.ArmorItemEndSlot; equipmentIndex++)
							{
								if (!agent.SpawnEquipment[equipmentIndex].IsEmpty)
								{
									bool hasGloves = equipmentIndex == EquipmentIndex.Body && agent.SpawnEquipment[EquipmentIndex.Gloves].Item != null;
									bool isFemale = agent.Age >= 14f && agent.IsFemale;
									// --------------------------------------
									MetaMesh multiMesh = null;
									if (equipmentIndex == EquipmentIndex.Body)
									{
										string meshName = agent.SpawnEquipment[equipmentIndex].Item?.MultiMeshName;
										multiMesh = meshName != null ? MorphedMeshCache.GetMorphedCopy(meshName, agent.BodyPropertiesValue.GetBreastSize()) : null;
									}
									else
									{
										multiMesh = agent.SpawnEquipment[equipmentIndex].GetMultiMesh(isFemale, hasGloves, true);
									}
									// --------------------------------------
									if (multiMesh != null)
									{
										if (randomizeColors)
										{
											multiMesh.SetGlossMultiplier(TaleWorlds.MountAndBlade.View.AgentVisuals.GetRandomGlossFactor(randomGenerator));
										}
										if (agent.SpawnEquipment[equipmentIndex].Item.IsUsingTeamColor)
										{
											for (int i = 0; i < multiMesh.MeshCount; i++)
											{
												Mesh meshAtIndex = multiMesh.GetMeshAtIndex(i);
												if (!meshAtIndex.HasTag("no_team_color"))
												{
													meshAtIndex.Color = color3;
													meshAtIndex.Color2 = color4;
													Material material = meshAtIndex.GetMaterial().CreateCopy();
													material.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
													meshAtIndex.SetMaterial(material);
													useTeamColor = true;
												}
												meshAtIndex.ManualInvalidate();
											}
										}
										if (agent.SpawnEquipment[equipmentIndex].Item.UsingFacegenScaling)
										{
											multiMesh.UseHeadBoneFaceGenScaling(agent.AgentVisuals.GetSkeleton(), agent.Monster.HeadLookDirectionBoneIndex, agent.AgentVisuals.GetFacegenScalingMatrix());
										}
										agent.AgentVisuals.AddMultiMesh(multiMesh, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
										multiMesh.ManualInvalidate();
									}
									if (equipmentIndex == EquipmentIndex.Body && !string.IsNullOrEmpty(agent.SpawnEquipment[equipmentIndex].Item.ArmBandMeshName))
									{
										MetaMesh copy = MetaMesh.GetCopy(agent.SpawnEquipment[equipmentIndex].Item.ArmBandMeshName, true, true);
										if (copy != null)
										{
											if (randomizeColors)
											{
												copy.SetGlossMultiplier(TaleWorlds.MountAndBlade.View.AgentVisuals.GetRandomGlossFactor(randomGenerator));
											}
											if (agent.SpawnEquipment[equipmentIndex].Item.IsUsingTeamColor)
											{
												for (int j = 0; j < copy.MeshCount; j++)
												{
													Mesh meshAtIndex2 = copy.GetMeshAtIndex(j);
													if (!meshAtIndex2.HasTag("no_team_color"))
													{
														meshAtIndex2.Color = color3;
														meshAtIndex2.Color2 = color4;
														Material material2 = meshAtIndex2.GetMaterial().CreateCopy();
														material2.AddMaterialShaderFlag("use_double_colormap_with_mask_texture", false);
														meshAtIndex2.SetMaterial(material2);
														useTeamColor = true;
													}
													meshAtIndex2.ManualInvalidate();
												}
											}
											agent.AgentVisuals.AddMultiMesh(copy, MBAgentVisuals.GetBodyMeshIndex(equipmentIndex));
											copy.ManualInvalidate();
										}
									}
								}
							}
							ItemObject item = agent.SpawnEquipment[EquipmentIndex.Body].Item;
							if (item != null)
							{
								int lodAtlasIndex = item.LodAtlasIndex;
								if (lodAtlasIndex != -1)
								{
									agent.AgentVisuals.SetLodAtlasShadingIndex(lodAtlasIndex, useTeamColor, agent.ClothingColor1, agent.ClothingColor2);
								}
							}
							break;
						}
					case Agent.CreationType.FromHorseObj:
						MountVisualCreator.AddMountMeshToAgentVisual(agent.AgentVisuals, agent.SpawnEquipment[EquipmentIndex.ArmorItemEndSlot].Item, agent.SpawnEquipment[EquipmentIndex.HorseHarness].Item, agent.HorseCreationKey, agent);
						break;
				}
				ArmorComponent.ArmorMaterialTypes bodyArmorMaterialType = ArmorComponent.ArmorMaterialTypes.None;
				ItemObject item2 = agent.SpawnEquipment[EquipmentIndex.Body].Item;
				if (item2 != null)
				{
					bodyArmorMaterialType = item2.ArmorComponent.MaterialType;
				}
				agent.SetBodyArmorMaterialType(bodyArmorMaterialType);*/
				return false; // skip original method
			}
		}
	}
}