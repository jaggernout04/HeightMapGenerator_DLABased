namespace DLA_Matrix
{
    public class Point
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Height { get; set; }
        public Point? Parent { get; set; } // Reference to parent object
        public bool IsAggregated { get; set; } = false;

        public Point(int x, int y, int height = 0, Point? parent = null)
        {
            X = x;
            Y = y;
            Height = height;
            Parent = parent;
        }

    }
}