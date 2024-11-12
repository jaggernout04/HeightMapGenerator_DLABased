using DLA_Matrix;

public class Program
{
    public static void Main()
    {
        int size = 1000;
        int particles =50000;

        DLAMatrix dla = new DLAMatrix(size);
        dla.RunDLA(particles);

        string filePath = "DLA_Output.bmp";
        dla.SaveAsGrayscaleImage(filePath);


        Console.WriteLine($"Matrix saved as bitmap at {filePath}");
    }
}
