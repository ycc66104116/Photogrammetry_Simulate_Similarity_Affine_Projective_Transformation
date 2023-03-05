using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Single;

namespace ImageProcessing
{ 

        public class ImagePoints
        {
            public string ImageName { get; set; }
            public List<Coordinate> PointList { get; set; }
            public ImagePoints()
            {
                PointList = new List<Coordinate>();
            }
           

            public class Coordinate
            {
                private float x, y, vx, vy;
                private CoordinateFormat format;
                public bool IsDistortion;

                public string Label { get; set; }
                public float X { get => x; set => x = value; }
                public float Y { get => y; set => y = value; }
                public float Vx { get => vx; set => vx = value; }
                public float Vy { get => vy; set => vy = value; }
                public CoordinateFormat Format { get => format; set => format = value; }

                public Coordinate()
                {

                }
                public Coordinate(float X, float Y, CoordinateFormat Format)
                {
                    this.X = X;
                    this.Y = Y;
                    this.Format = Format;
                   
                }
          

            

    
                public static Coordinate operator *(Coordinate ImageCoordinate, float Scale)
                {
                    ImageCoordinate.X = ImageCoordinate.X * Scale;
                    ImageCoordinate.Y = ImageCoordinate.Y * Scale;
                    return ImageCoordinate;
                }
                public static Coordinate operator /(Coordinate ImageCoordinate, float Scale)
                {
                    ImageCoordinate.X = ImageCoordinate.X / Scale;
                    ImageCoordinate.Y = ImageCoordinate.Y / Scale;
                    return ImageCoordinate;
                }
                public static Coordinate operator +(Coordinate ImageCoordinate, float Shift)
                {
                    ImageCoordinate.X = ImageCoordinate.X + Shift;
                    ImageCoordinate.Y = ImageCoordinate.Y + Shift;
                    return ImageCoordinate;
                }



            }
            
        }

       
 

    
    }
  

