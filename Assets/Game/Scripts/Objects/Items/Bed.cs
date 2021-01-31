namespace Game.Scripts.Objects.Items
{
    public class Bed : InteractableItemCanvass
    {
        public void Sleep()
        {
            // TODO: When we make a game internal time system this should advance the game time by 6 - 8 hours when sleeping in night
            ExitCanvass();
        }
    }
}