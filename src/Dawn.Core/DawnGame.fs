namespace Dawn.Core

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

    do
        gdm.PreferredBackBufferWidth    <- 1280
        gdm.PreferredBackBufferHeight   <- 720
   
    override self.Initialize () = 
        base.IsMouseVisible <- false
        base.IsFixedTimeStep <- true
        base.Window.AllowUserResizing <- true

        fontSystem.AddFont(File.ReadAllBytes(@"Content\clover-sans.ttf"))

        // dirty :(
        spriteBatch <-  new SpriteBatch(gdm.GraphicsDevice)

        let fps = new FrameCounter (self, fontSystem, spriteBatch)
        base.Components.Add fps

        base.Initialize ()

    override self.LoadContent () =
        base.LoadContent ()

    override self.UnloadContent () =
        base.UnloadContent ()

    override self.Update (gameTime: GameTime): unit = 
        let keyboard = Keyboard.GetState ()

        for key in keyboard.GetPressedKeys () do
            Console.WriteLine(key.ToString() + " was pressed")

        base.Update(gameTime: GameTime)

    override self.Draw (gameTime: GameTime) =
        base.GraphicsDevice.Clear (Color.LightSeaGreen)

        base.Draw gameTime
