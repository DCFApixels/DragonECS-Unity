namespace DCFApixels.DragonECS.Unity
{
    public class EcsUnityConsts
    {
        public const string PACK_GROUP = "_" + EcsConsts.FRAMEWORK_NAME + "/_Unity";
        public const string ENTITY_BUILDING_GROUP = "Entity Building";
        public const string PIPELINE_BUILDING_GROUP = "Pipeline Building";

        public const string DEBUG_LAYER = EcsConsts.NAME_SPACE + "Unity." + nameof(DEBUG_LAYER);

        public const string UNITY_PACKAGE_NAME = "com.dcfa_pixels.dragonecs-unity";

        public const string LOCAL_CACHE_FOLDER = "/Library/" + EcsConsts.AUTHOR + "/" + UNITY_PACKAGE_NAME;
        public const string USER_SETTINGS_FOLDER = "/UserSettings/" + EcsConsts.AUTHOR + "/" + UNITY_PACKAGE_NAME;
    }

    public class EcsUnityDefines
    {

        public const bool DRAGONECS_ENABLE_UNITY_CONSOLE_SHORTCUT_LINKS =
#if DRAGONECS_ENABLE_UNITY_CONSOLE_SHORTCUT_LINKS
            true;
#else
            false;
#endif
        public const bool ENABLE_IL2CPP =
#if ENABLE_IL2CPP
            true;
#else
            false;
#endif
        //        public const bool DISABLE_SERIALIZE_REFERENCE_RECOVERY =
        //#if DISABLE_SERIALIZE_REFERENCE_RECOVERY
        //            true;
        //#else
        //            false;
        //#endif
    }
}
