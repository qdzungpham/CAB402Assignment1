module SegmentModule

open System
open System.Reflection

type Coordinate = (int * int) // x, y coordinate of a pixel
type Colour = byte list       // one entry for each colour band, typically: [red, green and blue]

type Segment = 
    | Pixel of Coordinate * Colour
    | Parent of Segment * Segment 


// return a list of the standard deviations of the pixel colours in the given segment
// the list contains one entry for each colour band, typically: [red, green and blue]
let stddev (segment: Segment) : float list =
    raise (System.NotImplementedException())
    // Fixme: add implementation here


// determine the cost of merging the given segments: 
// equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments, 
// weighted by their respective sizes and summed over all colour bands
let mergeCost segment1 segment2 : float = 
    raise (System.NotImplementedException())
    // Fixme: add implementation here


/////////// Helper Functions

let rec convertSegmentIntoPixels (segment:Segment) : Segment list =
    match segment with
    | Pixel(_,_) -> [segment]
    | Parent(segment1, segment2) -> (convertSegmentIntoPixels segment1) @ (convertSegmentIntoPixels segment2)

let listOfLists = [ [1;2;3;4;5]; [6;7;8;9;10]; [11;12;13;14;15] ]

let rec extractColumnFromListOfLists (list: int list) : int list =
    match list with
    | [] -> []
    | head::tail -> (head |> List.map List.head) :: (extractColumnFromListOfLists tail)


let rec extractColourBandsFromPixels (pixels: Segment list) : byte list =
    match pixels with
    | [] -> []
    | head::tail -> 
        match head with
            | Pixel(_, colour) -> colour

let calculateStddev (input : float list) =
    let sampleSize = float input.Length
    let mean = (input |> List.fold ( + ) 0.0) / sampleSize
    let differenceOfSquares =
        input |> List.fold
            ( fun sum item -> sum + Math.Pow(item - mean, 2.0) ) 0.0
    let variance = differenceOfSquares / sampleSize
    Math.Sqrt(variance)



