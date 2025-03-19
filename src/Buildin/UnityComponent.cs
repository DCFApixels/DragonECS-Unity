using DCFApixels.DragonECS.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

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
    [MetaID("DragonECS_734F667C9201B80F1913388C2A8BB689")]
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
        IEnumerator<T> IEnumerable<T>.GetEnumerator() { throw new NotSupportedException(); }//IntelliSense hack 
        IEnumerator IEnumerable.GetEnumerator() { throw new NotSupportedException(); }//IntelliSense hack 
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator T(UnityComponent<T> a) { return a.obj; }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator UnityComponent<T>(T a) { return new UnityComponent<T>(a); }
        public override string ToString()
        {
            return $"UnityComponent<{typeof(T).ToMeta().TypeName}>";
        }
    }
}

namespace DCFApixels.DragonECS.Unity.Generated
{
    using static EcsConsts;

    #region Unity Component Templates Base
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, OTHER_GROUP)]
    [MetaDescription(AUTHOR, "Template for UnityComponent<T>")]
    [MetaID("DragonECS_13DAACF9910155DD27F822442987E0AE")]
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
    #endregion

    #region Unity Component Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_843B8EF991013F1BFD9133437E1AFE9C")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentTransformTemplate")]
    public sealed class Template_068A1DA395014799316EAD2F9495C57E : UnityComponentTemplate<Transform> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_9A4B8EF99101396C44BF789C3215E9A9")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentRigitBodyInitializer")]
    public sealed class Template_2FBD1DA395013588823232240D607899 : UnityComponentTemplate<Rigidbody> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_52598EF991016655335F234F20F44526")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentAnimatorInitializer")]
    public sealed class Template_ADCE1DA395013CDF357AE983D26934D8 : UnityComponentTemplate<Animator> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_AD658EF99101E8E38BB575D5353E7B1E")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentCharacterControllerInitializer")]
    public sealed class Template_C6DD1DA39501AC0B83DFBC5BB8960322 : UnityComponentTemplate<CharacterController> { }
    #endregion

    #region Render Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_6C6CA0F99101E80E013BCCCB5DA78FA5")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentMeshRendererTemplate")]
    public sealed class Template_6E7615A39501C9BD5010B1EE54536E51 : UnityComponentTemplate<MeshRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_5475A1F9910109A138F609268B697A62")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentMeshFilterTemplate")]
    public sealed class Template_DE9B15A395018C8C5DB56E886F0DAA36 : UnityComponentTemplate<MeshFilter>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_2C13A2F99101FAA3EA21BD351BF3B169")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentSkinnedMeshRendererTemplate")]
    public sealed class Template_A5B215A3950119C9A0688E80D567089F : UnityComponentTemplate<SkinnedMeshRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_8B57A1F991016B2E1FC57D16F2D20A64")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentSpriteRendererTemplate")]
    public sealed class Template_272C16A39501340FEC32FFCE3DA82BFB : UnityComponentTemplate<SpriteRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    #endregion

    #region Collider Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_557F8EF9910132FE990CF50CBF368412")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentColliderTemplate")]
    public sealed class Template_EE4916A39501FDAF46D5890449A4E655 : UnityComponentTemplate<Collider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_43669CF99101E94AB9EC19DC8EA3878B")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentBoxColliderTemplate")]
    public sealed class Template_BD6616A39501D86EF97887158FFC7B7B : UnityComponentTemplate<BoxCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_749F9CF991017792E288D4E3B5BFE340")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentSphereColliderTemplate")]
    public sealed class Template_D78D16A39501B9DC0EE0442F8079E56A : UnityComponentTemplate<SphereCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_72B09CF99101A33EBC4410B0FD8375E0")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentCapsuleColliderTemplate")]
    public sealed class Template_55A216A39501DAAB05E1C2CA616D1B9C : UnityComponentTemplate<CapsuleCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_3BBC9CF99101F7C00989D2E55A40EF1B")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentMeshColliderTemplate")]
    public sealed class Template_6EB916A395011FDDFA239772EF7E297E : UnityComponentTemplate<MeshCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    #endregion

    #region Joint Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_1AC79CF99101C4279852BB6AE12DC61B")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentJointTemplate")]
    public sealed class Template_7DCF16A39501C6CA9A862911585CFAB5 : UnityComponentTemplate<Joint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_E3D99CF991016428C6688672052C6F4E")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentFixedJointTemplate")]
    public sealed class Template_65ED16A395018249ADEC44BED95512B5 : UnityComponentTemplate<FixedJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_7BE59CF99101322AE307229E1466B225")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentCharacterJointTemplate")]
    public sealed class Template_960117A395014C46F58BF054964DBC76 : UnityComponentTemplate<CharacterJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaID("DragonECS_FBF29CF99101EE07543CFF460854B1F6")]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentConfigurableJointTemplate")]
    public sealed class Template_E61A17A39501E6C2B6B9EDABC328BEFB : UnityComponentTemplate<ConfigurableJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    #endregion
}
