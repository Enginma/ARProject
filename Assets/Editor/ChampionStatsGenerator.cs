#if UNITY_EDITOR
using System.IO;
using ARProject.Battle;
using UnityEditor;
using UnityEngine;

namespace ARProject.Editor
{
    public static class ChampionStatsGenerator
    {
        private const string OutputFolder = "Assets/Common/ScriptableObjects/Champions";

        [MenuItem("AR Battle/Generate Champion Stats")]
        public static void Generate()
        {
            if (!AssetDatabase.IsValidFolder(OutputFolder))
            {
                Directory.CreateDirectory(OutputFolder);
                AssetDatabase.Refresh();
            }

            Create("azir", "Azir", 100, 3, "Ascending Sands", 18, 2, "Shifting Sands", 24, 3, "Royal Guards", 30, 5, "Conquered Dawn", 40);
            Create("irelia", "Irelia", 110, 3, "Duelist's Resolve", 20, 2, "Blade Surge", 26, 3, "Bladesong", 31, 5, "Perfect Convergence", 42);
            Create("jarvan_iv", "Jarvan IV", 130, 3, "Regal March", 17, 2, "Dragon Strike", 28, 3, "Demacian Standard", 30, 5, "Cataclysm", 45);
            Create("jayce", "Jayce", 115, 3, "Hextech Surge", 19, 2, "Thunder Strike", 27, 3, "Cannon Blast", 32, 5, "Mercury Hammer", 43);
            Create("kennen", "Kennen", 90, 3, "Stormheart", 22, 2, "Thundering Shuriken", 25, 3, "Electrical Surge", 29, 5, "Slicing Maelstrom", 40);
            Create("master_yi", "Master Yi", 120, 3, "Meditate", 16, 2, "Double Strike", 28, 3, "Wuju Style", 30, 5, "Highlander", 44);
            Create("renekton", "Renekton", 125, 3, "Battle Fury", 18, 2, "Cull the Meek", 26, 3, "Ruthless Predator", 31, 5, "Dominus", 46);
            Create("riven", "Riven", 115, 3, "Runic Blade", 20, 2, "Broken Wings", 29, 3, "Valor", 24, 5, "Blade of the Exile", 47);
            Create("vi", "Vi", 125, 3, "Relentless Force", 19, 2, "Vault Breaker", 27, 3, "Excessive Force", 29, 5, "Ceiling Crash", 48);
            Create("yasuo", "Yasuo", 105, 3, "Way of the Wind", 21, 2, "Steel Tempest", 29, 3, "Sweeping Blade", 27, 5, "Last Breath", 45);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Generated 10 champion stats under " + OutputFolder);
        }

        private static void Create(string id, string name, int health,
            int passiveCooldown, string passiveName, int passiveDamage,
            int ability1Cooldown, string ability1Name, int ability1Damage,
            int ability2Cooldown, string ability2Name, int ability2Damage,
            int ultimateCooldown, string ultimateName, int ultimateDamage)
        {
            string assetPath = $"{OutputFolder}/{id}.asset";
            ChampionStats stats = AssetDatabase.LoadAssetAtPath<ChampionStats>(assetPath);

            if (stats == null)
            {
                stats = ScriptableObject.CreateInstance<ChampionStats>();
                AssetDatabase.CreateAsset(stats, assetPath);
            }

            var passive = new ChampionAbility(passiveName, 0, passiveDamage,
                new DamageEffect(passiveDamage));
            passive.SetDescription($"Passive effect for {name}; damage {passiveDamage}.");

            var ability1 = new ChampionAbility(ability1Name, ability1Cooldown, ability1Damage,
                new DamageEffect(ability1Damage));
            ability1.SetDescription($"A damaging ability that deals {ability1Damage} damage.");

            var ability2 = new ChampionAbility(ability2Name, ability2Cooldown, ability2Damage,
                new DamageEffect(ability2Damage));
            ability2.SetDescription($"A damaging ability that deals {ability2Damage} damage.");

            var ultimate = new ChampionAbility(ultimateName, ultimateCooldown, ultimateDamage,
                new DamageEffect(ultimateDamage));
            ultimate.SetDescription($"An ultimate ability that deals {ultimateDamage} damage.");

            GameObject model = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Models/{ModelFolderName(name)}/{id}.glb");
            stats.Configure(id, name, health, passive, ability1, ability2, ultimate, model);
            EditorUtility.SetDirty(stats);
        }

        private static string ModelFolderName(string name)
        {
            return name switch
            {
                "Jarvan IV" => "Jarvan",
                "Master Yi" => "Master Yi",
                _ => name
            };
        }
    }
}
#endif
