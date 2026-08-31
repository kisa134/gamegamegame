using UnityEngine;
// UnityEngine.Tree is the terrain-detail type; alias so the domain entity wins.
// Same collision class as EntityId in CLAUDE.md section 6.
using Tree = Game.Domain.World.Tree;

namespace Game.Unity.Adapters
{
    /// <summary>
    /// Binds one domain Tree to one GameObject. This is the identity bridge: the core
    /// reasons about the Tree, the engine draws the mesh, and neither knows the other's
    /// representation.
    /// </summary>
    public sealed class TreeView : MonoBehaviour
    {
        public Tree Tree { get; private set; }

        public void Bind(Tree tree)
        {
            Tree = tree;
        }
    }
}
