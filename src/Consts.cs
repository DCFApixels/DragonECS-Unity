namespace DCFApixels.DragonECS.Unity
{
    public class EcsUnityConsts
    {
        public const string PACK_GROUP = "_" + EcsConsts.FRAMEWORK_NAME + "/Unity";
        public const string ENTITY_BUILDING_GROUP = "Entity Building";
        public const string PIPELINE_BUILDING_GROUP = "Pipeline Building";

        public const string DEBUG_LAYER = EcsConsts.NAME_SPACE + "Unity." + nameof(DEBUG_LAYER);

        public const string UNITY_PACKAGE_NAME = "com.dcfa_pixels.dragonecs-unity";

        public const string LOCAL_CACHE_FOLDER = "/Library/" + EcsConsts.AUTHOR + "/" + UNITY_PACKAGE_NAME;
        public const string USER_SETTINGS_FOLDER = "/UserSettings/" + EcsConsts.AUTHOR + "/" + UNITY_PACKAGE_NAME;

        //EcsConsts.AUTHOR + "/" + EcsConsts.FRAMEWORK_NAME + "/" + nameof(DragonDocsPrefs) + ".prefs"
    }

    public class EcsUnityDefines
    {
        //        public const bool DISABLE_SERIALIZE_REFERENCE_RECOVERY =
        //#if DISABLE_SERIALIZE_REFERENCE_RECOVERY
        //            true;
        //#else
        //            false;
        //#endif
    }
}
