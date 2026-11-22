using UnityEngine;

namespace ARProject.Battle
{
    [CreateAssetMenu(fileName = "ChampionStats", menuName = "AR Battle/Champion Stats", order = 0)]
    public sealed class ChampionStats : ScriptableObject
    {
        [SerializeField] private string championId;
        [SerializeField] private string championName;
        [SerializeField, Min(1)] private int healthPoints = 100;

        [SerializeField] private ChampionAbility passiveAbility;
        [SerializeField] private AudioClip passiveAbilitySound;

        [SerializeField] private ChampionAbility ability1;
        [SerializeField] private AudioClip ability1Sound;

        [SerializeField] private ChampionAbility ability2;
        [SerializeField] private AudioClip ability2Sound;

        [SerializeField] private ChampionAbility ultimateAbility;
        [SerializeField] private AudioClip ultimateAbilitySound;

        [SerializeField] private GameObject modelPrefab;

        public string ChampionId => championId;
        public string ChampionName => championName;
        public int HealthPoints => healthPoints;
        public ChampionAbility PassiveAbility => passiveAbility;
        public AudioClip PassiveAbilitySound => passiveAbilitySound;
        public ChampionAbility Ability1 => ability1;
        public AudioClip Ability1Sound => ability1Sound;
        public ChampionAbility Ability2 => ability2;
        public AudioClip Ability2Sound => ability2Sound;
        public ChampionAbility UltimateAbility => ultimateAbility;
        public AudioClip UltimateAbilitySound => ultimateAbilitySound;
        public GameObject ModelPrefab => modelPrefab;

#if UNITY_EDITOR
        public void Configure(
            string id,
            string name,
            int hp,
            ChampionAbility passive,
            ChampionAbility first,
            ChampionAbility second,
            ChampionAbility ultimate,
            GameObject model = null)
        {
            championId = id;
            championName = name;
            healthPoints = hp;
            passiveAbility = passive;
            ability1 = first;
            ability2 = second;
            ultimateAbility = ultimate;
            modelPrefab = model;
        }

        public void SetPassiveAbilitySound(AudioClip clip) => passiveAbilitySound = clip;
        public void SetAbility1Sound(AudioClip clip) => ability1Sound = clip;
        public void SetAbility2Sound(AudioClip clip) => ability2Sound = clip;
        public void SetUltimateAbilitySound(AudioClip clip) => ultimateAbilitySound = clip;
#endif
    }
}
