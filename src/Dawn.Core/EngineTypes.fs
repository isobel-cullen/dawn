namespace Dawn.Core.Engine

open System
open System.Collections
open Microsoft.Xna.Framework.Input

type InputHandler () =
    let mutable previous = Unchecked.defaultof<KeyboardState>
    let mutable current  = Unchecked.defaultof<KeyboardState>

    let pressStarted = ResizeArray<Keys> ()
    let pressEnded   = ResizeArray<Keys> ()

    member _.Update newState =
        previous <- current
        current  <- newState

        pressStarted.Clear ()
        pressEnded.Clear ()

        let pKeys = previous.GetPressedKeys ()
        let cKeys = current.GetPressedKeys ()

        for k in cKeys do if not (Array.contains k pKeys) then pressStarted.Add k 
        for k in pKeys do if not (Array.contains k cKeys) then pressEnded.Add k

    member _.IsKeyDown key = current.IsKeyDown key
    member _.HasPressStarted key = pressStarted.Contains key
    member _.HasPressEnded key   = pressEnded.Contains key

    

