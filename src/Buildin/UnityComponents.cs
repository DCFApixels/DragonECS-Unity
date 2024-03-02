using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [Serializable]
    [MetaColor(255 / 3, 255, 0)]
    public struct UnityComponent<T> : IEcsComponent, IEnumerable<T>//IntelliSense hack
        where T : Component
    {
        public T obj;
        public UnityComponent(T obj)
        {
            this.obj = obj;
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException(); //IntelliSense hack
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); //IntelliSense hack
    }

    #region Unity Component Templates
    public class UnityComponentTemplate<T> : ComponentTemplateBase<UnityComponent<T>> where T : Component
    {
        public override string Name => "UnityComponent/" + typeof(T).Name;
        public sealed override void Apply(int worldID, int entityID)
        {
            EcsWorld.GetPool<EcsPool<UnityComponent<T>>>(worldID).TryAddOrGet(entityID) = component;
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
        public override string Name => "UnityComponent/Collider/" + nameof(Collider);
    }
    [Serializable]
    public sealed class UnityComponentBoxColliderTemplate : UnityComponentTemplate<BoxCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(BoxCollider);
    }
    [Serializable]
    public sealed class UnityComponentSphereColliderTemplate : UnityComponentTemplate<SphereCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(SphereCollider);
    }
    [Serializable]
    public sealed class UnityComponentCapsuleColliderTemplate : UnityComponentTemplate<CapsuleCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(CapsuleCollider);
    }
    [Serializable]
    public sealed class UnityComponentMeshColliderTemplate : UnityComponentTemplate<MeshCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(MeshCollider);
    }
    #endregion

    #region Joint Templates
    [Serializable]
    public sealed class UnityComponentJointTemplate : UnityComponentTemplate<Joint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(Joint);
    }
    [Serializable]
    public sealed class UnityComponentFixedJointTemplate : UnityComponentTemplate<FixedJoint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(FixedJoint);
    }
    [Serializable]
    public sealed class UnityComponentCharacterJointTemplate : UnityComponentTemplate<CharacterJoint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(CharacterJoint);
    }
    [Serializable]
    public sealed class UnityComponentConfigurableJointTemplate : UnityComponentTemplate<ConfigurableJoint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(ConfigurableJoint);
    }
    #endregion
}
