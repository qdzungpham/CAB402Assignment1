using CSharpSegmenter.Helpers;
using CSharpSegmenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSegmenter.Services
{
    public class SegmentationService
    {
        private Dictionary<Segment, Segment> segmentation;
        private Image image;

        public SegmentationService(Image image)
        {
            segmentation = new Dictionary<Segment, Segment>();
            this.image = image;
        }

        public Segment FindRoot(Segment segment)
        {
            var immediateParent = segment;
            if (segmentation.ContainsKey(segment))
            {
                immediateParent = segmentation[segment];
            }

            if (segmentation.ContainsKey(immediateParent))
            {
                return FindRoot(immediateParent);
            }
            else
            {
                return immediateParent;
            }

        }

        private Pixel MapCoordinateToSegment(Coordinate coordinate)
        {
            return new Pixel(coordinate, TiffModule.GetColourBands(image, coordinate));
        }

        private List<Segment> GetNeighbouringSegments(Segment givenSegment, int N)
        {
            List<Coordinate> coordinates = givenSegment.GetSegmentCoordinates();

            List<Coordinate> neighbouringCoordinates = new List<Coordinate>();

            foreach (Coordinate coordinate in coordinates)
            {
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X - 1, Y = coordinate.Y });
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X + 1, Y = coordinate.Y });
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X, Y = coordinate.Y - 1 });
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X, Y = coordinate.Y + 1 });
            }

            neighbouringCoordinates = neighbouringCoordinates
                .Where(coordinate => coordinate.X >= 0 && coordinate.Y >= 0 && coordinate.X < Math.Pow(2, N) && coordinate.Y < Math.Pow(2, N))
                .ToList();

            List<Segment> segments = new List<Segment>();
            foreach (Coordinate coordinate in neighbouringCoordinates)
            {
                segments.Add(MapCoordinateToSegment(coordinate));
            }


            ////////////////////// WARNING//////////////////////
            //havent filter and remove given segment


            for (int i = 0; i < segments.Count; i++)
            {
                segments[i] = FindRoot(segments[i]);
            }

            return segments;


        }

        //private List<Segment> GetBestNeighbouringSegments(List<Segment> neighbours, float threshold)
        //{
        //    var segmentsAndMergeCosts = new List<KeyValuePair<Segment, float>>();

        //}


    }
}
