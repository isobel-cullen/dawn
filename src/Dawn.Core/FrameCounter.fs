namespace Dawn.Core

open FontStashSharp
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Graphics
open System

type FrameCounter (game, fonts: FontSystem, sprites: SpriteBatch) =
    inherit DrawableGameComponent (game)

    let fontSize = 24f
    let buffer = CircularBuffer.create 60
    let font = fonts.GetFont(fontSize)

    let mutable framerate = String.Empty
    let mutable position = Vector2.Zero

    let updatePosition _ = 
        position <-  new Vector2(float32 game.Window.ClientBounds.Width - fontSize, 0f)

    //do updatePosition ()

    let mutable clientSizeChanged: IDisposable = Unchecked.defaultof<IDisposable>

    override self.Initialize (): unit = 
        clientSizeChanged <- game.Window.ClientSizeChanged.Subscribe updatePosition
        base.Initialize ()

    override self.Dispose (disposing: bool): unit = 
        if clientSizeChanged <> null then clientSizeChanged.Dispose ()
        base.Dispose(disposing: bool)

    override self.Draw (gameTime: GameTime): unit = 
        if buffer.Push gameTime.ElapsedGameTime.TotalSeconds then
            framerate <- truncate (1.0 / (Seq.average buffer)) |> string

        if self.Visible then
            sprites.Begin ()
            sprites.DrawString(font, framerate, position, Color.Violet) |> ignore
            sprites.End ()

        base.Draw(gameTime: GameTime)
