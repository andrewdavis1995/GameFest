namespace Assets.Scripts
{
    public enum PlayerStateEnum { NotStarted, NameEntry, CharacterSelection, Ready, Playing }

    /// <summary>
    /// Stores the current state of a player - i.e. which action they are taking part in
    /// </summary>
    public class PlayerState
    {
        // the state of the player
        private PlayerStateEnum _state;

        /// <summary>
        /// Constructor
        /// </summary>
        public PlayerState()
        {
            _state = PlayerStateEnum.NotStarted;
        }

        /// <summary>
        /// Gets the current state
        /// </summary>
        /// <returns>The enum value representing what state the player is in</returns>
        public PlayerStateEnum GetState()
        {
            return _state;
        }

        /// <summary>
        /// Sets the stored state to the specified state
        /// </summary>
        /// <param name="value">The current state to set</param>
        public void SetState(PlayerStateEnum value)
        {
            _state = value;
        }
    }
}
