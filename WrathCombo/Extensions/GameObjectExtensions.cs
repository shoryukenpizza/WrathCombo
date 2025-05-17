using System;
using System.Linq;
using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using ECommons.GameFunctions;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using WrathCombo.CustomComboNS.Functions;

namespace WrathCombo.Extensions;

public static class GameObjectExtensions
{
    /// <summary>
    ///     Converts a GameObject pointer to an IGameObject from the object table.
    /// </summary>
    /// <param name="ptr">The GameObject pointer to convert.</param>
    /// <returns>An IGameObject if found in the object table; otherwise, null.</returns>
    public static unsafe IGameObject? GetObjectFrom(GameObject* ptr) =>
        ptr == null ? null : Svc.Objects
            .FirstOrDefault(x => x.Address == (IntPtr)ptr);

    #region Target Classification

    public static IGameObject? IfFriendly (this IGameObject? obj) =>
        obj != null && CustomComboFunctions.TargetIsFriendly(obj) ? obj : null;

    public static IGameObject? IfHostile (this IGameObject? obj) =>
        obj != null && obj.IsHostile() ? obj : null;

    public static IGameObject? IfBoss (this IGameObject? obj) =>
        obj != null && CustomComboFunctions.IsBoss(obj) ? obj : null;

    public static IGameObject? IfQuestMob (this IGameObject? obj) =>
        obj != null && CustomComboFunctions.IsQuestMob(obj) ? obj : null;

    public static IGameObject? IfNeedsPositionals (this IGameObject? obj) =>
        obj != null && CustomComboFunctions.TargetNeedsPositionals(obj) ? obj : null;

    #endregion
}
