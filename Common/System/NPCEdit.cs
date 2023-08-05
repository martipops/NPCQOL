using Terraria.ModLoader;
using Terraria;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using MonoMod.Utils;
using NPCQOL.Common.Configs;
using System;
using System.Reflection;

namespace NPCQOL.Common.System
{
    public class NPCEdit : NPCTPEdit
    {
        private static readonly Config config = ModContent.GetInstance<Config>();
        private static Action VanillaSpawnTownNPCs;
        public override void Load()
        {
            On_Main.UpdateTime_SpawnTownNPCs += On_Main_UpdateTime_SpawnTownNPCs;
            if (config.canNPCSpawnAtNight)
            {
                IL_Main.UpdateTime += IL_Main_UpdateTime;
                MethodInfo updateTime = typeof(Main).GetMethod("UpdateTime_SpawnTownNPCs", BindingFlags.Static | BindingFlags.NonPublic);
                VanillaSpawnTownNPCs = Delegate.CreateDelegate(typeof(Action), updateTime) as Action;
            }
        }

        public override void Unload()
        {
            On_Main.UpdateTime_SpawnTownNPCs -= On_Main_UpdateTime_SpawnTownNPCs;
            if (config.canNPCSpawnAtNight)
                IL_Main.UpdateTime -= IL_Main_UpdateTime;
        }


        private void IL_Main_UpdateTime(ILContext il)
        {
            // Creates a cursor keeping track of position in IL instructions
            ILCursor cursor = new ILCursor(il);
            //insert a function at top of method since cursor index is 0
            cursor.EmitDelegate<Action>(delegate
            {
                if (Main.dayTime || config.canNPCSpawnAtNight)
                {
                    VanillaSpawnTownNPCs();
                }
            });
            //Searches for a call that matches UpdateTime_SpawnTownNPCs
            if (!cursor.TryGotoNext(MoveType.Before, (Instruction i) => i.MatchCallOrCallvirt<Main>("UpdateTime_SpawnTownNPCs")))
            {
                //if failed to find instruction
                ModContent.GetInstance<NPCQOL>().Logger.Warn("Spawn edit code failed.");
            }
            else
            {
                //add return instruction to new cursor location
                cursor.Emit(OpCodes.Ret);
            }
        }
    
        private void On_Main_UpdateTime_SpawnTownNPCs(On_Main.orig_UpdateTime_SpawnTownNPCs orig)
        {
            double desiredWorldTilesUpdateRate = Main.desiredWorldTilesUpdateRate;
            Main.desiredWorldTilesUpdateRate *= config.NPCSpawnRate;
            orig();
            Main.desiredWorldTilesUpdateRate = desiredWorldTilesUpdateRate;
        }
    }
}
