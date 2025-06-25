#region Dependencies
using Dalamud.Game.ClientState.JobGauge.Types;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using System;
using System.Collections.Generic;
using System.Linq;
using WrathCombo.CustomComboNS;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

#endregion

namespace WrathCombo.Combos.PvE;
internal partial class SCH
{
    #region Lists
    internal static readonly List<uint>
        BroilList = [Ruin, Broil, Broil2, Broil3, Broil4],
        AetherflowList = [EnergyDrain, Lustrate, SacredSoil, Indomitability, Excogitation],
        FairyList = [WhisperingDawn, FeyBlessing, FeyIllumination, Dissipation, Aetherpact, SummonSeraph];
    
    internal static readonly Dictionary<uint, ushort>
        BioList = new()
        {
            { Bio, Debuffs.Bio1 },
            { Bio2, Debuffs.Bio2 },
            { Biolysis, Debuffs.Biolysis }
        };
    #endregion
    internal static SCHGauge Gauge => GetJobGauge<SCHGauge>();
    
    internal static IBattleChara? AetherPactTarget => Svc.Objects.Where(x => x is IBattleChara chara && chara.StatusList.Any(y => y.StatusId == 1223 && y.SourceObject.GameObjectId == Svc.Buddies.PetBuddy.ObjectId)).Cast<IBattleChara>().FirstOrDefault();
    internal static bool HasAetherflow => Gauge.Aetherflow > 0;
    internal static bool FairyDismissed => Gauge.DismissedFairy > 0;
    
    
    #region Eos Summoner
    public static bool NeedToSummon => DateTime.Now > SummonTime && !HasPetPresent() && !FairyDismissed;
    private static DateTime SummonTime
    {
        get
        {
            if (HasPetPresent() || FairyDismissed)
                return field = DateTime.Now.AddSeconds(1);

            return field;
        }
    }
    
    #endregion
    
    #region Dot Checker
    internal static bool NeedsDoT()
    {
        var dotAction = OriginalHook(Bio);
        var hpThreshold = Config.SCH_DPS_BioSubOption == 1 ||
                          !InBossEncounter()
            ? Config.SCH_DPS_BioSubOption
            : 0;
        BioList.TryGetValue(dotAction, out var dotDebuffID);
        var dotRemaining = GetStatusEffectRemainingTime(dotDebuffID, CurrentTarget);

        return ActionReady(dotAction) &&
               CanApplyStatus(CurrentTarget, dotDebuffID) &&
               !JustUsedOn(dotAction, CurrentTarget, 5f) &&
               HasBattleTarget() &&
               GetTargetHPPercent() > hpThreshold &&
               dotRemaining <= Config.SCH_DPS_BioUptime_Threshold;
    }
    #endregion
    
    #region Get ST Heals
    internal static int GetMatchingConfigST(int i, IGameObject? OptionalTarget, out uint action, out bool enabled)
    {
        IGameObject? healTarget = OptionalTarget ?? SimpleTarget.Stack.AllyToHeal;
        bool ShieldCheck = !Config.SCH_ST_Heal_AldoquimOpts[0] || 
                           !HasStatusEffect(Buffs.Galvanize, healTarget, true) || 
                           HasStatusEffect(Buffs.EmergencyTactics);
        bool SageShieldCheck = !Config.SCH_ST_Heal_AldoquimOpts[1] ||
                               !HasStatusEffect(SGE.Buffs.EukrasianDiagnosis, healTarget, true) || 
                               !HasStatusEffect(SGE.Buffs.EukrasianPrognosis, healTarget, true) ||
                               HasStatusEffect(Buffs.EmergencyTactics);
        bool EmergencyAdlo = Config.SCH_ST_Heal_AldoquimOpts[2] && ActionReady(EmergencyTactics) &&
                             GetTargetHPPercent(healTarget, Config.SCH_ST_Heal_IncludeShields) <=
                             Config.SCH_ST_Heal_AdloquiumOption_Emergency;
        
        switch (i)
        {
            case 0:
                action = Lustrate;
                enabled = IsEnabled(CustomComboPreset.SCH_ST_Heal_Lustrate) && HasAetherflow;
                return Config.SCH_ST_Heal_LustrateOption;
            case 1:
                action = Excogitation;
                enabled = IsEnabled(CustomComboPreset.SCH_ST_Heal_Excogitation) && (HasAetherflow || HasStatusEffect(Buffs.Recitation));
                return Config.SCH_ST_Heal_ExcogitationOption;
            case 2:
                action = Protraction;
                enabled = IsEnabled(CustomComboPreset.SCH_ST_Heal_Protraction);
                return Config.SCH_ST_Heal_ProtractionOption;
            case 3:
                action = Aetherpact;
                enabled = IsEnabled(CustomComboPreset.SCH_ST_Heal_Aetherpact) && Gauge.FairyGauge >= Config.SCH_ST_Heal_AetherpactFairyGauge && IsOriginal(Aetherpact);
                return Config.SCH_ST_Heal_AetherpactOption;
            case 4:
                action = OriginalHook(Adloquium);
                enabled = IsEnabled(CustomComboPreset.SCH_ST_Heal_Adloquium) &&
                          ActionReady(OriginalHook(Adloquium)) &&
                          GetTargetHPPercent(healTarget, Config.SCH_ST_Heal_IncludeShields) <= Config.SCH_ST_Heal_AdloquiumOption &&
                          (EmergencyAdlo || ShieldCheck && SageShieldCheck);
                return Config.SCH_ST_Heal_AdloquiumOption;
        }

        enabled = false;
        action = 0;
        return 0;
    }
    #endregion
    
    #region Get Aoe Heals
    public static int GetMatchingConfigAoE(int i, out uint action, out bool enabled)
    {
        bool shieldCheck = GetPartyBuffPercent(Buffs.Galvanize) <= Config.SCH_AoE_Heal_SuccorShieldOption &&
                           GetPartyBuffPercent(SGE.Buffs.EukrasianPrognosis) <= Config.SCH_AoE_Heal_SuccorShieldOption;
        switch (i)
        {
            case 0:
                action = OriginalHook(WhisperingDawn);
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_WhisperingDawn) && HasPetPresent();
                return Config.SCH_AoE_Heal_WhisperingDawnOption;
            case 1:
                action = OriginalHook(FeyIllumination);
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_FeyIllumination) && HasPetPresent();
                return Config.SCH_AoE_Heal_FeyIlluminationOption;
            case 2:
                action = FeyBlessing;
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_FeyBlessing) && HasPetPresent();
                return Config.SCH_AoE_Heal_FeyBlessingOption;
            case 3:
                action = Consolation;
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_Consolation) && Gauge.SeraphTimer > 0;
                return Config.SCH_AoE_Heal_ConsolationOption;
            case 4:
                action = Seraphism;
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_Seraphism) && HasPetPresent();
                return Config.SCH_AoE_Heal_SeraphismOption;
            case 5:
                action = Indomitability;
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_Indomitability) && HasAetherflow;
                return Config.SCH_AoE_Heal_IndomitabilityOption;
            case 6:
                action = SummonSeraph;
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal_SummonSeraph) && HasPetPresent();
                return Config.SCH_AoE_Heal_SummonSeraph;
            
            case 7:
                action = OriginalHook(Succor);
                enabled = IsEnabled(CustomComboPreset.SCH_AoE_Heal) && shieldCheck;
                return 100; //Don't HP Check
        }

        enabled = false;
        action = 0;
        return 0;
    }
    #endregion
    
    #region Openers
    
    internal static SCHOpenerMaxLevel1 Opener1 = new();
    internal static WrathOpener Opener()
    {
        if (Opener1.LevelChecked)
            return Opener1;

        return WrathOpener.Dummy;
    }

    internal class SCHOpenerMaxLevel1 : WrathOpener
    {
        public override List<uint> OpenerActions { get; set; } =
        [
            Broil4,
            Biolysis,
            Dissipation,
            Broil4,
            ChainStratagem,
            Broil4,
            EnergyDrain,
            Broil4,
            EnergyDrain,
            Broil4,
            EnergyDrain,
            Broil4,
            Aetherflow,
            Broil4,
            BanefulImpaction,
            Broil4,
            EnergyDrain,
            Broil4,
            EnergyDrain,
            Broil4,
            EnergyDrain,
            Biolysis
        ];

        public override List<(int[] Steps, uint NewAction, Func<bool> Condition)> SubstitutionSteps { get; set; } =
        [
            ([3], Aetherflow, () => Config.SCH_ST_DPS_OpenerOption == 1),
            ([13], Dissipation, () => Config.SCH_ST_DPS_OpenerOption == 1)
        ];

        public override int MinOpenerLevel => 100;
        public override int MaxOpenerLevel => 109;

        internal override UserData ContentCheckConfig => Config.SCH_ST_DPS_OpenerContent;

        public override bool HasCooldowns()
        {
            if (!HasPetPresent())
                return false;

            if (!IsOffCooldown(ChainStratagem) ||
                !IsOffCooldown(Dissipation) ||
                !IsOffCooldown(Aetherflow))
                return false;

            return true;
        }
    }
    
    #endregion

    #region ID's

    public const byte ClassID = 26;
    public const byte JobID = 28;

    internal const uint

        // Heals
        Physick = 190,
        Adloquium = 185,
        Succor = 186,
        Concitation = 37013,
        Lustrate = 189,
        SacredSoil = 188,
        Indomitability = 3583,
        Excogitation = 7434,
        Consolation = 16546,
        Resurrection = 173,
        Protraction = 25867,
        Seraphism = 37014,
        Manifestation = 37015,
        Accession = 37016,

        // Offense
        Bio = 17864,
        Bio2 = 17865,
        Biolysis = 16540,
        Ruin = 17869,
        Ruin2 = 17870,
        Broil = 3584,
        Broil2 = 7435,
        Broil3 = 16541,
        Broil4 = 25865,
        EnergyDrain = 167,
        ArtOfWar = 16539,
        ArtOfWarII = 25866,
        BanefulImpaction = 37012,

        // Faerie
        SummonSeraph = 16545,
        SummonEos = 17215,
        WhisperingDawn = 16537,
        FeyIllumination = 16538,
        Dissipation = 3587,
        Aetherpact = 7437,
        DissolveUnion = 7869,
        FeyBlessing = 16543,

        // Other
        Aetherflow = 166,
        Recitation = 16542,
        ChainStratagem = 7436,
        DeploymentTactics = 3585,
        Expedient = 25868,
        EmergencyTactics = 3586;

    //Action Groups


    internal static class Buffs
    {
        internal const ushort
            Galvanize = 297,
            SacredSoil = 299,
            Dissipation = 791,
            EmergencyTactics = 792,
            Recitation = 1896,
            ImpactImminent = 3882;
    }

    internal static class Debuffs
    {
        internal const ushort
            Bio1 = 179,
            Bio2 = 189,
            Biolysis = 1895,
            ChainStratagem = 1221;
    }

    //Debuff Pairs of Actions and Debuff

    #endregion
}
