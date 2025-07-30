using Dalamud.Game.ClientState.Objects.Types;
using System.Linq;
using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using static WrathCombo.Combos.PvE.SGE.Config;
using Preset = WrathCombo.Combos.CustomComboPreset;
using EZ = ECommons.Throttlers.EzThrottler;
using TS = System.TimeSpan;

namespace WrathCombo.Combos.PvE;


internal partial class SGE : Healer
{
    #region Advanced ST
    internal class SGE_ST_DPS_AdvancedMode : CustomCombo
    {
        private static uint[] DosisActions => SGE_ST_DPS_Adv
            ? [Dosis2]
            : [.. DosisList.Keys];

        protected internal override Preset Preset => Preset.SGE_ST_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!DosisActions.Contains(actionID))
                return actionID;

            // Kardia Reminder
            if (IsEnabled(Preset.SGE_ST_DPS_Kardia) &&
                LevelChecked(Kardia) &&
                !HasStatusEffect(Buffs.Kardia) &&
                Target is not null)
                return Kardia
                    .Retarget(actionID, Target);

            // Opener for SGE
            if (IsEnabled(Preset.SGE_ST_DPS_Opener) &&
                Opener().FullOpener(ref actionID))
                return actionID;

            // Variant
            if (Variant.CanRampart(Preset.SGE_DPS_Variant_Rampart))
                return Variant.Rampart;

            //Occult skills
            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region Raidwide Feature
            if (RaidwideKerachole())
                return Kerachole;
            if (RaidwideHolos())
                return Holos;
            if (RaidwideEprognosis())
                return HasStatusEffect(Buffs.Eukrasia)
                    ? OriginalHook(Prognosis)
                    : Eukrasia;
            #endregion

            if (CanWeave() && !HasStatusEffect(Buffs.Eukrasia))
            {
                if (Variant.CanSpiritDart(Preset.SGE_DPS_Variant_SpiritDart))
                    return Variant.SpiritDart;

                // Lucid Dreaming
                if (IsEnabled(Preset.SGE_ST_DPS_Lucid) &&
                    Role.CanLucidDream(SGE_ST_DPS_Lucid))
                    return Role.LucidDreaming;

                // Addersgall Protection
                if (IsEnabled(Preset.SGE_ST_DPS_AddersgallProtect) &&
                    ActionReady(Druochole) && Addersgall >= SGE_ST_DPS_AddersgallProtect)
                    return Druochole
                        .RetargetIfEnabled(null, DosisActions);

                // Psyche
                if (IsEnabled(Preset.SGE_ST_DPS_Psyche) &&
                    ActionReady(Psyche) && InCombat())
                    return Psyche;

                // Rhizomata
                if (IsEnabled(Preset.SGE_ST_DPS_Rhizo) &&
                    ActionReady(Rhizomata) && Addersgall < SGE_ST_DPS_Rhizo)
                    return Rhizomata;

                //Soteria
                if (IsEnabled(Preset.SGE_ST_DPS_Soteria) &&
                    ActionReady(Soteria) && HasStatusEffect(Buffs.Kardia))
                    return Soteria;
            }

            if (HasBattleTarget() && !HasStatusEffect(Buffs.Eukrasia))
            {
                if (IsEnabled(Preset.SGE_ST_DPS_EDosis) &&
                    LevelChecked(Eukrasia) && InCombat() &&
                    !JustUsedOn(DosisList[OriginalHook(Dosis)].Eukrasian, CurrentTarget) &&
                    CanApplyStatus(CurrentTarget, DosisList[OriginalHook(Dosis)].Debuff))
                {
                    float refreshTimer = SGE_ST_DPS_EDosisRefresh;
                    int hpThreshold = SGE_ST_DPS_EDosisSubOption == 1 || !InBossEncounter() ? SGE_ST_DPS_EDosisOption : 0;

                    if (GetTargetHPPercent() > hpThreshold &&
                        ((DosisDebuff is null && DyskrasiaDebuff is null) ||
                         DosisDebuff?.RemainingTime <= refreshTimer ||
                         DyskrasiaDebuff?.RemainingTime <= refreshTimer))
                        return Eukrasia;
                }

                // Phlegma
                if (IsEnabled(Preset.SGE_ST_DPS_Phlegma) &&
                    InCombat() && InActionRange(OriginalHook(Phlegma)) &&
                    ActionReady(Phlegma))
                {
                    //If not enabled or not high enough level, follow slider
                    if ((!SGE_ST_DPS_Phlegma_Burst || !LevelChecked(Psyche)) &&
                        GetRemainingCharges(OriginalHook(Phlegma)) > SGE_ST_DPS_Phlegma)
                        return OriginalHook(Phlegma);

                    //If enabled and high enough level, burst
                    if (SGE_ST_DPS_Phlegma_Burst &&
                        ((GetCooldownRemainingTime(Psyche) > 40 && MaxPhlegma) ||
                         IsOffCooldown(Psyche) ||
                         JustUsed(Psyche, 5f)))
                        return OriginalHook(Phlegma);
                }

                // Movement Options
                if (IsEnabled(Preset.SGE_ST_DPS_Movement) &&
                    InCombat() && IsMoving())
                {
                    foreach(int priority in SGE_ST_DPS_Movement_Priority.Items.OrderBy(x => x))
                    {
                        int index = SGE_ST_DPS_Movement_Priority.IndexOf(priority);
                        if (CheckMovementConfigMeetsRequirements(index, out uint action))
                            return action;
                    }
                }
            }

            return actionID;
        }
    }
    #endregion
    
    #region Advanced AoE DPS
    internal class SGE_AoE_DPS_AdvancedMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_AoE_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (!DyskrasiaList.Contains(actionID) ||
                HasStatusEffect(Buffs.Eukrasia))
                return actionID;

            // Variant Rampart
            if (Variant.CanRampart(Preset.SGE_DPS_Variant_Rampart))
                return Variant.Rampart;

            //Occult skills
            if (OccultCrescent.ShouldUsePhantomActions())
                return OccultCrescent.BestPhantomAction();

            #region Raidwide Feature
            if (RaidwideKerachole())
                return Kerachole;
            if (RaidwideHolos())
                return Holos;
            if (RaidwideEprognosis())
                return HasStatusEffect(Buffs.Eukrasia)
                    ? OriginalHook(Prognosis)
                    : Eukrasia;
            #endregion

            if (CanWeave())
            {
                // Variant Spirit Dart
                if (Variant.CanSpiritDart(Preset.SGE_DPS_Variant_SpiritDart))
                    return Variant.SpiritDart;

                // Lucid Dreaming
                if (IsEnabled(Preset.SGE_AoE_DPS_Lucid) &&
                    Role.CanLucidDream(SGE_AoE_DPS_Lucid))
                    return Role.LucidDreaming;

                // Addersgall Protection
                if (IsEnabled(Preset.SGE_AoE_DPS_AddersgallProtect) &&
                    ActionReady(Druochole) && Addersgall >= SGE_AoE_DPS_AddersgallProtect)
                    return Druochole
                        .RetargetIfEnabled(null, OriginalHook(Dyskrasia));

                // Psyche
                if (IsEnabled(Preset.SGE_AoE_DPS_Psyche))
                    if (ActionReady(Psyche) && HasBattleTarget() &&
                        InActionRange(Psyche))
                        return Psyche;

                // Rhizomata
                if (IsEnabled(Preset.SGE_AoE_DPS_Rhizo) &&
                    ActionReady(Rhizomata) && Addersgall <= SGE_AoE_DPS_Rhizo)
                    return Rhizomata;

                //Soteria
                if (IsEnabled(Preset.SGE_AoE_DPS_Soteria) &&
                    ActionReady(Soteria) && HasStatusEffect(Buffs.Kardia))
                    return Soteria;
            }

            //Eukrasia for DoT
            if (IsEnabled(Preset.SGE_AoE_DPS_EDyskrasia) &&
                IsOffCooldown(Eukrasia) &&
                !JustUsedOn(EukrasianDyskrasia, CurrentTarget) && //AoE DoT can be slow to take affect, doesn't apply to target first before others
                TraitLevelChecked(Traits.OffensiveMagicMasteryII) &&
                HasBattleTarget() && InActionRange(Dyskrasia) &&
                CanApplyStatus(CurrentTarget, Debuffs.EukrasianDyskrasia) &&
                GetTargetHPPercent() > 25 &&
                ((DyskrasiaDebuff is null && DosisDebuff is null) ||
                 DyskrasiaDebuff?.RemainingTime <= 4 ||
                 DosisDebuff?.RemainingTime <= 4))
                return Eukrasia;

            //Phlegma
            if (IsEnabled(Preset.SGE_AoE_DPS_Phlegma) &&
                ActionReady(Phlegma) &&
                HasBattleTarget() &&
                InActionRange(OriginalHook(Phlegma)))
                return OriginalHook(Phlegma);

            //Toxikon
            if (IsEnabled(Preset.SGE_AoE_DPS_Toxikon) &&
                ActionReady(Toxikon) &&
                HasBattleTarget() && HasAddersting() &&
                InActionRange(OriginalHook(Toxikon)))
                return OriginalHook(Toxikon);

            //Pneuma
            if (IsEnabled(Preset.SGE_AoE_DPS_Pneuma) &&
                (SGE_AoE_DPS_Pneuma_SubOption == 0 || TargetIsBoss()) &&
                ActionReady(Pneuma) && HasBattleTarget() &&
                InActionRange(Pneuma))
                return Pneuma;

            return actionID;
        }
    }
    #endregion
    
    #region ST Healing
    internal class SGE_ST_Heal_AdvancedMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_ST_Heal;

        protected override uint Invoke(uint actionID)
        {
            IGameObject? healTarget = OptionalTarget ?? SimpleTarget.Stack.AllyToHeal;

            if (actionID is not Diagnosis)
                return actionID;

            #region Raidwide Feature
            if (RaidwideKerachole())
                return Kerachole;
            if (RaidwideHolos())
                return Holos;
            if (RaidwideEprognosis())
                return HasStatusEffect(Buffs.Eukrasia)
                    ? OriginalHook(Prognosis)
                    : Eukrasia;
            #endregion

            if (IsEnabled(Preset.SGE_ST_Heal_Esuna) &&
                ActionReady(Role.Esuna) &&
                GetTargetHPPercent(healTarget, SGE_ST_Heal_IncludeShields) >= SGE_ST_Heal_Esuna &&
                HasCleansableDebuff(healTarget))
                return Role.Esuna
                    .RetargetIfEnabled(OptionalTarget, Diagnosis);

            if (HasStatusEffect(Buffs.Eukrasia))
                return EukrasianDiagnosis
                    .RetargetIfEnabled(OptionalTarget, Diagnosis);

            if (IsEnabled(Preset.SGE_ST_Heal_Rhizomata) &&
                ActionReady(Rhizomata) && !HasAddersgall())
                return Rhizomata;

            if (IsEnabled(Preset.SGE_ST_Heal_Kardia) &&
                LevelChecked(Kardia) &&
                !HasStatusEffect(Buffs.Kardia) &&
                !HasStatusEffect(Buffs.Kardion, healTarget))
                return Kardia
                    .Retarget(actionID, Target);

            // Lucid Dreaming
            if (IsEnabled(Preset.SGE_ST_Heal_Lucid) &&
                Role.CanLucidDream(SGE_ST_Heal_LucidOption))
                return Role.LucidDreaming;

            for(int i = 0; i < SGE_ST_Heals_Priority.Count; i++)
            {
                int index = SGE_ST_Heals_Priority.IndexOf(i + 1);
                int config = GetMatchingConfigST(index, OptionalTarget, out uint spell, out bool enabled);

                if (enabled)
                    if (GetTargetHPPercent(healTarget, SGE_ST_Heal_IncludeShields) <= config &&
                        ActionReady(spell))
                        return spell
                            .RetargetIfEnabled(OptionalTarget, Diagnosis);
            }

            return actionID
                .RetargetIfEnabled(OptionalTarget, Diagnosis);
        }
    }
    #endregion
    
    #region AoE Healing
    internal class SGE_AoE_Heal_AdvancedMode : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_AoE_Heal;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Prognosis)
                return actionID;

            #region Raidwide Feature
            if (RaidwideKerachole())
                return Kerachole;
            if (RaidwideHolos())
                return Holos;
            if (RaidwideEprognosis())
                return HasStatusEffect(Buffs.Eukrasia)
                    ? OriginalHook(Prognosis)
                    : Eukrasia;
            #endregion

            //Zoe -> Pneuma like Eukrasia 
            if (SGE_AoE_Heal_ZoePneuma &&
                HasStatusEffect(Buffs.Zoe))
                return Pneuma;

            if (IsEnabled(Preset.SGE_AoE_Heal_EPrognosis) &&
                HasStatusEffect(Buffs.Eukrasia))
                return OriginalHook(Prognosis);

            if (IsEnabled(Preset.SGE_AoE_Heal_Rhizomata) &&
                ActionReady(Rhizomata) && !HasAddersgall())
                return Rhizomata;

            if (IsEnabled(Preset.SGE_AoE_Heal_Lucid) &&
                Role.CanLucidDream(SGE_AoE_Heal_LucidOption))
                return Role.LucidDreaming;

            float averagePartyHP = GetPartyAvgHPPercent();
            for(int i = 0; i < SGE_AoE_Heals_Priority.Count; i++)
            {
                int index = SGE_AoE_Heals_Priority.IndexOf(i + 1);
                int config = GetMatchingConfigAoE(index, out uint spell, out bool enabled);

                if (enabled && averagePartyHP <= config && ActionReady(spell))
                    return spell;
            }

            return actionID;
        }
    }
    #endregion
    
    #region Standalones
    internal class SGE_OverProtect : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_OverProtect;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Kerachole or Panhaima or Philosophia))
                return actionID;

            switch (actionID)
            {
                case Kerachole when IsEnabled(Preset.SGE_OverProtect_Kerachole) &&
                                    ActionReady(Kerachole) &&
                                    (HasStatusEffect(Buffs.Kerachole, anyOwner: true) ||
                                     IsEnabled(Preset.SGE_OverProtect_SacredSoil) && HasStatusEffect(SCH.Buffs.SacredSoil, anyOwner: true)):
                case Panhaima when IsEnabled(Preset.SGE_OverProtect_Panhaima) &&
                                   ActionReady(Panhaima) && HasStatusEffect(Buffs.Panhaima, anyOwner: true):
                    return SCH.SacredSoil;
                case Philosophia when IsEnabled(Preset.SGE_OverProtect_Philosophia) &&
                                      ActionReady(Philosophia) && HasStatusEffect(Buffs.Eudaimonia, anyOwner: true):
                    return SCH.Consolation;
                default:
                    return actionID;
            }
        }
    }
    internal class SGE_Raise : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Raise;

        protected override uint Invoke(uint actionID) =>
            actionID == Role.Swiftcast && IsOnCooldown(Role.Swiftcast)
                ? IsEnabled(Preset.SGE_Raise_Retarget)
                    ? Egeiro.Retarget(Role.Swiftcast,
                        SimpleTarget.Stack.AllyToRaise)
                    : Egeiro
                : actionID;
    }
    internal class SGE_ZoePneuma : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_ZoePneuma;

        protected override uint Invoke(uint actionID) =>
            actionID is Pneuma && ActionReady(Pneuma) && IsOffCooldown(Zoe)
                ? Zoe
                : actionID;
    }
    internal class SGE_Rhizo : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Rhizo;

        protected override uint Invoke(uint actionID) =>
            AddersgallList.Contains(actionID) &&
            ActionReady(Rhizomata) && !HasAddersgall() && IsOffCooldown(actionID)
                ? Rhizomata
                : actionID;
    }
    internal class SGE_Eukrasia : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Eukrasia;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Eukrasia || !HasStatusEffect(Buffs.Eukrasia))
                return actionID;

            IGameObject? healStack = SimpleTarget.Stack.AllyToHeal;

            if (SGE_Eukrasia_Mode == 0)
                return OriginalHook(Dosis);
            if (SGE_Eukrasia_Mode == 2)
                return OriginalHook(Prognosis);
            if (SGE_Eukrasia_Mode == 3)
                return OriginalHook(Dyskrasia);
            if (SGE_Eukrasia_Mode == 1)
                return IsEnabled(Preset.SGE_Retarget_EukrasianDiagnosis)
                    ? EukrasianDiagnosis.Retarget(Eukrasia, healStack)
                    : EukrasianDiagnosis;
            
            return actionID;
        }
    }
    internal class SGE_TauroDruo : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_TauroDruo;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Taurochole)
                return actionID;

            IGameObject? healStack = SimpleTarget.Stack.AllyToHeal;
            
            if (!LevelChecked(Taurochole) || IsOnCooldown(Taurochole))
                return IsEnabled(Preset.SGE_Retarget_Druochole)
                    ? Druochole.Retarget(Taurochole, healStack, true)
                    : Druochole;
            return IsEnabled(Preset.SGE_Retarget_Taurochole)
                    ? Taurochole.Retarget(healStack, true)
                    : Taurochole;
        }
    }
    internal class SGE_Kardia : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Kardia;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Soteria)
                return actionID;
            IGameObject? healStack = SimpleTarget.Stack.AllyToHeal;
            
            if (!HasStatusEffect(Buffs.Kardia) || IsOnCooldown(Soteria))
                return IsEnabled(Preset.SGE_Retarget_Kardia)
                    ? Kardia.Retarget(actionID, healStack, true)
                    : Kardia;
            
            return actionID;
        }
    }
    internal class SGE_Mit_ST : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Mit_ST;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Krasis)
                return actionID;
            
            IGameObject? healStack = SimpleTarget.Stack.AllyToHeal;

            if (ActionReady(Krasis))
                return IsEnabled(Preset.SGE_Retarget_Krasis)
                    ? Krasis.Retarget(healStack, dontCull: true)
                    : actionID;
            
            if (!HasStatusEffect(Buffs.EukrasianDiagnosis, healStack))
            {
                if (!HasStatusEffect(Buffs.Eukrasia))
                    return Eukrasia;
                
                return IsEnabled(Preset.SGE_Retarget_EukrasianDiagnosis)
                    ? EukrasianDiagnosis.Retarget(Krasis, healStack, true)
                    : EukrasianDiagnosis;
            }
            
            if (SGE_Mit_ST_Options[0] && !ActionReady(Krasis) &&
                ActionReady(Haima))
                return IsEnabled(Preset.SGE_Retarget_Haima)
                    ? Haima.Retarget(Krasis ,healStack, dontCull: true)
                    : Haima;
            
            if (SGE_Mit_ST_Options[1] && !ActionReady(Krasis) &&
                ActionReady(Taurochole) &&
                GetTargetHPPercent(healStack) <= SGE_Mit_ST_TaurocholeThreshold)
                return IsEnabled(Preset.SGE_Retarget_Taurochole)
                    ? Taurochole.Retarget(Krasis ,healStack, dontCull: true)
                    : Taurochole;
            
            return actionID;
        }
    }
    internal class SGE_Mit_AoE : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Mit_AoE;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Holos)
                return actionID;

            if (SGE_Mit_AoE_Options[0] &&
                ActionReady(Kerachole) &&
                !HasStatusEffect(Buffs.Kerachole, anyOwner: true) &&
                !HasStatusEffect(SCH.Buffs.SacredSoil, anyOwner: true))
                return Kerachole;
            
            if (SGE_Mit_AoE_Options[1] &&
                ActionReady(Philosophia))
                return Philosophia;

            if (GetPartyBuffPercent(Buffs.EukrasianPrognosis) <= SGE_Mit_AoE_PrognosisOption)
                return HasStatusEffect(Buffs.Eukrasia)
                    ? OriginalHook(Prognosis)
                    : Eukrasia;

            if (ActionReady(Holos) &&
                !HasStatusEffect(Buffs.Holosakos, anyOwner:true))
                return Holos;

            if (SGE_Mit_AoE_Options[2] &&
                ActionReady(Panhaima) &&
                !HasStatusEffect(Buffs.Panhaima, anyOwner:true))
                return Panhaima;

            return actionID;
        }
    }
    internal class SGE_Retarget : CustomCombo
    {
        protected internal override Preset Preset => Preset.SGE_Retarget;

        protected override uint Invoke(uint actionID)
        {
            if (!EZ.Throttle("SGERetargetingFeature", TS.FromSeconds(.1)))
                return actionID;
            
            IGameObject? healStack = SimpleTarget.Stack.AllyToHeal;

            if (IsEnabled(Preset.SGE_Retarget_Diagnosis))
                OriginalHook(Diagnosis).Retarget(healStack, true);
            
            if (IsEnabled(Preset.SGE_Retarget_EukrasianDiagnosis))
                EukrasianDiagnosis.Retarget(Diagnosis, healStack, true);
            
            if (IsEnabled(Preset.SGE_Retarget_Haima))
                Haima.Retarget(healStack, true);

            if (IsEnabled(Preset.SGE_Retarget_Druochole))
                Druochole.Retarget(healStack, true);

            if (IsEnabled(Preset.SGE_Retarget_Taurochole))
                Taurochole.Retarget(healStack, true);

            if (IsEnabled(Preset.SGE_Retarget_Krasis))
                Krasis.Retarget(healStack, true);

            if (IsEnabled(Preset.SGE_Retarget_Kardia))
                Kardia.Retarget(healStack, true);
            
            if (IsEnabled(Preset.SGE_Retarget_Icarus))
                Icarus.Retarget(SimpleTarget.Stack.MouseOver ?? SimpleTarget.HardTarget, dontCull: true);

            return actionID;
        }
    }
    #endregion
}
