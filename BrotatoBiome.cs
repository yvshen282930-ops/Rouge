using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace rouge
{
    public class BrotatoBiome : ModBiome
    {
        // 优先级设置：高于普通地形
        public override SceneEffectPriority Priority => SceneEffectPriority.Event;

        // 视觉效果
        public override string BestiaryIcon => "Terraria/Images/UI/Bestiary/Icon_Tags_Shadow";
        public override string BackgroundPath => "Terraria/Images/MapBG1";
        public override Color? BackgroundColor => new Color(20, 20, 30);

        // === 音乐控制 (核心修改) ===
        public override int Music
        {
            get
            {
                // 1. 如果正在选武器，或者游戏没激活，保持安静 (返回 -1)
                if (BrotatoSystem.IsSelectingWeapon || !BrotatoSystem.IsGameActive)
                {
                    return -1;
                }

                // 2. 战斗开始后，播放 Boss 2 (节奏感强的战斗曲)
                return MusicID.Boss2;
            }
        }

        // 判定条件
        public override bool IsBiomeActive(Player player)
        {
            return BrotatoSystem.IsPlayerInArena(player);
        }
    }
}