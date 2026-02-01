using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.UI;
using Terraria.ID;
using SubworldLibrary;

namespace rouge
{
    public class BrotatoSystem : ModSystem
    {
        // 状态
        public static int WaveNumber = 1;
        public static float TimeLeft = 0f;
        public static bool IsGameActive = false;
        public static bool IsWaveActive = false;
        public static bool IsSelectingWeapon = false;
        public static bool IsLevelingUp = false;

        // 配置
        public static int[] WaveDurations = new int[] { 0, 20, 30, 40, 50, 60, 60, 60, 60, 60 };
        public static int[] SpawnRates = new int[] { 0, 80, 70, 60, 50, 40, 35, 30, 25, 20 };
        public const int ARENA_WIDTH_TILES = 150;
        public const int ARENA_HEIGHT_TILES = 100;
        private int spawnTimer = 0;

        internal BrotatoInfoUI infoUI;
        internal WeaponSelectUI selectionUI;
        internal LevelUpUI levelUpUI;
        private UserInterface infoInterface;
        private UserInterface selectionInterface;
        private UserInterface levelUpInterface;

        public static float GetTotalTimeForCurrentWave() => (WaveNumber < WaveDurations.Length) ? WaveDurations[WaveNumber] : 60f;
        public static bool IsPlayerInArena(Player p) => SubworldSystem.IsActive<BrotatoDimension>();

        public override void Load()
        {
            if (!Main.dedServ)
            {
                infoUI = new BrotatoInfoUI(); infoUI.Activate();
                infoInterface = new UserInterface(); infoInterface.SetState(infoUI);

                selectionUI = new WeaponSelectUI(); selectionUI.Activate();
                selectionInterface = new UserInterface(); selectionInterface.SetState(null);

                levelUpUI = new LevelUpUI(); levelUpUI.Activate();
                levelUpInterface = new UserInterface(); levelUpInterface.SetState(null);
            }
        }

        public override void UpdateUI(GameTime gameTime)
        {
            if (infoInterface != null) infoInterface.Update(gameTime);
            if (selectionInterface != null) selectionInterface.Update(gameTime);
            if (levelUpInterface != null) levelUpInterface.Update(gameTime);
        }

        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            int mouseTextIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text"));
            if (mouseTextIndex != -1)
            {
                layers.Insert(mouseTextIndex, new LegacyGameInterfaceLayer("rouge: InfoUI", delegate { if (infoInterface?.CurrentState != null) infoInterface.Draw(Main.spriteBatch, new GameTime()); return true; }, InterfaceScaleType.UI));
                layers.Insert(mouseTextIndex + 1, new LegacyGameInterfaceLayer("rouge: Selection", delegate { if (selectionInterface?.CurrentState != null) selectionInterface.Draw(Main.spriteBatch, new GameTime()); return true; }, InterfaceScaleType.UI));
                layers.Insert(mouseTextIndex + 2, new LegacyGameInterfaceLayer("rouge: LevelUp", delegate { if (levelUpInterface?.CurrentState != null) levelUpInterface.Draw(Main.spriteBatch, new GameTime()); return true; }, InterfaceScaleType.UI));
            }
        }

        public override void PostUpdateWorld()
        {
            if (!IsGameActive && Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.P))
            {
                if (!SubworldSystem.IsActive<BrotatoDimension>())
                {
                    Main.LocalPlayer.GetModPlayer<BrotatoPlayer>().PrepareForEntry();
                    SubworldSystem.Enter<BrotatoDimension>();
                }
            }
            if (SubworldSystem.IsActive<BrotatoDimension>() && IsGameActive && IsWaveActive && !IsSelectingWeapon && !IsLevelingUp)
            {
                TimeLeft -= (float)(1.0 / 60.0);
                if (TimeLeft <= 0) EndWave();
                spawnTimer++;
                int currentRate = (WaveNumber < SpawnRates.Length) ? SpawnRates[WaveNumber] : 20;
                if (spawnTimer >= currentRate) { spawnTimer = 0; SpawnEnemyInArena(); }
            }
        }

        public override void OnWorldLoad()
        {
            if (SubworldSystem.IsActive<BrotatoDimension>())
            {
                IsGameActive = true; IsSelectingWeapon = true; IsLevelingUp = false;
                ModContent.GetInstance<BrotatoSystem>().selectionInterface.SetState(ModContent.GetInstance<BrotatoSystem>().selectionUI);
                Main.NewText("请选择你的初始装备...", 50, 200, 255);
            }
            else
            {
                IsGameActive = false; IsWaveActive = false; IsSelectingWeapon = false; IsLevelingUp = false;
                if (ModContent.GetInstance<BrotatoSystem>().selectionInterface != null) ModContent.GetInstance<BrotatoSystem>().selectionInterface.SetState(null);
                if (ModContent.GetInstance<BrotatoSystem>().levelUpInterface != null) ModContent.GetInstance<BrotatoSystem>().levelUpInterface.SetState(null);
            }
        }

        public static void FinishSelection()
        {
            IsSelectingWeapon = false;
            ModContent.GetInstance<BrotatoSystem>().selectionInterface.SetState(null);
            WaveNumber = 1;
            ModContent.GetInstance<BrotatoSystem>().StartNextWave();
        }

        public void StartNextWave()
        {
            IsWaveActive = true;
            TimeLeft = (WaveNumber < WaveDurations.Length) ? WaveDurations[WaveNumber] : 60;
            Main.NewText($">>> 第 {WaveNumber} 波 突袭开始 <<<", 255, 100, 100);
        }

        private void EndWave()
        {
            IsWaveActive = false;
            TimeLeft = 0;

            // 1. 静默清场
            ClearEnemies(silent: true);

            // 2. 吸取土豆币
            CollectAllCoins();

            // 3. 结算收获
            ApplyHarvesting();

            // 4. 【新增】回满玩家状态
            Player p = Main.LocalPlayer;
            p.statLife = p.statLifeMax2; // 回满血
            p.HealEffect(p.statLifeMax2); // 飘个绿字
            Main.NewText("波次结束，状态已恢复。", 50, 255, 50);

            // 5. 检查升级
            CheckForUpgrades();
        }

        // 修改：吸取 PotatoCoin
        private void CollectAllCoins()
        {
            Player player = Main.LocalPlayer;
            int coinType = ModContent.ItemType<PotatoCoin>();

            for (int i = 0; i < Main.maxItems; i++)
            {
                Item item = Main.item[i];
                // 如果是土豆币
                if (item.active && item.type == coinType)
                {
                    player.GetItem(player.whoAmI, item, GetItemSettings.PickupItemFromWorld);
                    item.active = false;
                }
            }
            // 播放音效
            Terraria.Audio.SoundEngine.PlaySound(SoundID.CoinPickup);
        }

        private void ApplyHarvesting()
        {
            BrotatoPlayer bp = Main.LocalPlayer.GetModPlayer<BrotatoPlayer>();
            if (bp.BonusHarvesting > 0)
            {
                bp.GainXP(bp.BonusHarvesting);

                // 给予土豆币
                Main.LocalPlayer.QuickSpawnItem(null, ModContent.ItemType<PotatoCoin>(), bp.BonusHarvesting);

                CombatText.NewText(Main.LocalPlayer.getRect(), Color.Gold, $"收获: +{bp.BonusHarvesting}", true);
                bp.BonusHarvesting = (int)(bp.BonusHarvesting * 1.05f);
            }
        }

        private void CheckForUpgrades()
        {
            BrotatoPlayer bp = Main.LocalPlayer.GetModPlayer<BrotatoPlayer>();
            if (bp.PendingUpgrades > 0)
            {
                IsLevelingUp = true;
                levelUpUI.GenerateOptions();
                levelUpInterface.SetState(levelUpUI);
                Main.NewText($"等级提升！你有 {bp.PendingUpgrades} 次强化机会！", 100, 255, 100);
            }
            else
            {
                ProceedToNextWave();
            }
        }

        public static void FinishOneUpgrade()
        {
            BrotatoPlayer bp = Main.LocalPlayer.GetModPlayer<BrotatoPlayer>();
            bp.PendingUpgrades--;
            if (bp.PendingUpgrades > 0)
            {
                ModContent.GetInstance<BrotatoSystem>().levelUpUI.GenerateOptions();
            }
            else
            {
                IsLevelingUp = false;
                ModContent.GetInstance<BrotatoSystem>().levelUpInterface.SetState(null);
                ModContent.GetInstance<BrotatoSystem>().ProceedToNextWave();
            }
        }

        public void ProceedToNextWave()
        {
            Main.NewText("准备进入下一波...", 255, 255, 255);
            WaveNumber++;
            StartNextWave();
        }

        public static void ForceStopGame()
        {
            IsGameActive = false; IsWaveActive = false;
            ClearEnemies(silent: true);
        }

        private static void ClearEnemies(bool silent = false)
        {
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (npc.active && !npc.friendly && npc.lifeMax > 5)
                {
                    if (silent) { npc.active = false; npc.netUpdate = true; }
                    else { NPC.HitInfo hit = new NPC.HitInfo(); hit.Damage = 99999; npc.StrikeNPC(hit); }
                }
            }
        }

        private void SpawnEnemyInArena()
        {
            Player player = Main.LocalPlayer;
            int mapCenterX = (Main.maxTilesX / 2) * 16;
            int mapCenterY = (Main.maxTilesY / 2) * 16;
            int halfWidth = (ARENA_WIDTH_TILES / 2 - 5) * 16;
            int halfHeight = (ARENA_HEIGHT_TILES / 2 - 5) * 16;
            int minX = mapCenterX - halfWidth; int maxX = mapCenterX + halfWidth;
            int minY = mapCenterY - halfHeight; int maxY = mapCenterY + halfHeight;

            Vector2 offset = Main.rand.NextVector2CircularEdge(1, 1);
            float distance = Main.rand.NextFloat(300f, 550f);
            Vector2 spawnPos = player.Center + offset * distance;

            if (spawnPos.X < minX) spawnPos.X = minX;
            if (spawnPos.X > maxX) spawnPos.X = maxX;
            if (spawnPos.Y < minY) spawnPos.Y = minY;
            if (spawnPos.Y > maxY) spawnPos.Y = maxY;

            int type = NPCID.BlueSlime;
            if (WaveNumber > 2) type = NPCID.Zombie;
            if (WaveNumber > 4) type = NPCID.DemonEye;

            int idx = NPC.NewNPC(null, (int)spawnPos.X, (int)spawnPos.Y, type);
            if (idx >= 0) Main.npc[idx].target = player.whoAmI;
        }
    }
}