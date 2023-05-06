using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace DCFApixels.DragonECS
{
    [Serializable]
    [DebugColor(255 / 3, 255, 0)]
    public struct UnityComponent<T> : IEcsComponent, IEnumerable<T>//IntelliSense hack
        where T : class
    {
        public T obj;

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => throw new NotImplementedException(); //IntelliSense hack
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException(); //IntelliSense hack
    }

    [Serializable]
    [MovedFrom(false, "Client", null, "RefRigitBodyInitializer")]
    public sealed class UnityComponentRigitBodyInitializer : TemplateComponentInitializer<UnityComponent<Rigidbody>>
    {
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<Rigidbody>>().Add(e) = component;
    }

    [Serializable]
    [MovedFrom(false, "Client", null, "RefAnimatorInitializer")]
    public sealed class UnityComponentAnimatorInitializer : TemplateComponentInitializer<UnityComponent<Animator>>
    {
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<Animator>>().Add(e) = component;
    }
    [Serializable]
    public sealed class UnityComponentCharacterControllerInitializer : TemplateComponentInitializer<UnityComponent<CharacterController>>
    {
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<CharacterController>>().Add(e) = component;
    }

    #region Colliders
    [Serializable]
    public sealed class UnityComponentColliderInitializer : TemplateComponentInitializer<UnityComponent<Collider>>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(Collider);
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<Collider>>().Add(e) = component;
    }
    [Serializable]
    public sealed class UnityComponentBoxColliderInitializer : TemplateComponentInitializer<UnityComponent<BoxCollider>>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(BoxCollider);
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<BoxCollider>>().Add(e) = component;
    }
    [Serializable]
    public sealed class UnityComponentSphereColliderInitializer : TemplateComponentInitializer<UnityComponent<SphereCollider>>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(SphereCollider);
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<SphereCollider>>().Add(e) = component;
    }
    [Serializable]
    public sealed class UnityComponentCapsuleColliderInitializer : TemplateComponentInitializer<UnityComponent<CapsuleCollider>>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(CapsuleCollider);
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<CapsuleCollider>>().Add(e) = component;
    }
    [Serializable]
    public sealed class UnityComponentMeshColliderInitializer : TemplateComponentInitializer<UnityComponent<MeshCollider>>
    {
        public override string Name => "UnityComponent/Collider/" + nameof(MeshCollider);
        public override void Add(EcsWorld w, int e) => w.GetPool<UnityComponent<MeshCollider>>().Add(e) = component;
    }
    #endregion

}
