using System;

namespace Platformer
{
    public class MiscEvents
    {
        public event Action onCoinCollected;
        public void CoinCollected()
        {
            if (onCoinCollected != null)
            {
                onCoinCollected();
            }
        }
        
        public event Action onEcliptiumCollected;
        public void EcliptiumCollected()
        {
            if (onEcliptiumCollected != null)
            {
                onEcliptiumCollected();
            }
        }
        public event Action onLuminCollected;
        public void LuminCollected()
        {
            if (onLuminCollected != null)
            {
                onLuminCollected();
            }
        }
        public event Action onXPCollected;
        public void XPCollected() 
        {
            if (onXPCollected != null) 
            {
                onXPCollected();
            }
        }
    }
}