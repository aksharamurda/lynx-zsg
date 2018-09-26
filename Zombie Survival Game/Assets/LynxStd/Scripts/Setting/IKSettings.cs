using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LynxStd
{
    [Serializable]
    public struct IKChain
    {
        [Tooltip("Defines quality of character IK.")]
        [Range(1, 10)]
        public int Iterations;

        [Tooltip("Time in seconds to wait between IK updates.")]
        public DistanceRange Delay;

        [Tooltip("Chains go in direction away from spine.")]
        public IKBone[] Bones;

        public bool IsEmpty
        {
            get { return Bones.Length == 0 || Iterations < 1; }
        }

        public static IKChain Default()
        {
            var chain = new IKChain();
            chain.Iterations = 2;
            chain.Delay = new DistanceRange(0, 0.1f, 4, 20);

            return chain;
        }
    }

    [Serializable]
    public struct IKSettings
    {
        [Tooltip("Chain of bones used to aim a gun towards a target.\nChains go in direction away from spine.\nIntended to be filled with arm bones startin with shoulders.")]
        public IKChain AimChain;

        [Tooltip("Chain of bones used to aim a torse towards the grenade target.\nChains go in direction away from spine.\nIntended to be filled with spine bones.")]
        public IKChain ThrowChain;

        [Tooltip("Chain of bones used to aim a head towards a target.\nChains go in direction away from spine.\nIntended to have neck as the first bone and head as the second.")]
        public IKChain SightChain;

        [Tooltip("Chain of bones used to position a left hand on a gun.\nChains go in direction away from spine.\nIntended to be filled with left arm bones starting with the left shoulder.")]
        public IKChain LeftArmChain;

        [Tooltip("Chain of bones used to adjust right hand by recoil.\nChains go in direction away from spine.\nIntended to be filled with right arm bones starting with the left shoulder.")]
        public IKChain RecoilChain;

        [Tooltip("Position of a left hand to maintain on a gun.\nBones defined in the LeftBones property are adjusted till LeftHand is in the intended position.\nFor this to work LeftHand must be in the same hierarchy as those bones.")]
        public Transform LeftHand;

        [Tooltip("Position of a right hand to adjust by recoil.\nBones defined in the RightBones property are adjusted till RightHand is in the intended position.\nFor this to work RightHand must be in the same hierarchy as those bones.")]
        public Transform RightHand;

        [Tooltip("Bone to adjust when a character is hit.")]
        public Transform HitBone;

        [Tooltip("Transform to manipulate so it is facing towards a target. Used when aiming a head.\nBones defined in the LookBones are modified till Look is pointing at the intended direction.\nTherefore it Look must be in the same hierarchy as thsoe bones.")]
        public Transform Sight;

        public static IKSettings Default()
        {
            var settings = new IKSettings();
            settings.AimChain = IKChain.Default();
            settings.SightChain = IKChain.Default();
            settings.LeftArmChain = IKChain.Default();
            settings.RecoilChain = IKChain.Default();

            return settings;
        }
    }

    [Serializable]
    public struct IKBone
    {
        [Tooltip("Link to the bone.")]
        public Transform Transform;

        [Tooltip("Link to the bone to be adjusted the same way as Transform. Used for left arm which is adjusted the same as the right.")]
        public Transform Sibling;

        [Tooltip("Defines bone's influence in a bone chain.")]
        [Range(0, 1)]
        public float Weight;

        [HideInInspector]
        internal IKTransform Link;

        public IKBone(Transform transform, float weight)
        {
            Transform = transform;
            Sibling = null;
            Weight = weight;
            Link = null;
        }

        public IKBone(Transform transform, Transform sibling, float weight)
        {
            Transform = transform;
            Sibling = sibling;
            Weight = weight;
            Link = null;
        }
    }
}
