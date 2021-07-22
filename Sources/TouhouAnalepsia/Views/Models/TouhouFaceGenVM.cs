using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using TaleWorlds.Core;
using TaleWorlds.Core.ViewModelCollection;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.Localization;
using TaleWorlds.MountAndBlade;
using TaleWorlds.MountAndBlade.ViewModelCollection;
using TouhouAnalepsia.Visuals;
using FaceGen = TaleWorlds.Core.FaceGen;

namespace TouhouAnalepsia.Views.Models
{
	public class TouhouFaceGenVM : ViewModel, INotifyPropertyChanged
	{
		private readonly ITouhouFaceGeneratorHandler _handler;

		private bool _characterRefreshEnabled = true;

		private bool _initialValuesSet = true;

		private readonly BodyGenerator _bodyGenerator;

		private TouhouFaceGenerationParams _faceGenerationParams = new TouhouFaceGenerationParams();

		private List<CommandStatus> _undoCommands;

		private List<CommandStatus> _redoCommands;

		private List<bool> _isVoiceTypeUsableForOnlyNpc;

		private readonly Dictionary<THFaceGenTabs, MBBindingList<FaceGenPropertyVM>> _tabProperties;

		private IList<bool> _tabAvailabilities;

		private List<uint> _skinColors;

		private readonly bool _showDebugValues;

		private bool _enforceConstraints;

		private IFaceGeneratorCustomFilter _filter;

		[DataSourceProperty] public bool IsBodyEditEnabled { set; get; }

		private BodyProperties _initialBodyProperties;

		private FaceGenPropertyVM _heightSlider;

		private FaceGenPropertyVM _breastSlider;

		private int hairNum;
		
		private int faceTextureNum;
		
		private int eyebrowTextureNum;

		private int _newSoundPresetSize;

		private float _scale = 1f;

		public int Tab { set; get; }

		[DataSourceProperty] public bool IsDressed { set; get; }

		[DataSourceProperty] public bool IsHairGradientColor { set; get; }

		[DataSourceProperty] public bool IsEyeSecondColor { set; get; }

		[DataSourceProperty] public bool IsEyeSecondLight { set; get; }

		[DataSourceProperty] public bool IsEyeSecondTex { set; get; }

		[DataSourceProperty] public FaceGenPropertyVM SoundPreset { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> FaceTexTypes { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> FaceMeshTypes { set; get; }

		[DataSourceProperty] public SelectorVM<SelectorItemVM> EyeColor1Selector { set; get; }

		[DataSourceProperty] public SelectorVM<SelectorItemVM> EyeColor2Selector { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> EyeTex1Types { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> EyeTex2Types { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> EyeBrowTypes { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> EyeLight1Types { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> EyeLight2Types { set; get; }

		[DataSourceProperty] public SelectorVM<SelectorItemVM> SkinColorSelector { set; get; }

		[DataSourceProperty] public SelectorVM<SelectorItemVM> NailColorSelector { set; get; }

		[DataSourceProperty] public SelectorVM<SelectorItemVM> HairColor1Selector { set; get; }

		[DataSourceProperty] public SelectorVM<SelectorItemVM> HairColor2Selector { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> HairFrontTypes { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> HairBackTypes { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> HairSideTypes { set; get; }

		[DataSourceProperty] public MBBindingList<FacegenListItemVM> HairPinTypes { set; get; }

		private FacegenListItemVM SelectedFaceTex { set; get; }

		private FacegenListItemVM SelectedFaceMesh { set; get; }

		private FacegenListItemVM SelectedEyeTex1 { set; get; }

		private FacegenListItemVM SelectedEyeTex2 { set; get; }

		private FacegenListItemVM SelectedEyeBrow { set; get; }

		private FacegenListItemVM SelectedEyeLight1 { set; get; }

		private FacegenListItemVM SelectedEyeLight2 { set; get; }

		private FacegenListItemVM SelectedHairFront { set; get; }

		private FacegenListItemVM SelectedHairBack { set; get; }

		private FacegenListItemVM SelectedHairSide { set; get; }

		[DataSourceProperty] public HintViewModel BodyHint { get; } = new HintViewModel(GameTexts.FindText("str_body"));

		[DataSourceProperty] public HintViewModel FaceHint { get; } = new HintViewModel(GameTexts.FindText("str_face"));

		[DataSourceProperty] public HintViewModel EyesHint { get; } = new HintViewModel(GameTexts.FindText("str_eyes"));

		[DataSourceProperty] public HintViewModel NoseHint { get; } = new HintViewModel(GameTexts.FindText("str_nose"));

		[DataSourceProperty] public HintViewModel HairHint { get; } = new HintViewModel(GameTexts.FindText("str_hair"));

		[DataSourceProperty] public HintViewModel TaintHint { get; } = new HintViewModel(GameTexts.FindText("str_face_gen_markings"));

		[DataSourceProperty] public HintViewModel MouthHint { get; } = new HintViewModel(GameTexts.FindText("str_mouth"));

		[DataSourceProperty] public HintViewModel RedoHint { get; } = new HintViewModel(GameTexts.FindText("str_redo"));

		[DataSourceProperty] public HintViewModel UndoHint { get; } = new HintViewModel(GameTexts.FindText("str_undo"));

		[DataSourceProperty] public HintViewModel RandomizeHint { get; } = new HintViewModel(GameTexts.FindText("str_randomize"));

		[DataSourceProperty] public HintViewModel RandomizeAllHint { get; } = new HintViewModel(GameTexts.FindText("str_randomize_all"));

		[DataSourceProperty] public HintViewModel ResetHint { get; } = new HintViewModel(GameTexts.FindText("str_reset"));

		[DataSourceProperty] public HintViewModel ResetAllHint { get; } = new HintViewModel(GameTexts.FindText("str_reset_all"));

		[DataSourceProperty] public HintViewModel ClothHint { get; } = new HintViewModel(GameTexts.FindText("str_face_gen_change_cloth"));

		[DataSourceProperty] public string FlipHairLbl { get; } = new TextObject("{=74PKmRWJ}Flip Hair", null).ToString();

		[DataSourceProperty] public string SkinColorLbl { get; } = GameTexts.FindText("sf_facegen_skin_color", null).ToString();

		[DataSourceProperty] public string Title { get; } = GameTexts.FindText("sf_facegen_title", null).ToString();

		[DataSourceProperty]
		public string DoneBtnLbl
		{
			get => _handler.AffirmitiveText.ToString();
		}

		[DataSourceProperty]
		public string CancelBtnLbl
		{
			get => _handler.NegativeText.ToString();
		}

		public TouhouFaceGenVM(ITouhouFaceGeneratorHandler handler)
		{
			_handler = handler;
			_bodyGenerator = handler.BodyGenerator;
			_showDebugValues = FaceGen.ShowDebugValues;
			_filter = handler.Filter;
			FaceTexTypes = new MBBindingList<FacegenListItemVM>();
			FaceMeshTypes = new MBBindingList<FacegenListItemVM>();
			EyeTex1Types = new MBBindingList<FacegenListItemVM>();
			EyeTex2Types = new MBBindingList<FacegenListItemVM>();
			EyeBrowTypes = new MBBindingList<FacegenListItemVM>();
			EyeLight1Types = new MBBindingList<FacegenListItemVM>();
			EyeLight2Types = new MBBindingList<FacegenListItemVM>();
			HairFrontTypes = new MBBindingList<FacegenListItemVM>();
			HairBackTypes = new MBBindingList<FacegenListItemVM>();
			HairSideTypes = new MBBindingList<FacegenListItemVM>();
			HairPinTypes = new MBBindingList<FacegenListItemVM>();
			_tabProperties = new Dictionary<THFaceGenTabs, MBBindingList<FaceGenPropertyVM>>();
			for (int i = 0; i < (int) THFaceGenTabs.NumOfFaceGenTabs; i++)
			{
				_tabProperties[(THFaceGenTabs) i] = new MBBindingList<FaceGenPropertyVM>();
			}

			Tab = -1;
			IsDressed = false;
			_undoCommands = new List<CommandStatus>(100);
			_redoCommands = new List<CommandStatus>(100);
			RefreshValues();
		}

		public override void RefreshValues()
		{
			base.RefreshValues();
			SoundPreset?.RefreshValues();
			RefreshBindingList(FaceTexTypes);
			RefreshBindingList(FaceMeshTypes);
			EyeColor1Selector?.RefreshValues();
			EyeColor2Selector?.RefreshValues();
			RefreshBindingList(EyeTex1Types);
			RefreshBindingList(EyeTex2Types);
			RefreshBindingList(EyeBrowTypes);
			RefreshBindingList(EyeLight1Types);
			RefreshBindingList(EyeLight2Types);
			SkinColorSelector?.RefreshValues();
			NailColorSelector?.RefreshValues();
			HairColor1Selector?.RefreshValues();
			HairColor2Selector?.RefreshValues();
			RefreshBindingList(HairFrontTypes);
			RefreshBindingList(HairBackTypes);
			RefreshBindingList(HairSideTypes);
			RefreshBindingList(HairPinTypes);
			SelectedFaceTex?.RefreshValues();
			SelectedFaceMesh?.RefreshValues();
			SelectedEyeTex1?.RefreshValues();
			SelectedEyeTex2?.RefreshValues();
			SelectedEyeBrow?.RefreshValues();
			SelectedEyeLight1?.RefreshValues();
			SelectedEyeLight2?.RefreshValues();
			SelectedHairFront?.RefreshValues();
			SelectedHairBack?.RefreshValues();
			SelectedHairSide?.RefreshValues();
			foreach (var pair in _tabProperties)
			{
				RefreshBindingList(pair.Value);
			}
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void RefreshBindingList<T>(MBBindingList<T> list) where T : ViewModel
		{
			if (list != null)
			{
				foreach (var propertyVm in list)
				{
					propertyVm.RefreshValues();
				}
			}
		}

		private void FilterCategories()
		{
			FaceGeneratorStage[] availableStages = _filter.GetAvailableStages();
			IsBodyEditEnabled = availableStages.Contains(FaceGeneratorStage.Body);
			Tab = IsBodyEditEnabled ? (int) THFaceGenTabs.Body : (int) THFaceGenTabs.HairFront;
		}

		private void SetColorCodes()
		{
			_skinColors = MBBodyProperties.GetSkinColorGradientPoints(1, (int)_bodyGenerator.Character.Age);
			var hairColors = SkinManager.HairColors;
			
			SkinColorSelector = new SelectorVM<SelectorItemVM>(_skinColors.Select(delegate (uint t)
			{
				t %= 4278190080U;
				return "#" + Convert.ToString((long)((ulong)t), 16).PadLeft(6, '0').ToUpper() + "FF";
			}).ToList(), _faceGenerationParams.SkinColorIndex, OnSelectSkinColor);

			NailColorSelector = new SelectorVM<SelectorItemVM>(hairColors.Select(t => t.ToString()).ToList(),
				_faceGenerationParams.HairColor1Index, OnSelectHairColor1);

			HairColor1Selector = new SelectorVM<SelectorItemVM>(hairColors.Select(t => t.ToString()).ToList(),
				_faceGenerationParams.HairColor1Index, OnSelectHairColor1);

			HairColor2Selector = new SelectorVM<SelectorItemVM>(hairColors.Select(t => t.ToString()).ToList(),
				_faceGenerationParams.HairColor2Index, OnSelectHairColor2);

			EyeColor1Selector = new SelectorVM<SelectorItemVM>(hairColors.Select(t => t.ToString()).ToList(),
				_faceGenerationParams.EyeColor1Index, OnSelectHairColor1);

			EyeColor2Selector = new SelectorVM<SelectorItemVM>(hairColors.Select(t => t.ToString()).ToList(),
				_faceGenerationParams.EyeColor2Index, OnSelectHairColor1);

			EyeColor2Selector = new SelectorVM<SelectorItemVM>(hairColors.Select(t => t.ToString()).ToList(),
				_faceGenerationParams.EyeColor2Index, OnSelectHairColor1);
		}

		private void OnSelectSkinColor(SelectorVM<SelectorItemVM> s)
		{
			AddCommand();
			_faceGenerationParams.SkinColorIndex = s.SelectedIndex;
			UpdateFace();
		}

		private void OnSelectHairColor1(SelectorVM<SelectorItemVM> s)
		{
			AddCommand();
			_faceGenerationParams.HairColor1Index = s.SelectedIndex;
			UpdateFace();
		}

		private void OnSelectHairColor2(SelectorVM<SelectorItemVM> s)
		{
			AddCommand();
			_faceGenerationParams.HairColor2Index = s.SelectedIndex;
			UpdateFace();
		}

		private void OnHeightChanged()
		{
			_handler.OnHeightChanged(_heightSlider != null ? _heightSlider.Value : 0f);
		}

		private void SetTabAvailabilities()
		{
			_tabAvailabilities = new List<bool>
			{
				IsBodyEditEnabled, // body
				IsBodyEditEnabled, // face
				IsBodyEditEnabled,
				IsBodyEditEnabled,
				IsBodyEditEnabled, // eye
				IsBodyEditEnabled,
				IsBodyEditEnabled,
				IsBodyEditEnabled,
				true, // hair
				true,
				true,
				true,
				true,
				true, // underwear
				true,
				true, // parts
				true,
				true,
				true,
				true,
				true // photo
			}.AsReadOnly();
		}

		private void OnTabClicked(int index)
		{
			Tab = index;
		}

		public void SelectPreviousTab()
		{
			int num = (Tab == 0) ? (int) (THFaceGenTabs.NumOfFaceGenTabs - 1) : (Tab - 1);
			while (!_tabAvailabilities[num] && num != Tab)
			{
				num = ((num == 0) ? (int)(THFaceGenTabs.NumOfFaceGenTabs - 1) : (num - 1));
			}
			Tab = num;
		}

		public void SelectNextTab()
		{
			int num = (Tab + 1) % (int) THFaceGenTabs.NumOfFaceGenTabs;
			while (!_tabAvailabilities[num] && num != Tab)
			{
				num = (num + 1) % (int)THFaceGenTabs.NumOfFaceGenTabs;
			}
			Tab = num;
		}

		private int GetVoiceUIIndex()
		{
			int num = 0;
			for (int i = 0; i < _faceGenerationParams.Voice; i++)
			{
				if (!_isVoiceTypeUsableForOnlyNpc[i])
				{
					num++;
				}
			}
			return num;
		}

		private int GetVoiceRealIndex(int UIValue)
		{
			int num = 0;
			for (int i = 0; i < _newSoundPresetSize; i++)
			{
				if (!_isVoiceTypeUsableForOnlyNpc[i])
				{
					if (num == UIValue)
					{
						return i;
					}
					num++;
				}
			}
			return -1;
		}

		private void ExecuteHearCurrentVoiceSample()
		{
			_handler.MakeVoice(_faceGenerationParams.Gender, _faceGenerationParams.VoicePitch);
		}

		private void ExecuteReset()
		{
			string titleText = GameTexts.FindText("str_reset").ToString();
			string text = new TextObject("{=hiKTvBgF}Are you sure want to reset changes done in this tab? Your changes will be lost.").ToString();
			InformationManager.ShowInquiry(
				new InquiryData(titleText, text, true, true, 
					GameTexts.FindText("str_yes").ToString(), 
					GameTexts.FindText("str_no").ToString(),
					Reset, null));
		}

		public void SetBodyProperties(BodyProperties bodyProperties, bool ignoreDebugValues, int gender = -1, bool recordChange = false)
		{
			this._characterRefreshEnabled = false;
			this._faceGenerationParams.Gender = 1;
			if (ignoreDebugValues)
			{
				bodyProperties = new BodyProperties(new DynamicBodyProperties(
					_bodyGenerator.CurrentBodyProperties.Age, 
					_bodyGenerator.CurrentBodyProperties.Weight, 
					_bodyGenerator.CurrentBodyProperties.Build), 
					bodyProperties.StaticProperties);
			}
			this._bodyGenerator.CurrentBodyProperties = bodyProperties;
			_faceGenerationParams = new TouhouFaceGenerationParams(bodyProperties, _bodyGenerator.Character.Equipment.EarsAreHidden);
			//MBBodyProperties.GetParamsFromKey(ref this._faceGenerationParams, bodyProperties, this._bodyGenerator.Character.Equipment.EarsAreHidden);
			this.UpdateFacegen();
			this._characterRefreshEnabled = true;
			this.UpdateFace();
			if (recordChange)
			{
				this._characterRefreshEnabled = true;
				this.AddCommand();
			}
		}

		private void ResetDo(THFaceGenTabs tab)
		{
			switch (tab)
			{
				case THFaceGenTabs.Body:
					SoundPreset.Reset();
					SkinColorSelector.SelectedIndex = _initialBodyProperties.GetSkinColorIndex();
					break;
				case THFaceGenTabs.HairFront:
					SetSelectedHairType(_initialBodyProperties.GetHairBackIndex(), false);
					break;
			}
			foreach (FaceGenPropertyVM faceGenPropertyVM in _tabProperties[(THFaceGenTabs)Tab])
			{
				if (faceGenPropertyVM.TabID == Tab)
				{
					faceGenPropertyVM.Reset();
				}
			}
		}

		private void Reset()
		{
			if (Tab >= 0 && Tab < (int) THFaceGenTabs.NumOfFaceGenTabs)
			{
				AddCommand();
				_characterRefreshEnabled = false;
				ResetDo((THFaceGenTabs) Tab);
				_characterRefreshEnabled = true;
				UpdateFace();
			}
		}

		private void ResetAll()
		{
			_characterRefreshEnabled = false;
			for (int i = 0; i < (int) THFaceGenTabs.NumOfFaceGenTabs; i++)
			{
				ResetDo((THFaceGenTabs) i);
			}

			_faceGenerationParams = new TouhouFaceGenerationParams(_bodyGenerator);
			_undoCommands.Clear();
			_redoCommands.Clear();
			_characterRefreshEnabled = true;
			Refresh();
		}

		private void ExecuteResetAll()
		{
			string titleText = GameTexts.FindText("str_reset_all", null).ToString();
			string text = new TextObject("{=1hnq3Kb1}Are you sure want to reset all properties? Your changes will be lost.").ToString();
			InformationManager.ShowInquiry(new InquiryData(titleText, text, true, true, GameTexts.FindText("str_yes").ToString(), GameTexts.FindText("str_no").ToString(), ResetAll, null, ""));
		}

		private void RandomizeDo(THFaceGenTabs tab)
		{
			foreach (FaceGenPropertyVM faceGenPropertyVM in _tabProperties[tab])
			{
				faceGenPropertyVM.Randomize();
			}
			switch (tab)
			{
				case THFaceGenTabs.Body:
					SkinColorSelector.SelectedIndex = MBRandom.RandomInt(_skinColors.Count);
					break;
				case THFaceGenTabs.HairFront:
					SetSelectedHairType(HairBackTypes[MBRandom.RandomInt(HairBackTypes.Count)], false);
					break;
			}
		}

		private void ExecuteRandomize()
		{
			if (Tab >= 0 && Tab < (int) THFaceGenTabs.NumOfFaceGenTabs)
			{
				AddCommand();
				_characterRefreshEnabled = false;
				RandomizeDo((THFaceGenTabs)Tab);
				_characterRefreshEnabled = true;
				UpdateFace();
			}
		}

		private void ExecuteRandomizeAll()
		{
			AddCommand();
			_characterRefreshEnabled = false;
			for (int i = 0; i < (int)THFaceGenTabs.NumOfFaceGenTabs; i++)
			{
				RandomizeDo((THFaceGenTabs)i);
			}
			_characterRefreshEnabled = true;
			UpdateFace();
		}

		private void ExecuteCancel()
		{
			_handler.Cancel();
		}

		private void ExecuteDone()
		{
			_handler.Done();
		}

		private CommandStatus GetCurrentStatus()
		{
			return new CommandStatus()
			{
				BodyProperties = _bodyGenerator.CurrentBodyProperties,
				IsEyeSecondColor = IsEyeSecondColor,
				IsEyeSecondLight = IsEyeSecondLight,
				IsEyeSecondTex = IsEyeSecondTex,
				IsHairGradientColor = IsHairGradientColor
			};
		}

		private void RecoverStatus(CommandStatus status)
		{
			_characterRefreshEnabled = false;
			IsHairGradientColor = status.IsHairGradientColor;
			IsEyeSecondColor = status.IsEyeSecondColor;
			IsEyeSecondLight = status.IsEyeSecondLight;
			IsEyeSecondTex = status.IsEyeSecondTex;
			SetBodyProperties(status.BodyProperties, false, 1);
			_characterRefreshEnabled = true;
		}

		private void ExecuteRedo()
		{
			if (_redoCommands.Count > 0)
			{
				int index = _redoCommands.Count - 1;
				var status = _redoCommands[index];
				_redoCommands.RemoveAt(index);
				_undoCommands.Add(GetCurrentStatus());
				RecoverStatus(status);
			}
		}

		private void ExecuteUndo()
		{
			if (_undoCommands.Count > 0)
			{
				int index = _undoCommands.Count - 1;
				var status = _undoCommands[index];
				_undoCommands.RemoveAt(index);
				_redoCommands.Add(GetCurrentStatus());
				RecoverStatus(status);
			}
		}

		private void ExecuteChangeClothing()
		{
			if (this.IsDressed)
			{
				_handler.UndressCharacterEntity();
				this.IsDressed = false;
				return;
			}
			_handler.DressCharacterEntity();
			this.IsDressed = true;
		}

		private void ExecuteChangeHairGradientColor()
		{
			AddCommand();
			if (IsHairGradientColor)
			{
				// TODO:
			}
			IsHairGradientColor = !IsHairGradientColor;
			UpdateFace();
		}

		public void AddCommand()
		{
			if (_characterRefreshEnabled)
			{
				if (_undoCommands.Count + 1 == _undoCommands.Capacity)
				{
					_undoCommands.RemoveAt(0);
				}
				_undoCommands.Add(GetCurrentStatus());
				_redoCommands.Clear();
			}
		}

		private void UpdateFace()
		{
			if (_characterRefreshEnabled)
			{
				_faceGenerationParams.RefreshFace(_bodyGenerator);
				_handler.RefreshCharacterEntity();
			}
		}

		private void UpdateFace(int keyNo, float value, bool calledFromInit, bool isNeedRefresh = true)
		{
			if (_enforceConstraints)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (keyNo > -1)
			{
				_faceGenerationParams.KeyWeights[keyNo] = value;
				_enforceConstraints = _faceGenerationParams.EnforceConstraints();
				flag3 = (_enforceConstraints && !calledFromInit);
			}
			else
			{
				if (Enum.IsDefined(typeof(DeformKeypoints), keyNo))
				{
					switch ((DeformKeypoints)keyNo)
					{
						case DeformKeypoints.Breast:
							_faceGenerationParams.Breast = value;
							break;
						case DeformKeypoints.VoicePitch:
							_faceGenerationParams.VoicePitch = value;
							break;
						case DeformKeypoints.Build:
							_faceGenerationParams.Build = value;
							_enforceConstraints = _faceGenerationParams.EnforceConstraints();
							flag3 = (_enforceConstraints && !calledFromInit);
							break;
						case DeformKeypoints.Weight:
							_faceGenerationParams.Weight = value;
							_enforceConstraints = _faceGenerationParams.EnforceConstraints();
							flag3 = (_enforceConstraints && !calledFromInit);
							break;
						case DeformKeypoints.Height:
							_faceGenerationParams.Height = MathF.Clamp(value, 0f, 1f);
							_enforceConstraints = _faceGenerationParams.EnforceConstraints();
							flag3 = (_enforceConstraints && !calledFromInit);
							flag2 = true;
							break;
						case DeformKeypoints.Age:
							_faceGenerationParams.Age = value;
							_enforceConstraints = _faceGenerationParams.EnforceConstraints();
							flag3 = (_enforceConstraints && !calledFromInit);
							flag = true;
							flag2 = true;
							break;
						default:
							MBDebug.ShowWarning("Unknown float preset: " + keyNo);
							break;
					}
				}
				else
				{
					MBDebug.ShowWarning("Unknown float preset: " + keyNo);
				}
			}
			if (flag3)
			{
				UpdateFacegen();
			}
			if (isNeedRefresh)
			{
				UpdateFace();
			}
			_enforceConstraints = false;
			if (flag)
			{
				_handler.OnAgeChanged();
			}
			if (flag2)
			{
				OnHeightChanged();
			}
		}

		private void UpdateFaceInt(int keyNo, int value, bool calledFromInit, bool isNeedRefresh = true)
		{
			if (_enforceConstraints)
			{
				return;
			}
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			if (keyNo > -1)
			{
				_faceGenerationParams.KeyWeights[keyNo] = value;
				_enforceConstraints = _faceGenerationParams.EnforceConstraints();
				flag3 = (_enforceConstraints && !calledFromInit);
			}
			else
			{
				if (Enum.IsDefined(typeof(DeformKeypoints), keyNo))
				{
					switch ((DeformKeypoints)keyNo)
					{
						case DeformKeypoints.Eyebrow:
							_faceGenerationParams.Eyebrow = value;
							break;
						case DeformKeypoints.Mouth:
							_faceGenerationParams.MouthTexture = value;
							break;
						case DeformKeypoints.EyeColor1:
							_faceGenerationParams.EyeColor1Index = value;
							break;
						case DeformKeypoints.Tattoo:
							_faceGenerationParams.FaceTattoo = value;
							break;
						case DeformKeypoints.Voice:
							_faceGenerationParams.Voice = GetVoiceRealIndex((int)value);
							break;
						case DeformKeypoints.Beard:
							// do nothing
							break;
						case DeformKeypoints.Hair:
							_faceGenerationParams.HairBack = value;
							break;
						case DeformKeypoints.FaceTex:
							_faceGenerationParams.FaceTexture = value;
							break;
						case DeformKeypoints.Gender:
							// do nothing
							break;
						default:
							MBDebug.ShowWarning("Unknown int preset: " + keyNo);
							break;
					}
				}
				else
				{
					MBDebug.ShowWarning("Unknown int preset: " + keyNo);
				}
			}
			if (flag3)
			{
				UpdateFacegen();
			}
			if (isNeedRefresh)
			{
				UpdateFace();
			}
			if (!calledFromInit && keyNo < 0)
			{
				if (keyNo != (int) DeformKeypoints.Mouth)
				{
					if (keyNo == (int)DeformKeypoints.Voice)
					{
						_handler.MakeVoice(_faceGenerationParams.Gender, _faceGenerationParams.VoicePitch);
					}
				}
				else
				{
					_handler.SetFacialAnimation("facegen_teeth", false);
				}
			}
			_enforceConstraints = false;
		}

		// Sync the changed values from ViewModel(data) to View(UI)
		public void UpdateFacegen()
		{
			foreach (MBBindingList<FaceGenPropertyVM> mbbindingList in this._tabProperties.Values)
			{
				foreach (FaceGenPropertyVM faceGenPropertyVM in mbbindingList)
				{
					if (faceGenPropertyVM.KeyNo < 0)
					{
						if (Enum.IsDefined(typeof(DeformKeypoints), faceGenPropertyVM.KeyNo))
						{
							switch ((DeformKeypoints) faceGenPropertyVM.KeyNo)
							{
								case DeformKeypoints.Breast:
									faceGenPropertyVM.Value = _faceGenerationParams.Breast;
									break;
								case DeformKeypoints.Build:
									faceGenPropertyVM.Value = _faceGenerationParams.Build;
									break;
								case DeformKeypoints.Weight:
									faceGenPropertyVM.Value = _faceGenerationParams.Weight;
									break;
								case DeformKeypoints.Height:
									faceGenPropertyVM.Value = MathF.Clamp(_faceGenerationParams.Height, 0f, 1f);
									break;
								case DeformKeypoints.Age:
									faceGenPropertyVM.Value = _faceGenerationParams.Age;
									break;
							}
						}
					}
					else
					{
						faceGenPropertyVM.Value = _faceGenerationParams.KeyWeights[faceGenPropertyVM.KeyNo];
					}
					faceGenPropertyVM.PrevValue = -1.0;
				}
			}
			SoundPreset.Value = GetVoiceUIIndex();
			SkinColorSelector.SelectedIndex = _faceGenerationParams.SkinColorIndex;
			EyeColor1Selector.SelectedIndex = _faceGenerationParams.EyeColor1Index;
			EyeColor2Selector.SelectedIndex = _faceGenerationParams.EyeColor1Index;
			//NailColorSelector.SelectedIndex = _faceGenerationParams.SkinColorIndex;
			HairColor1Selector.SelectedIndex = _faceGenerationParams.HairColor1Index;
			HairColor2Selector.SelectedIndex = _faceGenerationParams.HairColor2Index;
		}

		private void SetSelectedHairType(FacegenListItemVM item, bool addCommand)
		{
			if (SelectedHairFront != null)
			{
				SelectedHairFront.IsSelected = false;
			}
			SelectedHairFront = item;
			SelectedHairFront.IsSelected = true;
			_faceGenerationParams.HairBack = item.Index;
			if (!addCommand)
			{
				return;
			}
			AddCommand();
			UpdateFaceInt((int) DeformKeypoints.Hair, item.Index, false);
		}

		private void SetSelectedHairType(int index, bool addCommand)
		{
			foreach (FacegenListItemVM facegenListItemVM in HairBackTypes)
			{
				if (facegenListItemVM.Index == index)
				{
					this.SetSelectedHairType(facegenListItemVM, addCommand);
					break;
				}
			}
		}

		private void ResetSliderPrevValues()
		{
			foreach (MBBindingList<FaceGenPropertyVM> mbbindingList in _tabProperties.Values)
			{
				foreach (FaceGenPropertyVM faceGenPropertyVM in mbbindingList)
				{
					faceGenPropertyVM.PrevValue = -1.0;
				}
			}
		}

		public void Refresh()
		{
			if (!_characterRefreshEnabled)
			{
				return;
			}
			_characterRefreshEnabled = false;
			SetColorCodes();
			_isVoiceTypeUsableForOnlyNpc = MBBodyProperties.GetVoiceTypeUsableForPlayerData(1, _bodyGenerator.Character.Age, _newSoundPresetSize);
			foreach (var pair in _tabProperties)
			{
				pair.Value.Clear();
			}
			int faceGenInstancesLength = MBBodyProperties.GetFaceGenInstancesLength(_faceGenerationParams.Gender, (int)_faceGenerationParams.Age);
			int keyTimePoint = 0;
			int keyTimePoint2 = 0;
			int keyTimePoint3 = 0;
			int keyTimePoint4 = 0;
			FaceGenPropertyVM item;
			for (int i = 0; i < faceGenInstancesLength; i++)
			{
				DeformKeyData deformKeyData = MBBodyProperties.GetDeformKeyData(i, _faceGenerationParams.Gender, (int)_faceGenerationParams.Age);
				TextObject textObject = new TextObject("{=bsiRNJtk}{NAME}:", null);
				textObject.SetTextVariable("NAME", GameTexts.FindText("str_facegen_skin", deformKeyData.Id));
				if (deformKeyData.Id == "weight")
				{
					keyTimePoint2 = deformKeyData.KeyTimePoint;
				}
				else if (deformKeyData.Id == "build")
				{
					keyTimePoint4 = deformKeyData.KeyTimePoint;
				}
				else if (deformKeyData.Id == "height")
				{
					keyTimePoint = deformKeyData.KeyTimePoint;
				}
				else if (deformKeyData.Id == "age")
				{
					keyTimePoint3 = deformKeyData.KeyTimePoint;
				}
				else
				{
					item = new FaceGenPropertyVM(i, 0.0, 1.0, textObject, deformKeyData.KeyTimePoint, deformKeyData.GroupId, 
						_faceGenerationParams.KeyWeights[i], UpdateFace, AddCommand, ResetSliderPrevValues);
					if (deformKeyData.GroupId > -1 && deformKeyData.GroupId < (int) THFaceGenTabs.NumOfFaceGenTabs)
					{
						_tabProperties[(THFaceGenTabs) deformKeyData.GroupId].Add(item);
					}
				}
			}
			if (_filter != null)
			{
				FilterCategories();
			}
			else if (Tab == -1)
			{
				IsBodyEditEnabled = true;
				Tab = 0;
			}
			SetTabAvailabilities();

			item = new FaceGenPropertyVM((int)DeformKeypoints.VoicePitch, 0.0, 1.0, new TextObject("{=G6hYIR5k}Voice Pitch:"), 
				(int)DeformKeypoints.VoicePitch, 0, _faceGenerationParams.VoicePitch, 
				UpdateFace, AddCommand, ResetSliderPrevValues);
			_tabProperties[THFaceGenTabs.Body].Add(item);

			_heightSlider = new FaceGenPropertyVM((int)DeformKeypoints.Height, 0, 1, new TextObject("{=cLJdeUWz}Height:"), keyTimePoint, 0, 
				_heightSlider == null ? _faceGenerationParams.Height : _heightSlider.Value, 
				UpdateFace, AddCommand, ResetSliderPrevValues);
			_tabProperties[THFaceGenTabs.Body].Add(_heightSlider);

			_breastSlider = new FaceGenPropertyVM((int)DeformKeypoints.Breast, 0, 1, new TextObject("Breast:"), (int)DeformKeypoints.Breast, 0,
				_breastSlider == null ? _faceGenerationParams.Height : _breastSlider.Value,
				UpdateFace, AddCommand, ResetSliderPrevValues);
			_tabProperties[THFaceGenTabs.Body].Add(_breastSlider);

			if (_showDebugValues)
			{
				double min = 23;
				item = new FaceGenPropertyVM((int)DeformKeypoints.Age, min, 30.0, new TextObject("{=H1emUb6k}Age:"), keyTimePoint3, 0, _faceGenerationParams.Age, 
					UpdateFace, AddCommand, ResetSliderPrevValues);
				_tabProperties[THFaceGenTabs.Body].Add(item);
			}
			if (_showDebugValues)
			{
				item = new FaceGenPropertyVM((int)DeformKeypoints.Weight, 0.0, 1.0, new TextObject("{=zBld61ck}Weight:"), keyTimePoint2, 0, _faceGenerationParams.Weight, 
					UpdateFace, AddCommand, ResetSliderPrevValues);
				_tabProperties[THFaceGenTabs.Body].Add(item);
				item = new FaceGenPropertyVM((int)DeformKeypoints.Build, 0.0, 1.0, new TextObject("{=EUAKPHek}Build:"), keyTimePoint4, 0, _faceGenerationParams.Build, 
					UpdateFace, AddCommand, ResetSliderPrevValues);
				_tabProperties[THFaceGenTabs.Body].Add(item);
			}
			/*item = new FaceGenPropertyVM(-12, 0.0, 1.0, new TextObject("{=qXxpITdc}Eye Color:"), -12, 2, _faceGenerationParams._curEyeColorOffset, 
				UpdateFace, AddCommand, ResetSliderPrevValues);
			_tabProperties[THFaceGenTabs.EyesColor].Add(item);*/
			if (!_initialValuesSet)
			{
				_initialBodyProperties = _bodyGenerator.CurrentBodyProperties;
				_initialValuesSet = true;
			}
			_characterRefreshEnabled = true;
			UpdateFace();
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void OnPropertyChanged(string propertyName, object before, object after)
		{
			if (before != after)
			{
				OnPropertyChangedWithValue(after, propertyName);
			}
		}

		public enum THFaceGenTabs : int
		{
			None = -1,
			Body,
			FaceOverall,
			FaceEyepits,
			FaceMouth,
			EyesColor,
			EyesStyle,
			EyesLight,
			EyesBrows,
			HairColor,
			HairFront,
			HairBack,
			HairSide,
			HairPin,
			UnderwearsTop,
			UnderwearsBottom,
			PartsEars,
			PartsWings,
			PartsTail,
			PartsOther,
			PartsHands,
			Photo,
			NumOfFaceGenTabs
		}

		public enum DeformKeypoints : int
		{
			HairPin = -23,
			HairSide = -22,
			HairFront = -21,
			Breast = -20,
			VoicePitch = -19,
			Build = -18,
			Weight = -17,
			Height = -16,
			Eyebrow = -15,
			Mouth = -14,
			EyeColor1 = -12,
			Age = -11,
			Tattoo = -10,
			Voice = -9,
			Beard = -7,
			Hair = -6,
			FaceTex = -3,
			Gender = -1
		}

		public interface ITouhouFaceGeneratorHandler : IFaceGeneratorHandler
		{
			void OnHeightChanged(float height);

			void OnAgeChanged();

			TextObject AffirmitiveText { get; }

			TextObject NegativeText { get; }

			BodyGenerator BodyGenerator { get; }

			IFaceGeneratorCustomFilter Filter { get; }
		}

		private class CommandStatus
		{
			public bool IsHairGradientColor { set; get; }

			public bool IsEyeSecondColor { set; get; }

			public bool IsEyeSecondLight { set; get; }

			public bool IsEyeSecondTex { set; get; }

			public BodyProperties BodyProperties { set; get; }
		}
	}
}
