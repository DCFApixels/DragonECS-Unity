using DCFApixels.DragonECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DCFApixels.Assets.Plugins.DragonECS_Unity.src.Templates.ComponentTemplateGenerator
{
    internal class ComponentTemplateGenerator
    {
    }

    public abstract class Template_GUID<T> : ComponentTemplateBase<T> where T : struct, IEcsComponent
    {
        public override void Apply(short worldID, int entityID)
        {
            EcsPool<T>.Apply(ref component, entityID, worldID);
        }
    }
}