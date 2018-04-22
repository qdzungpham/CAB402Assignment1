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
        private Segment[,] pixelMap;
        private int N;
        private float threshold;

        public SegmentationService(TiffImage image, int N, float threshold)
        {
            segmentation = new Dictionary<Segment, Segment>();
            this.N = N;
            this.threshold = threshold;

            var imageSize = (int)Math.Pow(2, N);
            pixelMap = new Segment[imageSize, imageSize];

            for (var x = 0; x < imageSize; x++)
            {
                for (var y = 0; y < imageSize; y++)
                {
                    var coordinate = new Coordinate { X = x, Y = y };
                    pixelMap[x, y] = new Pixel(coordinate, image.getColourBands(x, y));
                }
            }

            GrowUntilNoChange();
        }

        public Segment Segmentation(int x, int y)
        {
            return FindRoot(MapCoordinateToSegment(new Coordinate { X = x, Y = y }));
        }

        public Segment FindRoot(Segment segment)
        {
            Segment immediateParent;

            if (segmentation.ContainsKey(segment))
            {
                immediateParent = FindRoot(segmentation[segment]);
            }
            else
            {
                immediateParent = segment;
            }

            return immediateParent;

        }

        public Segment MapCoordinateToSegment(Coordinate coordinate)
        {
            return pixelMap[coordinate.X, coordinate.Y];
        }

        public HashSet<Segment> GetNeighbouringSegments(Segment givenSegment)
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


            for (int i = 0; i < segments.Count; i++)
            {
                segments[i] = FindRoot(segments[i]);
            }

            HashSet<Segment> result = new HashSet<Segment>(segments);

            result.Remove(givenSegment);

            return result;


        }


        private HashSet<Segment> GetBestNeighbouringSegments(Segment givenSegment)
        {
            HashSet<Segment> neighbours = GetNeighbouringSegments(givenSegment);
            var result = new HashSet<Segment>();
            if (!neighbours.Any())
            {
                return result;
            }

            var segmentsAndMergeCosts = new Dictionary<Segment, float>();

            foreach (var neighbour in neighbours)
            {
                segmentsAndMergeCosts.Add(neighbour, SegmentService.GetMergeCost(givenSegment, neighbour));
            }

            var bestMergeCost = segmentsAndMergeCosts.OrderBy(x => x.Value).First().Value;


            return segmentsAndMergeCosts.Where(pair => pair.Value == bestMergeCost && pair.Value <= threshold).Select(pair => pair.Key).ToHashSet();
        }

        private void TryGrowOneSegment(Coordinate coordinate)
        {
            var segmentA = FindRoot(MapCoordinateToSegment(coordinate));
            var segmentABestNeighbours = GetBestNeighbouringSegments(segmentA);

            HashSet<Segment> mutallyOptimalNeighbours = new HashSet<Segment>();

            if (segmentABestNeighbours.Count != 0)
            {
                foreach (var segmentAbestNeighbour in segmentABestNeighbours)
                {
                    HashSet<Segment> segmentBBestNeighbours = GetBestNeighbouringSegments(FindRoot(segmentAbestNeighbour));
                    if (segmentBBestNeighbours.Contains(segmentA))
                    {
                        mutallyOptimalNeighbours.Add(segmentAbestNeighbour);
                    }
                }
            }

            if (mutallyOptimalNeighbours.Count == 0)
            {
                if (segmentABestNeighbours.Count != 0)
                {
                    TryGrowOneSegment(segmentABestNeighbours.First().GetSegmentCoordinates().First());
                }
            }
            else
            {
                Segment mutallyOptimalNeighbour = mutallyOptimalNeighbours.First();
                Segment mergeSegment = new Parent(segmentA, mutallyOptimalNeighbour);
                segmentation[mutallyOptimalNeighbour] = mergeSegment;
                segmentation[segmentA] = mergeSegment;
                //segmentation.Add(mutallyOptimalNeighbour, mergeSegment);
                //segmentation.Add(segmentA, mergeSegment);
            }
        }

        private void TryGrowAllCoordinates()
        {
            var coordinates = DitherModule.GetDitherCoordinates(N);

            foreach (var coordinate in coordinates)
            {
                TryGrowOneSegment(coordinate);
            }
        }




        private void GrowUntilNoChange()
        {
            var currentSegmentation = new Dictionary<Segment, Segment>(segmentation);

            TryGrowAllCoordinates();

            if (currentSegmentation.Count != segmentation.Count)
            {
                GrowUntilNoChange();
            }
        }




    }
}
