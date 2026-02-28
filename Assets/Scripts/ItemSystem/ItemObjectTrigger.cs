using System;
using UnityEngine;

namespace ItemSystem
{
    public class ItemObjectTrigger : MonoBehaviour
    {
        [SerializeField] private GameObject itemObject;
        
        private IPickable _pickable;

        private void Awake()
        {
            _pickable = itemObject.GetComponent<IPickable>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                _pickable.PickUp();
            }
        }

        private void OnValidate()
        {
            if (itemObject != null && !itemObject.TryGetComponent(out IPickable _))
            {
                itemObject = null;
            }
        }
    }
}