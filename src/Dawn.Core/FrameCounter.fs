namespace Dawn.Core

open FontStashSharp
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics

type FrameCounter (game, fonts: FontSystem, sprites: SpriteBatch) =
    inherit DrawableGameComponent (game)

    let fontSize = 32f
    let buffer = CircularBuffer.create 60
    let font24 = fonts.GetFont(fontSize)

    let getPosition () = 
        new Vector2(float32 game.Window.ClientBounds.Width - fontSize, 0f) 

    let mutable framerate = 0.0
    let mutable position = getPosition ()

    do 
        game.Window.ClientSizeChanged.Add (fun _ -> position <- getPosition ())

    override self.LoadContent (): unit = 
        base.LoadContent()

    override self.Draw (gameTime: GameTime): unit = 
        if buffer.Push gameTime.ElapsedGameTime.TotalSeconds then
            framerate <- truncate (1.0 / (Seq.average buffer))

        sprites.Begin ()
        sprites.DrawString(font24, (string framerate), position, Color.Violet) |> ignore
        sprites.End ()

        base.Draw(gameTime: GameTime)
