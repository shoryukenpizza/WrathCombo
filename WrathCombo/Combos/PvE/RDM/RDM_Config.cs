using Dalamud.Interface.Colors;
using ImGuiNET;
using WrathCombo.CustomComboNS.Functions;
using static WrathCombo.Extensions.UIntExtensions;
using static WrathCombo.Window.Functions.SliderIncrements;
using static WrathCombo.Window.Functions.UserConfig;

namespace WrathCombo.Combos.PvE;

internal partial class RDM
{
    internal static class Config
    {
        public static UserInt
            RDM_VariantCure = new("RDM_VariantCure"),
            RDM_ST_Lucid_Threshold = new("RDM_LucidDreaming_Threshold", 6500),
            RDM_AoE_Lucid_Threshold = new("RDM_AoE_Lucid_Threshold", 6500),
            RDM_BalanceOpener_Content = new("RDM_BalanceOpener_Content", 1),
            RDM_ST_Acceleration_Charges = new("RDM_ST_Acceleration_Charges", 0),
            RDM_AoE_Acceleration_Charges = new("RDM_AoE_Acceleration_Charges", 0);

        public static UserBool
            RDM_Reprise_oGCD_Engagement_Pooling = new("RDM_Reprise_oGCD_Engagement_Pooling"),
            RDM_Reprise_oGCD_CorpACorps_Melee =   new("RDM_Reprise_oGCD_CorpACorps_Melee"),
            RDM_Reprise_oGCD_CorpACorps_Pooling = new("RDM_Reprise_oGCD_CorpACorps_Pooling");
        public static UserBoolArray
            RDM_Reprise_oGCD_Actions = new("RDM_Reprise_oGCD_Actions");

        internal static void Draw(CustomComboPreset preset)
        {
            switch (preset)
            {
                case CustomComboPreset.RDM_Balance_Opener:
                    DrawBossOnlyChoice(RDM_BalanceOpener_Content);
                    break;

                case CustomComboPreset.RDM_ST_MeleeCombo:
                    if (P.IPCSearch.AutoActions[CustomComboPreset.RDM_ST_DPS] == true && 
                        CustomComboFunctions.IsNotEnabled(CustomComboPreset.RDM_ST_MeleeCombo_IncludeRiposte))
                    {
                        ImGui.Indent();
                        ImGui.TextColored(ImGuiColors.DalamudRed, "WARNING: RIPOSTE IS NOT ENABLED.");
                        ImGui.TextColored(ImGuiColors.DalamudRed, "AUTO ROTATION WILL NOT START THE MELEE COMBO AUTOMATICALLY");
                        ImGui.Unindent();
                    }
                    break;

                case CustomComboPreset.RDM_ST_Lucid:
                    DrawSliderInt(0, 10000, RDM_ST_Lucid_Threshold, $"Add {Role.LucidDreaming.ActionName()} when below this MP", sliderIncrement: Hundreds);
                    break;

                case CustomComboPreset.RDM_AoE_Lucid:
                    DrawSliderInt(0, 10000, RDM_AoE_Lucid_Threshold, $"Add {Role.LucidDreaming.ActionName()} when below this MP", sliderIncrement: Hundreds);
                    break;

                case CustomComboPreset.RDM_Variant_Cure:
                    DrawSliderInt(1, 100, RDM_VariantCure, "HP% to be at or under", 200);
                    break;

                case CustomComboPreset.RDM_ST_Acceleration:
                    DrawSliderInt(0, 1, RDM_ST_Acceleration_Charges, "How many charges to keep ready\n (0 = Use All)");
                    break;

                case CustomComboPreset.RDM_AoE_Acceleration:
                    DrawSliderInt(0, 1, RDM_AoE_Acceleration_Charges, "How many charges to keep ready?\n (0 = Use All)");
                    break;

            }
        }
    }
}
