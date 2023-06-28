using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DCFApixels.DragonECS
{
    [Serializable]
    [DebugColor(255 / 3, 255, 0)]
    public struct UnityComponent<T> : IEcsComponent, IEnumerable<T>//IntelliSense hack
        where T : Component
    {
        public T obj;
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException(); //IntelliSense hack
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); //IntelliSense hack
    }


    public class UnityComponentInitializer<T> : TemplateComponentInitializer<UnityComponent<T>> where T : Component
    {
        public override string Name => "UnityComponent/" + typeof(T).Name;
        public sealed override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<T>>().Add(e) = component;
        public override void OnValidate(GameObject gameObject)
        {
            if (component.obj == null)
                component.obj = gameObject.GetComponent<T>();
        }
    }


    [Serializable]
    public sealed class UnityComponentRigitBodyInitializer : UnityComponentInitializer<Rigidbody>
    {
    }

    [Serializable]
    public sealed class UnityComponentAnimatorInitializer : UnityComponentInitializer<Animator> 
    {
    }
    [Serializable]
    public sealed class UnityComponentCharacterControllerInitializer : UnityComponentInitializer<CharacterController>
    {
    }

    #region Colliders
    [Serializable]
    public sealed class UnityComponentColliderInitializer : UnityComponentInitializer<Collider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(Collider);
    }
    [Serializable]
    public sealed class UnityComponentBoxColliderInitializer : UnityComponentInitializer<BoxCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(BoxCollider);
    }
    [Serializable]
    public sealed class UnityComponentSphereColliderInitializer : UnityComponentInitializer<SphereCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(SphereCollider);
    }
    [Serializable]
    public sealed class UnityComponentCapsuleColliderInitializer : UnityComponentInitializer<CapsuleCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(CapsuleCollider);
    }
    [Serializable]
    public sealed class UnityComponentMeshColliderInitializer : UnityComponentInitializer<MeshCollider>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(MeshCollider);
    }
    #endregion

    #region Joints
    [Serializable]
    public sealed class UnityComponentJointInitializer : UnityComponentInitializer<Joint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(Joint);
    }
    [Serializable]
    public sealed class UnityComponentFixedJointInitializer : UnityComponentInitializer<FixedJoint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(FixedJoint);
    }
    [Serializable]
    public sealed class UnityComponentCharacterJointInitializer : UnityComponentInitializer<CharacterJoint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(CharacterJoint);
    }
    [Serializable]
    public sealed class UnityComponentConfigurableJointInitializer : UnityComponentInitializer<ConfigurableJoint>
    {
        public override string Name => "UnityComponent/Joint/" + nameof(ConfigurableJoint);
    }
    #endregion
}
