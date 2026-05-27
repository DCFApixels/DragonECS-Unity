using DCFApixels.DragonECS.Unity.Editors;
using UnityEngine;

namespace DCFApixels.DragonECS.Unity.Internal
{
	[MetaColor(MetaColor.Gray)]
	[MetaGroup(EcsUnityConsts.PACK_GROUP, EcsConsts.DEBUG_GROUP)]
	[MetaDescription(EcsConsts.AUTHOR, "...")]
	[MetaTags(MetaTags.HIDDEN)]
	[MetaID("DragonECS_A551B6809201D56AA0F1B51248174B4D")]
	internal class EntityMonitor : MonoBehaviour
	{
		private string _cachedDefaultName;
		private bool _isDefaultName = false;
		private int _cachedEntityID;
		private entlong _entity;
		public Color Color;
		public entlong Entity
		{
			get { return _entity; }
		}
		public void Set(entlong entity)
		{
			_entity = entity;
			_cachedEntityID = entity.GetIDUnchecked();
			SetMetaName(string.Empty);
#if UNITY_EDITOR
			var world = entity.GetWorldUnchecked();
			world.Get<DragonGUI.EntityLinksComponent>().SetMonitorLink(entity.GetIDUnchecked(), this);
#endif
		}
		public bool IsDefaultName
		{
			get
			{
				return _isDefaultName;
			}
		}
		public void SetMetaName(string metaName)
		{
			if (string.IsNullOrEmpty(metaName))
			{
				if (string.IsNullOrEmpty(_cachedDefaultName))
				{
					_cachedDefaultName = $"ENTITY ( {_cachedEntityID} )";
				}
				_isDefaultName = true;
				name = _cachedDefaultName;
				return;
			}
			_isDefaultName = false;
			name = $"{metaName} ( {_cachedEntityID} )";
		}
	}
}