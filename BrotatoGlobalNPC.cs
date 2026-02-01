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

        public override void ModifyNPCLoot(NPC npc, NPCLoot npcLoot)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                // 1. 清空所有默认掉落
                npcLoot.RemoveWhere(rule => true);

                // 2. 计算掉落数量 (基于怪物血量)
                // 简单公式：每 10 点血掉 1 个币，最少 1 个
                int minAmount = 1 + (npc.lifeMax / 20);
                int maxAmount = 2 + (npc.lifeMax / 10);

                // 3. 添加掉落规则
                npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<PotatoCoin>(), 1, minAmount, maxAmount));
            }
        }

        public override void OnKill(NPC npc)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                Player player = Main.LocalPlayer;
                int xpAmount = 2 + (npc.lifeMax / 10) + (npc.defDefense / 2);
                player.GetModPlayer<BrotatoPlayer>().GainXP(xpAmount);
                // 飘字颜色稍微调亮一点
                CombatText.NewText(npc.getRect(), new Microsoft.Xna.Framework.Color(200, 100, 255), $"+{xpAmount} XP");
            }
        }
    }
}