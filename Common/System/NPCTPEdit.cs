using Terraria;
using Terraria.ModLoader;
using static Terraria.ModLoader.PlayerDrawLayer;
using Terraria.ID;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using System;
using Microsoft.Xna.Framework.Input;
using Terraria.UI.Gamepad;
using Terraria.GameInput;

namespace NPCQOL.Common.System
{
    public class NPCTPEdit: ModSystem
    {
        public override void Load()
        {
            IL_Main.DrawNPCHousesInUI += IL_Main_DrawNPCHousesInUI;
        }

      

        public override void Unload()
        {
            IL_Main.DrawNPCHousesInUI -= IL_Main_DrawNPCHousesInUI;

        }

        private void IL_Main_DrawNPCHousesInUI(ILContext il)
        {
            ILCursor c = new(il);
            if(!c.TryGotoNext(MoveType.Before, i=>i.MatchLdsfld<Main>("mouseLeftRelease")))
            {
                ModContent.GetInstance<NPCQOL>().Logger.Warn("NPC Teleport hook code failed.");
                return;
            }
            //come back and make more suitable?
            c.Emit(OpCodes.Ldloc, 11);
            c.EmitDelegate(injectTPinUI);
            
        }

        private void injectTPinUI(int npcType)
        {
            //this is where the magic happens
            if (npcType == 0 && Main.mouseRight && Main.mouseRightRelease && !PlayerInput.UsingGamepad) teleportNPCsHome();
        }

        private void teleportNPCsHome()
        {
            for (int i = 0; i < Main.maxNPCs; i++) {
                if (Main.npc[i] == null) return;
                if (!Main.npc[i].townNPC) return;
                teleportNPCHome(i);
            } 
        }

        private void teleportNPCHome(int n)
        {
            bool flag = false;
            for (int i = 0; i < 3; i++)
            {
                int num2 = Main.npc[n].homeTileX + i switch
                {
                    1 => -1,
                    0 => 0,
                    _ => 1,
                };
                if (Main.npc[n].type == NPCID.OldMan || !Collision.SolidTiles(num2 - 1, num2 + 1, Main.npc[n].homeTileY- 3, Main.npc[n].homeTileY - 1))
                {
                    Main.npc[n].velocity.X = 0f;
                    Main.npc[n].velocity.Y = 0f;
                    Main.npc[n].position.X = num2 * 16 + 8 - Main.npc[n].width / 2;
                    Main.npc[n].position.Y = (float)(Main.npc[n].homeTileY * 16 - Main.npc[n].height) - 0.1f;
                    Main.npc[n].netUpdate = true;
                    flag = true;
                    break;
                }
            }
            if (!flag)
            {
                Main.npc[n].homeless = true;
                WorldGen.QuickFindHome(Main.npc[n].whoAmI);
                teleportNPCHome(n);
            }
        }

    }
}
