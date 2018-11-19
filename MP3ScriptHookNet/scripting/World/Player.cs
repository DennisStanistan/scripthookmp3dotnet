using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MP3.Native;

namespace MP3
{
    public sealed class Player : INativeValue
    {
        #region Fields
        private Ped _ped;
        #endregion

        public uint NativeValue
        {
            get { return (uint)Handle; }
            set { Handle = unchecked((int)value); }
        }

        public int Handle { get; private set; }

        /// <summary>
        /// Gets the <see cref="Ped"/> this <see cref="Player"/> is controlling.
        /// </summary>
        public Ped Character
        {
            get
            {
                int handle = Function.Call<int>(Hash.GET_PLAYER_PED, Handle);
                if(_ped == null || _ped.Handle != handle)
                {
                    _ped = new Ped(handle);
                }

                return _ped;
            }
        }

        /// <summary>
		/// Gets a value indicating whether this <see cref="Player"/> is dead.
		/// </summary>
		/// <value>
		///   <c>true</c> if this <see cref="Player"/> is dead; otherwise, <c>false</c>.
		/// </value>
		public bool IsDead
        {
            get
            {
                return Function.Call<bool>(Hash.IS_PLAYER_DEAD, Handle);
            }
        }

        /// <summary>
		/// Gets a value indicating whether this <see cref="Player"/> is climbing.
		/// </summary>
		/// <value>
		/// <c>true</c> if this <see cref="Player"/> is climbing; otherwise, <c>false</c>.
		/// </value>
		public bool IsClimbing
        {
            get
            {
                return Function.Call<bool>(Hash.IS_PLAYER_CLIMBING, Handle);
            }
        }

        /// <summary>
		/// Gets a value indicating whether this <see cref="Player"/> is playing.
		/// </summary>
		/// <value>
		/// <c>true</c> if this <see cref="Player"/> is playing; otherwise, <c>false</c>.
		/// </value>
		public bool IsPlaying
        {
            get
            {
                return Function.Call<bool>(Hash.IS_PLAYER_PLAYING, Handle);
            }
        }

        /// <summary>
		/// Sets a value indicating whether this <see cref="Player"/> is ignored by everyone.
		/// </summary>
		/// <value>
		///   <c>true</c> if this <see cref="Player"/> is ignored by everyone; otherwise, <c>false</c>.
		/// </value>
		public bool IgnoredByEveryone
        {
            set
            {
                Function.Call(Hash.SET_EVERYONE_IGNORE_PLAYER, Handle, value);
            }
        }

        /// <summary>
		/// Sets a value indicating whether this <see cref="Player"/> can use cover.
		/// </summary>
		/// <value>
		/// <c>true</c> if this <see cref="Player"/> can use cover; otherwise, <c>false</c>.
		/// </value>
		public bool CanUseCover
        {
            set
            {
                Function.Call(Hash.SET_PLAYER_CAN_USE_COVER, Handle, value);
            }
        }

        /// <summary>
		/// Sets a value indicating whether this <see cref="Player"/> can control ragdoll.
		/// </summary>
		/// <value>
		/// <c>true</c> if this <see cref="Player"/> can control ragdoll; otherwise, <c>false</c>.
		/// </value>
		public bool CanControlRagdoll
        {
            set
            {
                Function.Call(Hash.GIVE_PLAYER_RAGDOLL_CONTROL, Handle, value);
            }
        }

        /// <summary>
		/// Gets or sets a value indicating whether this <see cref="Player"/> can control its <see cref="Ped"/>.
		/// </summary>
		/// <value>
		/// <c>true</c> if this <see cref="Player"/> can control its <see cref="Ped"/>; otherwise, <c>false</c>.
		/// </value>
		public bool CanControlCharacter
        {
            get
            {
                return Function.Call<bool>(Hash.IS_PLAYER_CONTROL_ON, Handle);
            }
            set
            {
                Function.Call(Hash.SET_PLAYER_CONTROL, Handle, value, 0);
            }
        }

        public Player(int handle)
        {
            Handle = handle;
        }
    }
}
