using System;

namespace Assets
{
    /// <summary>
    /// Stores the necessary information for loading a mini game
    /// </summary>
    public class MiniGameConfiguration
    {
        public int SceneIndex { get; private set; }
        public Type InputHandler { get; private set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="index">The scene index</param>
        /// <param name="handler">The type of input handler required</param>
        public MiniGameConfiguration(int index, Type handler)
        {
            SceneIndex = index;
            InputHandler = handler;
        }
    }
}
