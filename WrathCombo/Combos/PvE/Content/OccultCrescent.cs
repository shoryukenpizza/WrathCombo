using ContentHelper = ECommons.GameHelpers;
using IntendedUse = ECommons.ExcelServices.TerritoryIntendedUseEnum;

namespace WrathCombo.Combos.PvE.Content;

internal class OccultCrescent
{
    //public static bool InOccultCrescent => ContentHelper.Content.ContentType == ContentHelper.ContentType.OccultCrescent;
    //public static bool InOccultRaids => ContentHelper.Content.ContentType == ContentHelper.ContentType.OccultCrescentRaid;
    //public static bool IsInOccultCrescent => ContentHelper.Content.TerritoryIntendedUse == IntendedUse.OccultCrescent && (InOccultCrescent || InOccultRaids);

    public static ushort
        //Freelancer
        OccultResuscitation = 60,
        OccultTreasuresight = 61,

        //Knight
        PhantomGuard = 0,
        Pray = 1,
        OccultHeal = 2,
        Pledge = 3,

        //Monk
        PhantomKick = 4,
        OccultCounter = 5,
        Counterstance = 6,
        OccultChakra = 7,

        //Thief
        OccultSprint = 8,
        Steal = 9,
        Vigilance = 10,
        TrapDetection = 11,
        PilferWeapon = 12,

        //Berserker
        Rage = 13,
        DeadlyBlow = 14,

        //Ranger
        PhantomAim = 15,
        OccultFeatherfoot = 16,
        OccultFalcon = 17,
        OccultUnicorn = 18,

        //Time Mage
        OccultSlowga = 19,
        OccultComet = 20,
        OccultMageMasher = 21,
        OccultDispel = 22,
        OccultQuick = 23,

        //Chemist
        OccultPotion = 24,
        OccultEther = 25,
        Revive = 26,
        OccultElixir = 27,

        //Bard
        OffensiveAria = 28,
        RomeosBallad = 29,
        MightyMarch = 30,
        HerosRime = 31,

        //Oracle
        Predict = 32,
        PhantomJudgment = 33,
        Cleansing = 34,
        Blessing = 35,
        Starfall = 36,
        Recuperation = 37,
        PhantomDoom = 38,
        PhantomRejuvenation = 39,
        Invulnerability = 40,

        //Cannoneer
        PhantomFire = 41,
        HolyCannon = 42,
        DarkCannon = 43,
        ShockCannon = 44,
        SilverCannon = 45,

        //Samurai
        Mineuchi = 46,
        Shirahadori = 47,
        Iainuki = 48,
        Zeninage = 49,

        //Geomancer
        BattleBell = 50,
        Weather = 51,
        Sunbath = 52,
        CloudyCaress = 53,
        BlessedRain = 54,
        MistyMirage = 55,
        HastyMirage = 56,
        AetherialGain = 57,
        RingingRespite = 58,
        Suspend = 59;

    public static class Buffs
    {
        public static ushort
            PhantomGuard = 0, //10s
            Pray = 0, //30s; TODO: Is this going to be labeled as a standard Regen? idk yet
            EnduringFortitude = 0, //30m
            Pledge = 0, //10s
            PhantomKick = 0, //40s
            CounterStance = 0, //60s
            Fleetfooted = 0, //30m
            OccultSprint = 0, //10s
            Vigilance = 0, //20s
            ForseenOffense = 0, //20s
            PilferWeapon = 0, //60s
            Rage = 0, //10s
            PentupRage = 0,
            PhantomAim = 0, //30s
            OccultUnicorn = 0, //30s
            OccultSlowga = 0, //30s
            OccultQuickRecast = 0, //20s
            OccultQuickMovement = 0, //10s
            OffensiveAria = 0, //70s
            RomeosBalladFreeze = 0, //3s
            RomeosBalladEXP = 0, //30m
            MightyMarch = 0, //30s
            HerosRime = 0, //20s
            PredictionOfJudgment = 0, //30s
            PredictionOfCleansing = 0, //30s
            PredictionOfBlessing = 0, //30s
            PredictionOfStarfall = 0, //30s
            Recuperation = 0, //20s
            FortifiedRecuperation = 0, //3s
            PhantomDoom = 0, //8s-60s
            PhantomRejuvenation = 0, //20s
            Invulnerability = 0, //8s
            SilverSickness = 0, //70s
            Shirahadori = 0, //4s
            BattleBell = 0, //60s
            BattlesClangor = 0, //30s
            CloudyCaress = 0, //20s
            BlessedRain = 0, //20s
            MistyMirage = 0, //20s
            HastyMirage = 0, //20s
            AetherialGain = 0, //20s
            RingingRespite = 0, //60s
            Suspend = 0; //60s
    }
    public static class Debuffs
    {
        public static ushort
            OccultMageMasher = 0, //60s
            Mineuchi = 0; //4s
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
}
