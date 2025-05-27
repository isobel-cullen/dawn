namespace Dawn.Core.Engine

open System
open Microsoft.Xna.Framework
open Microsoft.Xna.Framework.Input

type Palette = {
    Background: Color
    BackgroundLight: Color

    Content: Color

    Yellow: Color
    Orange: Color
    Red: Color
    Magenta: Color
    Violet: Color
    Blue: Color
    Cyan: Color
    Green: Color
} with
    static member Solarized = {
        Background      = Color.FromHex "002b36"
        BackgroundLight = Color.FromHex "073642"
        Content         = Color.FromHex "93a1a1"
        Yellow          = Color.FromHex "b58900"
        Orange          = Color.FromHex "cb4b16"
        Red             = Color.FromHex "dc322f"
        Magenta         = Color.FromHex "d33682"
        Violet          = Color.FromHex "6c71c4"
        Blue            = Color.FromHex "268bd2"
        Cyan            = Color.FromHex "2aa198"
        Green           = Color.FromHex "859900" }

type InputHandler () =
    let mutable previous = Unchecked.defaultof<KeyboardState>
    let mutable current  = Unchecked.defaultof<KeyboardState>

    let pressed     = ResizeArray<Keys> ()
    let released    = ResizeArray<Keys> ()
    let held        = ResizeArray<Keys> ()         

    member _.Update newState =
        previous <- current
        current  <- newState

        pressed.Clear ()
        released.Clear ()
        held.Clear ()

        let pKeys = previous.GetPressedKeys ()
        let cKeys = current.GetPressedKeys ()

        for k in cKeys do if Array.contains k pKeys then held.Add k else pressed.Add k
        for k in pKeys do if not (Array.contains k cKeys) then released.Add k

    member _.Pressed () = current.GetPressedKeys ()
    member _.IsKeyDown key = current.IsKeyDown key
    member _.HasBeenPressed key = pressed.Contains key
    member _.HasBeenReleased key   = released.Contains key
    member _.IsHeld key = held.Contains key

type VirtualScreen (width, height) =
    let mutable bufferWidth     = 0
    let mutable bufferHeight    = 0

    let mutable scale           = Matrix.Identity
    let mutable screenScale     = Vector2.Zero

    member _.Scale with get () = scale
    member _.ScreenScale with get () = screenScale
    member val VirtualResolution = Vector2(float32 width, float32 height)

    member self.Update (device: GraphicsDeviceManager) =
        if isNull device then raise (ArgumentNullException (nameof device))

        bufferWidth  <- device.GraphicsDevice.Viewport.Width
        bufferHeight <- device.GraphicsDevice.Viewport.Height

        let widthScale  = (float32 bufferWidth) / self.VirtualResolution.X
        let heightScale = float32 bufferHeight / self.VirtualResolution.Y

        scale           <- Matrix.CreateScale(Vector3(widthScale, heightScale, 1f))
        screenScale     <- Vector2(widthScale, heightScale)

