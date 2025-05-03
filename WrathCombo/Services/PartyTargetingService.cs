using Dalamud.Game.ClientState.Objects.Types;
using ECommons.DalamudServices;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using System;
using System.Linq;
using GameObject = FFXIVClientStructs.FFXIV.Client.Game.Object.GameObject;

namespace WrathCombo.Services
{
    public static unsafe class PartyTargetingService
    {
        private static readonly IntPtr pronounModule = (IntPtr)Framework.Instance()->GetUIModule()->GetPronounModule();

        public static ulong GetObjectID(GameObject* o)
        {
            var id = o->GetGameObjectId();
            return id.ObjectId;
        }


        // Mouse Over on the party list
        public static GameObject* UITarget => Framework.Instance()->GetUIModule()->GetPronounModule()->UiMouseOverTarget;
        public static IGameObject? UiMouseOverTarget =>
            UITarget != null
            ? (UITarget->GetGameObjectId() is var id && id.ObjectId != 0 ? Svc.Objects.FirstOrDefault(x => x.GameObjectId == id.ObjectId) : null)
            : null;


        private static readonly delegate* unmanaged<IntPtr, uint, GameObject*> getGameObjectFromPronounID = (delegate* unmanaged<IntPtr, uint, GameObject*>)Svc.SigScanner.ScanText("E8 ?? ?? ?? ?? 48 8B D8 48 85 C0 0F 85 ?? ?? ?? ?? 8D 4F DD");
        public static GameObject* GetGameObjectFromPronounID(uint id) => getGameObjectFromPronounID(pronounModule, id);
    }
}
