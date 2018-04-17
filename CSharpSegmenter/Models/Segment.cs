using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSegmenter.Models
{
    public class Segment 
    {
        public Segment()
        {

        }

        public List<Coordinate> Coordinate { get; protected set; }
        public List<byte[]> Colour { get; protected set; }

    }

    public class Pixel : Segment
    {
        

        public Pixel(Coordinate coordinate, byte[] colour)
        {
            Coordinate = new List<Coordinate>() { coordinate };
            Colour = new List<byte[]>() { colour };
        }
    }

    public class Parent : Segment
    {
        public Pixel Pixel1 { get; private set; }
        public Pixel Pixel2 { get; private set; }

        public Parent(Pixel pixel1, Pixel pixel2)
        {
            Coordinate = new List<Coordinate>();
            Coordinate.AddRange(pixel1.Coordinate);
            Coordinate.AddRange(pixel2.Coordinate);

            Colour = new List<byte[]>();
            Colour.AddRange(pixel1.Colour);
            Colour.AddRange(pixel2.Colour);
        }
    }
}
