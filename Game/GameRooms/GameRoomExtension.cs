using System;
using System.Xml;

namespace EmuWarface.Game.GameRooms
{
    public abstract class GameRoomExtension
    {
        private int _sendedRevision;
        public int Revision { get; private set; }

        public void Update() => Revision++;
        public bool Check()
        {
            if (Revision > _sendedRevision)
            {
                _sendedRevision = Revision;
                return true;
            }

            return false;
        }
        public abstract XmlElement Serialize();
    }
}
