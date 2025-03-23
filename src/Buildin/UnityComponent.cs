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