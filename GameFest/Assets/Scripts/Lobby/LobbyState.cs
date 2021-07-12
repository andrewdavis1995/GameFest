namespace Assets.Scripts
{
    public enum PlayerStateEnum { NameEntry, CharacterSelection, ChoosingGames, Ready }

    /// <summary>
    /// Stores the current state of a player - i.e. which action they are taking part in
    /// </summary>
    public class LobbyState
    {
        // the state of the player
        PlayerStateEnum _state;

        /// <summary>
        /// Constructor
        /// </summary>
        public LobbyState()
        {
            _state = PlayerStateEnum.NameEntry;
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
