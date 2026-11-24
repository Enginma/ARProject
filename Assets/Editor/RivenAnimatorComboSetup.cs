using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace ARProject.Editor
{
    [InitializeOnLoad]
    public static class RivenAnimatorComboSetup
    {
        private const string ControllerPath = "Assets/Models/Riven/riven.controller";
        private const string ModelPath = "Assets/Models/Riven/riven.glb";

        static RivenAnimatorComboSetup()
        {
            EditorApplication.delayCall += Configure;
        }

        [MenuItem("AR Project/Configure Riven Q Combo")]
        private static void Configure()
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
            AnimationClip[] clips = AssetDatabase.LoadAllAssetsAtPath(ModelPath)
                .OfType<AnimationClip>()
                .Where(clip => !clip.name.StartsWith("__preview__"))
                .ToArray();

            if (controller == null || controller.layers.Length == 0)
                return;

            AnimationClip q1Clip = clips.FirstOrDefault(clip => clip.name == "Spell1A");
            AnimationClip q2Clip = clips.FirstOrDefault(clip => clip.name == "Spell1B");
            AnimationClip q3Clip = clips.FirstOrDefault(clip => clip.name == "Spell1C");
            if (q1Clip == null || q2Clip == null || q3Clip == null)
            {
                Debug.LogError("Riven Q combo setup could not find Spell1A, Spell1B, and Spell1C.");
                return;
            }

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idle = FindState(stateMachine, "Idle1");
            if (idle == null)
                return;

            bool changed = false;
            AnimatorState q1 = EnsureState(stateMachine, "Q1", q1Clip, new Vector3(450, 50), ref changed);
            AnimatorState q2 = EnsureState(stateMachine, "Q2", q2Clip, new Vector3(450, 100), ref changed);
            AnimatorState q3 = EnsureState(stateMachine, "Q3", q3Clip, new Vector3(450, 150), ref changed);

            changed |= EnsureTrigger(controller, "Q1");
            changed |= EnsureTrigger(controller, "Q2");
            changed |= EnsureTrigger(controller, "Q3");
            changed |= EnsureBool(controller, "IsRunning");
            changed |= EnsureAnyStateTransition(stateMachine, q1, "Q1");
            changed |= EnsureAnyStateTransition(stateMachine, q2, "Q2");
            changed |= EnsureAnyStateTransition(stateMachine, q3, "Q3");
            changed |= EnsureReturnTransition(q1, idle);
            changed |= EnsureReturnTransition(q2, idle);
            changed |= EnsureReturnTransition(q3, idle);
            changed |= EnsureRunState(stateMachine, idle);

            if (!changed)
                return;

            EditorUtility.SetDirty(controller);
            AssetDatabase.SaveAssets();
            Debug.Log("✓ Configured Riven Q1/Q2/Q3 combo animations");
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string name)
        {
            return stateMachine.states.Select(child => child.state)
                .FirstOrDefault(state => state.name == name);
        }

        private static AnimatorState EnsureState(
            AnimatorStateMachine stateMachine,
            string name,
            AnimationClip clip,
            Vector3 position,
            ref bool changed)
        {
            AnimatorState state = FindState(stateMachine, name);
            if (state == null)
            {
                state = stateMachine.AddState(name, position);
                changed = true;
            }

            if (state.motion != clip)
            {
                state.motion = clip;
                changed = true;
            }

            return state;
        }

        private static bool EnsureTrigger(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
                return false;

            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
            return true;
        }

        private static bool EnsureBool(AnimatorController controller, string name)
        {
            if (controller.parameters.Any(parameter => parameter.name == name))
                return false;

            controller.AddParameter(name, AnimatorControllerParameterType.Bool);
            return true;
        }

        private static bool EnsureRunState(AnimatorStateMachine stateMachine, AnimatorState idle)
        {
            bool changed = false;
            AnimatorState run = FindState(stateMachine, "Run");
            if (run == null)
            {
                run = stateMachine.AddState("Run", new Vector3(200, 150));
                changed = true;
            }

            if (!idle.transitions.Any(transition =>
                    transition.destinationState == run &&
                    transition.conditions.Any(condition => condition.parameter == "IsRunning")))
            {
                AnimatorStateTransition transition = idle.AddTransition(run);
                transition.hasExitTime = false;
                transition.duration = 0.1f;
                transition.AddCondition(AnimatorConditionMode.If, 0, "IsRunning");
                changed = true;
            }

            if (!run.transitions.Any(transition =>
                    transition.destinationState == idle &&
                    transition.conditions.Any(condition => condition.parameter == "IsRunning")))
            {
                AnimatorStateTransition transition = run.AddTransition(idle);
                transition.hasExitTime = false;
                transition.duration = 0.1f;
                transition.AddCondition(AnimatorConditionMode.IfNot, 0, "IsRunning");
                changed = true;
            }

            return changed;
        }

        private static bool EnsureAnyStateTransition(
            AnimatorStateMachine stateMachine,
            AnimatorState destination,
            string trigger)
        {
            if (stateMachine.anyStateTransitions.Any(transition =>
                    transition.destinationState == destination &&
                    transition.conditions.Any(condition => condition.parameter == trigger)))
                return false;

            AnimatorStateTransition transition = stateMachine.AddAnyStateTransition(destination);
            transition.hasExitTime = false;
            transition.duration = 0.05f;
            transition.canTransitionToSelf = true;
            transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
            return true;
        }

        private static bool EnsureReturnTransition(AnimatorState state, AnimatorState idle)
        {
            if (state.transitions.Any(transition => transition.destinationState == idle))
                return false;

            AnimatorStateTransition transition = state.AddTransition(idle);
            transition.hasExitTime = true;
            transition.exitTime = 0.95f;
            transition.duration = 0.05f;
            return true;
        }
    }
}
