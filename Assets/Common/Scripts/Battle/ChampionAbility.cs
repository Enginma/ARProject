using System;
using UnityEngine;

namespace ARProject.Battle
{
    [Serializable]
    public abstract class ChampionAbilityEffect
    {
        [SerializeField] private string description;

        public string Description => description;

        protected ChampionAbilityEffect(string description)
        {
            this.description = description;
        }
    }

    [Serializable]
    public sealed class DamageEffect : ChampionAbilityEffect
    {
        [SerializeField] private int damage;

        public int Damage => damage;

        public DamageEffect() : this(0) { }

        public DamageEffect(int damage) : base($"Deal {damage} damage")
        {
            this.damage = damage;
        }

        public void SetDamage(int value)
        {
            damage = value;
        }
    }

    [Serializable]
    public sealed class HealingEffect : ChampionAbilityEffect
    {
        [SerializeField] private int healing;

        public int Healing => healing;

        public HealingEffect() : this(0) { }

        public HealingEffect(int healing) : base($"Restore {healing} health")
        {
            this.healing = healing;
        }

        public void SetHealing(int value)
        {
            healing = value;
        }
    }

    [Serializable]
    public sealed class ShieldEffect : ChampionAbilityEffect
    {
        [SerializeField] private int shieldAmount;

        public int ShieldAmount => shieldAmount;

        public ShieldEffect() : this(0) { }

        public ShieldEffect(int shieldAmount) : base($"Grant {shieldAmount} shield")
        {
            this.shieldAmount = shieldAmount;
        }

        public void SetShieldAmount(int value)
        {
            shieldAmount = value;
        }
    }

    [Serializable]
    public sealed class ChampionAbility
    {
        [SerializeField] private string abilityName;
        [SerializeField, TextArea] private string description;
        [SerializeField, Min(0)] private int turnCooldown;
        [SerializeField, Min(0)] private int damage;
        [SerializeField] private AudioClip activationSound;
        [SerializeReference] private ChampionAbilityEffect[] possibleEffects = Array.Empty<ChampionAbilityEffect>();

        public string AbilityName => abilityName;
        public string Description => description;
        public int TurnCooldown => turnCooldown;
        public int Damage => damage;
        public AudioClip ActivationSound => activationSound;
        public ChampionAbilityEffect[] PossibleEffects => possibleEffects;

        public ChampionAbility() { }

        public ChampionAbility(string abilityName, int turnCooldown, int damage, params ChampionAbilityEffect[] effects)
        {
            this.abilityName = abilityName;
            this.description = string.Empty;
            this.turnCooldown = turnCooldown;
            this.damage = damage;
            possibleEffects = effects ?? Array.Empty<ChampionAbilityEffect>();
        }

#if UNITY_EDITOR
        public void SetDescription(string value) => description = value;
        public void SetActivationSound(AudioClip value) => activationSound = value;
        public void SetPossibleEffects(params ChampionAbilityEffect[] effects) => possibleEffects = effects;
#endif
    }
}
