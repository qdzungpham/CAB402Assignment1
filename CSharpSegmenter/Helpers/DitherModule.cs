using CSharpSegmenter.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSegmenter.Helpers
{
    public static class DitherModule
    {
        private static readonly int maxBit = sizeof(int) * 8 - 1;

        private static bool IsBitSet(int number, int position)
        {
            if ((number & (1 << position)) != 0)
            {
                return true;
            }
            return false;
        }


        private static int ReverseBits(int number, int length)
        {
            int currentResult = 0;
            for (int i = 0; i <= length - 1; i++)
            {
                if (IsBitSet(number, i))
                {
                    currentResult = currentResult | (1 << (length - 1 - i));

                }
                else
                {
                    currentResult = currentResult | 0;
                }
            }
            return currentResult;
        }

        private static int ExtractBit(int number, int accumulatedResult, int position)
        {
            int currentResult = accumulatedResult;
            if (IsBitSet(number, position))
            {
                currentResult = currentResult | (1 << (position / 2));
            }
            else
            {
                currentResult = currentResult | 0;
            }
            return currentResult;
        }

        private static int ExtractOddBits(int number)
        {
            int currentResult = 0;
            for (int i = 1; i <= maxBit; i = i + 2)
            {
                currentResult = ExtractBit(number, currentResult, i);
            }
            return currentResult;
        }

        private static int ExtractEvenBits(int number)
        {
            int currentResult = 0;
            for (int i = 0; i <= maxBit; i = i + 2)
            {
                currentResult = ExtractBit(number, currentResult, i);
            }
            return currentResult;
        }

        public static List<Coordinate> GetDitherCoordinates(int size)
        {
            List<Coordinate> ditherOrder = new List<Coordinate>();
            int width = 1 << size;
            int height = 1 << size;
            int numberOfCoordinates = width * height;
            for (int i = 0; i < numberOfCoordinates; i++)
            {
                int r = ReverseBits(i, 2 * size);
                int xCoord = ExtractOddBits(r);
                int yCoord = xCoord ^ ExtractEvenBits(r);
                Coordinate currentCoordinates = new Coordinate { X = xCoord, Y = yCoord};

                ditherOrder.Add(currentCoordinates);

            }
            return ditherOrder;
        }
    }
}
