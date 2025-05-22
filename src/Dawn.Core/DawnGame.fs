namespace Dawn.Core

open System
open System.IO
open System.Diagnostics
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open Dawn.Core.Engine
open FontStashSharp

type DawnGame () as self =
    inherit Game ()

    let mutable state = Types.StartScreen

    let gdm = new GraphicsDeviceManager (self)

    let fontSystem = new FontSystem()
    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>
    let mutable fps: IGameComponent = Unchecked.defaultof<IGameComponent>
    let input = InputHandler ()

    do
        gdm.PreferredBackBufferWidth    <- 1280
        gdm.PreferredBackBufferHeight   <- 720
   
    override self.Initialize () = 
        base.IsMouseVisible <- false
        base.IsFixedTimeStep <- true
        base.Window.AllowUserResizing <- true

        fontSystem.AddFont(File.ReadAllBytes(@"Content\clover-sans.ttf"))
        spriteBatch <-  new SpriteBatch(gdm.GraphicsDevice)
        
        fps <- new FrameCounter (self, fontSystem, spriteBatch)
        base.Components.Add fps

        base.Initialize ()

    override self.LoadContent () =


        base.LoadContent ()

    override self.UnloadContent () =
        if fps <> null then base.Components.Remove fps |> ignore
        base.UnloadContent ()

    override self.Update (gameTime: GameTime): unit = 
        input.Update (Keyboard.GetState ())

        if input.HasPressEnded Keys.Escape then self.Exit ()


        base.Update(gameTime: GameTime)

    override self.Draw (gameTime: GameTime) =
        base.GraphicsDevice.Clear (Color.LightSeaGreen)

        base.Draw gameTime
