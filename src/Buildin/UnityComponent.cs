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
}

namespace DCFApixels.DragonECS.Unity.ComponentTemplates
{
    using static EcsConsts;

    #region Unity Component Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentTransformTemplate")]
    [MetaID("DragonECS_843B8EF991013F1BFD9133437E1AFE9C")]
    public sealed class Template_DragonECS_843B8EF991013F1BFD9133437E1AFE9C : UnityComponentTemplate<Transform> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentRigitBodyInitializer")]
    [MetaID("DragonECS_9A4B8EF99101396C44BF789C3215E9A9")]
    public sealed class Template_DragonECS_9A4B8EF99101396C44BF789C3215E9A9 : UnityComponentTemplate<Rigidbody> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentAnimatorInitializer")]
    [MetaID("DragonECS_52598EF991016655335F234F20F44526")]
    public sealed class Template_DragonECS_52598EF991016655335F234F20F44526 : UnityComponentTemplate<Animator> { }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentCharacterControllerInitializer")]
    [MetaID("DragonECS_AD658EF99101E8E38BB575D5353E7B1E")]
    public sealed class Template_DragonECS_AD658EF99101E8E38BB575D5353E7B1E : UnityComponentTemplate<CharacterController> { }
    #endregion

    #region Render Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentMeshRendererTemplate")]
    [MetaID("DragonECS_6C6CA0F99101E80E013BCCCB5DA78FA5")]
    public sealed class Template_DragonECS_6C6CA0F99101E80E013BCCCB5DA78FA5 : UnityComponentTemplate<MeshRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentMeshFilterTemplate")]
    [MetaID("DragonECS_5475A1F9910109A138F609268B697A62")]
    public sealed class Template_DragonECS_5475A1F9910109A138F609268B697A62 : UnityComponentTemplate<MeshFilter>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentSkinnedMeshRendererTemplate")]
    [MetaID("DragonECS_2C13A2F99101FAA3EA21BD351BF3B169")]
    public sealed class Template_DragonECS_2C13A2F99101FAA3EA21BD351BF3B169 : UnityComponentTemplate<SkinnedMeshRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentSpriteRendererTemplate")]
    [MetaID("DragonECS_8B57A1F991016B2E1FC57D16F2D20A64")]
    public sealed class Template_DragonECS_8B57A1F991016B2E1FC57D16F2D20A64 : UnityComponentTemplate<SpriteRenderer>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.RenderGroup; } }
    }
    #endregion

    #region Collider Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentColliderTemplate")]
    [MetaID("DragonECS_557F8EF9910132FE990CF50CBF368412")]
    public sealed class Template_DragonECS_557F8EF9910132FE990CF50CBF368412 : UnityComponentTemplate<Collider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentBoxColliderTemplate")]
    [MetaID("DragonECS_43669CF99101E94AB9EC19DC8EA3878B")]
    public sealed class Template_DragonECS_43669CF99101E94AB9EC19DC8EA3878B : UnityComponentTemplate<BoxCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentSphereColliderTemplate")]
    [MetaID("DragonECS_749F9CF991017792E288D4E3B5BFE340")]
    public sealed class Template_DragonECS_749F9CF991017792E288D4E3B5BFE340 : UnityComponentTemplate<SphereCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentCapsuleColliderTemplate")]
    [MetaID("DragonECS_72B09CF99101A33EBC4410B0FD8375E0")]
    public sealed class Template_DragonECS_72B09CF99101A33EBC4410B0FD8375E0 : UnityComponentTemplate<CapsuleCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentMeshColliderTemplate")]
    [MetaID("DragonECS_3BBC9CF99101F7C00989D2E55A40EF1B")]
    public sealed class Template_DragonECS_3BBC9CF99101F7C00989D2E55A40EF1B : UnityComponentTemplate<MeshCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    #endregion

    #region Joint Templates
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentJointTemplate")]
    [MetaID("DragonECS_1AC79CF99101C4279852BB6AE12DC61B")]
    public sealed class Template_DragonECS_1AC79CF99101C4279852BB6AE12DC61B : UnityComponentTemplate<Joint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentFixedJointTemplate")]
    [MetaID("DragonECS_E3D99CF991016428C6688672052C6F4E")]
    public sealed class Template_DragonECS_E3D99CF991016428C6688672052C6F4E : UnityComponentTemplate<FixedJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentCharacterJointTemplate")]
    [MetaID("DragonECS_7BE59CF99101322AE307229E1466B225")]
    public sealed class Template_DragonECS_7BE59CF99101322AE307229E1466B225 : UnityComponentTemplate<CharacterJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    [MetaTags(MetaTags.HIDDEN)]
    [MetaColor(MetaColor.DragonCyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, TEMPLATES_GROUP)]
    [MetaDescription(AUTHOR, "...")]
    [MovedFrom(true, "DCFApixels.DragonECS", "DCFApixels.DragonECS.Unity", "UnityComponentConfigurableJointTemplate")]
    [MetaID("DragonECS_FBF29CF99101EE07543CFF460854B1F6")]
    public sealed class Template_DragonECS_FBF29CF99101EE07543CFF460854B1F6 : UnityComponentTemplate<ConfigurableJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    #endregion
}
