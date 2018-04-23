module SegmentationModule

open SegmentModule

// Maps segments to their immediate parent segment that they are contained within (if any) 
type Segmentation = Map<Segment, Segment>


// Helper Functions

let findImmediateParent (segmentation: Segmentation) segment : Segment =
    match segmentation.TryFind(segment) with 
    | Some(foundSegment) -> foundSegment 
    | None -> segment

let rec convertSegmentIntoCoordinates (segment:Segment) : Coordinate list =
    match segment with
    | Pixel(coordinate,_) -> [coordinate]
    | Parent(segment1, segment2) -> (convertSegmentIntoCoordinates segment1) @ (convertSegmentIntoCoordinates segment2)

let getNeighbouringCoodinatesOfSingleCoordinate (coordinate:Coordinate) : Coordinate list =
    let x, y = coordinate
    [(x-1,y);(x+1,y);(x,y+1);(x,y-1)]

let rec flatten list =
    match list with 
    | [] -> []
    | head::tail -> head @ (flatten tail)

let getNeighbouringCoordinates (segment:Segment) : Coordinate list =
    segment |> convertSegmentIntoCoordinates |> List.map getNeighbouringCoodinatesOfSingleCoordinate |> flatten

let filterCoordinate (N:int) (coordinate:Coordinate) =
    let x, y = coordinate
    x >= 0 && y >= 0 && x < (pown 2 N) && y < (pown 2 N)

let rec calculateSegmentMergeCost list segment =
    match list with
    | [] -> []
    | head::tail -> [(head, mergeCost segment head)] @ calculateSegmentMergeCost tail segment
    
let getBestMergeCost list =
    List.minBy snd list |> snd

let rec getBestNeighbours list bestMergeCost threshold =
    match list with 
    | [] -> []
    | head::tail ->
        let rest = getBestNeighbours tail bestMergeCost threshold
        let segment, mergeCost = head
        if mergeCost = bestMergeCost && mergeCost <= threshold then [segment] @ rest
        else rest

let rec getMutallyOptimalNeighbours bestNeighbours segmentation segmentA segmentABestNeighbours =
    match segmentABestNeighbours with 
        | [] -> []
        | head::tail -> 
            let rest = getMutallyOptimalNeighbours bestNeighbours segmentation segmentA tail
            let x = bestNeighbours segmentation head
            if (Set.contains segmentA x) then [head] @ rest else rest

// ------------- End of Helper Functions --------------------


// Find the largest/top level segment that the given segment is a part of (based on the current segmentation)
let rec findRoot (segmentation: Segmentation) segment : Segment =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let immediateParent = findImmediateParent segmentation segment
    match segmentation.TryFind(immediateParent) with
    | Some(_) -> findRoot segmentation immediateParent
    | None -> immediateParent


// Initially, every pixel/coordinate in the image is a separate Segment
// Note: this is a higher order function which given an image, 
// returns a function which maps each coordinate to its corresponding (initial) Segment (of kind Pixel)
let createPixelMap (image:TiffModule.Image) : (Coordinate -> Segment) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    (fun coordinate -> Pixel (coordinate, TiffModule.getColourBands image coordinate))

// Find the neighbouring segments of the given segment (assuming we are only segmenting the top corner of the image of size 2^N x 2^N)
// Note: this is a higher order function which given a pixelMap function and a size N, 
// returns a function which given a current segmentation, returns the set of Segments which are neighbours of a given segment
let createNeighboursFunction (pixelMap:Coordinate->Segment) (N:int) : (Segmentation -> Segment -> Set<Segment>) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let theFunction segmentation segment : Set<Segment> =
        segment 
        |> getNeighbouringCoordinates 
        |> List.filter (filterCoordinate N) 
        |> List.map pixelMap 
        |> List.map (findRoot segmentation) 
        |> Set.ofList 
        |> Set.remove segment
    theFunction

// The following are also higher order functions, which given some inputs, return a function which ...


 // Find the neighbour(s) of the given segment that has the (equal) best merge cost
 // (exclude neighbours if their merge cost is greater than the threshold)
let createBestNeighbourFunction (neighbours:Segmentation->Segment->Set<Segment>) (threshold:float) : (Segmentation->Segment->Set<Segment>) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let theFunction segmentation segment : Set<Segment>  =
        let allNeighbours =  neighbours segmentation segment |> Set.toList
        match allNeighbours with 
        | [] -> Set.empty
        | _ -> 
            let segmentAndMergeCost = calculateSegmentMergeCost allNeighbours segment
            let bestMergeCost = getBestMergeCost segmentAndMergeCost
            getBestNeighbours segmentAndMergeCost bestMergeCost threshold |> Set.ofList

    theFunction

// Try to find a neighbouring segmentB such that:
//     1) segmentB is one of the best neighbours of segment A, and 
//     2) segmentA is one of the best neighbours of segment B
// if such a mutally optimal neighbour exists then merge them,
// otherwise, choose one of segmentA's best neighbours (if any) and try to grow it instead (gradient descent)
let createTryGrowOneSegmentFunction (bestNeighbours:Segmentation->Segment->Set<Segment>) (pixelMap:Coordinate->Segment) : (Segmentation->Coordinate->Segmentation) =
    //raise (System.NotImplementedException())
    // Fixme: add implementation here
    let rec theFunction segmentation coordinate : Segmentation =
        let segmentA = pixelMap coordinate |> findRoot segmentation
        let segmentABestNeighbours = bestNeighbours segmentation segmentA |> Set.toList
        let mutallyOptimalNeighbours = getMutallyOptimalNeighbours bestNeighbours segmentation segmentA segmentABestNeighbours
        match mutallyOptimalNeighbours with
        | [] -> if List.isEmpty segmentABestNeighbours then segmentation 
                else theFunction segmentation (segmentABestNeighbours |> List.head |> convertSegmentIntoCoordinates |> List.head)
        | head::_ -> segmentation.Add(segmentA, Parent(segmentA, head)).Add(head, Parent(segmentA, head))

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

    let pixelMap = createPixelMap image
    let neighbours = createNeighboursFunction pixelMap N
    let bestNeighbours = createBestNeighbourFunction neighbours threshold
    let tryGrowOneSegment = createTryGrowOneSegmentFunction bestNeighbours pixelMap
    let tryGrowAllCoordinates = createTryGrowAllCoordinatesFunction tryGrowOneSegment N
    let growUntilNoChange = createGrowUntilNoChangeFunction tryGrowAllCoordinates

    let finalSegmentation = growUntilNoChange Map.empty

    let theFunction (coordinate:Coordinate) : Segment =
        findRoot finalSegmentation (pixelMap coordinate)

    theFunction
