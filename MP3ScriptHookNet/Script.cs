using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MP3ScriptHookNet
{
    public class Script
    {
        /// <summary>
        /// Called when the script gets loaded.
        /// </summary>
        public virtual void Init()
        {

        }

        /// <summary>
        /// Called every tick.
        /// </summary>
        public virtual void OnTick()
        {

        }

        /// <summary>
        /// Called when a key is pressed.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="keyState"></param>
        public virtual void OnKey(Keys key, bool status, bool statusCtrl, bool statusShift, bool statusAlt)
        {

        }
    }
}
