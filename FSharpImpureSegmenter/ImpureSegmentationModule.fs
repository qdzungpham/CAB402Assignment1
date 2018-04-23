module ImpureSegmentationModule

open ImpureSegmentModule
open System.Collections.Generic
open System.Linq

// Maps segments to their immediate parent segment that they are contained within (if any) 
type Segmentation = Dictionary<Segment, Segment>

// Helper Functions

let findImmediateParent (segmentation: Segmentation) segment : Segment =
    match segmentation.ContainsKey(segment) with 
    | true -> segmentation.[segment] 
    | false -> segment

let rec convertSegmentIntoCoordinates (segment:Segment) : Coordinate list =
    match segment with
    | Pixel(coordinate,_) -> [coordinate]
    | Parent(segment1, segment2) -> (convertSegmentIntoCoordinates segment1) @ (convertSegmentIntoCoordinates segment2)



 /////////////////////


// Find the largest/top level segment that the given segment is a part of (based on the current segmentation)
let rec findRoot (segmentation: Segmentation) segment : Segment =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let immediateParent = findImmediateParent segmentation segment
    match segmentation.ContainsKey(immediateParent) with
    | true -> findRoot segmentation immediateParent
    | false -> immediateParent


// Initially, every pixel/coordinate in the image is a separate Segment
// Note: this is a higher order function which given an image, 
// returns a function which maps each coordinate to its corresponding (initial) Segment (of kind Pixel)
let createPixelMap (pixelArray:Segment[,]) : (Coordinate -> Segment) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    (fun coordinate -> let x, y = coordinate
                       pixelArray.[x, y])

// Find the neighbouring segments of the given segment (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
// Note: this is a higher order function which given a pixelMap function and a size N, 
// returns a function which given a current segmentation, returns the set of Segments which are neighbours of a given segment
let createNeighboursFunction (pixelMap:Coordinate->Segment) (N:int) : (Segmentation -> Segment -> HashSet<Segment>) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let theFunction segmentation segment : HashSet<Segment> =
        let coordinates = convertSegmentIntoCoordinates segment
        let neighbouringCoodinates = new List<Coordinate>()

        for coordinate in coordinates do
            let x, y = coordinate
            neighbouringCoodinates.Add((x-1,y))
            neighbouringCoodinates.Add((x+1,y))
            neighbouringCoodinates.Add((x,y+1))
            neighbouringCoodinates.Add((x,y-1))

        let filteredNeighbouringCoodinates = neighbouringCoodinates.Where(fun coordinate -> let x, y = coordinate
                                                                                            x >= 0 && y >= 0 && x < (pown 2 N) && y < (pown 2 N))

        let segments = new List<Segment>()

        for coordinate in filteredNeighbouringCoodinates do
            segments.Add(pixelMap coordinate |> findRoot segmentation)

        let result = new HashSet<Segment>(segments)
        
        result.Remove(segment) |> ignore

        result

        
    theFunction

// The following are also higher order functions, which given some inputs, return a function which ...


 // Find the neighbour(s) of the given segment that has the (equal) best merge cost
 // (exclude neighbours if their merge cost is greater than the threshold)
let createBestNeighbourFunction (neighbours:Segmentation->Segment->HashSet<Segment>) (threshold:float) : (Segmentation->Segment->HashSet<Segment>) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let theFunction segmentation segment : HashSet<Segment>  =
        let neighbours = neighbours segmentation segment
        let result = new HashSet<Segment>()
        match neighbours.Count with
        | 0 -> result
        | _ -> let segmentsAndMergeCosts = new Dictionary<Segment, float>()
               for neighbour in neighbours do
                   segmentsAndMergeCosts.Add(neighbour, (mergeCost segment neighbour))
               let bestMergeCost = segmentsAndMergeCosts.OrderBy(fun x -> x.Value).First().Value

               segmentsAndMergeCosts.Where(fun pair -> pair.Value = bestMergeCost && pair.Value <= threshold).Select(fun pair -> pair.Key).ToHashSet()

    theFunction

// Try to find a neighbouring segmentB such that:
//     1) segmentB is one of the best neighbours of segment A, and 
//     2) segmentA is one of the best neighbours of segment B
// if such a mutally optimal neighbour exists then merge them,
// otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
let createTryGrowOneSegmentFunction (bestNeighbours:Segmentation->Segment->HashSet<Segment>) (pixelMap:Coordinate->Segment) : (Segmentation->Coordinate->Segmentation) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let rec theFunction segmentation coordinate : Segmentation =
        let segmentA = pixelMap coordinate |> findRoot segmentation
        let segmentABestNeighbours = bestNeighbours segmentation segmentA 
        let mutallyOptimalNeighbours = new HashSet<Segment>()
        //mutallyOptimalNeighbours.Contains()
        if segmentABestNeighbours.Count <> 0 then do
            for segmentABestNeighbour in segmentABestNeighbours do
                let segmentBBestNeibours = segmentABestNeighbour |> findRoot (segmentation)|> bestNeighbours (segmentation) 
                if segmentBBestNeibours.Contains(segmentA) 
                then do mutallyOptimalNeighbours.Add(segmentABestNeighbour) |> ignore

        if mutallyOptimalNeighbours.Count = 0 then do
            if segmentABestNeighbours.Count <> 0 then do theFunction segmentation (segmentABestNeighbours.First() |> convertSegmentIntoCoordinates |> List.head) |> ignore
        else do
            let mutallyOptimalNeighbour = mutallyOptimalNeighbours.First()
            let mergeSegment = Parent(segmentA, mutallyOptimalNeighbour)
            segmentation.Add(mutallyOptimalNeighbour, mergeSegment)
            segmentation.Add(segmentA, mergeSegment) |> ignore

        segmentation
    theFunction

// Try to grow the segments corresponding to every pixel on the image in turn 
// (considering pixel coordinates in special dither order)
let createTryGrowAllCoordinatesFunction (tryGrowPixel:Segmentation->Coordinate->Segmentation) (N:int) : (Segmentation->Segmentation) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let coordinates = DitherModule.coordinates N
    let theFunction segmentation : Segmentation =
        
        let rec growAllCoordinates segmentation seq =
            if Seq.isEmpty seq then segmentation else growAllCoordinates (tryGrowPixel segmentation (Seq.head seq)) (Seq.tail seq)
            
        growAllCoordinates segmentation coordinates

    theFunction

// Keep growing segments as above until no further merging is possible
let createGrowUntilNoChangeFunction (tryGrowAllCoordinates:Segmentation->Segmentation) : (Segmentation->Segmentation) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let rec theFunction segmentation : Segmentation =
        let newSegmentation = tryGrowAllCoordinates segmentation
        if segmentation = newSegmentation then newSegmentation else theFunction newSegmentation

    theFunction

// Segment the given image based on the given merge cost threshold, but only for the top left corner of the image of size (2^N x 2^N)
let segment (image:TiffModule.Image) (N: int) (threshold:float)  : (Coordinate -> Segment) =
    //raise (System.NotImplementedException())
    // Fixme: use the functions above to help implement this function

    let pixelArray =
        let imageSize = pown 2 N
        Array2D.init imageSize imageSize (fun x y -> Pixel ((x, y), TiffModule.getColourBands image (x, y)))

    let pixelMap = createPixelMap pixelArray
    let neighbours = createNeighboursFunction pixelMap N
    let bestNeighbours = createBestNeighbourFunction neighbours threshold
    let tryGrowOneSegment = createTryGrowOneSegmentFunction bestNeighbours pixelMap
    let tryGrowAllCoordinates = createTryGrowAllCoordinatesFunction tryGrowOneSegment N
    let growUntilNoChange = createGrowUntilNoChangeFunction tryGrowAllCoordinates

    let finalSegmentation = growUntilNoChange (new Dictionary<Segment, Segment>())

    let theFunction (coordinate:Coordinate) : Segment =
        findRoot finalSegmentation (pixelMap coordinate)

    theFunction