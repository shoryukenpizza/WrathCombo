#region

using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Statuses;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;
using WrathCombo.Extensions;

// ReSharper disable AccessToStaticMemberViaDerivedType
// ReSharper disable UnusedType.Global
// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace

#endregion

namespace WrathCombo.Combos.PvE;

internal partial class WHM : Healer
{
    #region Simple DPS

    internal class WHM_ST_Simple_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_ST_Simple_DPS;

        protected override uint Invoke(uint actionID)
        {
            var actionFound = StoneGlareList.Contains(actionID);
           
            if (!actionFound)
                return actionID;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            if (!InCombat()) return actionID;
           
            #region Weaves

            if (CanWeave())
            {
                if (Variant.CanRampart(CustomComboPreset.WHM_DPS_Variant_Rampart))
                    return Variant.Rampart;

                if (ActionReady(PresenceOfMind) && !HasStatusEffect(Buffs.SacredSight))
                    return PresenceOfMind;

                if (ActionReady(Assize))
                    return Assize;

                if (Role.CanLucidDream(7500))
                    return Role.LucidDreaming;

                if (Variant.CanSpiritDart(
                        CustomComboPreset.WHM_DPS_Variant_SpiritDart))
                    return Variant.SpiritDart;
            }

            #endregion

            #region GCDS and Casts

            // DoTs
            if (NeedsDoT())
                return OriginalHook(Aero);

            // Glare IV
            if (HasStatusEffect(Buffs.SacredSight))
                return Glare4;

            // Lily Heal Overcap
            if (ActionReady(AfflatusRapture) &&
                (FullLily || AlmostFullLily))
                return AfflatusRapture;

            // Blood Lily Spend
            if (BloodLilyReady)
                return AfflatusMisery;

            
            return actionID;

            #endregion
        }
    }

    internal class WHM_AoE_Simple_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_AoE_Simple_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Holy or Holy3))
                return actionID;

            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region Weaves

            if (CanWeave() || IsMoving())
            {
                if (ActionReady(Assize))
                    return Assize;

                if (ActionReady(PresenceOfMind) && !HasStatusEffect(Buffs.SacredSight))
                    return PresenceOfMind;

                if (Role.CanLucidDream(7500))
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
            if (HasStatusEffect(Buffs.SacredSight))
                return OriginalHook(Glare4);

            if (ActionReady(AfflatusRapture) &&
                (FullLily || AlmostFullLily))
                return AfflatusRapture;

            if (BloodLilyReady &&
                HasBattleTarget())
                return AfflatusMisery;

            #endregion

            return actionID;
        }
    }

    #endregion
    
    #region DPS

    internal class WHM_ST_MainCombo : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_ST_MainCombo;

        protected override uint Invoke(uint actionID)
        {
            #region Button Selection

            bool actionFound = Config.WHM_ST_MainCombo_Actions == 0 && StoneGlareList.Contains(actionID) ||
                               Config.WHM_ST_MainCombo_Actions == 1 && AeroList.ContainsKey(actionID) ||
                               Config.WHM_ST_MainCombo_Actions == 2 && actionID is Stone2;
            
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
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_AoE_DPS;

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
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_STHeals;

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
            
            // Divine Caress
            if (IsEnabled(CustomComboPreset.WHM_STHeals_Temperance) && HasStatusEffect(Buffs.DivineGrace) &&
                (!Config.WHM_STHeals_TemperanceOptions[1] || !InBossEncounter()) &&
                (!Config.WHM_STHeals_TemperanceOptions[0] || CanWeave()))
                return OriginalHook(Temperance);
            
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
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_AoEHeals;

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
                int config = GetMatchingConfigAoE(index, OptionalTarget, out uint spell, out bool enabled);
                
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
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_SolaceMisery;

        protected override uint Invoke(uint actionID) =>
            actionID is AfflatusSolace && gauge.BloodLily == 3
                ? AfflatusMisery
                : actionID.RetargetIfEnabled(OptionalTarget);
    }

    internal class WHM_RaptureMisery : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_RaptureMisery;

        protected override uint Invoke(uint actionID) =>
            actionID is AfflatusRapture && gauge.BloodLily == 3
                ? AfflatusMisery
                : actionID;
    }

    internal class WHM_CureSync : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_CureSync;

        protected override uint Invoke(uint actionID) =>
            actionID is Cure2 && !LevelChecked(Cure2)
                ? Cure.RetargetIfEnabled(OptionalTarget, Cure2)
                : actionID.RetargetIfEnabled(OptionalTarget);
    }

    internal class WHM_Raise : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Raise;

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
    
    
    internal class WHM_Asylum : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Asylum;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Asylum)
                return actionID;
            
            var asylumTarget =
                (Config.WHM_AsylumOptions[0]
                    ? SimpleTarget.HardTarget.IfHostile()
                    : null) ??
                (Config.WHM_AsylumOptions[1]
                    ? SimpleTarget.HardTarget.IfFriendly()
                    : null) ??
                SimpleTarget.Self;

            if (Config.WHM_AsylumOptions[2] &&
                ActionReady(OriginalHook(Temperance)) &&
                IsOnCooldown(Asylum))
                return OriginalHook(Temperance);

            return Asylum.Retarget(asylumTarget);
        }
    }
    internal class WHM_Aquaveil : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Aquaveil;

        protected override uint Invoke(uint actionID)
        {
            IGameObject? healTarget = OptionalTarget ?? SimpleTarget.Stack.AllyToHeal;
            Status? benisonShield = GetStatusEffect(Buffs.DivineBenison, healTarget);
            
            if (actionID is not Aquaveil)
                return actionID;

            if (ActionReady(Aquaveil))
                return actionID.RetargetIfEnabled(OptionalTarget);
            
            if (Config.WHM_AquaveilOptions[0] &&
                ActionReady(DivineBenison) &&
                benisonShield == null)
                return DivineBenison.RetargetIfEnabled(OptionalTarget, Aquaveil);

            if (Config.WHM_AquaveilOptions[1] &&
                ActionReady(Tetragrammaton) && 
                GetTargetHPPercent(healTarget) < Config.WHM_Aquaveil_TetraThreshold)
                return Tetragrammaton.RetargetIfEnabled(OptionalTarget, Aquaveil);

            return actionID;
        }
    }
    internal class WHM_LiturgyOfTheBell : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_LiturgyOfTheBell;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not LiturgyOfTheBell)
                return actionID;
            
            var bellTarget =
                (Config.WHM_LiturgyOfTheBellOptions[0]
                    ? SimpleTarget.HardTarget.IfHostile()
                    : null) ??
                (Config.WHM_LiturgyOfTheBellOptions[1]
                    ? SimpleTarget.HardTarget.IfFriendly()
                    : null) ??
                SimpleTarget.Self;
                
            return LiturgyOfTheBell.Retarget(bellTarget);
        }
    }
    internal class WHM_Cure3 : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Cure3;

        protected override uint Invoke(uint actionID) =>
            actionID is not Cure3
                ? actionID
                : actionID.RetargetIfEnabled(OptionalTarget);
    }
    
    internal class WHM_Benediction : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Benediction;

        protected override uint Invoke(uint actionID) =>
            actionID is not Benediction
                ? actionID
                : actionID.RetargetIfEnabled(OptionalTarget);
    }
    
    internal class WHM_Tetragrammaton : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Tetragrammaton;

        protected override uint Invoke(uint actionID) =>
            actionID is not Tetragrammaton
                ? actionID
                : actionID.RetargetIfEnabled(OptionalTarget);
    }
    
    internal class WHM_Regen : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_Regen;

        protected override uint Invoke(uint actionID) =>
            actionID is not Regen
                ? actionID
                : actionID.RetargetIfEnabled(OptionalTarget);
    }
    
    internal class WHM_DivineBenison : CustomCombo
    {
        protected internal override CustomComboPreset Preset => CustomComboPreset.WHM_DivineBenison;

        protected override uint Invoke(uint actionID) =>
            actionID is not DivineBenison
                ? actionID
                : actionID.RetargetIfEnabled(OptionalTarget);
    }
    #endregion
}
