namespace DCFApixels.DragonECS.Unity.Internal
{
    internal static class EntLongExtentions
    {
        public static bool TryUnpackForUnityEditor(this entlong self, out int id, out short gen, out short worldID, out EcsWorld world)
        {
            self.UnpackUnchecked(out id, out gen, out worldID);
            world = EcsWorld.GetWorld(worldID);
            return world.IsNullOrDetroyed() == false && self.IsAlive;
        }
    }
}
