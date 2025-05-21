module Types

open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

type GameState =
    | StartScreen
    | Playing
    | Paused
    | Finished

type Actor = {
    Id: int64
    Sprite: Texture2D
    Position: Vector2
}


