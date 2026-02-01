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

        // 【新增】属性面板 (背包打开时显示)
        public UIPanel statsPanel;
        public UIText statsText;

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

            // === 【新增】背包属性面板 ===
            statsPanel = new UIPanel();
            statsPanel.Width.Set(200, 0);
            statsPanel.Height.Set(350, 0);
            // 放在屏幕左侧，稍微往下一点，避开原版背包区域
            statsPanel.HAlign = 0.05f;
            statsPanel.VAlign = 0.5f;
            statsPanel.BackgroundColor = new Color(30, 30, 40, 230);

            statsText = new UIText("属性...", 0.75f);
            statsText.HAlign = 0.5f;
            statsText.VAlign = 0.5f;

            statsPanel.Append(statsText);
            // 注意：我们不直接 Append(statsPanel)，而是在 Update 里手动控制它的显示
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

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
                float xpPercent = (float)bp.CurrentXP / (float)bp.XPToNextLevel;
                if (xpPercent > 1f) xpPercent = 1f;

                xpBarFill.Width.Set(0, xpPercent);
                levelText.SetText($"LV.{bp.Level} ({bp.CurrentXP}/{bp.XPToNextLevel})");

                // === 属性面板显示逻辑 ===
                // 如果背包打开了，且面板还没有加上去 -> 加上
                if (Main.playerInventory && !HasChild(statsPanel))
                {
                    Append(statsPanel);
                }
                // 如果背包关了，且面板还在 -> 移除
                else if (!Main.playerInventory && HasChild(statsPanel))
                {
                    RemoveChild(statsPanel);
                }

                // 更新面板文字
                if (Main.playerInventory)
                {
                    statsText.SetText(bp.GetStatInfo());
                }
            }
            else
            {
                // 如果不在土豆世界，确保面板不显示
                if (HasChild(statsPanel)) RemoveChild(statsPanel);
            }
        }
    }
}