using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.Core;

namespace TouhouAnalepsia.Behaviors {
	class InitialRelationsCampaignBehavior : CampaignBehaviorBase {
		public override void RegisterEvents() {
			CampaignEvents.OnNewGameCreatedEvent.AddNonSerializedListener(this, new Action<CampaignGameStarter>(this.OnNewGameCreated));
		}

		public override void SyncData(IDataStore dataStore) {
		}

		public void OnNewGameCreated(CampaignGameStarter campaignGameStarter) {

			//Imports from warband
			ChangeRelations("lord_remelia", "lord_sakuya", troop_relation_servant);
			ChangeRelations("lord_remelia", "lord_flandre", troop_relation_younger_sister);
			ChangeRelations("lord_remelia", "lord_patchouli", troop_relation_bosom);
			ChangeRelations("lord_remelia", "lord_hong", troop_relation_servant);
			ChangeRelations("lord_remelia", "lord_satori", troop_relation_friend);
			ChangeRelations("lord_remelia", "lord_kanako", troop_relation_opponent);
			ChangeRelations("lord_remelia", "lord_reimu", troop_relation_opponent);
			ChangeRelations("lord_yuyuko", "lord_youmu", troop_relation_servant);
			//("lord_yuyuko", "trp_knight_2_8", troop_relation_servant);
			ChangeRelations("lord_yuyuko", "lord_yamaxanadu", troop_relation_friend);
			ChangeRelations("lord_yuyuko", "lord_yukari", troop_relation_bosom);
			ChangeRelations("lord_yuyuko", "lord_kanako", troop_relation_opponent);
			ChangeRelations("lord_satori", "lord_rin", troop_relation_servant);
			ChangeRelations("lord_satori", "lord_ustuho", troop_relation_servant);
			ChangeRelations("lord_satori", "lord_ustuho", troop_relation_servant);
			ChangeRelations("lord_satori", "lord_koishi", troop_relation_younger_sister);
			ChangeRelations("lord_satori", "lord_yugi", troop_relation_friend);
			ChangeRelations("lord_satori", "lord_konngara", troop_relation_friend);
			//("trp_kingdom_4_lord", "trp_knight_4_1", troop_relation_bosom);
			//("trp_kingdom_4_lord", "trp_knight_4_6", troop_relation_bosom);
			//("trp_kingdom_4_lord", "trp_knight_4_7", troop_relation_bosom);
			//("trp_kingdom_4_lord", "trp_knight_4_9", troop_relation_bosom);
			//("trp_kingdom_4_lord", "trp_knight_4_2", troop_relation_friend);
			//("trp_kingdom_4_lord", "lord_nue", troop_relation_friend);
			//("trp_kingdom_4_lord", "trp_kingdom_7_lord", troop_relation_opponent);
			ChangeRelations("lord_kanako", "lord_sanae", troop_relation_pupillus);
			ChangeRelations("lord_kanako", "lord_suwako", troop_relation_bosom);
			ChangeRelations("lord_kanako", "lord_tengu", troop_relation_friend);
			ChangeRelations("lord_reimu", "lord_marisa", troop_relation_bosom);
			ChangeRelations("lord_reimu", "lord_mima", troop_relation_friend);
			ChangeRelations("lord_reimu", "lord_suika", troop_relation_friend);
			ChangeRelations("lord_reimu", "lord_sukuna", troop_relation_friend);
			//("lord_reimu", "trp_knight_6_12", troop_relation_friend);
			ChangeRelations("lord_reimu", "lord_yukari", troop_relation_friend);
			//("trp_kingdom_7_lord", "trp_knight_7_1", troop_relation_servant);
			//("trp_kingdom_7_lord", "trp_knight_7_2", troop_relation_servant);
			//("trp_kingdom_7_lord", "trp_knight_7_3", troop_relation_friend);
			//("trp_kingdom_7_lord", "lord_kokoro", troop_relation_pupillus);
			ChangeRelations("lord_kaguya", "lord_eirin", troop_relation_bosom);
			ChangeRelations("lord_kaguya", "lord_reisen", troop_relation_servant);
			ChangeRelations("lord_kaguya", "lord_tewi", troop_relation_servant);
			ChangeRelations("lord_kaguya", "lord_fujiwara", troop_relation_enemy);
			ChangeRelations("lord_sakuya", "lord_flandre", troop_relation_master);
			ChangeRelations("lord_sakuya", "lord_hong", troop_relation_friend);
			ChangeRelations("lord_sakuya", "lord_aya", troop_relation_opponent);
			ChangeRelations("lord_flandre", "lord_hong", troop_relation_servant);
			ChangeRelations("lord_flandre", "lord_marisa", troop_relation_opponent);
			ChangeRelations("lord_patchouli", "lord_koakuma", troop_relation_servant);
			ChangeRelations("lord_patchouli", "lord_marisa", troop_relation_friend);
			ChangeRelations("lord_lily1", "lord_wakasagihime", troop_relation_friend);
			ChangeRelations("lord_lily1", "lord_letty", troop_relation_friend);
			ChangeRelations("lord_lily1", "lord_lily0", troop_relation_friend);
			//("lord_wakasagihime", "trp_knight_8_6", troop_relation_friend);
			//("lord_youmu", "trp_knight_2_8", troop_relation_mentor);
			ChangeRelations("lord_youmu", "lord_momizi", troop_relation_opponent);
			ChangeRelations("lord_yukari", "lord_yamaxanadu", troop_relation_friend);
			ChangeRelations("lord_yukari", "lord_ran", troop_relation_servant);
			ChangeRelations("lord_yukari", "lord_chen", troop_relation_servant);
			ChangeRelations("lord_yukari", "lord_suika", troop_relation_friend);
			ChangeRelations("lord_yamaxanadu", "lord_komachi", troop_relation_servant);
			ChangeRelations("lord_ran", "lord_chen", troop_relation_servant);
			ChangeRelations("lord_chen", "lord_letty", troop_relation_friend);
			ChangeRelations("lord_lunasa", "lord_merlin", troop_relation_younger_sister);
			ChangeRelations("lord_lunasa", "lord_lyrica", troop_relation_younger_sister);
			ChangeRelations("lord_merlin", "lord_lyrica", troop_relation_younger_sister);
			ChangeRelations("lord_rin", "lord_koishi", troop_relation_master);
			ChangeRelations("lord_rin", "lord_ustuho", troop_relation_bosom);
			ChangeRelations("lord_ustuho", "lord_koishi", troop_relation_master);
			ChangeRelations("lord_yugi", "lord_konngara", troop_relation_friend);
			ChangeRelations("lord_yugi", "lord_suika", troop_relation_opponent);
			//("lord_yugi", "trp_knight_6_8", troop_relation_opponent);
			ChangeRelations("lord_kisume", "lord_yamame", troop_relation_friend);
			ChangeRelations("lord_kisume", "lord_parsee", troop_relation_friend);
			ChangeRelations("lord_yamame", "lord_parsee", troop_relation_friend);
			ChangeRelations("lord_konngara", "lord_suika", troop_relation_opponent);
			//("lord_konngara", "trp_knight_6_8", troop_relation_opponent);
			//("trp_knight_4_1", "trp_knight_4_6", troop_relation_friend);
			//("trp_knight_4_1", "trp_knight_4_7", troop_relation_friend);
			//("trp_knight_4_1", "trp_knight_4_9", troop_relation_friend);
			//("trp_knight_4_1", "trp_knight_7_1", troop_relation_opponent);
			//("trp_knight_4_2", "lord_nue", troop_relation_bosom);
			//("trp_knight_4_3", "trp_knight_4_5", troop_relation_bosom);
			//("trp_knight_4_3", "lord_nue", troop_relation_bosom);
			//("lord_nue", "trp_knight_4_5", troop_relation_bosom);
			//("trp_knight_4_5", "lord_sekibanki", troop_relation_bosom);
			//("trp_knight_4_6", "trp_knight_4_7", troop_relation_friend);
			//("trp_knight_4_6", "trp_knight_4_9", troop_relation_friend);
			//("trp_knight_4_7", "trp_knight_4_9", troop_relation_friend);
			//("trp_knight_4_7", "trp_knight_4_8", troop_relation_servant);
			ChangeRelations("lord_nitori", "lord_momizi", troop_relation_bosom);
			ChangeRelations("lord_tengu", "lord_aya", troop_relation_servant);
			ChangeRelations("lord_tengu", "lord_momizi", troop_relation_servant);
			ChangeRelations("lord_tengu", "lord_hatate", troop_relation_servant);
			ChangeRelations("lord_suwako", "lord_sanae", troop_relation_pupillus);
			ChangeRelations("lord_aya", "lord_momizi", troop_relation_friend);
			ChangeRelations("lord_aya", "lord_hatate", troop_relation_friend);
			ChangeRelations("lord_momizi", "lord_hatate", troop_relation_friend);
			ChangeRelations("lord_hina", "lord_sizuha", troop_relation_friend);
			ChangeRelations("lord_minoriko", "lord_sizuha", troop_relation_elder_sister);
			ChangeRelations("lord_marisa", "lord_mima", troop_relation_mentor);
			ChangeRelations("lord_marisa", "lord_alice", troop_relation_friend);
			//("lord_marisa", "trp_knight_6_12", troop_relation_friend);
			//("lord_fujiwara", "trp_knight_6_6", troop_relation_bosom);
			//("lord_suika", "trp_knight_6_8", troop_relation_friend);
			ChangeRelations("lord_alice", "lord_yuuka", troop_relation_friend);
			ChangeRelations("lord_alice", "lord_medicine", troop_relation_opponent);
			//("trp_knight_6_6", "trp_knight_6_12", troop_relation_friend);
			ChangeRelations("lord_sukuna", "lord_benben", troop_relation_opponent);
			ChangeRelations("lord_sukuna", "lord_yatsuhashi", troop_relation_opponent);
			ChangeRelations("lord_sukuna", "lord_seija", troop_relation_friend);
			ChangeRelations("lord_sunny", "lord_luna", troop_relation_bosom);
			ChangeRelations("lord_sunny", "lord_star", troop_relation_bosom);
			ChangeRelations("lord_luna", "lord_star", troop_relation_bosom);
			//("trp_knight_6_12", "trp_knight_6_13", troop_relation_friend);
			//("trp_knight_7_1", "trp_knight_7_2", troop_relation_friend);
			//("trp_knight_7_1", "trp_knight_7_3", troop_relation_friend);
			//("trp_knight_7_2", "trp_knight_7_3", troop_relation_friend);
			//("trp_knight_7_3", "trp_knight_7_4", troop_relation_production);
			ChangeRelations("lord_benben", "lord_yatsuhashi", troop_relation_younger_sister);
			ChangeRelations("lord_benben", "lord_raiko", troop_relation_bosom);
			ChangeRelations("lord_yatsuhashi", "lord_raiko", troop_relation_bosom);
			ChangeRelations("lord_eirin", "lord_reisen", troop_relation_apprentice);
			ChangeRelations("lord_eirin", "lord_tewi", troop_relation_servant);
			ChangeRelations("lord_reisen", "lord_tewi", troop_relation_friend);
			//ChangeRelations("trp_knight_8_4", "lord_wriggle", troop_relation_friend);
			ChangeRelations("lord_wriggle", "lord_yuuka", troop_relation_friend);
			ChangeRelations("lord_yuuka", "lord_medicine", troop_relation_friend);
		}
		public void ChangeRelations(string hero, string hero2, int change) {
			try {
				Hero heroObject = Game.Current.ObjectManager.GetObject<CharacterObject>(hero).HeroObject;
				Hero heroObject2 = Game.Current.ObjectManager.GetObject<CharacterObject>(hero2).HeroObject;

				ChangeRelationAction.ApplyRelationChangeBetweenHeroes(heroObject, heroObject2, change, false);
			} catch {
				throw new Exception("Either " + hero + " or " + hero2 + " does not exist!");
			}
		}
		//Imported from warband
		public const int troop_relation_unfamiliar = 0;
		public const int troop_relation_friend = 20;
		public const int troop_relation_bosom = 60;
		public const int troop_relation_fiance = 100;
		public const int troop_relation_spouse = 200;
		public const int troop_relation_opponent = -20;
		public const int troop_relation_enemy = -40;
		public const int troop_relation_creator = 100;
		public const int troop_relation_production = 20;
		public const int troop_relation_elder_sister = 60;
		public const int troop_relation_younger_sister = 60;
		public const int troop_relation_guardian = 50;
		public const int troop_relation_pupillus = 50;
		public const int troop_relation_master = 100;
		public const int troop_relation_servant = 20;
		public const int troop_relation_mentor = 20;
		public const int troop_relation_apprentice = 20;
	}
}
