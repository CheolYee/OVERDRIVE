namespace ItemSystem
{
    public interface IPickable
    {
        public void PickUp();
        public void PickUpComplete(bool isSuccess);
    }
}