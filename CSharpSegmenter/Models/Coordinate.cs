using System;
using System.Collections.Generic;
using System.Text;

namespace CSharpSegmenter.Models
{
    public class Coordinate : IEquatable<Coordinate>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public bool Equals(Coordinate other)
        {
            //Check whether the compared object is null. 
            if (Object.ReferenceEquals(other, null)) return false;

            //Check whether the compared object references the same data. 
            if (Object.ReferenceEquals(this, other)) return true;

            //Check whether the products' properties are equal. 
            return X.Equals(other.X) && Y.Equals(other.Y);
        }

        // If Equals() returns true for a pair of objects  
        // then GetHashCode() must return the same value for these objects. 

        public override int GetHashCode()
        {

            //Get hash code for the X field. 
            int hashCoordinateX =  X.GetHashCode();

            //Get hash code for the Y field. 
            int hashCoordinateY = Y.GetHashCode();

            //Calculate the hash code for the product. 
            return hashCoordinateX ^ hashCoordinateY;
        }
    }
}
