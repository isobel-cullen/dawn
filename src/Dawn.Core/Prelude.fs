[<AutoOpen>]
module Prelude

let clamp min max x =
    if x < min then min
    else if x > max then max
    else x

type Direction =
| Centre
| North
| NorthEast
| East
| SouthEast
| South
| SouthWest
| West
| NorthWest




