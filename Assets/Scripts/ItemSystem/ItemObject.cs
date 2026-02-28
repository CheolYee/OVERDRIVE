using System;
using Gamelib.EventSystem;
using Systems;
using Systems.GameEvents;
using UnityEngine;

namespace ItemSystem
{
    public class ItemObject : MonoBehaviour, IPickable
    {
        [field: SerializeField] public EventChannelSO PlayerChannel { get; private set; }
        [field: SerializeField] public AbstractItemDataSo ItemData { get; private set; }
        [field: SerializeField] public int Amount { get; private set; } = 1;

        private Rigidbody2D _rigidbody;
        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        public void SetUpItem(AbstractItemDataSo itemData, Vector2 velocity, int amount = 1)
        {
            ItemData = itemData;
            Amount = amount;
            _rigidbody.linearVelocity = velocity;
            _spriteRenderer.sprite = itemData.itemIcon;
        }

        public void PickUp()
        {
            PlayerChannel.RaiseEvent(PlayerEvents.PickUpItem.Init(this));
        }

        public void PickUpComplete(bool isSuccess)
        {
            if (isSuccess)
            {
                Destroy(gameObject);
            }
            else
            {
                _rigidbody.AddForce(new Vector2(0f, 5f), ForceMode2D.Impulse);
            }
        }

        private void OnValidate()
        {
            if (ItemData == null) return;
            if(_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

            _spriteRenderer.sprite = ItemData.itemIcon;
            gameObject.name = $"ItemObject-[{ItemData.itemName}]";
        }
    }
}