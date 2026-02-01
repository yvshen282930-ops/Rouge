using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Creative;

namespace rouge
{
    public class PotatoCoin : ModItem
    {
        public override void SetStaticDefaults()
        {
            // 注册到创造模式
            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = 9999; // 允许堆叠
            Item.value = 0; // 本身没价值，防止被卖给NPC换钱
            Item.rare = ItemRarityID.Green; // 绿色稀有度
        }
    }
}