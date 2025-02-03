namespace Player.Movement
{
    public interface IMovementHandler
    {
        public void ResetValues();
    
        void Move(PlayerCharacterController player);

        bool ShouldGravityApply(PlayerCharacterController player)
        {
            return true;
        }

        bool ShouldHoverApply(PlayerCharacterController player)
        {
            return true;
        }
    }
}