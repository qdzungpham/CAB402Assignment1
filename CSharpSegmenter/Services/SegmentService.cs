using CSharpSegmenter.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpSegmenter.Services
{
    public static class SegmentService
    {
        private static List<List<byte>> ExtractColourBands(List<byte[]> colours)
        {
            var numColourBands = colours[0].Length;

            var result = new List<List<byte>>();

            for (int i = 0; i < numColourBands; i++)
            {
                result.Add(new List<byte>());
            }
            foreach (var colour in colours)
            {
                for (int i = 0; i < numColourBands; i++)
                {
                    result[i].Add(colour[i]);
                }
            }

            return result;
        }

        private static float CalculateStddev(float[] input)
        {
            float average = input.Average();
            float sumOfSquaresOfDifferences = input.Select(val => (val - average) * (val - average)).Sum();
            return (float)Math.Sqrt(sumOfSquaresOfDifferences / input.Length);

        }

        // return the sum of the standard deviations of the individual segments
        private static float GetSumStddevOfAllBands(Segment segment)
        {
            var colourList = segment.GetSegmentColours();
            var colourBandList = ExtractColourBands(colourList);

            var result = 0.0F;

            foreach (var colourBand in colourBandList)
            {
                var colourBandFloat = new float[colourBand.Count];

                for (int i = 0; i < colourBand.Count; i++)
                {
                    colourBandFloat[i] = colourBand[i];
                }
                result += CalculateStddev(colourBandFloat);
            }

            return result;
        }

        // determine the cost of merging the given segments: 
        // equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments, 
        // weighted by their respective sizes and summed over all colour bands
        public static float GetMergeCost(Segment segment1, Segment segment2)
        {
            var segment1SumStddev = GetSumStddevOfAllBands(segment1);
            var segment2SumStddev = GetSumStddevOfAllBands(segment2);

            var combinedSegment = new Parent(segment1, segment2);
            var combinedSegmentSumStddev = GetSumStddevOfAllBands(combinedSegment);

            var result = (combinedSegmentSumStddev * combinedSegment.GetNumPixels()) - 
                (segment1SumStddev * segment1.GetNumPixels() + segment2SumStddev * segment2.GetNumPixels());

            return result;
        }
    }
}
