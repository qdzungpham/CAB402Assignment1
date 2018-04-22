module ImpureSegmentModule

open System

type Coordinate = (int * int) // x, y coordinate of a pixel
type Colour = byte list       // one entry for each colour band, typically: [red, green and blue]

type Segment = 
    | Pixel of Coordinate * Colour
    | Parent of Segment * Segment 

/////////// Helper Functions

let rec convertBytestoFloats bytes =
    match bytes with
    | [] -> []
    | head::tail -> [List.map (fun x -> float x) head] @ (convertBytestoFloats tail) 


let rec convertSegmentIntoPixelColours (segment:Segment) : Colour list =
    match segment with
    | Pixel(_,colour) -> [colour]
    | Parent(segment1, segment2) -> (convertSegmentIntoPixelColours segment1) @ (convertSegmentIntoPixelColours segment2)


let getFirstColumn list = 
    list |> List.map List.head

let rec removeFirstItem list =
    match list with
    | [] -> []
    | head::tail -> tail

let chopHeadsOfListsInList list =
    list |> List.map removeFirstItem 

let rec extractColumns (list: 'T List list) = 
    if list.[0] = [] then [] 
    else [getFirstColumn list] @ (list |> chopHeadsOfListsInList |> extractColumns)

let rec extractColourBands (colours: Colour list) =
    extractColumns colours

let calculateStddev (input : float list) =
    let sampleSize = float input.Length
    let mean = (input |> List.fold ( + ) 0.0) / sampleSize
    let differenceOfSquares =
        input |> List.fold
            ( fun sum item -> sum + Math.Pow(item - mean, 2.0) ) 0.0
    let variance = differenceOfSquares / sampleSize
    Math.Sqrt(variance)

let rec getNumPixels (segment: Segment) : float =
    match segment with
    | Pixel(_,_) -> 1.0
    | Parent(segment1, segment2) -> (getNumPixels segment1) + (getNumPixels segment2)

let sumStddevOfAllBands list =
    List.reduce (+) list

// return a list of the standard deviations of the pixel colours in the given segment
// the list contains one entry for each colour band, typically: [red, green and blue]
let stddev (segment: Segment) : float list =
    // raise (System.NotImplementedException())
    // Fixme: add implementation here
    segment 
    |> convertSegmentIntoPixelColours 
    |> extractColourBands 
    |> convertBytestoFloats 
    |> List.map calculateStddev



// determine the cost of merging the given segments: 
// equal to the standard deviation of the combined the segments minus the sum of the standard deviations of the individual segments, 
// weighted by their respective sizes and summed over all colour bands
let mergeCost segment1 segment2 : float = 
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let segment1Stddev = segment1 |> stddev |> sumStddevOfAllBands
    let segment2Stddev = segment2 |> stddev |> sumStddevOfAllBands
    let combinedSegmentStddev = Parent (segment1, segment2) |> stddev |> sumStddevOfAllBands

    (combinedSegmentStddev * (getNumPixels (Parent (segment1, segment2)))) - (segment1Stddev * (getNumPixels segment1) + segment2Stddev * (getNumPixels segment2))



