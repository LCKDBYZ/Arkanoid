namespace Arkanoid {
    internal class Brick {
        public float X;
        public float Y;
        public int Width;
        public int Height;
        public bool IsAlive;
        public int Points;

        public Brick(float x, float y, int width, int height, int points) {
            X = x;
            Y = y;
            Width = width;
            Height = height;
            IsAlive = true;
            Points = points;
        }
    }
}
