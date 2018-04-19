using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSegmenter.Models
{
    public interface Segment
    {
        List<byte[]> GetSegmentColours();
        List<Coordinate> GetSegmentCoordinates();
        int GetNumPixels();

    }

    public class Pixel : Segment
    {
        private static readonly int numPixels = 1;
        private List<Coordinate> coordinates;
        private List<byte[]> colours;

        public Pixel(Coordinate coordinate, byte[] colour)
        {
            coordinates = new List<Coordinate>();
            colours = new List<byte[]>();

            coordinates.Add(coordinate);
            colours.Add(colour);
        }

        
        public int GetNumPixels()
        {
            return numPixels;
        }

        public List<byte[]> GetSegmentColours()
        {
            return colours;
        }

        public List<Coordinate> GetSegmentCoordinates()
        {
            return coordinates;
        }
    }

    public class Parent : Segment
    {
        private int numPixels;
        private List<Coordinate> coordinates;
        private List<byte[]> colours;

        public Parent(Segment segment1, Segment segment2)
        {
            numPixels = segment1.GetNumPixels() + segment2.GetNumPixels();

            coordinates = new List<Coordinate>();
            colours = new List<byte[]>();

            coordinates.AddRange(segment1.GetSegmentCoordinates());
            coordinates.AddRange(segment2.GetSegmentCoordinates());

            colours.AddRange(segment1.GetSegmentColours());
            colours.AddRange(segment2.GetSegmentColours());
        }

       

        public int GetNumPixels()
        {
            return numPixels;
        }

        public List<byte[]> GetSegmentColours()
        {
            return colours;
        }

        public List<Coordinate> GetSegmentCoordinates()
        {
            return coordinates;
        }
    }
}
