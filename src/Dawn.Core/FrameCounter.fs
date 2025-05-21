namespace Dawn.Core

open FontStashSharp
open System.Diagnostics
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type FrameCounter (game, fonts: FontSystem, sprites: SpriteBatch) =
    inherit DrawableGameComponent (game)

    let buffer = CircularBuffer.create 60
    let font24 = fonts.GetFont(24f)
    let mutable framerate = 0.0

    override self.LoadContent (): unit = 
        base.LoadContent()

    override self.Draw (gameTime: GameTime): unit = 
        if buffer.Push gameTime.ElapsedGameTime.TotalSeconds then
            framerate <- truncate (1.0 / (Seq.average buffer))

        sprites.Begin ()
        sprites.DrawString(font24, (string framerate), Vector2.Zero, Color.Violet) |> ignore
        sprites.End ()

        base.Draw(gameTime: GameTime)
