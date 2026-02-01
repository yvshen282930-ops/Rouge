using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;

namespace rouge
{
    public class BrotatoInfoUI : UIState
    {
        public UIPanel backgroundBar;
        public UIPanel progressBar;
        public UIText statusText;

        public UIPanel xpBarBackground;
        public UIPanel xpBarFill;
        public UIText levelText;

        public UIPanel statsPanel;
        public UIText statsText;

        // 【新增】土豆币计数器
        public UIPanel coinPanel;
        public UIText coinText;

        public override void OnInitialize()
        {
            // === 倒计时 ===
            backgroundBar = new UIPanel();
            backgroundBar.Width.Set(300, 0); backgroundBar.Height.Set(40, 0);
            backgroundBar.HAlign = 0.5f; backgroundBar.Top.Set(50, 0);
            backgroundBar.BackgroundColor = new Color(50, 50, 50, 200);

            progressBar = new UIPanel();
            progressBar.BackgroundColor = new Color(100, 255, 100, 200);
            progressBar.BorderColor = Color.Transparent;
            progressBar.Width.Set(0, 0f); progressBar.Height.Set(0, 1f);

            statusText = new UIText("准备中...", 0.8f);
            statusText.HAlign = 0.5f; statusText.VAlign = 0.5f;

            backgroundBar.Append(progressBar);
            backgroundBar.Append(statusText);
            Append(backgroundBar);

            // === 经验条 ===
            xpBarBackground = new UIPanel();
            xpBarBackground.Width.Set(400, 0); xpBarBackground.Height.Set(25, 0);
            xpBarBackground.HAlign = 0.5f;
            xpBarBackground.Top.Set(90, 0);
            xpBarBackground.BackgroundColor = new Color(30, 30, 30, 200);

            xpBarFill = new UIPanel();
            xpBarFill.BackgroundColor = new Color(180, 80, 255, 200);
            xpBarFill.BorderColor = Color.Transparent;
            xpBarFill.Width.Set(0, 0f); xpBarFill.Height.Set(0, 1f);

            levelText = new UIText("LV.1", 0.7f);
            levelText.HAlign = 0.5f; levelText.VAlign = 0.5f;

            xpBarBackground.Append(xpBarFill);
            xpBarBackground.Append(levelText);
            Append(xpBarBackground);

            // === 背包属性面板 ===
            statsPanel = new UIPanel();
            statsPanel.Width.Set(200, 0); statsPanel.Height.Set(350, 0);
            statsPanel.HAlign = 0.05f; statsPanel.VAlign = 0.5f;
            statsPanel.BackgroundColor = new Color(30, 30, 40, 230);

            statsText = new UIText("属性...", 0.75f);
            statsText.HAlign = 0.5f; statsText.VAlign = 0.5f;
            statsPanel.Append(statsText);

            // === 【新增】右上角土豆币 UI ===
            coinPanel = new UIPanel();
            coinPanel.Width.Set(150, 0);
            coinPanel.Height.Set(40, 0);
            // HAlign 1f = 最右边，Left 设置负数往回拉一点
            coinPanel.HAlign = 1f;
            coinPanel.Left.Set(-20, 0); // 距离右边缘 20 像素
            coinPanel.Top.Set(20, 0);   // 距离顶边缘 20 像素
            coinPanel.BackgroundColor = new Color(255, 215, 0, 150); // 金色半透明背景

            coinText = new UIText("Coins: 0", 0.8f, true);
            coinText.HAlign = 0.5f;
            coinText.VAlign = 0.5f;

            coinPanel.Append(coinText);
            Append(coinPanel);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // 倒计时更新
            if (!BrotatoSystem.IsWaveActive && !BrotatoSystem.IsSelectingWeapon && !BrotatoSystem.IsLevelingUp)
            {
                statusText.SetText("等待中..."); progressBar.Width.Set(0, 0f);
            }
            else
            {
                float total = BrotatoSystem.GetTotalTimeForCurrentWave();
                float current = BrotatoSystem.TimeLeft;
                if (total <= 0) total = 1;
                progressBar.Width.Set(0, current / total);
                statusText.SetText($"第 {BrotatoSystem.WaveNumber} 波 - {current:F1}s");
            }

            if (SubworldLibrary.SubworldSystem.IsActive<BrotatoDimension>())
            {
                BrotatoPlayer bp = Main.LocalPlayer.GetModPlayer<BrotatoPlayer>();

                // 经验条更新
                float xpPercent = (float)bp.CurrentXP / (float)bp.XPToNextLevel;
                if (xpPercent > 1f) xpPercent = 1f;
                xpBarFill.Width.Set(0, xpPercent);
                levelText.SetText($"LV.{bp.Level} ({bp.CurrentXP}/{bp.XPToNextLevel})");

                if (xpBarBackground.IsMouseHovering) Main.hoverItemName = bp.GetStatInfo();

                // 属性面板开关
                if (Main.playerInventory && !HasChild(statsPanel)) Append(statsPanel);
                else if (!Main.playerInventory && HasChild(statsPanel)) RemoveChild(statsPanel);
                if (Main.playerInventory) statsText.SetText(bp.GetStatInfo());

                // === 【新增】土豆币数量更新 ===
                // 遍历背包计算总数
                long totalCoins = 0;
                int coinType = ModContent.ItemType<PotatoCoin>();
                foreach (var item in Main.LocalPlayer.inventory)
                {
                    if (item.active && item.type == coinType)
                    {
                        totalCoins += item.stack;
                    }
                }
                coinText.SetText($"土豆币: {totalCoins}");
            }
            else
            {
                if (HasChild(statsPanel)) RemoveChild(statsPanel);
            }
        }
    }
}