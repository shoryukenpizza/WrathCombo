#region

using System.Linq;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;

// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

namespace WrathCombo.Combos.PvE;

internal partial class WHM : Healer
{
    #region DPS

    internal class WHM_ST_MainCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_ST_MainCombo;

        protected override uint Invoke(uint actionID)
        {
            #region Button Selection

            bool actionFound;

            if (Config.WHM_ST_MainCombo_Adv &&
                Config.WHM_ST_MainCombo_Adv_Actions.Count > 0)
            {
                var isStoneGlare = Config.WHM_ST_MainCombo_Adv_Actions[0] &&
                                   StoneGlareList.Contains(actionID);
                var isAero = Config.WHM_ST_MainCombo_Adv_Actions[1] &&
                             AeroList.ContainsKey(actionID);
                var isStone2 = Config.WHM_ST_MainCombo_Adv_Actions[2] &&
                               actionID is Stone2;
                actionFound = isStoneGlare || isAero || isStone2;
            }
            else
            {
                actionFound = StoneGlareList.Contains(actionID); //default handling
            }

            // If the action is not in the list, return the actionID
            if (!actionFound)
                return actionID;

            #endregion

            #region Opener

            if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_Opener))
                if (Opener().FullOpener(ref actionID))
                    return actionID;

            #endregion

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (!InCombat()) return actionID;
            
            #region Special Feature Raidwide

            if (RaidwideTemperance())
                return OriginalHook(Temperance);
            if (RaidwideAsylum())
                return Asylum.Retarget(actionID, SimpleTarget.Self);
            if (RaidwideLiturgyOfTheBell())
                return LiturgyOfTheBell.Retarget(actionID, SimpleTarget.Self);
           
            #endregion

            #region Weaves

            if (CanWeave())
            {
                if (Variant.CanRampart(CustomComboPreset.WHM_DPS_Variant_Rampart))
                    return Variant.Rampart;

                if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_PresenceOfMind) &&
                    ActionReady(PresenceOfMind) && !HasStatusEffect(Buffs.SacredSight))
                    return PresenceOfMind;

                if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_Assize) &&
                    ActionReady(Assize))
                    return Assize;

                if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_Lucid) &&
                    Role.CanLucidDream(Config.WHM_STDPS_Lucid))
                    return Role.LucidDreaming;

                if (Variant.CanSpiritDart(
                        CustomComboPreset.WHM_DPS_Variant_SpiritDart))
                    return Variant.SpiritDart;
            }

            #endregion

            #region GCDS and Casts

            // DoTs
            if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_DoT) && NeedsDoT())
                return OriginalHook(Aero);

            // Glare IV
            if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_GlareIV) &&
                HasStatusEffect(Buffs.SacredSight))
                return Glare4;

            // Lily Heal Overcap
            if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_LilyOvercap) &&
                ActionReady(AfflatusRapture) &&
                (FullLily || AlmostFullLily))
                return AfflatusRapture;

            // Blood Lily Spend
            if (IsEnabled(CustomComboPreset.WHM_ST_MainCombo_Misery_oGCD) &&
                BloodLilyReady)
                return AfflatusMisery;

            // Needed Because of Button Selection
            return OriginalHook(Stone1);

            #endregion
        }
    }

    internal class WHM_AoE_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_AoE_DPS;

        private static int AssizeCount =>
            ActionWatching.CombatActions.Count(x => x == Assize);

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Holy or Holy3))
                return actionID;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();


            #region Swiftcast Opener

            if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_SwiftHoly) &&
                ActionReady(Role.Swiftcast) &&
                AssizeCount == 0 && !IsMoving() && InCombat())
                return Role.Swiftcast;

            if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_SwiftHoly) &&
                WasLastAction(Role.Swiftcast))
                return actionID;

            #endregion
            
            #region Special Feature Raidwide

            if (RaidwideTemperance())
                return OriginalHook(Temperance);
            if (RaidwideAsylum())
                return Asylum.Retarget([Holy, Holy3], SimpleTarget.Self);
            if (RaidwideLiturgyOfTheBell())
                return LiturgyOfTheBell.Retarget([Holy, Holy3], SimpleTarget.Self);
           
            #endregion

            #region Weaves

            if (CanWeave() || IsMoving())
            {
                if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_Assize) &&
                    ActionReady(Assize))
                    return Assize;

                if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_PresenceOfMind) &&
                    ActionReady(PresenceOfMind) && !HasStatusEffect(Buffs.SacredSight))
                    return PresenceOfMind;

                if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_Lucid) &&
                    Role.CanLucidDream(Config.WHM_AoEDPS_Lucid))
                    return Role.LucidDreaming;

                if (Variant.CanRampart(CustomComboPreset.WHM_DPS_Variant_Rampart))
                    return Variant.Rampart;

                if (Variant.CanSpiritDart(CustomComboPreset
                        .WHM_DPS_Variant_SpiritDart))
                    return Variant.SpiritDart;
            }

            #endregion

            #region GCDS and Casts

            // Glare IV
            if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_GlareIV) &&
                HasStatusEffect(Buffs.SacredSight))
                return OriginalHook(Glare4);

            if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_LilyOvercap) &&
                ActionReady(AfflatusRapture) &&
                (FullLily || AlmostFullLily))
                return AfflatusRapture;

            if (IsEnabled(CustomComboPreset.WHM_AoE_DPS_Misery) &&
                BloodLilyReady &&
                HasBattleTarget())
                return AfflatusMisery;

            #endregion

            return actionID;
        }
    }

    #endregion

    #region Heals

    internal class WHM_ST_Heals : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_STHeals;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Cure)
                return actionID;

            #region Variables

            var healTarget = OptionalTarget ?? SimpleTarget.Stack.AllyToHeal;

            var canThinAir = LevelChecked(ThinAir) &&
                             !HasStatusEffect(Buffs.ThinAir) &&
                             GetRemainingCharges(ThinAir) >
                             Config.WHM_STHeals_ThinAir;


            var canRegen = ActionReady(Regen) &&
                             !JustUsedOn(Regen, healTarget) &&
                             GetStatusEffectRemainingTime(Buffs.Regen, healTarget)
                             <= Config.WHM_STHeals_RegenTimer && //Refresh Time Threshold
                             GetTargetHPPercent(healTarget, Config.WHM_STHeals_IncludeShields)
                             >= Config.WHM_STHeals_RegenHPLower &&
                             GetTargetHPPercent(healTarget, Config.WHM_STHeals_IncludeShields)
                             <= Config.WHM_STHeals_RegenHPUpper;  
            #endregion
            
            #region Special Feature Raidwide

            if (RaidwideTemperance())
                return OriginalHook(Temperance);
            if (RaidwideAsylum())
                return Asylum.Retarget(Cure, SimpleTarget.Self);
            if (RaidwideLiturgyOfTheBell())
                return LiturgyOfTheBell.Retarget(Cure, SimpleTarget.Self);
           
            #endregion

            #region Priority Cleansing

            if (IsEnabled(CustomComboPreset.WHM_STHeals_Esuna) &&
                ActionReady(Role.Esuna) &&
                GetTargetHPPercent(
                    healTarget, Config.WHM_STHeals_IncludeShields) >=
                Config.WHM_STHeals_Esuna &&
                HasCleansableDebuff(healTarget))
                return Role.Esuna
                    .RetargetIfEnabled(OptionalTarget, Cure);

            #endregion

            if (IsEnabled(CustomComboPreset.WHM_STHeals_Lucid) && CanWeave() &&
                Role.CanLucidDream(Config.WHM_STHeals_Lucid))
                return Role.LucidDreaming;
            
            //Priority List
            for(int i = 0; i < Config.WHM_ST_Heals_Priority.Count; i++)
            {
                int index = Config.WHM_ST_Heals_Priority.IndexOf(i + 1);
                int config = GetMatchingConfigST(index, OptionalTarget, out uint spell, out bool enabled);

                if (enabled)
                {
                    if (GetTargetHPPercent(healTarget, Config.WHM_STHeals_IncludeShields) <= config &&
                        ActionReady(spell))
                        return spell.RetargetIfEnabled(OptionalTarget, Cure);
                }
            }
            if (LevelChecked(Cure2))
                return IsEnabled(CustomComboPreset.WHM_STHeals_ThinAir) && canThinAir
                    ? ThinAir
                    : Cure2.RetargetIfEnabled(OptionalTarget, Cure);
            
            return Cure.RetargetIfEnabled(OptionalTarget);
        }
    }

    internal class WHM_AoEHeals : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_AoEHeals;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Medica1)
                return actionID;

            #region Variables

            var canThinAir = LevelChecked(ThinAir) &&
                             !HasStatusEffect(Buffs.ThinAir) &&
                             GetRemainingCharges(ThinAir) >
                             Config.WHM_AoEHeals_ThinAir;
            #endregion
            
            #region Special Feature Raidwide

            if (RaidwideTemperance())
                return OriginalHook(Temperance);
            if (RaidwideAsylum())
                return Asylum.Retarget(Medica1, SimpleTarget.Self);
            if (RaidwideLiturgyOfTheBell())
                return LiturgyOfTheBell.Retarget(Medica1 ,SimpleTarget.Self);
           
            #endregion
            
            if (IsEnabled(CustomComboPreset.WHM_AoEHeals_Lucid) &&
                CanWeave() &&
                Role.CanLucidDream(Config.WHM_AoEHeals_Lucid))
                return Role.LucidDreaming;

            // Blood Overcap
            if (IsEnabled(CustomComboPreset.WHM_AoEHeals_Misery) &&
                gauge.BloodLily == 3)
                return AfflatusMisery;
            
            //Priority List
            for(int i = 0; i < Config.WHM_AoE_Heals_Priority.Count; i++)
            {
                int index = Config.WHM_AoE_Heals_Priority.IndexOf(i + 1);
                int config = GetMatchingConfigAoE(index, out uint spell, out bool enabled);
                
                if (enabled && GetPartyAvgHPPercent() <= config && ActionReady(spell))
                    return IsEnabled(CustomComboPreset.WHM_AoEHeals_ThinAir) && canThinAir && spell is Cure3 or Medica2 or Medica3?
                        ThinAir:
                        spell.RetargetIfEnabled(OptionalTarget, Medica1);
            }
           

            return actionID;
        }
    }

    #endregion

    #region Small Features

    internal class WHM_SolaceMisery : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_SolaceMisery;

        protected override uint Invoke(uint actionID) =>
            actionID is AfflatusSolace && gauge.BloodLily == 3
                ? AfflatusMisery
                : actionID;
    }

    internal class WHM_RaptureMisery : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_RaptureMisery;

        protected override uint Invoke(uint actionID) =>
            actionID is AfflatusRapture && gauge.BloodLily == 3
                ? AfflatusMisery
                : actionID;
    }

    internal class WHM_CureSync : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_CureSync;

        protected override uint Invoke(uint actionID) =>
            actionID is Cure2 && !LevelChecked(Cure2)
                ? Cure
                : actionID;
    }

    internal class WHM_Raise : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } =
            CustomComboPreset.WHM_Raise;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != Role.Swiftcast)
                return actionID;

            var canThinAir = !HasStatusEffect(Buffs.ThinAir) && ActionReady
                (ThinAir);

            if (HasStatusEffect(Role.Buffs.Swiftcast))
                return IsEnabled(CustomComboPreset.WHM_ThinAirRaise) && canThinAir
                    ? ThinAir
                    : IsEnabled(CustomComboPreset.WHM_Raise_Retarget)
                        ? Raise.Retarget(Role.Swiftcast,
                            SimpleTarget.Stack.AllyToRaise)
                        : Raise;

            return actionID;
        }
    }

    #endregion
}
