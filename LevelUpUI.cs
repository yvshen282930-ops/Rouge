using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ModLoader;
using System.Collections.Generic;
using Terraria.ID;

namespace rouge
{
    public class LevelUpUI : UIState
    {
        public UIPanel panel;

        public override void OnInitialize()
        {
            panel = new UIPanel();
            panel.Width.Set(400, 0);
            panel.Height.Set(350, 0); // 稍微加高一点
            panel.HAlign = 0.5f;
            panel.VAlign = 0.5f;
            panel.BackgroundColor = new Color(40, 40, 50, 240);
            Append(panel);

            UIText title = new UIText("等级提升！选择一项强化", 0.8f, true);
            title.HAlign = 0.5f;
            title.Top.Set(10, 0);
            panel.Append(title);
        }

        public void GenerateOptions()
        {
            if (panel.Children != null)
            {
                panel.RemoveAllChildren();

                UIText title = new UIText($"等级提升！剩余次数: {Main.LocalPlayer.GetModPlayer<BrotatoPlayer>().PendingUpgrades}", 0.8f, true);
                title.HAlign = 0.5f;
                title.Top.Set(10, 0);
                panel.Append(title);

                for (int i = 0; i < 4; i++)
                {
                    CreateRandomOptionButton(50 + (i * 55));
                }
            }
        }

        private void CreateRandomOptionButton(float topY)
        {
            // 现在有 11 种属性可选
            int type = Main.rand.Next(11);
            string text = "";
            Color textColor = Color.White;
            System.Action applyAction = null;
            BrotatoPlayer bp = Main.LocalPlayer.GetModPlayer<BrotatoPlayer>();

            switch (type)
            {
                case 0: // 最大生命
                    int hp = Main.rand.Next(5, 15);
                    text = $"最大生命值 +{hp}"; textColor = Color.Red;
                    applyAction = () => bp.BonusMaxHP += hp;
                    break;
                case 1: // 伤害
                    int dmg = Main.rand.Next(3, 8);
                    text = $"造成伤害 +{dmg}%"; textColor = Color.Orange;
                    applyAction = () => bp.BonusDamage += (dmg / 100f);
                    break;
                case 2: // 防御
                    int def = Main.rand.Next(1, 4);
                    text = $"防御力 +{def}"; textColor = Color.Gray;
                    applyAction = () => bp.BonusDefense += def;
                    break;
                case 3: // 移速
                    int spd = Main.rand.Next(2, 6);
                    text = $"移动速度 +{spd}%"; textColor = Color.Cyan;
                    applyAction = () => bp.BonusMoveSpeed += (spd / 100f);
                    break;
                case 4: // 暴击
                    int crit = Main.rand.Next(3, 6);
                    text = $"暴击率 +{crit}%"; textColor = Color.Yellow;
                    applyAction = () => bp.BonusCritChance += crit;
                    break;
                case 5: // 生命再生 (新)
                    int regen = Main.rand.Next(1, 3);
                    text = $"生命再生 +{regen}"; textColor = Color.Pink;
                    applyAction = () => bp.BonusLifeRegen += regen;
                    break;
                case 6: // 吸血 (新)
                    int steal = Main.rand.Next(1, 3); // 1-2%
                    text = $"生命窃取 +{steal}%"; textColor = Color.Crimson;
                    applyAction = () => bp.BonusLifeSteal += (steal / 100f);
                    break;
                case 7: // 闪避 (新)
                    int dodge = Main.rand.Next(1, 4); // 1-3%
                    text = $"闪避几率 +{dodge}%"; textColor = Color.AliceBlue;
                    applyAction = () => bp.BonusDodge += (dodge / 100f);
                    break;
                case 8: // 收获 (新)
                    int harv = Main.rand.Next(5, 15);
                    text = $"收获 +{harv}"; textColor = Color.Gold;
                    applyAction = () => bp.BonusHarvesting += harv;
                    break;
                case 9: // 经验 (新)
                    int xp = Main.rand.Next(5, 10);
                    text = $"经验获取 +{xp}%"; textColor = Color.MediumPurple;
                    applyAction = () => bp.BonusXPGain += (xp / 100f);
                    break;
                case 10: // 幸运 (新)
                    int luck = Main.rand.Next(2, 6);
                    text = $"幸运 +{luck}"; textColor = Color.LightGreen;
                    applyAction = () => bp.BonusLuck += luck;
                    break;
            }

            UITextPanel<string> button = new UITextPanel<string>(text);
            button.Width.Set(350, 0);
            button.Height.Set(45, 0);
            button.HAlign = 0.5f;
            button.Top.Set(topY, 0);
            button.TextColor = textColor;

            button.OnMouseOver += (evt, element) => button.BackgroundColor = new Color(60, 60, 70);
            button.OnMouseOut += (evt, element) => button.BackgroundColor = new Color(63, 82, 151);

            button.OnLeftClick += (evt, element) => {
                applyAction.Invoke();
                Terraria.Audio.SoundEngine.PlaySound(SoundID.MenuTick);
                BrotatoSystem.FinishOneUpgrade();
            };

            panel.Append(button);
        }
    }
}