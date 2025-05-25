using Dalamud.Game.ClientState.JobGauge.Types;
using WrathCombo.CustomComboNS;
using WrathCombo.Extensions;

namespace WrathCombo.Combos.PvE;

internal partial class PCT : Caster
{
    internal class PCT_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireInRed)
                return actionID;

            PCTGauge gauge = GetJobGauge<PCTGauge>();
            
            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // ScenicMuse

                if (ScenicMuse.LevelChecked() &&
                    gauge.LandscapeMotifDrawn &&
                    gauge.WeaponMotifDrawn &&
                    IsOffCooldown(ScenicMuse))
                {
                    return OriginalHook(ScenicMuse);
                }

                // LivingMuse

                if (LivingMuse.LevelChecked() &&
                    gauge.CreatureMotifDrawn &&
                    (!(gauge.MooglePortraitReady || gauge.MadeenPortraitReady) ||
                     GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)))
                {
                    if (HasCharges(OriginalHook(LivingMuse)))
                    {
                        if (!ScenicMuse.LevelChecked() ||
                            GetCooldown(ScenicMuse).CooldownRemaining > GetCooldownChargeRemainingTime(LivingMuse))
                        {
                            return OriginalHook(LivingMuse);
                        }
                    }
                }

                // SteelMuse

                if (SteelMuse.LevelChecked() &&
                    !HasStatusEffect(Buffs.HammerTime) &&
                    gauge.WeaponMotifDrawn &&
                    HasCharges(OriginalHook(SteelMuse)) &&
                    (GetCooldown(SteelMuse).CooldownRemaining < GetCooldown(ScenicMuse).CooldownRemaining ||
                     GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                     !ScenicMuse.LevelChecked()))
                {
                    return OriginalHook(SteelMuse);
                }

                // MogoftheAges

                if (MogoftheAges.LevelChecked() &&
                    (gauge.MooglePortraitReady || gauge.MadeenPortraitReady) &&
                    IsOffCooldown(OriginalHook(MogoftheAges)) &&
                    (GetCooldownRemainingTime(StarryMuse) >= 60 || !ScenicMuse.LevelChecked()))
                {
                    return OriginalHook(MogoftheAges);
                }

                // Swiftcast

                if (IsMoving() &&
                    IsOffCooldown(Role.Swiftcast) &&
                    Role.Swiftcast.LevelChecked() &&
                    !HasStatusEffect(Buffs.HammerTime) &&
                    gauge.Paint < 1 &&
                    (!gauge.CreatureMotifDrawn || !gauge.WeaponMotifDrawn || !gauge.LandscapeMotifDrawn))
                {
                    return Role.Swiftcast;
                }

                // SubtractivePalette

                if (SubtractivePalette.LevelChecked() &&
                    !HasStatusEffect(Buffs.SubtractivePalette) &&
                    !HasStatusEffect(Buffs.MonochromeTones))
                {
                    if (HasStatusEffect(Buffs.SubtractiveSpectrum) || gauge.PalleteGauge >= 50)
                    {
                        return SubtractivePalette;
                    }
                }
            }

            // Swiftcast Motifs
            if (HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (!gauge.CreatureMotifDrawn && CreatureMotif.LevelChecked() && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(CreatureMotif);
                if (!gauge.WeaponMotifDrawn && HammerMotif.LevelChecked() && !HasStatusEffect(Buffs.HammerTime) && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(HammerMotif);
                if (!gauge.LandscapeMotifDrawn && LandscapeMotif.LevelChecked() && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(LandscapeMotif);
            }

            // IsMoving logic
            if (IsMoving() && InCombat())
            {
                if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (CometinBlack.LevelChecked() && gauge.Paint >= 1 && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (HasStatusEffect(Buffs.RainbowBright) || (HasStatusEffect(Buffs.RainbowBright) && GetStatusEffectRemainingTime(Buffs.StarryMuse) <= 3f))
                    return RainbowDrip;

                if (HolyInWhite.LevelChecked() && gauge.Paint >= 1)
                    return OriginalHook(HolyInWhite);
            }

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn)
                    return OriginalHook(LandscapeMotif);

                if (CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn)
                    return OriginalHook(CreatureMotif);

                if (WeaponMotif.LevelChecked() && !gauge.WeaponMotifDrawn && !HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(WeaponMotif);
            }

            // Burst
            if (HasStatusEffect(Buffs.StarryMuse))
            {

                if (CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                    return CometinBlack;

                if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime) && !HasStatusEffect(Buffs.Starstruck))
                    return OriginalHook(HammerStamp);

                if (HasStatusEffect(Buffs.Starstruck) || (HasStatusEffect(Buffs.Starstruck) && GetStatusEffectRemainingTime(Buffs.Starstruck) <= 3f))
                    return StarPrism;

                if (HasStatusEffect(Buffs.RainbowBright) || (HasStatusEffect(Buffs.RainbowBright) && GetStatusEffectRemainingTime(Buffs.StarryMuse) <= 3f))
                    return RainbowDrip;

            }

            if (HasStatusEffect(Buffs.RainbowBright) && !HasStatusEffect(Buffs.StarryMuse))
                return RainbowDrip;

            if (CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint && GetCooldownRemainingTime(StarryMuse) > 30f)
                return OriginalHook(CometinBlack);

            if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                return OriginalHook(HammerStamp);

            if (!HasStatusEffect(Buffs.StarryMuse))
            {
                // LandscapeMotif

                if (LandscapeMotif.LevelChecked() &&
                    !gauge.LandscapeMotifDrawn &&
                    GetCooldownRemainingTime(ScenicMuse) <= 20)
                {
                    return OriginalHook(LandscapeMotif);
                }

                // CreatureMotif

                if (CreatureMotif.LevelChecked() &&
                    !gauge.CreatureMotifDrawn &&
                    (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                {
                    return OriginalHook(CreatureMotif);
                }

                // WeaponMotif

                if (WeaponMotif.LevelChecked() &&
                    !HasStatusEffect(Buffs.HammerTime) &&
                    !gauge.WeaponMotifDrawn &&
                    (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                {
                    return OriginalHook(WeaponMotif);
                }
            }

            if (Role.CanLucidDream(6500))
                return Role.LucidDreaming;

            if (BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardinCyan);

            return actionID;
        }
    }

    internal class PCT_ST_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_ST_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireInRed)
                return actionID;

            #region Enables and Configs
           
            int creatureStop = Config.PCT_ST_CreatureStop;
            int landscapeStop = Config.PCT_ST_LandscapeStop;
            int weaponStop = Config.PCT_ST_WeaponStop;

            bool prepullEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_PrePullMotifs);
            bool openerEnabled = IsEnabled(CustomComboPreset.PCT_ST_Advanced_Openers);
            bool burstPhaseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_Burst_Phase);
            bool noTargetMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_NoTargetMotifs);
            bool scenicMuseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_ScenicMuse);
            bool livingMuseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_LivingMuse);
            bool steelMuseEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SteelMuse);
            bool portraitEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MogOfTheAges);
            bool paletteEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SubtractivePalette);
            bool lucidDreamingEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_LucidDreaming);
            bool swiftMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SwiftMotifs);
            bool hammerMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MovementOption_HammerStampCombo);
            bool cometMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MovementOption_CometinBlack);
            bool holyMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_MovementOption_HolyInWhite);
            bool swiftcastMotifMovementEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_SwitfcastOption);
            bool landscapeMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_LandscapeMotif);
            bool creatureMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_CreatureMotif);
            bool weaponMotifEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_WeaponMotif);
            bool starPrismEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_StarPrism);
            bool rainbowDripEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_RainbowDrip);
            bool cometEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_CometinBlack);
            bool hammerEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_HammerStampCombo);
            bool blizzardComboEnabled = IsEnabled(CustomComboPreset.PCT_ST_AdvancedMode_BlizzardInCyan);




            #endregion

            #region Variants
            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;
            #endregion

            #region Prepull
            if ((prepullEnabled && !InCombat()) || (noTargetMotifEnabled && InCombat() && CurrentTarget == null))
            {
                if (CreatureMotifReady)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady)
                    return OriginalHook(LandscapeMotif);
            }
            #endregion

            #region Opener
           
            if (openerEnabled && Opener().FullOpener(ref actionID))
                return actionID;

            #endregion

            #region Burst Window
            if (burstPhaseEnabled && InCombat() && (ScenicCD <= 5 || HasStatusEffect(Buffs.StarryMuse)))
                return BurstWindow(actionID);

            #endregion

            #region OGCD
            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // ScenicMuse
                if (scenicMuseEnabled && ScenicMuseReady && !burstPhaseEnabled)
                    return OriginalHook(ScenicMuse);
                    
                // LivingMuse
                if (livingMuseEnabled && LivingMuseReady && 
                    (!PortraitReady || GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)) &&
                    (!LevelChecked(ScenicMuse) || ScenicCD > GetCooldownChargeRemainingTime(LivingMuse)))
                    return OriginalHook(LivingMuse);
                        
                // SteelMuse
                if (steelMuseEnabled && SteelMuseReady &&
                    (SteelCD < ScenicCD || GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                    !LevelChecked(ScenicMuse)))
                    return OriginalHook(SteelMuse);
                    
                // Portrait Mog or Madeen
                if (portraitEnabled && PortraitReady && 
                    IsOffCooldown(OriginalHook(MogoftheAges)) && 
                    (ScenicCD >= 60 || !LevelChecked(ScenicMuse)))
                    return OriginalHook(MogoftheAges);
                    
                // SubtractivePalette
                if (paletteEnabled && PaletteReady)
                    return SubtractivePalette;
                        
                //LucidDreaming
                if (lucidDreamingEnabled && Role.CanLucidDream(Config.PCT_ST_AdvancedMode_LucidOption))
                    return Role.LucidDreaming;
            }
            #endregion

            #region Swiftcast Motifs
            // Swiftcast Motifs
            if (swiftMotifEnabled && HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);
            }

            #endregion

            #region Moving
            if (IsMoving() && InCombat())
            {
                if (HyperPhantasiaMovementPaint())
                    return HasStatusEffect(Buffs.MonochromeTones) ? OriginalHook(CometinBlack) : OriginalHook(HolyInWhite);

                if (hammerMovementEnabled && HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (cometMovementEnabled && CometinBlack.LevelChecked() && HasPaint && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (rainbowDripEnabled && HasStatusEffect(Buffs.RainbowBright))
                    return RainbowDrip;

                if (holyMovementEnabled && HolyInWhite.LevelChecked() && HasPaint & !HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(HolyInWhite);

                if (swiftcastMotifMovementEnabled && ActionReady(Role.Swiftcast) &&
                    (CreatureMotifReady || WeaponMotifReady || LandscapeMotifReady))
                    return Role.Swiftcast;
            }

            #endregion

            #region Pre_Burst Motifs

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (landscapeMotifEnabled && LandscapeMotifReady && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);

                if (creatureMotifEnabled && CreatureMotifReady && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);

                if (weaponMotifEnabled && WeaponMotifReady && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
            }

            #endregion

            #region GCDs

            //StarPrism
            if (starPrismEnabled && HasStatusEffect(Buffs.Starstruck))
                return StarPrism;

            //RainbowDrip
            if (rainbowDripEnabled && HasStatusEffect(Buffs.RainbowBright))
                return RainbowDrip;

            //Comet in Black
            if (cometEnabled && CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                return OriginalHook(CometinBlack);

            //Hammer Stamp Combo
            if (hammerEnabled && ActionReady(OriginalHook(HammerStamp)) && ScenicCD > 10)
                return OriginalHook(HammerStamp);
            
            // LandscapeMotif
            if (landscapeMotifEnabled && LandscapeMotifReady && GetTargetHPPercent() > landscapeStop && 
                GetCooldownRemainingTime(ScenicMuse) <= 20)
                return OriginalHook(LandscapeMotif);

            // CreatureMotif
            if (creatureMotifEnabled && CreatureMotifReady &&  GetTargetHPPercent() > creatureStop &&
                (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                return OriginalHook(CreatureMotif);                  

            // WeaponMotif
            if (weaponMotifEnabled && WeaponMotifReady && GetTargetHPPercent() > weaponStop && 
                (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                return OriginalHook(WeaponMotif);            

            //Subtractive Combo
            if (blizzardComboEnabled && BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardinCyan);

            return actionID;
            #endregion
        }
    }

    internal class PCT_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_AoE_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireIIinRed)
                return actionID;

            PCTGauge gauge = GetJobGauge<PCTGauge>();

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;

            // Prepull logic

            if (!InCombat() || (InCombat() && CurrentTarget == null))
            {
                if (CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn)
                    return OriginalHook(CreatureMotif);
                if (WeaponMotif.LevelChecked() && !gauge.WeaponMotifDrawn && !HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(WeaponMotif);
                if (LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(LandscapeMotif);
            }

            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // LivingMuse

                if (LivingMuse.LevelChecked() &&
                    gauge.CreatureMotifDrawn &&
                    (!(gauge.MooglePortraitReady || gauge.MadeenPortraitReady) ||
                     GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)))
                {
                    if (HasCharges(OriginalHook(LivingMuse)))
                    {
                        if (!ScenicMuse.LevelChecked() ||
                            GetCooldown(ScenicMuse).CooldownRemaining > GetCooldownChargeRemainingTime(LivingMuse))
                        {
                            return OriginalHook(LivingMuse);
                        }
                    }
                }

                // ScenicMuse

                if (ScenicMuse.LevelChecked() &&
                    gauge.LandscapeMotifDrawn &&
                    gauge.WeaponMotifDrawn &&
                    IsOffCooldown(ScenicMuse))
                {
                    return OriginalHook(ScenicMuse);
                }

                // SteelMuse

                if (SteelMuse.LevelChecked() &&
                    !HasStatusEffect(Buffs.HammerTime) &&
                    gauge.WeaponMotifDrawn &&
                    HasCharges(OriginalHook(SteelMuse)) &&
                    (GetCooldown(SteelMuse).CooldownRemaining < GetCooldown(ScenicMuse).CooldownRemaining ||
                     GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                     !ScenicMuse.LevelChecked()))
                {
                    return OriginalHook(SteelMuse);
                }

                // MogoftheAges

                if (MogoftheAges.LevelChecked() &&
                    (gauge.MooglePortraitReady || gauge.MadeenPortraitReady) &&
                    (IsOffCooldown(OriginalHook(MogoftheAges)) || !ScenicMuse.LevelChecked()))
                {
                    return OriginalHook(MogoftheAges);
                }

                if (IsMoving() &&
                    IsOffCooldown(Role.Swiftcast) &&
                    Role.Swiftcast.LevelChecked() &&
                    !HasStatusEffect(Buffs.HammerTime) &&
                    gauge.Paint < 1 &&
                    (!gauge.CreatureMotifDrawn || !gauge.WeaponMotifDrawn || !gauge.LandscapeMotifDrawn))
                {
                    return Role.Swiftcast;
                }

                // Subtractive Palette
                if (SubtractivePalette.LevelChecked() &&
                    !HasStatusEffect(Buffs.SubtractivePalette) &&
                    !HasStatusEffect(Buffs.MonochromeTones))
                {
                    if (HasStatusEffect(Buffs.SubtractiveSpectrum) || gauge.PalleteGauge >= 50)
                        return SubtractivePalette;
                }
            }

            if (HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (!gauge.CreatureMotifDrawn && CreatureMotif.LevelChecked() && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(CreatureMotif);
                if (!gauge.WeaponMotifDrawn && HammerMotif.LevelChecked() && !HasStatusEffect(Buffs.HammerTime) && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(HammerMotif);
                if (!gauge.LandscapeMotifDrawn && LandscapeMotif.LevelChecked() && !HasStatusEffect(Buffs.StarryMuse))
                    return OriginalHook(LandscapeMotif);
            }

            if (IsMoving() && InCombat())
            {
                if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (CometinBlack.LevelChecked() && gauge.Paint >= 1 && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (HasStatusEffect(Buffs.RainbowBright) || (HasStatusEffect(Buffs.RainbowBright) && GetStatusEffectRemainingTime(Buffs.StarryMuse) < 3))
                    return RainbowDrip;

                if (HolyInWhite.LevelChecked() && gauge.Paint >= 1)
                    return OriginalHook(HolyInWhite);

            }

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn)
                    return OriginalHook(LandscapeMotif);

                if (CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn)
                    return OriginalHook(CreatureMotif);

                if (WeaponMotif.LevelChecked() && !gauge.WeaponMotifDrawn && !HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(WeaponMotif);
            }

            // Burst
            if (HasStatusEffect(Buffs.StarryMuse))
            {
                // Check for CometInBlack
                if (CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                    return CometinBlack;

                // Check for HammerTime
                if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime) && !HasStatusEffect(Buffs.Starstruck))
                    return OriginalHook(HammerStamp);

                // Check for Starstruck
                if (HasStatusEffect(Buffs.Starstruck) || (HasStatusEffect(Buffs.Starstruck) && GetStatusEffectRemainingTime(Buffs.Starstruck) < 3))
                    return StarPrism;

                // Check for RainbowBright
                if (HasStatusEffect(Buffs.RainbowBright) || (HasStatusEffect(Buffs.RainbowBright) && GetStatusEffectRemainingTime(Buffs.StarryMuse) < 3))
                    return RainbowDrip;
            }

            if (HasStatusEffect(Buffs.RainbowBright) && !HasStatusEffect(Buffs.StarryMuse))
                return RainbowDrip;

            if (CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint && GetCooldownRemainingTime(StarryMuse) > 60)
                return OriginalHook(CometinBlack);

            if (HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                return OriginalHook(HammerStamp);

            if (!HasStatusEffect(Buffs.StarryMuse))
            {
                if (LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn && GetCooldownRemainingTime(ScenicMuse) <= 20)
                    return OriginalHook(LandscapeMotif);

                if (CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn && (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                    return OriginalHook(CreatureMotif);

                if (WeaponMotif.LevelChecked() && !HasStatusEffect(Buffs.HammerTime) && !gauge.WeaponMotifDrawn && (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                    return OriginalHook(WeaponMotif);
            }
            //Saves one Charge of White paint for movement/Black paint.
            if (HolyInWhite.LevelChecked() && gauge.Paint >= 2)
                return OriginalHook(HolyInWhite);

            if (Role.CanLucidDream(6500))
                return Role.LucidDreaming;

            if (BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardIIinCyan);
            return actionID;
        }
    }

    internal class PCT_AoE_AdvancedMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.PCT_AoE_AdvancedMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not FireIIinRed)
                return actionID;

            PCTGauge gauge = GetJobGauge<PCTGauge>();
            int creatureStop = Config.PCT_AoE_CreatureStop;
            int landscapeStop = Config.PCT_AoE_LandscapeStop;
            int weaponStop = Config.PCT_AoE_WeaponStop;

            // Variant Cure
            if (Variant.CanCure(CustomComboPreset.PCT_Variant_Cure, Config.PCT_VariantCure))
                return Variant.Cure;

            // Variant Rampart
            if (Variant.CanRampart(CustomComboPreset.PCT_Variant_Rampart, WeaveTypes.SpellWeave))
                return Variant.Rampart;

            // Prepull logic
            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_PrePullMotifs))
            {
                if (!InCombat() || (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_NoTargetMotifs) && InCombat() && CurrentTarget == null))
                {
                    if (CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn)
                        return OriginalHook(CreatureMotif);
                    if (WeaponMotif.LevelChecked() && !gauge.WeaponMotifDrawn && !HasStatusEffect(Buffs.HammerTime))
                        return OriginalHook(WeaponMotif);
                    if (LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn && !HasStatusEffect(Buffs.StarryMuse))
                        return OriginalHook(LandscapeMotif);
                }
            }

            // General Weaves
            if (InCombat() && CanSpellWeave())
            {
                // LivingMuse
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LivingMuse))
                {
                    if (LivingMuse.LevelChecked() &&
                        gauge.CreatureMotifDrawn &&
                        (!(gauge.MooglePortraitReady || gauge.MadeenPortraitReady) ||
                         GetRemainingCharges(LivingMuse) == GetMaxCharges(LivingMuse)))
                    {
                        if (HasCharges(OriginalHook(LivingMuse)))
                        {
                            if (!ScenicMuse.LevelChecked() ||
                                GetCooldown(ScenicMuse).CooldownRemaining > GetCooldownChargeRemainingTime(LivingMuse))
                            {
                                return OriginalHook(LivingMuse);
                            }
                        }
                    }
                }

                // ScenicMuse
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_ScenicMuse))
                {
                    if (ScenicMuse.LevelChecked() &&
                        gauge.LandscapeMotifDrawn &&
                        gauge.WeaponMotifDrawn &&
                        IsOffCooldown(ScenicMuse))
                    {
                        return OriginalHook(ScenicMuse);
                    }
                }

                // SteelMuse
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SteelMuse))
                {
                    if (SteelMuse.LevelChecked() &&
                        !HasStatusEffect(Buffs.HammerTime) &&
                        gauge.WeaponMotifDrawn &&
                        HasCharges(OriginalHook(SteelMuse)) &&
                        (GetCooldown(SteelMuse).CooldownRemaining < GetCooldown(ScenicMuse).CooldownRemaining ||
                         GetRemainingCharges(SteelMuse) == GetMaxCharges(SteelMuse) ||
                         !ScenicMuse.LevelChecked()))
                    {
                        return OriginalHook(SteelMuse);
                    }
                }

                // MogoftheAges
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MogOfTheAges))
                {
                    if (MogoftheAges.LevelChecked() &&
                        (gauge.MooglePortraitReady || gauge.MadeenPortraitReady) &&
                        (IsOffCooldown(OriginalHook(MogoftheAges)) || !ScenicMuse.LevelChecked()))
                    {
                        return OriginalHook(MogoftheAges);
                    }
                }

                // Subtractive Palette
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SubtractivePalette) &&
                    SubtractivePalette.LevelChecked() &&
                    !HasStatusEffect(Buffs.SubtractivePalette) &&
                    !HasStatusEffect(Buffs.MonochromeTones))
                {
                    if (HasStatusEffect(Buffs.SubtractiveSpectrum) || gauge.PalleteGauge >= 50)
                        return SubtractivePalette;
                }
            }

            if (HasStatusEffect(Role.Buffs.Swiftcast))
            {
                if (!gauge.CreatureMotifDrawn && CreatureMotif.LevelChecked() && !HasStatusEffect(Buffs.StarryMuse) && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);
                if (!gauge.WeaponMotifDrawn && HammerMotif.LevelChecked() && !HasStatusEffect(Buffs.HammerTime) && !HasStatusEffect(Buffs.StarryMuse) && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(HammerMotif);
                if (!gauge.LandscapeMotifDrawn && LandscapeMotif.LevelChecked() && !HasStatusEffect(Buffs.StarryMuse) && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);
            }

            if (IsMoving() && InCombat())
            {
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MovementOption_HammerStampCombo) && HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MovementOption_CometinBlack) && CometinBlack.LevelChecked() && gauge.Paint >= 1 && HasStatusEffect(Buffs.MonochromeTones))
                    return OriginalHook(CometinBlack);

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_Burst_RainbowDrip))
                {
                    if (HasStatusEffect(Buffs.RainbowBright) || (HasStatusEffect(Buffs.RainbowBright) && GetStatusEffectRemainingTime(Buffs.StarryMuse) < 3))
                        return RainbowDrip;
                }

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_MovementOption_HolyInWhite) && HolyInWhite.LevelChecked() && gauge.Paint >= 1)
                    return OriginalHook(HolyInWhite);

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_SwitfcastOption) && ActionReady(Role.Swiftcast) &&
                    ((LevelChecked(CreatureMotif) && !gauge.CreatureMotifDrawn) ||
                     (LevelChecked(WeaponMotif) && !gauge.WeaponMotifDrawn) ||
                     (LevelChecked(LandscapeMotif) && !gauge.LandscapeMotifDrawn)))
                    return Role.Swiftcast;
            }

            //Prepare for Burst
            if (GetCooldownRemainingTime(ScenicMuse) <= 20)
            {
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LandscapeMotif) && LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn && GetTargetHPPercent() > landscapeStop)
                    return OriginalHook(LandscapeMotif);

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_CreatureMotif) && CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn && GetTargetHPPercent() > creatureStop)
                    return OriginalHook(CreatureMotif);

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_WeaponMotif) && WeaponMotif.LevelChecked() && !gauge.WeaponMotifDrawn && !HasStatusEffect(Buffs.HammerTime) && GetTargetHPPercent() > weaponStop)
                    return OriginalHook(WeaponMotif);
            }

            // Burst
            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_Burst_Phase) && HasStatusEffect(Buffs.StarryMuse))
            {
                // Check for CometInBlack
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_Burst_CometInBlack) && CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint)
                    return CometinBlack;

                // Check for HammerTime
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_Burst_HammerCombo) && HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime) && !HasStatusEffect(Buffs.Starstruck))
                    return OriginalHook(HammerStamp);

                // Check for Starstruck
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_Burst_StarPrism))
                {
                    if (HasStatusEffect(Buffs.Starstruck) || (HasStatusEffect(Buffs.Starstruck) && GetStatusEffectRemainingTime(Buffs.Starstruck) < 3))
                        return StarPrism;
                }

                // Check for RainbowBright
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_Burst_RainbowDrip))
                {
                    if (HasStatusEffect(Buffs.RainbowBright) || (HasStatusEffect(Buffs.RainbowBright) && GetStatusEffectRemainingTime(Buffs.StarryMuse) < 3))
                        return RainbowDrip;
                }
            }

            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_HolyinWhite) && !HasStatusEffect(Buffs.StarryMuse) && !HasStatusEffect(Buffs.MonochromeTones))
            {
                if (gauge.Paint > Config.PCT_AoE_AdvancedMode_HolyinWhiteOption ||
                    (Config.PCT_AoE_AdvancedMode_HolyinWhiteOption == 5 && gauge.Paint == 5 && !HasStatusEffect(Buffs.HammerTime) &&
                     (HasStatusEffect(Buffs.RainbowBright) || WasLastSpell(AeroIIinGreen) || WasLastSpell(StoneIIinYellow))))
                    return OriginalHook(HolyInWhite);
            }

            if (HasStatusEffect(Buffs.RainbowBright) && !HasStatusEffect(Buffs.StarryMuse))
                return RainbowDrip;

            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_CometinBlack) && CometinBlack.LevelChecked() && HasStatusEffect(Buffs.MonochromeTones) && HasPaint && GetCooldownRemainingTime(StarryMuse) > 60)
                return OriginalHook(CometinBlack);

            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_HammerStampCombo) && HammerStamp.LevelChecked() && HasStatusEffect(Buffs.HammerTime))
                return OriginalHook(HammerStamp);

            if (!HasStatusEffect(Buffs.StarryMuse))
            {
                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LandscapeMotif) && GetTargetHPPercent() > landscapeStop)
                {
                    if (LandscapeMotif.LevelChecked() && !gauge.LandscapeMotifDrawn && GetCooldownRemainingTime(ScenicMuse) <= 20)
                        return OriginalHook(LandscapeMotif);
                }

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_CreatureMotif) && GetTargetHPPercent() > creatureStop)
                {
                    if (CreatureMotif.LevelChecked() && !gauge.CreatureMotifDrawn && (HasCharges(LivingMuse) || GetCooldownChargeRemainingTime(LivingMuse) <= 8))
                        return OriginalHook(CreatureMotif);
                }

                if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_WeaponMotif) && GetTargetHPPercent() > weaponStop)
                {
                    if (WeaponMotif.LevelChecked() && !HasStatusEffect(Buffs.HammerTime) && !gauge.WeaponMotifDrawn && (HasCharges(SteelMuse) || GetCooldownChargeRemainingTime(SteelMuse) <= 8))
                        return OriginalHook(WeaponMotif);
                }
            }

            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_LucidDreaming) && Role.CanLucidDream(Config.PCT_ST_AdvancedMode_LucidOption))
                return Role.LucidDreaming;

            if (IsEnabled(CustomComboPreset.PCT_AoE_AdvancedMode_BlizzardInCyan) && BlizzardIIinCyan.LevelChecked() && HasStatusEffect(Buffs.SubtractivePalette))
                return OriginalHook(BlizzardIIinCyan);
            return actionID;
        }
    }

    internal class CombinedAetherhues : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.CombinedAetherhues;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (FireInRed or FireIIinRed))
                return actionID;

            int choice = Config.CombinedAetherhueChoices;

            if (actionID == FireInRed && choice is 0 or 1)
            {
                if (HasStatusEffect(Buffs.SubtractivePalette))
                    return OriginalHook(BlizzardinCyan);
            }

            if (actionID == FireIIinRed && choice is 0 or 2)
            {
                if (HasStatusEffect(Buffs.SubtractivePalette))
                    return OriginalHook(BlizzardIIinCyan);
            }

            return actionID;
        }
    }

    internal class CombinedMotifs : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.CombinedMotifs;

        protected override uint Invoke(uint actionID)
        {
            PCTGauge gauge = GetJobGauge<PCTGauge>();

            if (actionID == CreatureMotif)
            {
                if ((Config.CombinedMotifsMog && gauge.MooglePortraitReady) || (Config.CombinedMotifsMadeen && gauge.MadeenPortraitReady && IsOffCooldown(OriginalHook(MogoftheAges))))
                    return OriginalHook(MogoftheAges);

                if (gauge.CreatureMotifDrawn)
                    return OriginalHook(LivingMuse);
            }

            if (actionID == WeaponMotif)
            {
                if (Config.CombinedMotifsWeapon && HasStatusEffect(Buffs.HammerTime))
                    return OriginalHook(HammerStamp);

                if (gauge.WeaponMotifDrawn)
                    return OriginalHook(SteelMuse);
            }

            if (actionID == LandscapeMotif)
            {
                if (Config.CombinedMotifsLandscape && HasStatusEffect(Buffs.Starstruck))
                    return OriginalHook(StarPrism);

                if (gauge.LandscapeMotifDrawn)
                    return OriginalHook(ScenicMuse);
            }

            return actionID;
        }
    }

    internal class CombinedPaint : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.CombinedPaint;

        protected override uint Invoke(uint actionID)
        {
            if (actionID != HolyInWhite)
                return actionID;
            if (HasStatusEffect(Buffs.MonochromeTones))
                return CometinBlack;
            return actionID;
        }
    }
}
