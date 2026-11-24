using UnityEngine;

namespace ARProject.AR
{
    // Keeps the existing Riven scene component and serialized Inspector values intact.
    // New champions can use TrackedCharacterController directly.
    public class RivenAbilityController : TrackedCharacterController
    {
        [Header("Riven Q Combo Audio")]
        [SerializeField] private AudioClip q2Sound;
        [SerializeField] private AudioClip q3Sound;
        [SerializeField, Min(0.1f)] private float qComboWindow = 1.25f;

        protected override bool UsesQCombo => true;
        protected override float QComboWindow => qComboWindow;

        protected override AudioClip GetQSound(int comboStep)
        {
            switch (comboStep)
            {
                case 1: return QSound;
                case 2: return q2Sound;
                case 3: return q3Sound;
                default: return null;
            }
        }
    }
}
