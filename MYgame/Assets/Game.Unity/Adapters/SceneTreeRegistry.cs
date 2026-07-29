using System.Collections.Generic;
using Game.Application.Ports;
using UnityEngine;
using NVector3 = System.Numerics.Vector3;
using Tree = Game.Domain.World.Tree;

namespace Game.Unity.Adapters
{
    /// <summary>
    /// Implements the world-query port over whatever is actually in the scene.
    /// Never hands back a felled tree or a destroyed view, so the core cannot be
    /// tricked into chopping a stump.
    /// </summary>
    public sealed class SceneTreeRegistry : ITreeRegistry
    {
        private readonly List<TreeView> _views = new List<TreeView>();

        public void Register(TreeView view)
        {
            if (view != null && !_views.Contains(view))
            {
                _views.Add(view);
            }
        }

        public GameObject FindGameObject(string treeId)
        {
            foreach (var view in _views)
            {
                // Unity's fake-null: a destroyed object is not reference-null.
                if (view != null && view.Tree != null && view.Tree.Id == treeId)
                {
                    return view.gameObject;
                }
            }

            return null;
        }

        public Tree FindStandingTreeWithinReach(NVector3 origin, float reach)
        {
            var from = NumericsBridge.ToUnity(origin);
            Tree best = null;
            var bestDistance = reach;

            foreach (var view in _views)
            {
                if (view == null || view.Tree == null || view.Tree.IsFelled)
                {
                    continue;
                }

                var distance = Vector3.Distance(from, view.transform.position);
                if (distance <= bestDistance)
                {
                    bestDistance = distance;
                    best = view.Tree;
                }
            }

            return best;
        }
    }
}
