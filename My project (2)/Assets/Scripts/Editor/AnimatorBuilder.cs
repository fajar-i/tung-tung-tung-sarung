using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace ToughLoveArena.Editor
{
    public class AnimatorBuilder
    {
        [MenuItem("Tools/TLA/Build Animator Controller")]
        public static void BuildController()
        {
            string path = "Assets/PlayerAnimator.controller";
            
            // This creates a clean Animator Controller at the path
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            
            // Clear existing layers if any, and set up Base Layer
            while (controller.layers.Length > 0)
            {
                controller.RemoveLayer(0);
            }
            
            controller.AddLayer("Base Layer");
            var rootStateMachine = controller.layers[0].stateMachine;

            // Define trigger parameters for state transitions
            controller.AddParameter("Idle", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("WalkForward", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("WalkBackward", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Crouch", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("LightAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("HeavyAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("SpecialAttack", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Block", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Knockdown", AnimatorControllerParameterType.Trigger);

            // Load and extract the animation clips from FBX files
            AnimationClip idleClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield idle.fbx");
            AnimationClip walkFwdClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield walk.fbx");
            AnimationClip walkBwdClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield walk (2).fbx");
            AnimationClip jumpClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield jump.fbx");
            AnimationClip crouchClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield crouch.fbx");
            AnimationClip lightAttackClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield slash.fbx");
            AnimationClip heavyAttackClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield slash (2).fbx");
            AnimationClip specialAttackClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield kick.fbx");
            AnimationClip hurtClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield impact.fbx");
            AnimationClip blockClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield block.fbx");
            AnimationClip knockdownClip = LoadClip("Assets/Pro Sword and Shield Pack/sword and shield death.fbx");

            // Define Animator States and assign their respective motions
            var idleState = AddStateWithMotion(rootStateMachine, "Idle", idleClip);
            var walkFwdState = AddStateWithMotion(rootStateMachine, "WalkForward", walkFwdClip);
            var walkBwdState = AddStateWithMotion(rootStateMachine, "WalkBackward", walkBwdClip);
            var jumpState = AddStateWithMotion(rootStateMachine, "Jump", jumpClip);
            var crouchState = AddStateWithMotion(rootStateMachine, "Crouch", crouchClip);
            var lightAttackState = AddStateWithMotion(rootStateMachine, "LightAttack", lightAttackClip);
            var heavyAttackState = AddStateWithMotion(rootStateMachine, "HeavyAttack", heavyAttackClip);
            var specialAttackState = AddStateWithMotion(rootStateMachine, "SpecialAttack", specialAttackClip);
            var hurtState = AddStateWithMotion(rootStateMachine, "Hurt", hurtClip);
            var blockState = AddStateWithMotion(rootStateMachine, "Block", blockClip);
            var knockdownState = AddStateWithMotion(rootStateMachine, "Knockdown", knockdownClip);

            // Set default state
            rootStateMachine.defaultState = idleState;

            // Set up responsive transition paths from AnyState using triggers
            AddAnyStateTransition(rootStateMachine, idleState, "Idle");
            AddAnyStateTransition(rootStateMachine, walkFwdState, "WalkForward");
            AddAnyStateTransition(rootStateMachine, walkBwdState, "WalkBackward");
            AddAnyStateTransition(rootStateMachine, jumpState, "Jump");
            AddAnyStateTransition(rootStateMachine, crouchState, "Crouch");
            AddAnyStateTransition(rootStateMachine, lightAttackState, "LightAttack");
            AddAnyStateTransition(rootStateMachine, heavyAttackState, "HeavyAttack");
            AddAnyStateTransition(rootStateMachine, specialAttackState, "SpecialAttack");
            AddAnyStateTransition(rootStateMachine, hurtState, "Hurt");
            AddAnyStateTransition(rootStateMachine, blockState, "Block");
            AddAnyStateTransition(rootStateMachine, knockdownState, "Knockdown");

            AssetDatabase.SaveAssets();
            Debug.Log("TLA Animator Controller built successfully at: " + path);
        }

        private static AnimationClip LoadClip(string fbxPath)
        {
            Object[] assets = AssetDatabase.LoadAllAssetsAtPath(fbxPath);
            foreach (Object asset in assets)
            {
                // Mixamo animations contain clip data. We filter out the default preview clip
                if (asset is AnimationClip clip && !clip.name.Contains("__preview__"))
                {
                    return clip;
                }
            }
            Debug.LogWarning("No animation clip sub-asset found in FBX: " + fbxPath);
            return null;
        }

        private static AnimatorState AddStateWithMotion(AnimatorStateMachine stateMachine, string name, AnimationClip clip)
        {
            var state = stateMachine.AddState(name);
            state.motion = clip;
            return state;
        }

        private static void AddAnyStateTransition(AnimatorStateMachine stateMachine, AnimatorState destinationState, string triggerName)
        {
            var transition = stateMachine.AddAnyStateTransition(destinationState);
            transition.AddCondition(AnimatorConditionMode.If, 0, triggerName);
            // Responsive settings for a snappy 2.5D fighting feel
            transition.duration = 0.05f; 
            transition.canTransitionToSelf = false;
        }
    }
}
