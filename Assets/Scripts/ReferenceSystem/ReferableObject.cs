using UnityEngine;

namespace ReferenceSystem
{
    public class ReferableObject : MonoBehaviour
    {
        protected virtual void Awake()
        {
            ReferenceManager.Instance.SetReferableObject(gameObject.name, this, false);
        }
    }
}
