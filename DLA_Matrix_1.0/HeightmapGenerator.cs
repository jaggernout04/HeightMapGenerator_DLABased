using System.Drawing;
using System.Drawing.Imaging;

namespace DLA_Matrix
{
    public class HeightMapGenerator
    {
        private int[,] heightMapMatrix;  // The final map matrix that gets updated progressively
        private int currentSize;

        /// <summary>
        /// Initializes a new instance of the <see cref="HeightMapGenerator"/> class.
        /// </summary>
        /// <param name="initialSize">The initial size of the height map matrix. Default value is 2.</param>
        /// <exception cref="ArgumentException">Thrown when the initial size is not a positive integer.</exception>
        public HeightMapGenerator(int initialSize = 2)
        {
            if (initialSize <= 0)
            {
                throw new ArgumentException("Size must be a positive integer.");
            }
            currentSize = initialSize;
            heightMapMatrix = new int[,]{
                                        {0,0},
                                        {0,0}
                                    };
        }



        
        /// <summary>
        /// Calculates the heights of the given points in the height map matrix.
        /// </summary>
        /// <param name="points">An array of points for which the heights need to be calculated.</param>
        public void CalculateHeights(Point[] points)
        {
            /*for(int i = 0; i < points.Length; i++)
            {
                points[i].Height = 1;
            }*/
            // Traverse the points in reverse to calculate heights
            for (int i = points.Length - 1; i >= 0; i--)
            {
                Point point = points[i];
                while(point.Parent != null && point.Parent.Height <= point.Height + 1)
                {
                    point.Parent.Height = point.Height + 1;
                    point = point.Parent;
                }
            }
        }


        
        /// <summary>
        /// Creates a new matrix with the given points and size.
        /// </summary>
        /// <param name="points">An array of points to be included in the new matrix.</param>
        /// <param name="newSize">The size of the new matrix.</param>
        /// <returns>A new matrix of size <paramref name="newSize"/> with the heights of the given points.</returns>
        private int[,] CreateNewMatrixWithPoints(Point[] points, int newSize)
        {
            int[,] newMatrix = new int[newSize, newSize];

            foreach (var point in points)
            {
                if (point.X < newSize && point.Y < newSize)
                {
                    newMatrix[point.X, point.Y] = point.Height;
                }
            }

            return newMatrix;
        }


        
        /// <summary>
        /// Scales the given matrix to a new size using bilinear interpolation.
        /// </summary>
        /// <param name="oldMatrix">The original matrix to be scaled.</param>
        /// <param name="oldSize">The size of the original matrix.</param>
        /// <param name="newSize">The size of the new matrix.</param>
        /// <returns>A new matrix of size <paramref name="newSize"/>, scaled from the original matrix.</returns>
        private int[,] ScaleMatrix(int[,] oldMatrix, int oldSize, int newSize)
        {
            int[,] scaledMatrix = new int[newSize, newSize];
            double scale = (double)newSize / oldSize;

            for (int y = 0; y < newSize; y++)
            {
                for (int x = 0; x < newSize; x++)
                {
                    double gx = x / scale;
                    double gy = y / scale;
                    int gxi = (int)gx;
                    int gyi = (int)gy;

                    // Clamp to boundaries
                    gxi = Math.Min(gxi, oldSize - 2);
                    gyi = Math.Min(gyi, oldSize - 2);

                    // Perform bilinear interpolation
                    int c00 = oldMatrix[gxi, gyi];
                    int c10 = oldMatrix[gxi + 1, gyi];
                    int c01 = oldMatrix[gxi, gyi + 1];
                    int c11 = oldMatrix[gxi + 1, gyi + 1];

                    double tx = gx - gxi;
                    double ty = gy - gyi;

                    int interpolatedHeight = (int)Blerp(c00, c10, c01, c11, tx, ty);
                    scaledMatrix[x, y] = interpolatedHeight;
                }
            }

            return scaledMatrix;
        }


        // Bilinear interpolation helper
        private double Blerp(double c00, double c10, double c01, double c11, double tx, double ty)
        {
            return Lerp(Lerp(c00, c10, tx), Lerp(c01, c11, tx), ty);
        }

        private double Lerp(double s, double e, double t)
        {
            return s + (e - s) * t;
        }

        //////////////////////////////////////////

        
        /// <summary>
        /// Merges two matrices by adding their corresponding elements.
        /// </summary>
        /// <param name="baseMatrix">The base matrix to be merged.</param>
        /// <param name="detailMatrix">The detail matrix to be merged.</param>
        /// <param name="size">The size of the resulting merged matrix.</param>
        /// <returns>A new matrix of size <paramref name="size"/>, where each element is the sum of corresponding elements from the base and detail matrices.</returns>
        private int[,] MergeMatrices(int[,] baseMatrix, int[,] detailMatrix, int size)
        {
            int[,] mergedMatrix = new int[size, size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    mergedMatrix[x, y] = baseMatrix[x, y] + detailMatrix[x, y];
                }
            }

            return mergedMatrix;
        }

        int[,] kernel = {       { 3,    13, 22, 13, 3 },

                                { 13,   60, 98, 60, 13}, 
                                { 22,   98, 162,98, 22},
                                { 13,   60, 98, 60, 13},
                                { 3,    13, 22, 13, 3 }
                                 };
        int kernelSum = 946;

        /// <summary>
        /// Applies a convolution operation to the given matrix using a specified kernel.
        /// </summary>
        /// <param name="matrix">The input matrix to apply the convolution operation.</param>
        /// <returns>A new matrix resulting from the convolution operation.</returns>
        private int[,] ApplyConvolution(int[,] matrix)
        {
            int[,] tempMatrix = (int[,])matrix.Clone();

            // Iterate over the matrix, excluding the border pixels
            for (int y = 2; y < currentSize - 2; y++)
            {
                for (int x = 2; x < currentSize - 2; x++)
                {
                    int sum = 0;

                    // Apply the kernel to the surrounding pixels
                    for (int ky = -2; ky <= 2; ky++)
                    {
                        for (int kx = -2; kx <= 2; kx++)
                        {
                            sum += matrix[x + kx, y + ky] * kernel[ky + 2, kx + 2];
                        }
                    }

                    // Store the result in the temporary matrix
                    tempMatrix[x, y] = sum / kernelSum;
                }
            }

            // Return the modified matrix
            return tempMatrix;
        }


        
        /// <summary>
        /// Updates the height map matrix with the given points and new size.
        /// </summary>
        /// <param name="points">An array of points to be included in the height map matrix.</param>
        /// <param name="newSize">The new size of the height map matrix.</param>
        public void UpdateHeightMap(Point[] points, int newSize)
        {
            // Calculate heights for the given points
            CalculateHeights(points);

            // Create a new matrix with the given points and new size
            var newMatrixWithPoints = CreateNewMatrixWithPoints(points, newSize);

            // Initialize the scaled matrix with the current height map matrix
            int[,] scaledMatrix = heightMapMatrix;

            // If the new size is different from the current size, scale the matrix
            if (newSize != currentSize)
            {
                scaledMatrix = ScaleMatrix(heightMapMatrix, currentSize, newSize);
            }

            // Merge the scaled matrix and the new matrix with points
            var mergedMatrix = MergeMatrices(scaledMatrix, newMatrixWithPoints, newSize);

            // Apply convolution operation to the merged matrix
            mergedMatrix = ApplyConvolution(mergedMatrix);

            // Update the height map matrix with the merged matrix and new size
            heightMapMatrix = mergedMatrix;
            currentSize = newSize;
        }





        /// <summary>
        /// Saves the current height map matrix as a grayscale image.
        /// </summary>
        /// <param name="filename">The name of the file where the image will be saved.</param>
        public void SaveAsGrayscaleImage(string filename)
        {
            // Apply convolution operation 10 times to smooth the image
            for(int i = 0; i < 10; i++) heightMapMatrix = ApplyConvolution(heightMapMatrix);

            int width = heightMapMatrix.GetLength(0);
            int height = heightMapMatrix.GetLength(1);

            // Find the maximum height for normalization
            int maxHeight = 0;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (heightMapMatrix[x, y] > maxHeight)
                        maxHeight = heightMapMatrix[x, y];
                }
            }

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Apply square root normalization
                        double normalizedHeight = (double)heightMapMatrix[x, y] / maxHeight;
                        normalizedHeight = Math.Sqrt(normalizedHeight);

                        // Scale to 0-255 for grayscale
                        int grayscaleValue = (int)(normalizedHeight * 255);
                        grayscaleValue = Math.Clamp(grayscaleValue, 0, 255);

                        // Set pixel color
                        Color color = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                // Save the bitmap as an image file in PNG format
                bitmap.Save(filename, ImageFormat.Png);
            }
        }


        /*public void SaveAsGrayscaleImage(string filename)
        {
            int width = heightMapMatrix.GetLength(0);
            int height = heightMapMatrix.GetLength(1);

            using (Bitmap bitmap = new Bitmap(width, height))
            {
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Calculate the clamped height value
                        double normalizedHeight = 1 - (1.0 / (1 + Math.Pow(heightMapMatrix[x, y],0.2f)));
                        // Scale to 0-255 for grayscale
                        int grayscaleValue = (int)(normalizedHeight * 255);
                        grayscaleValue = Math.Clamp(grayscaleValue, 0, 255);

                        // Set pixel color
                        Color color = Color.FromArgb(grayscaleValue, grayscaleValue, grayscaleValue);
                        bitmap.SetPixel(x, y, color);
                    }
                }

                // Save the bitmap as an image file
                bitmap.Save(filename, ImageFormat.Png);
            }
        }*/


    
    }
    
}
