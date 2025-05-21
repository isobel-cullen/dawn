module Cellular
module Types =
    open System
    open System.Text
    open FSharp.Core.Printf

    type Cell =
    | O
    | X
        override x.ToString() =
            match x with
            | O -> "O"
            | X -> "X"

    let randomCell (random: Random) _ =
        match random.Next(2) with
        | 0 -> O
        | 1 -> X
        | _ -> failwith "Maths is broken"

    type Rule = Cell * Cell * Cell -> Cell

    type Row = {
        State: Cell array
        Iteration: int64
    } with
        static member Initialize size =
            let r = Random()
            { 
                State = Array.init size (randomCell r)
                Iteration = 0L
            }

        member this.Evolve (rule: Rule) =
            let row = this.State
            let getNeighbours index (cell: Cell)=
                match index, row.Length with
                | _ when index = 0 ->
                    (row.[row.Length - 1], cell, row.[index + 1])
                | _ when index = (row.Length - 1) ->
                    (row.[index - 1], cell, row.[0])
                | _ -> 
                    (row.[index - 1], cell, row.[index + 1])
            {
                State = row |> Array.mapi (fun i c -> getNeighbours i c |> rule)
                Iteration = this.Iteration + 1L
            }

        member this.Print () =
            let buffer = StringBuilder(this.State.Length)
            this.State |> Array.iter (bprintf buffer "%O")
            buffer.ToString ()

module Rules =
    open Types
    let Rule110 (cells: Cell * Cell * Cell) =
        match cells with
        | X,X,X -> O
        | X,X,O -> X
        | X,O,X -> X
        | X,O,O -> O
        | O,X,X -> X
        | O,X,O -> X
        | O,O,X -> X
        | O,O,O -> O

    let namedRule (name: int32) (cells: Cell * Cell * Cell) =
        let isSet c =
            match name &&& c with
            | 0 -> O
            | _ -> X

        match cells with
        | X,X,X -> isSet 128
        | X,X,O -> isSet 64
        | X,O,X -> isSet 32
        | X,O,O -> isSet 16
        | O,X,X -> isSet 8
        | O,X,O -> isSet 4
        | O,O,X -> isSet 2
        | O,O,O -> isSet 1
