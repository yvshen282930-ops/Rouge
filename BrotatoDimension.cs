using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.WorldBuilding;
using Terraria.IO;
using SubworldLibrary;

namespace rouge
{
    public class BrotatoDimension : Subworld
    {
        public override int Width => 400;
        public override int Height => 300;
        public override bool ShouldSave => false;
        public override bool NoPlayerSaving => false;

        public override List<GenPass> Tasks => new List<GenPass>()
        {
            new BrotatoGenPass()
        };

        // 1. 强制白天和亮度
        public override void Load()
        {
            Main.dayTime = true; // 白天
            Main.time = 27000;   // 正午 (太阳最大)
            Main.rainTime = 0;   // 无雨
            Main.raining = false;
            Main.cloudAlpha = 0; // 无云
        }
    }

    public class BrotatoGenPass : GenPass
    {
        public BrotatoGenPass() : base("Generating Arena", 1f) { }

        protected override void ApplyPass(GenerationProgress progress, GameConfiguration configuration)
        {
            progress.Message = "正在铺设发光地板...";

            // A. 填充背景：使用 钻石晶火墙 (DiamondGemsparkOff)
            // 注意：GemsparkOff 在有光源时会发光，或者我们可以直接用 Gemspark
            // 这里我们为了保险，配合后面的 Gemspark 砖块，整个世界会非常亮
            ushort wallType = WallID.DiamondGemspark;

            for (int x = 0; x < Main.maxTilesX; x++)
            {
                for (int y = 0; y < Main.maxTilesY; y++)
                {
                    Main.tile[x, y].ClearEverything();
                    WorldGen.PlaceWall(x, y, wallType);
                }
            }

            int centerX = Main.maxTilesX / 2;
            int centerY = Main.maxTilesY / 2;
            int width = 150;
            int height = 100;

            for (int x = centerX - width / 2; x <= centerX + width / 2; x++)
            {
                for (int y = centerY - height / 2; y <= centerY + height / 2; y++)
                {
                    // B. 边框：钻石晶火块 (自带超强光)
                    if (x == centerX - width / 2 || x == centerX + width / 2 ||
                        y == centerY - height / 2 || y == centerY + height / 2)
                    {
                        WorldGen.PlaceTile(x, y, TileID.DiamondGemspark, true, true);
                    }
                    // C. 地板：沥青 (跑得快)
                    else if (y == centerY + height / 2 - 1)
                    {
                        WorldGen.PlaceTile(x, y, TileID.Asphalt, true, true);
                    }
                }
            }

            Main.spawnTileX = centerX;
            Main.spawnTileY = centerY + height / 2 - 4;
        }
    }
}