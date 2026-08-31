namespace Game.Unity.Adapters
{
    /// <summary>
    /// The only file in the project allowed to see both Vector3 types.
    /// The core speaks System.Numerics; the engine speaks UnityEngine. Everywhere else
    /// one `using` too many produces CS0104, which is exactly the collision CLAUDE.md
    /// section 6 already tells us to isolate.
    /// </summary>
    public static class NumericsBridge
    {
        public static UnityEngine.Vector3 ToUnity(System.Numerics.Vector3 v) =>
            new UnityEngine.Vector3(v.X, v.Y, v.Z);

        public static System.Numerics.Vector3 ToNumerics(UnityEngine.Vector3 v) =>
            new System.Numerics.Vector3(v.x, v.y, v.z);
    }
}
