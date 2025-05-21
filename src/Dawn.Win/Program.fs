open Dawn.Core
open System

[<STAThread>]
[<EntryPoint>]
let main args =
    let game = new DawnGame ()
    game.Run ()
    0

