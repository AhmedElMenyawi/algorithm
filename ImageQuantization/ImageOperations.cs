using System;
using System.Windows;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using Priority_Queue;

///Algorithms Project
///Intelligent Scissors
///

namespace ImageQuantization
{
    /// <summary>
    /// Holds the pixel color in 3 byte values: red, green and blue
    /// </summary>
    public struct RGBPixel
    {
        public byte red, green, blue;
        
      public  void Default ()
        {
            red = 0;
            green = 0;
            blue = 0; 
        }
        
    }
    public struct RGBPixelD
    {
        public double red, green, blue;
    }


    public struct Edge 
    {
        public
        double From , To;
       public double Distance;
        
      public  Edge Set( double from , double to , double distance )
        {
            this.From = from;
            this.To = to;
            this.Distance = distance;
            return this;
        }
        
    }

    
    /// <summary>
    /// Library of static functions that deal with images
    /// </summary>
    /// 

    
    
    public class ImageOperations
    {

        /// <summary>
        /// Open an image and load it into 2D array of colors (size: Height x Width)
        /// </summary>
        /// <param name="ImagePath">Image file path</param>
        /// <returns>2D array of colors</returns>
        public static RGBPixel[,] OpenImage(string ImagePath)
        {
            Bitmap original_bm = new Bitmap(ImagePath);
            int Height = original_bm.Height;
            int Width = original_bm.Width;

            RGBPixel[,] Buffer = new RGBPixel[Height, Width];

            unsafe
            {
                BitmapData bmd = original_bm.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, original_bm.PixelFormat);
                int x, y;
                int nWidth = 0;
                bool Format32 = false;
                bool Format24 = false;
                bool Format8 = false;

                if (original_bm.PixelFormat == PixelFormat.Format24bppRgb)
                {
                    Format24 = true;
                    nWidth = Width * 3;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format32bppArgb || original_bm.PixelFormat == PixelFormat.Format32bppRgb || original_bm.PixelFormat == PixelFormat.Format32bppPArgb)
                {
                    Format32 = true;
                    nWidth = Width * 4;
                }
                else if (original_bm.PixelFormat == PixelFormat.Format8bppIndexed)
                {
                    Format8 = true;
                    nWidth = Width;
                }
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (y = 0; y < Height; y++)
                {
                    for (x = 0; x < Width; x++)
                    {
                        if (Format8)
                        {
                            Buffer[y, x].red = Buffer[y, x].green = Buffer[y, x].blue = p[0];
                            p++;
                        }
                        else
                        {
                            Buffer[y, x].red = p[2];
                            Buffer[y, x].green = p[1];
                            Buffer[y, x].blue = p[0];
                            if (Format24) p += 3;
                            else if (Format32) p += 4;
                        }
                    }
                    p += nOffset;
                }
                original_bm.UnlockBits(bmd);
            }

            return Buffer;
        }

        /// <summary>
        /// Get the height of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Height</returns>
        public static int GetHeight(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(0);
        }

        /// <summary>
        /// Get the width of the image 
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <returns>Image Width</returns>
        public static int GetWidth(RGBPixel[,] ImageMatrix)
        {
            return ImageMatrix.GetLength(1);
        }

        /// <summary>
        /// Display the given image on the given PictureBox object
        /// </summary>
        /// <param name="ImageMatrix">2D array that contains the image</param>
        /// <param name="PicBox">PictureBox object to display the image on it</param>
        public static void DisplayImage(RGBPixel[,] ImageMatrix, PictureBox PicBox)
        {
            // Create Image:
            //==============
            int Height = ImageMatrix.GetLength(0);
            int Width = ImageMatrix.GetLength(1);

            Bitmap ImageBMP = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);

            unsafe
            {
                BitmapData bmd = ImageBMP.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadWrite, ImageBMP.PixelFormat);
                int nWidth = 0;
                nWidth = Width * 3;
                int nOffset = bmd.Stride - nWidth;
                byte* p = (byte*)bmd.Scan0;
                for (int i = 0; i < Height; i++)
                {
                    for (int j = 0; j < Width; j++)
                    {
                        p[2] = ImageMatrix[i, j].red;
                        p[1] = ImageMatrix[i, j].green;
                        p[0] = ImageMatrix[i, j].blue;
                        p += 3;
                    }

                    p += nOffset;
                }
                ImageBMP.UnlockBits(bmd);
            }
            PicBox.Image = ImageBMP;
        }

        /// <summary>
        /// Counts The Number Of Unique Colors In The Picture
        /// </summary>
        /// <param name="Possible_Combinations">3D Array For All The Possible Combinations For R,G,B Colors In One Pixel</param>
        /// <param name="Colors">List Contains Distinct Colors In The Image</param>
        /// <param name="Width">Image Width</param>
        /// <param name="Height">Image Height</param>
        public static List<RGBPixel> GetDistinctColors (RGBPixel[,] ImageMatrix)
        {

            bool[,,] Possible_Combinations = new bool[256, 256, 256];
            List<RGBPixel> Colors = new List<RGBPixel>();
            int Width = ImageOperations.GetWidth(ImageMatrix);
            int Height =  ImageOperations.GetHeight(ImageMatrix); 
           
            for (int i = 0; i < Height ; i++)
            {
                for (int j = 0 ; j < Width; j++)
                {
                    int Red =  ImageMatrix[i, j].red  ;
                    int Green = ImageMatrix[i,j].green;
                    int Blue = ImageMatrix[i,j].blue;

                    if ( Possible_Combinations [ Red , Green , Blue ] == false) /* if Combination not found before*/
                    {
                        Colors.Add(ImageMatrix[i, j]);
                        Possible_Combinations[Red , Green , Blue ] = true; /*Check that combination already existed */
                    }
                }
            }

           
            MessageBox.Show(Colors.Count.ToString(),"Number Of Distinct Colors ");
            return Colors;
        }


        /// <summary>
        /// Construct The MST Edge By Edge
        /// </summary>
        /// <param name="MST_Tree"> List Will Contains MST Edges </param>
        /// <param name="Checked_Nodes"> Node That Are Visited Before</param>
        /// <param name="Edges_Cost"> Array Carries The Weight Of The Edges</param>
        /// <param name="Parents_Nodes">Array Carries The Previous Parent Node For The Specific Index Color</param>
        /// <param name="Total_Cost"> Array Carries The Weight Of The Edges</param>
        /// <param name="Current"> </param>
        /// <param name="Distance"> Calculated Distance Between Two Colors Using Euclidean Rule </param>
        /// <param name="MinDistance"> Minimum Distance Found During The Current Calculations</param>
        /// <param name="MinValue"> The To-Node Index</param>
        public static List<Edge> MST(List<RGBPixel> Distinct_Colors)
        {

            List<Edge> MST_Tree = new List<Edge>(); 
            Edge edge = new Edge();
            bool[] Checked_Nodes =  new bool [Distinct_Colors.Count()];
            double[] Edges_Cost = new double[ Distinct_Colors.Count()];
            int[] Parents_Nodes = new int[Distinct_Colors.Count()];
            int Current = 0;
            double Total_Cost = 0;
            double Distance = 0  ;
            double MinDistance = Double.PositiveInfinity; /*Set To Infinty */
            int MinValue = -1; 
           for ( int i = 0; i < Distinct_Colors.Count(); i++)
            {
                Edges_Cost[i] = Double.PositiveInfinity;
                
            }
           
            for (int j = 0; j < Distinct_Colors.Count() - 1; j++)
            {

                Checked_Nodes[Current] = true;
                MinValue = -1;
                MinDistance = Double.PositiveInfinity; 
               for ( int k = 0; k < Distinct_Colors.Count(); k++)
                {

                    if (!Checked_Nodes[k])
                    {
                        Distance = Math.Sqrt(Math.Pow((Distinct_Colors[Current].red - Distinct_Colors[k].red), 2) + Math.Pow((Distinct_Colors[Current].blue - Distinct_Colors[k].blue), 2) + Math.Pow((Distinct_Colors[Current].green - Distinct_Colors[k].green), 2));
                        if (Distance < Edges_Cost[k] )
                        {
                            Edges_Cost[k] = Distance;
                            Parents_Nodes[k] = Current; 
                        }
                        if ( Edges_Cost[k] < MinDistance)
                        {
                            MinDistance = Edges_Cost[k];
                            MinValue = k; 
                        }
                    }
                  
                }
                MST_Tree.Add(edge.Set(Parents_Nodes[MinValue],MinValue, MinDistance) );
                Current = MinValue;
                Total_Cost += MinDistance;
            }
            MessageBox.Show(Total_Cost.ToString(), " Minimum Spanning Tree Cost ");
            return MST_Tree; 
        }
    
    
        /// <summary>
        /// Apply Gaussian smoothing filter to enhance the edge detection 
        /// </summary>
        /// <param name="ImageMatrix">Colored image matrix</param>
        /// <param name="filterSize">Gaussian mask size</param>
        /// <param name="sigma">Gaussian sigma</param>
        /// <returns>smoothed color image</returns>
        public static RGBPixel[,] GaussianFilter1D(RGBPixel[,] ImageMatrix, int filterSize, double sigma)
        {
            int Height = GetHeight(ImageMatrix);
            int Width = GetWidth(ImageMatrix);

            RGBPixelD[,] VerFiltered = new RGBPixelD[Height, Width];
            RGBPixel[,] Filtered = new RGBPixel[Height, Width];


            // Create Filter in Spatial Domain:
            //=================================
            //make the filter ODD size
            if (filterSize % 2 == 0) filterSize++;

            double[] Filter = new double[filterSize];

            //Compute Filter in Spatial Domain :
            //==================================
            double Sum1 = 0;
            int HalfSize = filterSize / 2;
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                //Filter[y+HalfSize] = (1.0 / (Math.Sqrt(2 * 22.0/7.0) * Segma)) * Math.Exp(-(double)(y*y) / (double)(2 * Segma * Segma)) ;
                Filter[y + HalfSize] = Math.Exp(-(double)(y * y) / (double)(2 * sigma * sigma));
                Sum1 += Filter[y + HalfSize];
            }
            for (int y = -HalfSize; y <= HalfSize; y++)
            {
                Filter[y + HalfSize] /= Sum1;
            }

            //Filter Original Image Vertically:
            //=================================
            int ii, jj;
            RGBPixelD Sum;
            RGBPixel Item1;
            RGBPixelD Item2;

            for (int j = 0; j < Width; j++)
                for (int i = 0; i < Height; i++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int y = -HalfSize; y <= HalfSize; y++)
                    {
                        ii = i + y;
                        if (ii >= 0 && ii < Height)
                        {
                            Item1 = ImageMatrix[ii, j];
                            Sum.red += Filter[y + HalfSize] * Item1.red;
                            Sum.green += Filter[y + HalfSize] * Item1.green;
                            Sum.blue += Filter[y + HalfSize] * Item1.blue;
                        }
                    }
                    VerFiltered[i, j] = Sum;
                }

            //Filter Resulting Image Horizontally:
            //===================================
            for (int i = 0; i < Height; i++)
                for (int j = 0; j < Width; j++)
                {
                    Sum.red = 0;
                    Sum.green = 0;
                    Sum.blue = 0;
                    for (int x = -HalfSize; x <= HalfSize; x++)
                    {
                        jj = j + x;
                        if (jj >= 0 && jj < Width)
                        {
                            Item2 = VerFiltered[i, jj];
                            Sum.red += Filter[x + HalfSize] * Item2.red;
                            Sum.green += Filter[x + HalfSize] * Item2.green;
                            Sum.blue += Filter[x + HalfSize] * Item2.blue;
                        }
                    }
                    Filtered[i, j].red = (byte)Sum.red;
                    Filtered[i, j].green = (byte)Sum.green;
                    Filtered[i, j].blue = (byte)Sum.blue;
                }

            return Filtered;
        }


    }
}
