using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;

namespace rouge
{
    public class WeaponSelectUI : UIState
    {
        public UIPanel panel;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.Width.Set(400, 0);
            panel.Height.Set(250, 0);
            panel.HAlign = 0.5f;
            panel.VAlign = 0.5f;
            panel.BackgroundColor = new Color(30, 30, 40, 240);
            Append(panel);

            UIText title = new UIText("选择你的初始武器", 0.8f, true);
            title.HAlign = 0.5f;
            title.Top.Set(10, 0);
            panel.Append(title);

            CreateWeaponButton("铜短剑 (近战)", ItemID.CopperShortsword, 60);
            CreateWeaponButton("木弓 (远程)", ItemID.WoodenBow, 100);
            CreateWeaponButton("火花魔杖 (魔法)", ItemID.WandofSparking, 140);
            CreateWeaponButton("雀杖 (召唤)", ItemID.BabyBirdStaff, 180);
        }

        private void CreateWeaponButton(string text, int itemId, float topY)
        {
            UITextPanel<string> button = new UITextPanel<string>(text);
            button.Width.Set(300, 0);
            button.Height.Set(35, 0);
            button.HAlign = 0.5f;
            button.Top.Set(topY, 0);

            button.OnMouseOver += (evt, element) => button.BackgroundColor = new Color(73, 94, 171);
            button.OnMouseOut += (evt, element) => button.BackgroundColor = new Color(63, 82, 151);

            button.OnLeftClick += (evt, element) => {
                SelectWeapon(itemId);
            };

            panel.Append(button);
        }

        private void SelectWeapon(int itemId)
        {
            Player p = Main.LocalPlayer;

            // 【核心修复】直接操作内存，修改第0格（快捷栏第一格）的数据
            // 这样绝对不会被由于掉落检测而被误删
            p.inventory[0].SetDefaults(itemId);

            // 如果是弓箭，把箭直接塞到第50格（专门放弹药的格子），或者背包里
            if (itemId == ItemID.WoodenBow)
            {
                // 查找第一个弹药栏
                p.inventory[54].SetDefaults(ItemID.WoodenArrow);
                p.inventory[54].stack = 100;
            }

            // 播放一个拿起物品的声音，增加反馈感
            Terraria.Audio.SoundEngine.PlaySound(SoundID.Grab);

            // 通知系统
            BrotatoSystem.FinishSelection();
        }
    }
}