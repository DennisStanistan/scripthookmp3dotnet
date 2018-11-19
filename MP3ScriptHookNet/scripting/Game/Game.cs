using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3.Native;

namespace MP3
{
    public static class Game
    {
        #region Fields
        static Player _cachedPlayer;
        #endregion

        /// <summary>
		/// Gets the <see cref="MP3.Player"/> that you are controlling.
		/// </summary>
        public static Player Player
        {
            get
            {
                int handle = Function.Call<int>(Hash.GET_PLAYER_ID);
                if (_cachedPlayer == null || _cachedPlayer.Handle != handle)
                    _cachedPlayer = new Player(handle);

                return _cachedPlayer;
            }
        }

        /// <summary>
		/// Gets or sets the game's time scale value.
		/// </summary>
		/// <value>
		/// The time scale, only accepts values in range 0.0f to 1.0f.
		/// </value>
		public static float TimeScale
        {
            get
            {
                return Function.Call<float>(Hash.GET_TIME_SCALE);
            }
            set
            {
                Function.Call(Hash.SET_TIME_SCALE, value);
            }
        }

        /// <summary>
		/// Gets the time in seconds it took for the last frame to render.
		/// </summary>
		public static float LastFrameTime
        {
            get
            {
                return Function.Call<float>(Hash.GET_FRAME_TIME);
            }
        }

        /// <summary>
        /// Gets the current frame rate in frames per second.
        /// </summary>
        public static float FPS
        {
            get
            {
                return 1.0f / LastFrameTime;
            }
        }

        /// <summary>
		/// Gets or sets a value indicating whether the pause menu is active.
		/// </summary>
		public static bool IsPaused
        {
            get
            {
                return Function.Call<bool>(Hash.IS_PAUSE_MENU_ACTIVE);
            }
            set
            {
                Function.Call(Hash.SET_PAUSE_MENU_ACTIVE, value);
            }
        }

        /// <summary>
		/// Pause/resume the game.
		/// </summary>
		/// <param name="value">True/false for pause/resume.</param>
		public static void Pause(bool value)
        {
            Function.Call(Hash.SET_GAME_PAUSED, value);
        }
    }
}
