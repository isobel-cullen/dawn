namespace Dawn.Core

open System
open System.IO
open System.Diagnostics
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open Dawn.Game.Types
open Dawn.Core.Engine
open FontStashSharp

type DawnGame () as self =
    inherit Game ()

    let gdm = new GraphicsDeviceManager (self)

    let input = InputHandler ()
    let fontSystem = new FontSystem ()
    let screen = VirtualScreen (640, 360)

    let mutable contentManager     = Unchecked.defaultof<Content.ContentManager>
    let mutable spriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable fps         = Unchecked.defaultof<IGameComponent>

    let pong = SlimePong (fontSystem, input)

    do
        gdm.PreferredBackBufferWidth    <- 1280
        gdm.PreferredBackBufferHeight   <- 720
   
    override self.Initialize () = 
        base.IsMouseVisible <- false
        base.IsFixedTimeStep <- true
        base.Window.AllowUserResizing <- true

        self.Window.ClientSizeChanged.Add(fun _ -> screen.Update gdm)

        screen.Update gdm
        fontSystem.AddFont(File.ReadAllBytes(@"Content\Fonts\clover-sans.ttf"))
        spriteBatch <-  new SpriteBatch(gdm.GraphicsDevice)
        fps <- new FrameCounter (self, fontSystem, spriteBatch)
        base.Components.Add fps

        contentManager <- new Content.ContentManager (self.Services)
        contentManager.RootDirectory <- "Content"

        pong.Initialize ()
        base.Initialize ()

    override self.LoadContent () =
        pong.LoadContent contentManager
        base.LoadContent ()

    override self.UnloadContent () =
        if fps <> null then base.Components.Remove fps |> ignore
        base.UnloadContent ()

    override self.Update (gameTime: GameTime): unit = 
        input.Update (Keyboard.GetState ())

        if input.HasBeenReleased Keys.Escape then self.Exit ()

        pong.Update gameTime
        base.Update gameTime

    override self.Draw (gameTime: GameTime) =
        base.GraphicsDevice.Clear (Palette.Solarized.Background)

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullCounterClockwise,
            null,
            screen.Scale )

        pong.Draw (spriteBatch, gameTime)

        spriteBatch.End ()

        base.Draw gameTime
