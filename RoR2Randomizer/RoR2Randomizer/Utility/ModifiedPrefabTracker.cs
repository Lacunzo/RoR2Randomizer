using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace RoR2Randomizer.Utility
{
    public delegate void PrefabUndoModificationDelegate(GameObject prefab);
    public delegate PrefabUndoModificationDelegate PrefabPerformModificationDelegate(GameObject prefab);

    public class PrefabModificationTracker : MonoBehaviour
    {
        readonly List<PrefabUndoModificationDelegate> _undoCallbacks = new List<PrefabUndoModificationDelegate>();

        public void PerformModification(PrefabPerformModificationDelegate modification)
        {
            if (modification is null)
                throw new ArgumentNullException(nameof(modification));

            PrefabUndoModificationDelegate undoCallback = modification(gameObject);
            if (undoCallback != null)
            {
                _undoCallbacks.Add(undoCallback);
            }
        }

        public void Undo()
        {
            foreach (PrefabUndoModificationDelegate undoCallback in _undoCallbacks)
            {
                undoCallback(gameObject);
            }

            Destroy(this);
        }
    }
}
