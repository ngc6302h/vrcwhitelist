using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using VRC;

namespace VRCWhitelist
{
    public class VRCWhitelist : MelonMod
    {
        private delegate void OnPlayerJoined_target(IntPtr @this, IntPtr player);
        private static OnPlayerJoined_target onPlayerJoined_Target;
        private static string[] allowed_players;
        private static bool enabled = true;
        private static void hoop(IntPtr @this, IntPtr player)
        {
            string id = new Player(player).prop_String_0;
            //!allowed_players.Contains(id) && 
            if (!allowed_players.Contains(id) && enabled)//&& !new Player(player).field_Private_Player_0.field_Private_Boolean_1 && new Player(player).prop_String_0 != Player.prop_Player_0.prop_String_0)
            {
                MelonLogger.Msg($"Denied player {id}");
                new Player(player).prop_VRCPlayer_0.gameObject.active = false;
                new Player(player).prop_VRCPlayer_0.field_Private_VRCAvatarManager_0.OnDestroy();
                new Player(player).OnDestroy();
                return;
            }

            onPlayerJoined_Target(@this, player);
            MelonLogger.Msg($"Allowed player {id}");
        }

        public unsafe override void OnApplicationStart()
        {
            IntPtr func_ptr = (IntPtr)typeof(NetworkManager).GetField("NativeMethodInfoPtr_Method_Public_Void_Player_1", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static).GetValue(null);
            MelonUtils.NativeHookAttach(func_ptr, new Action<IntPtr, IntPtr>(hoop).Method.MethodHandle.GetFunctionPointer());
            onPlayerJoined_Target = Marshal.GetDelegateForFunctionPointer<OnPlayerJoined_target>(*(IntPtr*)func_ptr);
            allowed_players = File.ReadAllLines("whitelist.txt");
        }

        public override void OnUpdate()
        {
            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F8))
            {
                enabled = !enabled;
                MelonLogger.Msg(ConsoleColor.Blue, $"The whitelist is now: {(enabled ? "enabled" : "disabled")}");
            }

            if (UnityEngine.Input.GetKeyDown(UnityEngine.KeyCode.F9))
            {
                File.WriteAllLines($"vrcwhitelist_snapshot_{DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss")}.txt", UnityEngine.Object.FindObjectsOfType<Player>().Select((p) => p.prop_String_0));
            }
        }
    }
}
