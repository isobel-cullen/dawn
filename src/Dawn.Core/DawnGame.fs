namespace Dawn.Core

open Cellular.Types
open Cellular.Rules

open System
open System.IO
open System.Diagnostics
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics

open FontStashSharp

type DawnGame () as self =
    inherit Game ()

    let mutable state = Types.StartScreen

    let gdm = new GraphicsDeviceManager (self)

    let fontSystem = new FontSystem()
    let mutable spriteBatch: SpriteBatch = Unchecked.defaultof<SpriteBatch>

    let rows = CircularBuffer (720 / 32)
    let mutable cells = Row.Initialize (1280 / 32)
    let mutable sinceLastUpdate =  TimeSpan.Zero
    let mutable font: DynamicSpriteFont = Unchecked.defaultof<DynamicSpriteFont>

    do
        gdm.PreferredBackBufferWidth    <- 1280
        gdm.PreferredBackBufferHeight   <- 720
   
    override self.Initialize () = 
        base.IsMouseVisible <- false
        base.IsFixedTimeStep <- true

        fontSystem.AddFont(File.ReadAllBytes(@"Content\clover-sans.ttf"))

        // dirty :(
        spriteBatch <-  new SpriteBatch(gdm.GraphicsDevice)
        font <- fontSystem.GetFont(32f)

        let fps = new FrameCounter (self, fontSystem, spriteBatch)
        base.Components.Add fps

        while not (rows.Push (cells.Print ())) do
            cells <- cells.Evolve Rule110

        base.Initialize ()

    override self.LoadContent () =
        base.LoadContent ()

    override self.UnloadContent () =
        base.UnloadContent ()

    override self.Update (gameTime: GameTime): unit = 
        let keyboard = Keyboard.GetState ()

        if keyboard.[Keys.Escape] = KeyState.Down then
            base.Exit ()

        sinceLastUpdate <- sinceLastUpdate + gameTime.ElapsedGameTime
        if sinceLastUpdate >= TimeSpan.FromSeconds 1.0 then
            cells <- cells.Evolve Rule110
            rows.Push (cells.Print ()) |> ignore
            sinceLastUpdate <- TimeSpan.Zero
        
        base.Update(gameTime: GameTime)

    override self.Draw (gameTime: GameTime) =
        base.GraphicsDevice.Clear (Color.LightSeaGreen)

        spriteBatch.Begin ()
        Seq.iteri (fun i r ->
            let vec = Vector2(0f, float32 i * 32f)
            spriteBatch.DrawString(font, string r, vec, Color.Tomato) |> ignore) rows
        spriteBatch.End()

        base.Draw gameTime
