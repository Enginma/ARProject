using TMPro;
using UnityEngine;

namespace ARProject.AR
{
    [RequireComponent(typeof(TMP_Text))]
    public class ActiveCharacterUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text label;
        [SerializeField] private string prefix = "Controlling: ";
        [SerializeField] private string noCharacterText = "No champion detected";

        private void Awake()
        {
            if (label == null)
                label = GetComponent<TMP_Text>();
        }

        private void OnEnable()
        {
            TrackedCharacterController.ActiveCharacterChanged += UpdateLabel;
            UpdateLabel(TrackedCharacterController.ActiveCharacter);
        }

        private void OnDisable()
        {
            TrackedCharacterController.ActiveCharacterChanged -= UpdateLabel;
        }

        private void UpdateLabel(TrackedCharacterController character)
        {
            if (label == null)
                return;

            label.text = character == null
                ? noCharacterText
                : prefix + character.CharacterName;
        }
    }
}
