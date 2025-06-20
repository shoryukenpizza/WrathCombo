using WrathCombo.Core;
using WrathCombo.CustomComboNS;
using WrathCombo.Data;


namespace WrathCombo.Combos.PvE;

internal partial class RDM : Caster
{

    #region Simple Modes
    
    internal class RDM_ST_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_ST_SimpleMode;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Jolt or Jolt2 or Jolt3))
                return actionID;
            
            #region Variants
            if (Variant.CanCure(CustomComboPreset.RDM_Variant_Cure, Config.RDM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.RDM_Variant_Rampart))
                return Variant.Rampart;
            #endregion
            
            #region OGCDs
            if (CanSpellWeave() && !ActionWatching.HasDoubleWeaved())
            {
                //Gap Closer
                if (ActionReady(Corpsacorps) && (HasEnoughMana || CanMagickedSwordplay) && !InMeleeRange()) 
                    return Corpsacorps;
                 
                if (ActionReady(Manafication) && (EmboldenCD <= 5 || HasEmbolden) && !CanPrefulgence) 
                    return Manafication;
                
                if (ActionReady(Embolden) && !HasEmbolden && CanDelayedWeave()) 
                    return Embolden;
                
                if (ActionReady(ContreSixte)) 
                    return ContreSixte;
                
                if (ActionReady(Fleche)) 
                    return Fleche;
                
                if (IsEnabled(CustomComboPreset.RDM_ST_Engagement) && CanEngagement)  
                    return Engagement;
                
                if (IsEnabled(CustomComboPreset.RDM_ST_Corpsacorps) && CanCorps && InMeleeRange())
                    return Corpsacorps;
                
                if (CanPrefulgence)
                    return Prefulgence;
               
                if (CanViceOfThorns)
                    return ViceOfThorns;
               
                if (Role.CanLucidDream(8000))
                    return Role.LucidDreaming;
               
                if (CanAcceleration || CanAccelerationMovement) 
                    return Acceleration;
               
                if (CanSwiftcast || CanSwiftcastMovement)
                    return Role.Swiftcast;
            }
            #endregion
            
            #region Melee Combo and Finishers 
            // Verholy, Verflare, Scorch, Resolution
            if (ComboAction is Scorch or Verholy or Verflare) 
                return actionID;
            
            if (HasManaStacks) 
                return UseHolyFlare(actionID);
            
            //Melee Combo 
            if (IsEnabled(CustomComboPreset.RDM_ST_MeleeCombo) && InMeleeRange())
            {
                if (ComboAction is Zwerchhau or EnchantedZwerchhau) 
                    return EnchantedRedoublement;
                
                if (ComboAction is Riposte or EnchantedRiposte)
                    return EnchantedZwerchhau;
                
                if (ActionReady(EnchantedRiposte) && InMeleeRange() && !HasDualcast && !HasAccelerate && !HasSwiftcast &&
                    (HasEnoughMana || CanMagickedSwordplay)) 
                    return EnchantedRiposte;
            }
            
            #endregion
            
            #region GCD Casts

            //Verthunder and Veraero
            if (CanInstantCast)
                return UseInstantCastST(actionID);
            
            if (UseGrandImpact()) 
                return GrandImpact;
            
            if (UseVerStone())
                return Verstone;
            
            if (UseVerFire())
                return Verfire;
            
            return actionID;
            
            #endregion
        }
    }
    
    internal class RDM_AoE_SimpleMode : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_AoE_SimpleMode;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Scatter or Impact))
                return actionID;
            
            #region Variants
            if (Variant.CanCure(CustomComboPreset.RDM_Variant_Cure, Config.RDM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.RDM_Variant_Rampart))
                return Variant.Rampart;
            #endregion

            #region OGCDs
            if (CanSpellWeave() && !ActionWatching.HasDoubleWeaved())
            {
                //Gap Closer Option
                if (ActionReady(Corpsacorps) && (HasEnoughMana || CanMagickedSwordplay) && !InMeleeRange()) 
                    return Corpsacorps;
                 
                if (ActionReady(Manafication) && (EmboldenCD <= 5 || HasEmbolden) && !CanPrefulgence) 
                    return Manafication;
                
                if (ActionReady(Embolden) && !HasEmbolden && CanDelayedWeave()) 
                    return Embolden;
                
                if (ActionReady(ContreSixte)) 
                    return ContreSixte;
                
                if (ActionReady(Fleche)) 
                    return Fleche;
                
                if (CanEngagement)  
                    return Engagement;
                
                if (CanCorps && GetTargetDistance() == 0)
                    return Corpsacorps;
                
                if (CanPrefulgence)
                    return Prefulgence;
                
                if (CanViceOfThorns)
                    return ViceOfThorns;
                
                if (Role.CanLucidDream(8000))
                    return Role.LucidDreaming;
                
                if (CanAcceleration && GetRemainingCharges(Acceleration) > 1 || CanAccelerationMovement) 
                    return Acceleration;
                
                if (CanSwiftcast || CanSwiftcastMovement)
                    return Role.Swiftcast;
            }
            #endregion
            
            #region Melee Combo and Finishers 
            //Replaces Scatter, Needs no enable
            if (ComboAction is Scorch or Verholy or Verflare) 
                return actionID;
            
            //VerHoly and Verflare
            if (HasManaStacks) 
                return UseHolyFlare(actionID);
            
            //Melee Combo 
            if (IsEnabled(CustomComboPreset.RDM_AoE_MeleeCombo))
            {
                if (ActionReady(Moulinet) && HasBattleTarget() && GetTargetDistance() < 8 && 
                    (CanMagickedSwordplay ||HasEnoughMana || ComboAction is EnchantedMoulinet or Moulinet or EnchantedMoulinetDeux))
                    return OriginalHook(Moulinet);
                
                if (!ActionReady(Moulinet) && InMeleeRange())
                {
                    if (ComboAction is Zwerchhau or EnchantedZwerchhau) 
                        return EnchantedRedoublement;
                    if (ComboAction is Riposte or EnchantedRiposte)
                        return EnchantedZwerchhau;
                    if (ActionReady(EnchantedRiposte) && !HasDualcast && !HasAccelerate && !HasSwiftcast && HasEnoughMana) 
                        return EnchantedRiposte; 
                }
                
            }
            
            #endregion
            
            #region GCD Casts
            
            if (UseGrandImpact()) 
                return GrandImpact;
            
            if (!CanInstantCast)
                return UseThunderAeroAoE(actionID);
            
            return actionID;
            
            #endregion
           
        }
    }
    #endregion
    
    #region Advanced Modes

    internal class RDM_ST_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_ST_DPS;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Jolt or Jolt2 or Jolt3))
                return actionID;
            
            #region Opener
            // OPENER
            if (IsEnabled(CustomComboPreset.RDM_Balance_Opener) && HasBattleTarget() &&
                Opener().FullOpener(ref actionID)) 
                return actionID;
            #endregion
            
            #region Variants
            if (Variant.CanCure(CustomComboPreset.RDM_Variant_Cure, Config.RDM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.RDM_Variant_Rampart))
                return Variant.Rampart;
            #endregion

            #region OGCDs
            if (CanSpellWeave() && !ActionWatching.HasDoubleWeaved())
            {
                //Gap Closer Option
                if (IsEnabled(CustomComboPreset.RDM_ST_MeleeCombo_GapCloser) && 
                    ActionReady(Corpsacorps) && (HasEnoughMana || CanMagickedSwordplay) && !InMeleeRange()) 
                    return Corpsacorps;
                 
                if (IsEnabled(CustomComboPreset.RDM_ST_Manafication) && ActionReady(Manafication) && (EmboldenCD <= 5 || HasEmbolden) && !CanPrefulgence) 
                    return Manafication;
                
                if (IsEnabled(CustomComboPreset.RDM_ST_Embolden) && ActionReady(Embolden) && !HasEmbolden && CanDelayedWeave()) 
                    return Embolden;
                
                //ContreSixte Option
                if (IsEnabled(CustomComboPreset.RDM_ST_ContreSixte) && ActionReady(ContreSixte)) 
                    return ContreSixte;
                
                //Flèche Option
                if (IsEnabled(CustomComboPreset.RDM_ST_Fleche) && ActionReady(Fleche)) 
                    return Fleche;
                
                if (IsEnabled(CustomComboPreset.RDM_ST_Engagement) && CanEngagement)  
                    return Engagement;
                
                if (IsEnabled(CustomComboPreset.RDM_ST_Corpsacorps) && CanCorps && 
                    (IsNotEnabled(CustomComboPreset.RDM_ST_Corpsacorps_MeleeOnly) || GetTargetDistance() == 0))
                    return Corpsacorps;
                
                //Prefulgence Option
                if (IsEnabled(CustomComboPreset.RDM_ST_Prefulgence) && CanPrefulgence)
                    return Prefulgence;
                
                //Vice of Thorns Option
                if (IsEnabled(CustomComboPreset.RDM_ST_ViceOfThorns) && CanViceOfThorns)
                    return ViceOfThorns;
                
                //Lucid Dreaming Option
                if (IsEnabled(CustomComboPreset.RDM_ST_Lucid) && Role.CanLucidDream(Config.RDM_ST_Lucid_Threshold))
                    return Role.LucidDreaming;
                
                //Acceleration and Movement Option
                if (IsEnabled(CustomComboPreset.RDM_ST_Acceleration) && 
                    (CanAcceleration && GetRemainingCharges(Acceleration) > Config.RDM_ST_Acceleration_Charges || 
                    CanAccelerationMovement && IsEnabled(CustomComboPreset.RDM_ST_Acceleration_Movement))) 
                    return Acceleration;
                
                //Swiftcast Option
                if (IsEnabled(CustomComboPreset.RDM_ST_Swiftcast) && 
                    (!IsEnabled(CustomComboPreset.RDM_ST_SwiftcastMovement) && CanSwiftcast || CanSwiftcastMovement))
                    return Role.Swiftcast;
                
                //   IsEnabled(CustomComboPreset.) && 
            }
            #endregion
            
            #region Melee Combo and Finishers 
            //Replaces Jolt, Needs no enable
            if (ComboAction is Scorch or Verholy or Verflare) 
                return actionID;
            
            //VerHoly and Verflare
            if (IsEnabled(CustomComboPreset.RDM_ST_HolyFlare) && HasManaStacks) 
                return UseHolyFlare(actionID);
            
            //Melee Combo 
            if (IsEnabled(CustomComboPreset.RDM_ST_MeleeCombo) )
            {
                //Melee Combo Hit 2/3 with Range Check option
                if (InMeleeRange() || IsEnabled(CustomComboPreset.RDM_ST_MeleeCombo_MeleeCheck))
                {
                    if (ComboAction is Zwerchhau or EnchantedZwerchhau) 
                        return EnchantedRedoublement;
                    if (ComboAction is Riposte or EnchantedRiposte)
                        return EnchantedZwerchhau;
                }
                
                //Riposte Option for Manual Starting
                if (IsEnabled(CustomComboPreset.RDM_ST_MeleeCombo_IncludeRiposte) && ActionReady(EnchantedRiposte) && 
                    InMeleeRange() && !HasDualcast && !HasAccelerate && !HasSwiftcast &&
                    (HasEnoughMana || CanMagickedSwordplay)) 
                    return EnchantedRiposte;
            }
            
            #endregion
            
            #region GCD Casts

            //Verthunder and Veraero
            if (IsEnabled(CustomComboPreset.RDM_ST_ThunderAero) && CanInstantCast)
                return UseInstantCastST(actionID);
            
            //Replaces Jolt, Needs no enable
            if (UseGrandImpact()) 
                return GrandImpact;

            //Verstone and Verfire
            if (IsEnabled(CustomComboPreset.RDM_ST_FireStone))
            {
                if (UseVerStone())
                    return Verstone;
                if (UseVerFire())
                    return Verfire;
            }
            return actionID;
            
            #endregion
        }
    }
   
    internal class RDM_AoE_DPS : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_AoE_DPS;
        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Scatter or Impact))
                return actionID;

            #region Variants
            if (Variant.CanCure(CustomComboPreset.RDM_Variant_Cure, Config.RDM_VariantCure))
                return Variant.Cure;

            if (Variant.CanRampart(CustomComboPreset.RDM_Variant_Rampart))
                return Variant.Rampart;
            #endregion

            #region OGCDs
            if (CanSpellWeave() && !ActionWatching.HasDoubleWeaved())
            {
                //Gap Closer Option
                if (IsEnabled(CustomComboPreset.RDM_AoE_MeleeCombo_GapCloser) && 
                    ActionReady(Corpsacorps) && (HasEnoughMana || CanMagickedSwordplay) && !InMeleeRange()) 
                    return Corpsacorps;
                 
                if (IsEnabled(CustomComboPreset.RDM_AoE_Manafication) && ActionReady(Manafication) && (EmboldenCD <= 5 || HasEmbolden) && !CanPrefulgence) 
                    return Manafication;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Embolden) && ActionReady(Embolden) && !HasEmbolden && CanDelayedWeave()) 
                    return Embolden;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_ContreSixte) && ActionReady(ContreSixte)) 
                    return ContreSixte;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Fleche) && ActionReady(Fleche)) 
                    return Fleche;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Engagement) && CanEngagement)  
                    return Engagement;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Corpsacorps) && CanCorps && 
                    (IsNotEnabled(CustomComboPreset.RDM_AoE_Corpsacorps_MeleeOnly) || GetTargetDistance() == 0))
                    return Corpsacorps;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Prefulgence) && CanPrefulgence)
                    return Prefulgence;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_ViceOfThorns) && CanViceOfThorns)
                    return ViceOfThorns;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Lucid) && Role.CanLucidDream(Config.RDM_AoE_Lucid_Threshold))
                    return Role.LucidDreaming;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Acceleration) && 
                    (CanAcceleration && GetRemainingCharges(Acceleration) > Config.RDM_AoE_Acceleration_Charges || 
                    CanAccelerationMovement && IsEnabled(CustomComboPreset.RDM_AoE_Acceleration_Movement))) 
                    return Acceleration;
                
                if (IsEnabled(CustomComboPreset.RDM_AoE_Swiftcast) && 
                    (!IsEnabled(CustomComboPreset.RDM_AoE_SwiftcastMovement) && CanSwiftcast || CanSwiftcastMovement))
                    return Role.Swiftcast;
            }
            #endregion
            
            #region Melee Combo and Finishers 
            //Replaces Scatter, Needs no enable
            if (ComboAction is Scorch or Verholy or Verflare) 
                return actionID;
            
            //VerHoly and Verflare
            if (IsEnabled(CustomComboPreset.RDM_AoE_HolyFlare) && HasManaStacks) 
                return UseHolyFlare(actionID);
            
            //Melee Combo 
            if (IsEnabled(CustomComboPreset.RDM_AoE_MeleeCombo))
            {
                if (ActionReady(Moulinet) && 
                    (IsNotEnabled(CustomComboPreset.RDM_AoE_MeleeCombo_Target) && !HasBattleTarget() || HasBattleTarget() && GetTargetDistance() < 8) && 
                    (CanMagickedSwordplay ||HasEnoughMana || ComboAction is EnchantedMoulinet or Moulinet or EnchantedMoulinetDeux))
                    return OriginalHook(Moulinet);
                
                if (!ActionReady(Moulinet) && InMeleeRange())
                {
                    if (ComboAction is Zwerchhau or EnchantedZwerchhau) 
                        return EnchantedRedoublement;
                    if (ComboAction is Riposte or EnchantedRiposte)
                        return EnchantedZwerchhau;
                    if (ActionReady(EnchantedRiposte) && !HasDualcast && !HasAccelerate && !HasSwiftcast && HasEnoughMana) 
                        return EnchantedRiposte; 
                }
                
            }
            
            #endregion
            
            #region GCD Casts
            
            if (UseGrandImpact()) 
                return GrandImpact;
            
            if (IsEnabled(CustomComboPreset.RDM_AoE_ThunderAero) && !CanInstantCast)
                return UseThunderAeroAoE(actionID);
            
            return actionID;
            
            #endregion
        }
    }

    #endregion
    
    #region Standalone Features
    internal class RDM_VariantVerCure : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_Variant_Cure2;

        protected override uint Invoke(uint actionID) =>
            actionID is Vercure && Variant.CanCure(Preset, 100)
                ? Variant.Cure
                : actionID;
    }
    
    internal class RDM_Verraise : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_Raise;
        protected override uint Invoke(uint actionID)
        {
            /*
            RDM_Verraise
            Swiftcast combos to Verraise when:
            -Swiftcast is on cooldown.
            -Swiftcast is available, but we we have Dualcast (Dualcasting Verraise)
            Using this variation other than the alternate feature style, as Verraise is level 63
            and swiftcast is unlocked way earlier and in theory, on a hotbar somewhere
            */
            
            if (actionID != Role.Swiftcast)
                return actionID;

            if (Variant.CanRaise(CustomComboPreset.RDM_Variant_Raise))
                return IsEnabled(CustomComboPreset.RDM_Raise_Retarget)
                    ? Variant.Raise.Retarget(Role.Swiftcast,
                        SimpleTarget.Stack.AllyToRaise)
                    : Variant.Raise;

            if (LevelChecked(Verraise))
            {
                bool schwifty = HasStatusEffect(Role.Buffs.Swiftcast);
                if (schwifty || HasStatusEffect(Buffs.Dualcast))
                    return IsEnabled(CustomComboPreset.RDM_Raise_Retarget)
                        ? Verraise.Retarget(Role.Swiftcast,
                            SimpleTarget.Stack.AllyToRaise)
                        : Verraise;
                if (IsEnabled(CustomComboPreset.RDM_Raise_Vercure) &&
                    !schwifty &&
                    ActionReady(Vercure) &&
                    IsOnCooldown(Role.Swiftcast))
                    return IsEnabled(CustomComboPreset.RDM_Raise_Retarget)
                        ? Vercure.Retarget(Role.Swiftcast,
                            SimpleTarget.Stack.AllyToHeal)
                        : Vercure;
            }

            // Else we just exit normally and return Swiftcast
            return actionID;
        }
    }

    internal class RDM_CorpsDisplacement : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_CorpsDisplacement;
        protected override uint Invoke(uint actionID) =>
            actionID is Displacement
            && LevelChecked(Displacement)
            && HasTarget()
            && GetTargetDistance() >= 5 ? Corpsacorps : actionID;
    }

    internal class RDM_EmboldenManafication : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_EmboldenManafication;
        protected override uint Invoke(uint actionID) =>
            actionID is Embolden
            && IsOnCooldown(Embolden)
            && ActionReady(Manafication) ? Manafication : actionID;
    }

    internal class RDM_MagickBarrierAddle : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_MagickBarrierAddle;
        protected override uint Invoke(uint actionID) =>
            actionID is MagickBarrier
            && (IsOnCooldown(MagickBarrier) || !LevelChecked(MagickBarrier))
            && Role.CanAddle() ? Role.Addle : actionID;
    }

    internal class RDM_EmboldenProtection : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_EmboldenProtection;
        protected override uint Invoke(uint actionID) =>
            actionID is Embolden &&
            ActionReady(Embolden) &&
            HasStatusEffect(Buffs.EmboldenOthers, anyOwner: true) ? All.SavageBlade : actionID;
    }

    internal class RDM_MagickProtection : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_MagickProtection;
        protected override uint Invoke(uint actionID) =>
            actionID is MagickBarrier &&
            ActionReady(MagickBarrier) &&
            HasStatusEffect(Buffs.MagickBarrier, anyOwner: true) ? All.SavageBlade : actionID;
    }

    internal class RDM_ST_Melee_Combo : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_Riposte;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not Riposte)
                return actionID;

            return ComboAction switch
            {
                Zwerchhau or EnchantedZwerchhau => EnchantedRedoublement,
                Riposte or EnchantedRiposte => EnchantedZwerchhau,
                _ => actionID
            };
        }
    }

    internal class RDM_VerAero : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_VerAero;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Veraero or Veraero3))
                return actionID;

            if (ComboAction is Scorch or Verholy or Verflare)
                return OriginalHook(Jolt);

            if (HasManaStacks)
                return UseHolyFlare(actionID);

            if (IsEnabled(CustomComboPreset.RDM_VerAero_Stone) && CanVerStone)
                return Verstone;

            if (!HasDualcast && !HasSwiftcast)
                return OriginalHook(Jolt);

            return actionID;
        }
    }

    internal class RDM_VerThunder : CustomCombo
    {
        protected internal override CustomComboPreset Preset { get; } = CustomComboPreset.RDM_VerThunder;

        protected override uint Invoke(uint actionID)
        {
            if (actionID is not (Verthunder or Verthunder3))
                return actionID;

            if (ComboAction is Scorch or Verholy or Verflare) 
                return OriginalHook(Jolt);

            if (HasManaStacks) 
                return UseHolyFlare(actionID);

            if (IsEnabled(CustomComboPreset.RDM_VerThunder_Fire) && CanVerFire) 
                return Verfire;
        
            if (!HasDualcast && !HasSwiftcast)
                return OriginalHook(Jolt);
        
            return actionID;
        }
    }
    #endregion 
}
