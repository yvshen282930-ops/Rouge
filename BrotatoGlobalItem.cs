using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using SubworldLibrary;

namespace rouge
{
    public class BrotatoGlobalItem : GlobalItem
    {
        public override void OnSpawn(Item item, Terraria.DataStructures.IEntitySource source)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                // 白名单：土豆币
                bool isPotatoCoin = (item.type == ModContent.ItemType<PotatoCoin>());

                // 如果既不是原版币也不是土豆币，就删除
                if (!isPotatoCoin)
                {
                    item.TurnToAir();
                    item.active = false;
                }
            }
        }
    }
}