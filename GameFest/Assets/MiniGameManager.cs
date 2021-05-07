using System.Collections.Generic;

namespace Assets
{
    /// <summary>
    /// Class to control the loading of mini games
    /// </summary>
    public class MiniGameManager
    {
        List<MiniGameConfiguration> _games = new List<MiniGameConfiguration>();

        /// <summary>
        /// Constructor
        /// </summary>
        public MiniGameManager()
        {
            // get list of all configurations
            _games = new List<MiniGameConfiguration>()
            {
                // TODO: Proper game configs
                new MiniGameConfiguration(2, typeof(GameCentralController))
            };
        }
    }
}