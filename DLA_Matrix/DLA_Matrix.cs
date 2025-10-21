using System.Drawing;
using System.Drawing.Imaging;

namespace DLA_Matrix
{

    public class DLAMatrix
    {
        private Point[,] matrix;
        private List<Point> aggregatedPoints;  // Stores only the aggregated points
        private int matrixSize;
        private int targetSize;
        private Random random = new Random();
        private int center;


        private HeightMapGenerator heightMap;

        
        /// <summary>
        /// Initializes a new instance of the DLAMatrix class with the specified target size and initial size.
        /// </summary>
        /// <param name="targetSize">The target size of the matrix. Must be a positive integer.</param>
        /// <param name="initialSize">The initial size of the matrix. Default is 2. Must be a non-negative integer.</param>
        /// <exception cref="ArgumentException">Thrown when either targetSize or initialSize is not a positive integer.</exception>
        public DLAMatrix(int targetSize, int initialSize = 2)
        {
            if (targetSize <= 0 || initialSize < 0)
            {
                throw new ArgumentException("Size must be a positive integer.");
            }

            this.targetSize = targetSize;
            matrixSize = initialSize;
            aggregatedPoints = new List<Point>();
            matrix = InitializeMatrix();

            heightMap = new HeightMapGenerator();
        }


        private Point[,] InitializeMatrix()
        {
            center = (matrixSize / 2) - 1;
            Point[,] newMatrix = new Point[matrixSize, matrixSize];
            for (int i = 0; i < matrixSize; i++)
                for (int j = 0; j < matrixSize; j++)
                    newMatrix[i, j] = new Point(i, j);

            // Seed with initial point and add it to the aggregated list
            var startingPoint = newMatrix[center, center];
            startingPoint.IsAggregated = true;
            aggregatedPoints.Add(startingPoint);

            return newMatrix;
        }

        /// <summary>
        /// Runs the Diffusion-Limited Aggregation (DLA) simulation for the specified number of particles.
        /// </summary>
        /// <param name="particles">The number of particles to simulate. Must be a positive integer.</param>
        public void RunDLA(int particles)
        {
            //Initializes the DLA simulation with the specified number of particles
            //Performs a random walk for each particle, aggregating them to the existing cluster
            for (int i = 0; i < particles; i++)
            {
                RandomWalkAndAggregate();
                //Console.WriteLine(i);
            }
            //Expands the matrix if the current size is less than the target size.
            if(matrixSize < targetSize) 
            {
                ExpandMatrix(targetSize);
            }
            //Updates the height map with the aggregated points.
            heightMap.UpdateHeightMap(aggregatedPoints.ToArray(), matrixSize);
        }

        

        int heightMapUpdateFrequency = 1, heightMapUpdateUpdateIndex = 0;

        /// <summary>
        /// Expands the matrix by the specified expansion size, ensuring it does not exceed the target size.
        /// </summary>
        /// <param name="expansionSize">The number of cells to expand the matrix by. Default is 10.</param>
        private void ExpandMatrix(int expansionSize = 10)
        {
            Console.WriteLine(matrixSize);

            int newSize = Math.Min(matrixSize + expansionSize  , targetSize); // Ensure we don't exceed target size
            var newMatrix = new Point[newSize, newSize];
            int newCenter = newSize / 2;
            int offset = newCenter - center;

            // Initialize the new matrix with points
            for (int i = 0; i < newSize; i++)
                for (int j = 0; j < newSize; j++)
                    newMatrix[i, j] = new Point(i, j);

            // Update positions of existing aggregated points and place them in the new matrix
            foreach (var point in aggregatedPoints)
            {
                // Update point coordinates
                point.X += offset;
                point.Y += offset;

                // Place the updated point in the new matrix
                newMatrix[point.X, point.Y] = point;
            }

            matrix = newMatrix;
            matrixSize = newSize;
            center = newCenter;
            if(heightMapUpdateUpdateIndex % heightMapUpdateFrequency == 0) heightMap.UpdateHeightMap(aggregatedPoints.ToArray(), matrixSize);;
            heightMapUpdateUpdateIndex++;
        }

        
        private (int x, int y) GetRandomStartingEdgePosition()
        {
            int x, y;
            int edge = random.Next(4); // 0 = left, 1 = right, 2 = top, 3 = bottom

            switch (edge)
            {
                case 0: // Left edge
                    x = 0;
                    y = random.Next(0, matrixSize);
                    break;
                case 1: // Right edge
                    x = matrixSize - 1;
                    y = random.Next(0, matrixSize);
                    break;
                case 2: // Top edge
                    x = random.Next(0, matrixSize);
                    y = 0;
                    break;
                default: // Bottom edge
                    x = random.Next(0, matrixSize);
                    y = matrixSize - 1;
                    break;
            }

            return (x, y);
        }


        /// <summary>
        /// Performs a random walk and aggregates the particle to the existing cluster.
        /// </summary>
        private void RandomWalkAndAggregate()
        {
            //(int x, int y) = GetRandomStartingEdgePosition();
            (int x, int y) = (random.Next(0, matrixSize),random.Next(0, matrixSize));
            while (true)
            {
                if (IsAdjacentToAggregate(x, y, out Point? parent))
                {
                    aggregatedPoints.Add(matrix[x, y]);
                    matrix[x, y].IsAggregated = true;
                    matrix[x, y].Parent = parent;

                    // Check if expansion is needed based on proximity to the matrix border
                    if (x <= 1 || x >= matrixSize - 2 || y <= 1 || y >= matrixSize - 2)
                    {
                        if (matrixSize < targetSize)
                            ExpandMatrix();
                    }
                    break;
                }

                int direction = GetRandomDirection(x, y);

                switch (direction)
                {
                    case 0: x = (x > 0) ? x - 1 : x; break;
                    case 1: x = (x < matrixSize - 1) ? x + 1 : x; break;
                    case 2: y = (y > 0) ? y - 1 : y; break;
                    case 3: y = (y < matrixSize - 1) ? y + 1 : y; break;
                }
            }
        }



        private int GetRandomDirection(int x, int y)
        {
            int direction = random.Next(4);

            if (random.NextDouble() < 0.03)
            {
                int x1 = -1, y1=-1;
                if     (x < center && direction == 0) x1 = 1;
                else if(x > center && direction == 1) x1 = 0;
                if(y < center && direction == 2) y1 = 3;
                else if(y > center && direction == 3) y1 = 2;

                switch(random.NextDouble())
                { 
                    case <.5 : 
                        if(x1 != -1)direction = x1;
                        break;
                    case >= .5:
                        if(y1!= -1) direction = y1;
                        break;
                }

            }
            return direction; 
        }


        private bool IsAdjacentToAggregate(int x, int y, out Point? parent)
        {
            if(!matrix[x,y].IsAggregated)
            {
                if (x > 0 && matrix[x - 1, y].IsAggregated) { parent = matrix[x - 1, y]; return true; }
                if (x < matrixSize - 1 && matrix[x + 1, y].IsAggregated) { parent = matrix[x + 1, y]; return true; }
                if (y > 0 && matrix[x, y - 1].IsAggregated) { parent = matrix[x, y - 1]; return true; }
                if (y < matrixSize - 1 && matrix[x, y + 1].IsAggregated) { parent = matrix[x, y + 1]; return true; }
            }
            parent = null;
            return false;
        }




        ///////////////////////////////////////////////////////////
        //// OUTPUTS //////////////////////////////////////////////
        ///////////////////////////////////////////////////////////

        public void SaveAsBitmap(string filePath)
        {

            using (Bitmap bitmap = new Bitmap(matrixSize, matrixSize))
            {
                var graphics = Graphics.FromImage(bitmap);
                graphics.Clear(Color.White);
                
                foreach(Point point in aggregatedPoints)
                {
                    bitmap.SetPixel(point.X, point.Y, Color.Black);
                }
                        
                bitmap.Save(filePath);
            }
            SaveAsGrayscaleImage(filePath);
        }

        /// <summary>
        /// Saves the current matrix as a grayscale image. The image is normalized using square root normalization,
        /// and the height values are scaled to the range of 0-255 for grayscale representation.
        /// </summary>
        /// <param name="filename">The name of the file where the grayscale image will be saved.</param>
        public void SaveAsGrayscaleImage(string filename)
        {
            heightMap.SaveAsGrayscaleImage("final_heightmap.png");
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            // Find the maximum height for normalization
            int maxHeight = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (matrix[x, y].Height > maxHeight)
                        maxHeight = matrix[x, y].Height;
                }
            }

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Apply square root normalization
                        double normalizedHeight = (double)matrix[x, y].Height / maxHeight;
                        normalizedHeight = Math.Sqrt(normalizedHeight);

                        // Scale to 0-255 for grayscale
                        int grayscaleValue = (int)(normalizedHeight * 255);
                        grayscaleValue = Math.Clamp(grayscaleValue, 0, 255);

                        // Set pixel color
                        Color color = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                bitmap.Save(filename, ImageFormat.Png);
            }
        }

    }
}