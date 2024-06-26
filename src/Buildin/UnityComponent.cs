﻿using DCFApixels.DragonECS.Unity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    internal static class UnityComponentConsts
    {
        internal const string UNITY_COMPONENT_NAME = "UnityComponent";
        public static readonly MetaGroup BaseGroup = new MetaGroup(UNITY_COMPONENT_NAME);
        public static readonly MetaGroup ColliderGroup = new MetaGroup($"{UNITY_COMPONENT_NAME}/Collider/");
        public static readonly MetaGroup JointGroup = new MetaGroup($"{UNITY_COMPONENT_NAME}/Joint/");
    }
    [Serializable]
    [MetaColor(MetaColor.Cyan)]
    [MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.COMPONENTS_GROUP)]
    [MetaDescription(EcsConsts.AUTHOR, "Component-reference to Unity object for EcsPool.")]
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
    public class UnityComponentTemplate<T> : ComponentTemplateBase<UnityComponent<T>> where T : Component
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
    public sealed class UnityComponentRigitBodyInitializer : UnityComponentTemplate<Rigidbody> { }
    [Serializable]
    public sealed class UnityComponentAnimatorInitializer : UnityComponentTemplate<Animator> { }
    [Serializable]
    public sealed class UnityComponentCharacterControllerInitializer : UnityComponentTemplate<CharacterController> { }
    #endregion

    #region Collider Templates
    [Serializable]
    public sealed class UnityComponentColliderTemplate : UnityComponentTemplate<Collider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentBoxColliderTemplate : UnityComponentTemplate<BoxCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentSphereColliderTemplate : UnityComponentTemplate<SphereCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentCapsuleColliderTemplate : UnityComponentTemplate<CapsuleCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentMeshColliderTemplate : UnityComponentTemplate<MeshCollider>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.ColliderGroup; } }
    }
    #endregion

    #region Joint Templates
    [Serializable]
    public sealed class UnityComponentJointTemplate : UnityComponentTemplate<Joint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentFixedJointTemplate : UnityComponentTemplate<FixedJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentCharacterJointTemplate : UnityComponentTemplate<CharacterJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    [Serializable]
    public sealed class UnityComponentConfigurableJointTemplate : UnityComponentTemplate<ConfigurableJoint>
    {
        public override MetaGroup Group { get { return UnityComponentConsts.JointGroup; } }
    }
    #endregion
}
