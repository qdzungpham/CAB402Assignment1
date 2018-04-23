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

        // Contructor
        public SegmentationService(TiffImage image, int N, float threshold)
        {
            segmentation = new Dictionary<Segment, Segment>();
            this.N = N;
            this.threshold = threshold;

            var imageSize = (int)Math.Pow(2, N);
            pixelMap = new Segment[imageSize, imageSize];

            // init pixel map
            for (var x = 0; x < imageSize; x++)
            {
                for (var y = 0; y < imageSize; y++)
                {
                    var coordinate = new Coordinate { X = x, Y = y };
                    pixelMap[x, y] = new Pixel(coordinate, image.getColourBands(x, y));
                }
            }

            // run the whole algorithm
            GrowUntilNoChange();
        }

        // find root segmentation 
        public Segment Segmentation(int x, int y)
        {
            return FindRoot(MapCoordinateToSegment(new Coordinate { X = x, Y = y }));
        }

        // Find the largest/top level segment that the given segment is a part of(based on the current segmentation)
        private Segment FindRoot(Segment segment)
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

        // look up segment object based on coordinate
        private Segment MapCoordinateToSegment(Coordinate coordinate)
        {
            return pixelMap[coordinate.X, coordinate.Y];
        }

        // Find the neighbouring segments of the given segment (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
        // Note: this is a higher order function which given a pixelMap function and a size N, 
        // returns a function which given a current segmentation, returns the set of Segments which are neighbours of a given segment
        private HashSet<Segment> GetNeighbouringSegments(Segment givenSegment)
        {
            List<Coordinate> coordinates = givenSegment.GetSegmentCoordinates();
            
            List<Coordinate> neighbouringCoordinates = new List<Coordinate>();

            // add all possible coordinates
            foreach (Coordinate coordinate in coordinates)
            {
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X - 1, Y = coordinate.Y });
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X + 1, Y = coordinate.Y });
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X, Y = coordinate.Y - 1 });
                neighbouringCoordinates.Add(new Coordinate { X = coordinate.X, Y = coordinate.Y + 1 });
            }

            // filter out neighbours
            neighbouringCoordinates = neighbouringCoordinates
                .Where(coordinate => coordinate.X >= 0 && coordinate.Y >= 0 && coordinate.X < Math.Pow(2, N) && coordinate.Y < Math.Pow(2, N))
                .ToList();


            // map each neighbouring coordinates to root segments
            List<Segment> segments = new List<Segment>();
            foreach (Coordinate coordinate in neighbouringCoordinates)
            {
                segments.Add(FindRoot(MapCoordinateToSegment(coordinate)));
            }

            // convert to hashset to remove duplicates
            HashSet<Segment> result = new HashSet<Segment>(segments);

            // dont want the input segment
            result.Remove(givenSegment);

            return result;


        }

        // Find the neighbour(s) of the given segment that has the (equal) best merge cost
        // (exclude neighbours if their merge cost is greater than the threshold)
        private HashSet<Segment> GetBestNeighbouringSegments(Segment givenSegment)
        {
            HashSet<Segment> neighbours = GetNeighbouringSegments(givenSegment);
            var result = new HashSet<Segment>();
            if (!neighbours.Any())
            {
                return result;
            }

            // dictionary to store neighbouring segments and their coressponding merge cost
            var segmentsAndMergeCosts = new Dictionary<Segment, float>();

            foreach (var neighbour in neighbours)
            {
                segmentsAndMergeCosts.Add(neighbour, SegmentService.GetMergeCost(givenSegment, neighbour));
            }

            // extract the best merge cost here
            var bestMergeCost = segmentsAndMergeCosts.OrderBy(x => x.Value).First().Value;

            // return the best segments based on their merge cost 
            return segmentsAndMergeCosts.Where(pair => pair.Value == bestMergeCost && pair.Value <= threshold).Select(pair => pair.Key).ToHashSet();
        }

        // Try to find a neighbouring segmentB such that:
        //     1) segmentB is one of the best neighbours of segment A, and 
        //     2) segmentA is one of the best neighbours of segment B
        // if such a mutally optimal neighbour exists then merge them,
        // otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
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
            }
        }

        // Try to grow the segments corresponding to every pixel on the image in turn 
        // (considering pixel coordinates in special dither order)
        private void TryGrowAllCoordinates()
        {
            var coordinates = DitherModule.GetDitherCoordinates(N);

            foreach (var coordinate in coordinates)
            {
                TryGrowOneSegment(coordinate);
            }
        }

        // Keep growing segments as above until no further merging is possible
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
