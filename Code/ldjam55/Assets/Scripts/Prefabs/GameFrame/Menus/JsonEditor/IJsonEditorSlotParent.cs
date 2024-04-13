using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public interface IJsonEditorSlotParent
    {
        public abstract void UpdateByChild(string childName);

        public abstract void RemoveChild(JsonEditorSlotBaseBehaviour child);
    }
}
