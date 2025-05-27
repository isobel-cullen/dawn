[<AutoOpen>]
module Extensions

open System
open System.Globalization
open Microsoft.Xna.Framework

module Color =
    let FromHex (hex: string) =
        let hex = if hex.[0] = '#' then hex.[1..] else hex
        if hex.Length <> 6 then raise (InvalidOperationException ("bad hex string"))
        let red     = Int32.Parse(hex[0..1], NumberStyles.HexNumber)
        let green   = Int32.Parse(hex[2..3], NumberStyles.HexNumber)
        let blue    = Int32.Parse(hex[4..5], NumberStyles.HexNumber)
        Color(red, green, blue)

