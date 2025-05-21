namespace Dawn.Core

open System.Collections
open System.Collections.Generic
open System.Linq

type CircularBuffer<'a> (capacity) =
    let buffer = Array.zeroCreate<'a> capacity
    let mutable head    = -1;
    let mutable version = 0;
    let mutable count   = 0

    let enumerate initialVersion count =
        seq {
            for i in 0 .. count do
                if initialVersion <> version then raise (invalidOp "Collection was modified")
                yield buffer.[(head + i) % buffer.Length]
        }

    member _.Capacity with get () = buffer.Length
    member _.Count with get () = count

    /// returns true if the buffer wrapped around
    member _.Push (item) =
        // unchecked addition
        version <- Microsoft.FSharp.Core.Operators.(+) version 1
        count <- if count = buffer.Length then count else count + 1
        head <- if head + 1 < buffer.Length then head + 1 else 0

        buffer.[head] <- item
        head = 0

    member _.Reset () =
        version <- -1
        head    <- -1
        count   <-  0
                 
    interface IEnumerable<'a> with
        member self.GetEnumerator () =
            if self.Count = 0 then Enumerable.Empty<'a>().GetEnumerator ()
            else (enumerate version self.Count).GetEnumerator ()

    interface IEnumerable with
        member self.GetEnumerator (): IEnumerator =
            (self :> IEnumerable<'a>).GetEnumerator()

module CircularBuffer =
    let create<'a> capacity = CircularBuffer<'a>(capacity)
    let push (b: CircularBuffer<'a>) (i: 'a) = b.Push(i)

    let createFrom (source: seq<'a>) =
        let buffer = CircularBuffer<'a>(Seq.length source)
        Seq.iter (buffer.Push >> ignore) source
        buffer


