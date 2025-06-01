using ContentHelper = ECommons.GameHelpers;
using static WrathCombo.CustomComboNS.Functions.CustomComboFunctions;

namespace WrathCombo.Combos.PvE.Content;

internal class OccultCrescent
{
    public static bool InFieldOps => ContentHelper.Content.ContentType == ContentHelper.ContentType.FieldOperations;
    public static bool InSouthHorn => InFieldOps && ContentHelper.Content.TerritoryID == 1252; //South Horn
    internal static uint BestPhantomAction()
    {
        if (!InSouthHorn)
            return 0;

        bool CanUse(uint action) => HasActionEquipped(action) && IsOffCooldown(action);
        bool IsEnabledAndUsable(CustomComboPreset preset, uint action) => IsEnabled(preset) && CanUse(action);

        if (HasStatusEffect(Buffs.PhantomBard))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Bard_MightyMarch, MightyMarch),
            (CustomComboPreset.Phantom_Bard_OffensiveAria, OffensiveAria),
            (CustomComboPreset.Phantom_Bard_RomeosBallad, RomeosBallad),
            (CustomComboPreset.Phantom_Bard_HerosRime, HerosRime), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomBerserker))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Berserker_Rage, Rage),
            (CustomComboPreset.Phantom_Berserker_DeadlyBlow, DeadlyBlow), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomCannoneer))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Cannoneer_PhantomFire, PhantomFire),
            (CustomComboPreset.Phantom_Cannoneer_HolyCannon, HolyCannon),
            (CustomComboPreset.Phantom_Cannoneer_DarkCannon, DarkCannon),
            (CustomComboPreset.Phantom_Cannoneer_ShockCannon, ShockCannon),
            (CustomComboPreset.Phantom_Cannoneer_SilverCannon, SilverCannon), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomChemist))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Chemist_OccultPotion, OccultPotion),
            (CustomComboPreset.Phantom_Chemist_OccultEther, OccultEther),
            (CustomComboPreset.Phantom_Chemist_Revive, Revive),
            (CustomComboPreset.Phantom_Chemist_OccultElixir, OccultElixir), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomFreelancer))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Freelancer_OccultResuscitation, OccultResuscitation),
            (CustomComboPreset.Phantom_Freelancer_OccultTreasuresight, OccultTreasuresight), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomGeomancer))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Geomancer_BattleBell, BattleBell),
            (CustomComboPreset.Phantom_Geomancer_Weather, Weather),
            (CustomComboPreset.Phantom_Geomancer_Sunbath, Sunbath),
            (CustomComboPreset.Phantom_Geomancer_CloudyCaress, CloudyCaress),
            (CustomComboPreset.Phantom_Geomancer_BlessedRain, BlessedRain),
            (CustomComboPreset.Phantom_Geomancer_MistyMirage, MistyMirage),
            (CustomComboPreset.Phantom_Geomancer_HastyMirage, HastyMirage),
            (CustomComboPreset.Phantom_Geomancer_AetherialGain, AetherialGain),
            (CustomComboPreset.Phantom_Geomancer_RingingRespite, RingingRespite),
            (CustomComboPreset.Phantom_Geomancer_Suspend, Suspend), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomKnight))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Knight_PhantomGuard, PhantomGuard),
            (CustomComboPreset.Phantom_Knight_Pray, Pray),
            (CustomComboPreset.Phantom_Knight_OccultHeal, OccultHeal),
            (CustomComboPreset.Phantom_Knight_Pledge, Pledge), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomMonk))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Monk_PhantomKick, PhantomKick),
            (CustomComboPreset.Phantom_Monk_OccultCounter, OccultCounter),
            (CustomComboPreset.Phantom_Monk_Counterstance, Counterstance),
            (CustomComboPreset.Phantom_Monk_OccultChakra, OccultChakra), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomOracle))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Oracle_Predict, Predict),
            (CustomComboPreset.Phantom_Oracle_PhantomJudgment, PhantomJudgment),
            (CustomComboPreset.Phantom_Oracle_Cleansing, Cleansing),
            (CustomComboPreset.Phantom_Oracle_Blessing, Blessing),
            (CustomComboPreset.Phantom_Oracle_Starfall, Starfall),
            (CustomComboPreset.Phantom_Oracle_Recuperation, Recuperation),
            (CustomComboPreset.Phantom_Oracle_PhantomDoom, PhantomDoom),
            (CustomComboPreset.Phantom_Oracle_PhantomRejuvenation, PhantomRejuvenation),
            (CustomComboPreset.Phantom_Oracle_Invulnerability, Invulnerability), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomRanger))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Ranger_PhantomAim, PhantomAim),
            (CustomComboPreset.Phantom_Ranger_OccultFeatherfoot, OccultFeatherfoot),
            (CustomComboPreset.Phantom_Ranger_OccultFalcon, OccultFalcon),
            (CustomComboPreset.Phantom_Ranger_OccultUnicorn, OccultUnicorn), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomSamurai))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Samurai_Mineuchi, Mineuchi),
            (CustomComboPreset.Phantom_Samurai_Shirahadori, Shirahadori),
            (CustomComboPreset.Phantom_Samurai_Iainuki, Iainuki),
            (CustomComboPreset.Phantom_Samurai_Zeninage, Zeninage), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomThief))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_Thief_OccultSprint, OccultSprint),
            (CustomComboPreset.Phantom_Thief_Steal, Steal),
            (CustomComboPreset.Phantom_Thief_Vigilance, Vigilance),
            (CustomComboPreset.Phantom_Thief_TrapDetection, TrapDetection),
            (CustomComboPreset.Phantom_Thief_PilferWeapon, PilferWeapon), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        if (HasStatusEffect(Buffs.PhantomTimeMage))
        {
            foreach (var (preset, action) in new[]
            { (CustomComboPreset.Phantom_TimeMage_OccultSlowga, OccultSlowga),
            (CustomComboPreset.Phantom_TimeMage_OccultComet, OccultComet),
            (CustomComboPreset.Phantom_TimeMage_OccultMageMasher, OccultMageMasher),
            (CustomComboPreset.Phantom_TimeMage_OccultDispel, OccultDispel),
            (CustomComboPreset.Phantom_TimeMage_OccultQuick, OccultQuick), })
                if (IsEnabledAndUsable(preset, action))
                    return action;
        }

        return 0; // No conditions met
    }
    public static bool ShouldUsePhantomActions(uint actionID) => BestPhantomAction() != 0 && IsOffCooldown(actionID);
    public static bool ExecutePhantomActions(uint actionID) => InSouthHorn && ShouldUsePhantomActions(actionID);


    public static ushort
        //Freelancer
        OccultResuscitation = 41650,
        OccultTreasuresight = 41651,

        //Knight
        PhantomGuard = 41588,
        Pray = 41589,
        OccultHeal = 41590,
        Pledge = 41591,

        //Monk
        PhantomKick = 41595,
        OccultCounter = 41596,
        Counterstance = 41597,
        OccultChakra = 41598,

        //Thief
        OccultSprint = 41646,
        Steal = 41647,
        Vigilance = 41648,
        TrapDetection = 41649,
        PilferWeapon = 41650,

        //Berserker
        Rage = 41592,
        DeadlyBlow = 41594,

        //Ranger
        PhantomAim = 41599,
        OccultFeatherfoot = 41600,
        OccultFalcon = 41601,
        OccultUnicorn = 41602,

        //Time Mage
        OccultSlowga = 41621,
        OccultComet = 41622,
        OccultMageMasher = 41623,
        OccultDispel = 41624,
        OccultQuick = 41625,

        //Chemist
        OccultPotion = 41631,
        OccultEther = 41633,
        Revive = 41634,
        OccultElixir = 41635,

        //Bard
        MightyMarch = 41607,
        OffensiveAria = 41608,
        RomeosBallad = 41609,
        HerosRime = 41610,

        //Oracle
        Predict = 41636,
        PhantomJudgment = 41637,
        Cleansing = 41638,
        Blessing = 41639,
        Starfall = 41640,
        Recuperation = 41641,
        PhantomDoom = 41642,
        PhantomRejuvenation = 41643,
        Invulnerability = 41644,

        //Cannoneer
        PhantomFire = 41626,
        HolyCannon = 41627,
        DarkCannon = 41628,
        ShockCannon = 41629,
        SilverCannon = 41630,

        //Samurai
        Mineuchi = 41603,
        Shirahadori = 41604,
        Iainuki = 41605,
        Zeninage = 41606,

        //Geomancer
        BattleBell = 41611,
        Weather = 41612,
        Sunbath = 41613,
        CloudyCaress = 41614,
        BlessedRain = 41615,
        MistyMirage = 41616,
        HastyMirage = 41617,
        AetherialGain = 41618,
        RingingRespite = 41619,
        Suspend = 41620;

    public static class Buffs
    {
        public static ushort
            PhantomGuard = 4231,
            Pray = 4232,
            EnduringFortitude = 4233,
            Pledge = 4234,
            Rage = 4235,
            PentupRage = 4236,
            PhantomKick = 4237,
            Fleetfooted = 4239,
            PhantomAim = 4240,
            OccultUnicorn = 4243,
            RomeosBallad = 4244,
            MightyMarch = 4246,
            OffensiveAria = 4247,
            HerosRime = 4249,
            BattleBell = 4251,
            BattlesClangor = 4252,
            BlessedRain = 4253,
            MistyMirage = 4254,
            HastyMirage = 4255,
            AetherialGain = 4256,
            RingingRespite = 4257,
            Suspend = 4258,
            OccultQuick = 4260,
            OccultSprint = 4261,
            OccultSwift = 4262,
            SilverSickness = 4264,
            PredictionOfJudgment = 4265,
            PredictionOfCleansing = 4266,
            PredictionOfBlessing = 4267,
            PredictionOfStarfall = 4268,
            Recuperation = 4271,
            FortifiedRecuperation = 4272,
            PhantomDoom = 4273,
            PhantomRejuvenation = 4274,
            Invulnerability = 4275,
            Shirahadori = 4245,
            Vigilance = 4277,
            CloudyCaress = 4280,

            //Job Equipped
            PhantomFreelancer = 4355,
            PhantomKnight = 4356,
            PhantomMonk = 4357,
            PhantomThief = 4358,
            PhantomBerserker = 4359,
            PhantomRanger = 4360,
            PhantomTimeMage = 4361,
            PhantomChemist = 4362,
            PhantomBard = 4363,
            PhantomOracle = 4364,
            PhantomCannoneer = 4365,
            PhantomSamurai = 4366,
            PhantomGeomancer = 4367;
    }
    public static class Debuffs
    {
        public static ushort
            OccultMageMasher = 4259,
            FalsePrediction = 4269,
            WeaponPlifered = 4279;
    }
    public static class Traits
    {
        public static ushort
            EnhancedPhantomGuard = 0,
            EnhancedPray = 1,
            EnhancedPhantomKick = 2,
            EnhancedPhantomKickII = 3,
            Lockpicker = 4,
            EnhancedRage = 5,
            EnhancedPhantomAim = 6,
            EnhancedPhantomAimII = 7,
            EnhancedVocals = 8,
            EnhancedPhantomFire = 9,
            EnhancedIainuki = 10,
            EnhancedBell = 11;
    }
    public static class Items
    {
        public static ushort
            OccultPotion = 47741,
            OccultElixir = 47743;
    }
}
