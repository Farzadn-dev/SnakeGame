namespace SnakeGame
{
    class PlayerObject : GameObject
    {
        public PlayerObject()
        {
           
        }
        public PlayerObject(PlayerObject player)
        {
            this.X = player.X;
            this.Y = player.Y;
        }
    }

}
