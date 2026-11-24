using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using Vuforia;

namespace ARProject.AR
{
    public class TrackedCharacterController : MonoBehaviour
    {
        private static readonly List<TrackedCharacterController> TrackedCharacters =
            new List<TrackedCharacterController>();

        private static TrackedCharacterController activeCharacter;
        private static int lastCharacterSwitchFrame = -1;
        private static readonly int IsRunningHash = Animator.StringToHash("IsRunning");

        public static event Action<TrackedCharacterController> ActiveCharacterChanged;
        public static TrackedCharacterController ActiveCharacter => activeCharacter;

        [Header("Character")]
        [SerializeField] private string characterName;
        [SerializeField] private Animator animator;

        [Header("Movement")]
        [SerializeField] private Transform characterTransform;
        [SerializeField, Min(0f)] private float moveSpeed = 0.15f;
        [SerializeField, Min(0f)] private float turnSpeed = 720f;

        [Header("Q Audio")]
        [FormerlySerializedAs("q1Sound")]
        [SerializeField] private AudioClip qSound;

        [Header("Other Ability Audio")]
        [SerializeField] private AudioClip wSound;
        [SerializeField] private AudioClip eSound;
        [SerializeField] private AudioClip rSound;

        private ObserverBehaviour observerBehaviour;
        private AudioSource audioSource;
        private bool isCurrentlyTracked;
        private bool hasRunningParameter;
        private int qComboStep;
        private float lastQPressTime = float.NegativeInfinity;
        private Vector3 characterBasePosition;
        private Vector3 characterPositionOffset;
        private Quaternion characterBaseRotation;
        private Quaternion characterFacingRotation;
        private Quaternion characterAppliedRotation;
        private bool hasMovementTransform;

        public bool IsTracked => isCurrentlyTracked;
        public bool IsActiveCharacter => activeCharacter == this;
        public string CharacterName => string.IsNullOrWhiteSpace(characterName) ? gameObject.name : characterName;
        protected virtual bool UsesQCombo => false;
        protected virtual float QComboWindow => 1.25f;
        protected AudioClip QSound => qSound;

        protected virtual void Start()
        {
            observerBehaviour = GetComponent<ObserverBehaviour>();
            if (observerBehaviour == null)
            {
                Debug.LogError($"✗ {CharacterName}: TrackedCharacterController requires an ObserverBehaviour on the same ImageTarget.");
            }
            else
            {
                observerBehaviour.OnTargetStatusChanged += OnTargetStatusChanged;
            }

            ResolveAnimator();
            if (animator == null)
            {
                Debug.LogError($"✗ {CharacterName}: No Animator found on the child character model.");
            }
            else
            {
                // Only accept the instantiated model under this ImageTarget. A Transform
                // dragged from the Project window points at the GLB asset, not the scene model.
                if (characterTransform == null || !characterTransform.IsChildOf(transform))
                    characterTransform = animator.transform;
                characterTransform = CreateMovementRoot(characterTransform);
                characterBasePosition = characterTransform.localPosition;
                characterBaseRotation = characterTransform.localRotation;
                characterFacingRotation = characterBaseRotation;
                characterAppliedRotation = characterBaseRotation;
                hasMovementTransform = true;
                hasRunningParameter = HasAnimatorParameter("IsRunning");
                Debug.Log($"✓ {CharacterName}: Using Animator on '{animator.gameObject.name}'.");
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }

        private Transform CreateMovementRoot(Transform modelTransform)
        {
            Transform existingParent = modelTransform.parent;
            GameObject movementRoot = new GameObject($"{CharacterName} Movement Root");
            Transform movementTransform = movementRoot.transform;

            movementTransform.SetParent(existingParent, false);
            movementTransform.localPosition = Vector3.zero;
            movementTransform.localRotation = Quaternion.identity;
            movementTransform.localScale = Vector3.one;

            // Keep the imported model's existing local placement. Animation can modify
            // the model, while gameplay movement remains isolated on its new parent.
            modelTransform.SetParent(movementTransform, false);
            return movementTransform;
        }

        protected virtual void Update()
        {
            if (!isCurrentlyTracked || activeCharacter != this)
                return;

            if (Input.GetKeyDown(KeyCode.Tab) && lastCharacterSwitchFrame != Time.frameCount)
            {
                lastCharacterSwitchFrame = Time.frameCount;
                SelectNextTrackedCharacter();
                return;
            }

            UpdateMovement();

            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (UsesQCombo)
                    PlayQCombo();
                else
                    PlayNonComboAbility("Q", qSound);
            }
            else if (Input.GetKeyDown(KeyCode.W))
                PlayNonComboAbility("W", wSound);
            else if (Input.GetKeyDown(KeyCode.E))
                PlayNonComboAbility("E", eSound);
            else if (Input.GetKeyDown(KeyCode.R))
                PlayNonComboAbility("R", rSound);
        }

        private void UpdateMovement()
        {
            float horizontal = 0f;
            float vertical = 0f;

            if (Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;

            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            bool isMoving = direction.sqrMagnitude > 0f;

            if (isMoving && characterTransform != null)
            {
                characterPositionOffset += direction * (moveSpeed * Time.deltaTime);

                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
                characterFacingRotation = Quaternion.Euler(0f, targetAngle, 0f) * characterBaseRotation;
            }

            SetRunning(isMoving);
        }

        protected virtual void LateUpdate()
        {
            if (!hasMovementTransform || characterTransform == null)
                return;

            // Some imported clips animate their root transform. Apply gameplay movement
            // after Animator evaluation so the clip cannot reset the model's position.
            characterTransform.localPosition = characterBasePosition + characterPositionOffset;
            characterAppliedRotation = Quaternion.RotateTowards(
                characterAppliedRotation,
                characterFacingRotation,
                turnSpeed * Time.deltaTime);
            characterTransform.localRotation = characterAppliedRotation;
        }

        private void PlayQCombo()
        {
            if (Time.time - lastQPressTime > QComboWindow)
                qComboStep = 0;

            qComboStep++;
            lastQPressTime = Time.time;
            PlayAbility($"Q{qComboStep}", GetQSound(qComboStep));

            if (qComboStep >= 3)
                qComboStep = 0;
        }

        private void PlayNonComboAbility(string trigger, AudioClip sound)
        {
            ResetQCombo();
            PlayAbility(trigger, sound);
        }

        private void PlayAbility(string trigger, AudioClip sound)
        {
            if (animator != null && HasAnimatorParameter(trigger))
            {
                animator.SetTrigger(trigger);
                Debug.Log($"★ {CharacterName} used {trigger}");
            }
            else if (animator != null)
            {
                Debug.LogWarning($"⚠ {CharacterName}: Animator trigger '{trigger}' has not been configured yet.");
            }

            if (sound != null && audioSource != null)
                audioSource.PlayOneShot(sound);
        }

        protected virtual AudioClip GetQSound(int comboStep)
        {
            return qSound;
        }

        private void ResolveAnimator()
        {
            if (animator != null && animator.transform != transform)
                return;

            foreach (Animator candidate in GetComponentsInChildren<Animator>(true))
            {
                if (candidate.transform == transform)
                    continue;

                animator = candidate;
                return;
            }

            animator = GetComponent<Animator>();
        }

        private bool HasAnimatorParameter(string parameterName)
        {
            foreach (AnimatorControllerParameter parameter in animator.parameters)
            {
                if (parameter.name == parameterName)
                    return true;
            }

            return false;
        }

        private void SetRunning(bool isRunning)
        {
            if (animator != null && hasRunningParameter)
                animator.SetBool(IsRunningHash, isRunning);
        }

        private void ResetQCombo()
        {
            qComboStep = 0;
            lastQPressTime = float.NegativeInfinity;
        }

        private void OnTargetStatusChanged(ObserverBehaviour behaviour, TargetStatus targetStatus)
        {
            bool tracked = targetStatus.Status == Status.TRACKED ||
                           targetStatus.Status == Status.EXTENDED_TRACKED;
            if (tracked == isCurrentlyTracked)
                return;

            isCurrentlyTracked = tracked;
            if (tracked)
                RegisterTrackedCharacter();
            else
                UnregisterTrackedCharacter();
        }

        private void RegisterTrackedCharacter()
        {
            if (!TrackedCharacters.Contains(this))
                TrackedCharacters.Add(this);

            if (activeCharacter == null || !activeCharacter.isCurrentlyTracked)
                SetActiveCharacter(this);

            Debug.Log($"✓ {CharacterName} detected. Press Tab to switch when multiple champions are tracked.");
        }

        private void UnregisterTrackedCharacter()
        {
            bool wasActive = activeCharacter == this;
            TrackedCharacters.Remove(this);
            ResetInputState();

            if (wasActive)
                SetActiveCharacter(TrackedCharacters.Count > 0 ? TrackedCharacters[0] : null);

            Debug.Log($"✗ {CharacterName} lost.");
        }

        private static void SelectNextTrackedCharacter()
        {
            TrackedCharacters.RemoveAll(character => character == null || !character.isCurrentlyTracked);
            if (TrackedCharacters.Count == 0)
            {
                SetActiveCharacter(null);
                return;
            }

            int currentIndex = TrackedCharacters.IndexOf(activeCharacter);
            int nextIndex = (currentIndex + 1) % TrackedCharacters.Count;
            SetActiveCharacter(TrackedCharacters[nextIndex]);
        }

        private static void SetActiveCharacter(TrackedCharacterController character)
        {
            if (activeCharacter == character)
                return;

            if (activeCharacter != null)
                activeCharacter.ResetInputState();

            activeCharacter = character;
            if (activeCharacter != null)
                Debug.Log($"★ Active champion: {activeCharacter.CharacterName}");

            ActiveCharacterChanged?.Invoke(activeCharacter);
        }

        private void ResetInputState()
        {
            ResetQCombo();
            SetRunning(false);
        }

        protected virtual void OnDestroy()
        {
            if (observerBehaviour != null)
                observerBehaviour.OnTargetStatusChanged -= OnTargetStatusChanged;

            if (TrackedCharacters.Contains(this) || activeCharacter == this)
                UnregisterTrackedCharacter();
        }
    }
}
