using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using SubworldLibrary;
using System.Text;

namespace rouge
{
    public class BrotatoPlayer : ModPlayer
    {
        // ... (RP数据与属性，保持不变) ...
        public int Level = 1;
        public int CurrentXP = 0;
        public int XPToNextLevel = 10;
        public int PendingUpgrades = 0;

        public int BonusMaxHP = 0;
        public float BonusDamage = 0f;
        public int BonusDefense = 0;
        public float BonusMoveSpeed = 0f;
        public float BonusCritChance = 0f;
        public int BonusLifeRegen = 0;

        public float BonusLifeSteal = 0f;
        public float BonusDodge = 0f;
        public int BonusHarvesting = 0;
        public float BonusXPGain = 0f;
        public int BonusLuck = 0;

        public Item[] savedInventory;
        public Item[] savedArmor;
        public Item[] savedDye;
        public Item[] savedMiscEquips;
        public Item[] savedMiscDyes;

        public void PrepareForEntry()
        {
            savedInventory = CloneInventory(Player.inventory);
            savedArmor = CloneInventory(Player.armor);
            savedDye = CloneInventory(Player.dye);
            savedMiscEquips = CloneInventory(Player.miscEquips);
            savedMiscDyes = CloneInventory(Player.miscDyes);

            ClearArray(Player.inventory);
            ClearArray(Player.armor);
            ClearArray(Player.dye);
            ClearArray(Player.miscEquips);
            ClearArray(Player.miscDyes);

            Level = 1; CurrentXP = 0; XPToNextLevel = 10; PendingUpgrades = 0;
            BonusMaxHP = 0; BonusDamage = 0f; BonusDefense = 0;
            BonusMoveSpeed = 0f; BonusCritChance = 0f; BonusLifeRegen = 0;
            BonusLifeSteal = 0f; BonusDodge = 0f; BonusHarvesting = 0;
            BonusXPGain = 0f; BonusLuck = 0;

            Player.statLife = Player.statLifeMax = 100;
        }

        public void ExitBrotatoMode()
        {
            if (savedInventory != null)
            {
                RestoreArray(Player.inventory, savedInventory);
                RestoreArray(Player.armor, savedArmor);
                RestoreArray(Player.dye, savedDye);
                RestoreArray(Player.miscEquips, savedMiscEquips);
                RestoreArray(Player.miscDyes, savedMiscDyes);
            }
            SubworldSystem.Exit();
            Player.statLife = Player.statLifeMax;
            Player.HealEffect(Player.statLifeMax);
            BrotatoSystem.ForceStopGame();
        }

        public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource)
        {
            if (BrotatoSystem.IsPlayerInArena(Player))
            {
                playSound = false; genGore = false; ExitBrotatoMode(); return false;
            }
            return true;
        }

        public void GainXP(int amount)
        {
            float multiplier = 1f + BonusXPGain;
            int finalAmount = (int)(amount * multiplier);
            if (finalAmount < 1) finalAmount = 1;

            CurrentXP += finalAmount;

            while (CurrentXP >= XPToNextLevel)
            {
                CurrentXP -= XPToNextLevel;
                Level++;
                PendingUpgrades++;
                XPToNextLevel += 5 + (Level * 3);

                Terraria.Audio.SoundEngine.PlaySound(SoundID.Item29);
                CombatText.NewText(Player.getRect(), Color.Gold, "LEVEL UP!", true);
            }
        }

        public override void ResetEffects()
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                Player.statLifeMax2 += BonusMaxHP;
                Player.GetDamage(DamageClass.Generic) += BonusDamage;
                Player.statDefense += BonusDefense;
                Player.moveSpeed += BonusMoveSpeed;
                Player.GetCritChance(DamageClass.Generic) += BonusCritChance;
                Player.lifeRegen += BonusLifeRegen;
                Player.luck += (BonusLuck * 0.01f);
            }
        }

        public override bool FreeDodge(Player.HurtInfo info)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                float effectiveDodge = BonusDodge > 0.6f ? 0.6f : BonusDodge;
                if (Main.rand.NextFloat() < effectiveDodge)
                {
                    CombatText.NewText(Player.getRect(), Color.Cyan, "MISS", true);
                    return true;
                }
            }
            return false;
        }

        public override void OnHitNPCWithItem(Item item, NPC target, NPC.HitInfo hit, int damageDone) { ApplyLifeSteal(); }
        public override void OnHitNPCWithProj(Projectile proj, NPC target, NPC.HitInfo hit, int damageDone) { ApplyLifeSteal(); }

        private void ApplyLifeSteal()
        {
            if (SubworldSystem.IsActive<BrotatoDimension>() && BonusLifeSteal > 0)
            {
                // 【修复】：吸血几率
                if (Main.rand.NextFloat() < BonusLifeSteal)
                {
                    if (Player.statLife < Player.statLifeMax2)
                    {
                        Player.Heal(1); // 实际回血
                        // 【新增】吸血特效提示
                        CombatText.NewText(Player.getRect(), Color.Crimson, "+1", false, true);
                    }
                }
            }
        }

        public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>()) mult = 0f;
        }
        public override bool CanConsumeAmmo(Item weapon, Item ammo)
        {
            if (SubworldSystem.IsActive<BrotatoDimension>()) return false;
            return base.CanConsumeAmmo(weapon, ammo);
        }

        public string GetStatInfo()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== 土豆属性 ===");
            sb.AppendLine($"等级: {Level}");
            sb.AppendLine($"---------------");
            sb.AppendLine($"生命上限: +{BonusMaxHP}");
            sb.AppendLine($"生命再生: +{BonusLifeRegen}");
            sb.AppendLine($"伤害加成: +{(int)(BonusDamage * 100)}%");
            sb.AppendLine($"攻击吸血: {(int)(BonusLifeSteal * 100)}%");
            sb.AppendLine($"防御力: +{BonusDefense}");
            sb.AppendLine($"闪避几率: {(int)(BonusDodge * 100)}%");
            sb.AppendLine($"移动速度: +{(int)(BonusMoveSpeed * 100)}%");
            sb.AppendLine($"暴击率: +{(int)BonusCritChance}%");
            sb.AppendLine($"收获: {BonusHarvesting}");
            sb.AppendLine($"经验修正: +{(int)(BonusXPGain * 100)}%");
            sb.AppendLine($"幸运: {BonusLuck}");
            return sb.ToString();
        }

        private Item[] CloneInventory(Item[] source) { Item[] t = new Item[source.Length]; for (int i = 0; i < source.Length; i++) { if (source[i] == null) source[i] = new Item(); t[i] = source[i].Clone(); } return t; }
        private void ClearArray(Item[] target) { for (int i = 0; i < target.Length; i++) { if (target[i] != null) target[i].TurnToAir(); else target[i] = new Item(); } }
        private void RestoreArray(Item[] current, Item[] saved) { int len = System.Math.Min(current.Length, saved.Length); for (int i = 0; i < len; i++) { if (saved[i] != null) current[i] = saved[i].Clone(); else current[i].TurnToAir(); } }
    }
}