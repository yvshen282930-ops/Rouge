using Terraria;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using SubworldLibrary;

namespace rouge
{
    public class BrotatoGlobalNPC : GlobalNPC
    {
        public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
        {
            if (BrotatoSystem.IsPlayerInArena(player))
            {
                spawnRate = int.MaxValue;
                maxSpawns = 0;
            }
        }

        // 移除 ModifyNPCLoot，改用 OnKill 手动生成，确保绝对掉落
        public override void OnKill(NPC npc)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                Player player = Main.LocalPlayer;

                // 1. 给经验
                int xpAmount = 2 + (npc.lifeMax / 10) + (npc.defDefense / 2);
                player.GetModPlayer<BrotatoPlayer>().GainXP(xpAmount);
                CombatText.NewText(npc.getRect(), new Microsoft.Xna.Framework.Color(200, 100, 255), $"+{xpAmount} XP");

                // 2. 【核心修复】直接生成土豆币物品
                int coinAmount = 1 + (npc.lifeMax / 20); // 掉落数量

                // Item.NewItem(源, 位置X, 位置Y, 宽, 高, 类型, 数量)
                Item.NewItem(
                    npc.GetSource_Death(),
                    (int)npc.position.X,
                    (int)npc.position.Y,
                    npc.width,
                    npc.height,
                    ModContent.ItemType<PotatoCoin>(),
                    coinAmount
                );
            }
        }
    }
}