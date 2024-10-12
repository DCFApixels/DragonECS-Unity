using DCFApixels.DragonECS.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    using static EcsConsts;

    internal static class UnityComponentConsts
    {
        internal const string UNITY_COMPONENT_NAME = "UnityComponent";
        public static readonly MetaGroup BaseGroup = new MetaGroup(UNITY_COMPONENT_NAME);
        public static readonly MetaGroup RenderGroup = new MetaGroup($"{UNITY_COMPONENT_NAME}/Render/");
        public static readonly MetaGroup ColliderGroup = new MetaGroup($"{UNITY_COMPONENT_NAME}/Collider/");
        public static readonly MetaGroup JointGroup = new MetaGroup($"{UNITY_COMPONENT_NAME}/Joint/");
    }
    [Serializable]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, COMPONENTS_GROUP)]
    [MetaDescription(AUTHOR, "Component-reference to Unity object for EcsPool.")]
    [MetaID("734F667C9201B80F1913388C2A8BB689")]
    [MetaTags(MetaTags.ENGINE_MEMBER)]
    public struct UnityComponent<T> : IEcsComponent, IEnumerable<T>//IntelliSense hack
        where T : Component
    {
        public T obj;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public UnityComponent(T obj)
        {
            this.obj = obj;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() //IntelliSense hack
        {
            throw new NotImplementedException();
        }
        IEnumerator IEnumerable.GetEnumerator() //IntelliSense hack
        {
            throw new NotImplementedException();
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(UnityComponent<T> a) { return a.obj; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityComponent<T>(T a) { return new UnityComponent<T>(a); }
    }

    #region Unity Component Templates
    [MetaID("13DAACF9910155DD27F822442987E0AE")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, OTHER_GROUP)]
    public abstract class UnityComponentTemplate<T> : ComponentTemplateBase<UnityComponent<T>> where T : Component
    {
        public override string Name
        {
            get { return typeof(T).Name; }
        }
        public override MetaGroup Group
        {
            get { return UnityComponentConsts.BaseGroup; }
        }
        public sealed override void Apply(short worldID, int entityID)
        {
            EcsWorld.GetPoolInstance<EcsPool<UnityComponent<T>>>(worldID).TryAddOrGet(entityID) = component;
        }
        public override void OnValidate(UnityEngine.Object obj)
        {
            if (component.obj == null)
            {
                if (obj is GameObject go)
                {
                    component.obj = go.GetComponent<T>();
                }
            }
        }
    }

    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("843B8EF991013F1BFD9133437E1AFE9C")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentTransformTemplate : UnityComponentTemplate<Transform> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("9A4B8EF99101396C44BF789C3215E9A9")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentRigitBodyInitializer : UnityComponentTemplate<Rigidbody> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("52598EF991016655335F234F20F44526")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentAnimatorInitializer : UnityComponentTemplate<Animator> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("AD658EF99101E8E38BB575D5353E7B1E")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentCharacterControllerInitializer : UnityComponentTemplate<CharacterController> { }
    #endregion

    #region Render Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("6C6CA0F99101E80E013BCCCB5DA78FA5")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentMeshRendererTemplate : UnityComponentTemplate<MeshRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("5475A1F9910109A138F609268B697A62")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentMeshFilterTemplate : UnityComponentTemplate<MeshFilter>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("2C13A2F99101FAA3EA21BD351BF3B169")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentSkinnedMeshRendererTemplate : UnityComponentTemplate<SkinnedMeshRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("8B57A1F991016B2E1FC57D16F2D20A64")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentSpriteRendererTemplate : UnityComponentTemplate<SpriteRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    #endregion

    #region Collider Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("557F8EF9910132FE990CF50CBF368412")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentColliderTemplate : UnityComponentTemplate<Collider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("43669CF99101E94AB9EC19DC8EA3878B")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentBoxColliderTemplate : UnityComponentTemplate<BoxCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("749F9CF991017792E288D4E3B5BFE340")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentSphereColliderTemplate : UnityComponentTemplate<SphereCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("72B09CF99101A33EBC4410B0FD8375E0")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentCapsuleColliderTemplate : UnityComponentTemplate<CapsuleCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("3BBC9CF99101F7C00989D2E55A40EF1B")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentMeshColliderTemplate : UnityComponentTemplate<MeshCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    #endregion

    #region Joint Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("1AC79CF99101C4279852BB6AE12DC61B")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentJointTemplate : UnityComponentTemplate<Joint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("E3D99CF991016428C6688672052C6F4E")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentFixedJointTemplate : UnityComponentTemplate<FixedJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("7BE59CF99101322AE307229E1466B225")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentCharacterJointTemplate : UnityComponentTemplate<CharacterJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("FBF29CF99101EE07543CFF460854B1F6")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    public sealed class UnityComponentConfigurableJointTemplate : UnityComponentTemplate<ConfigurableJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    #endregion
}
