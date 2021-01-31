using UnityEngine;

namespace Game.Scripts.Objects.Items
{
    public class TV : InteractableItemCanvass
    {
        [SerializeField] private int coinsToGrant;

        public void WatchAd()
        {
            // TODO: play a rewarded ad using monetization here or something and give the player the coins to grant on ad complete. 
            ExitCanvass();
        }
    }
}