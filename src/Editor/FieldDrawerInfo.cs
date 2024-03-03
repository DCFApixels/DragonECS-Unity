using System;
using System.Reflection;

namespace DCFApixels.DragonECS.Unity.Editors
{
    public struct FieldDrawerInfo
    {
        private static readonly BindingFlags fieldFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        public Type type;
        public object data;
        public FieldInfo[] fields;
        public FieldDrawerInfo(object data)
        {
            type = data.GetType();
            this.data = data;
            fields = type.GetFields(fieldFlags);
        }
        public void Set(object data)
        {
            type = data.GetType();
            this.data = data;
            fields = type.GetFields(fieldFlags);
        }
    }
}
