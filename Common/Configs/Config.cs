using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader.Config;

namespace NPCQOL.Common.Configs
{
    public class Config : ModConfig
    {
        [DefaultValue(1)]
        [Range(1, 3000)]
        [ReloadRequired()]
        public int NPCSpawnRate;

        [DefaultValue(false)]
        [ReloadRequired()]
        public bool canNPCSpawnAtNight;


        public override ConfigScope Mode => ConfigScope.ServerSide;
    }
}
