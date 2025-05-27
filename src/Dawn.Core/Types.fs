namespace Dawn.Game.Types

open System
open Dawn.Core.Engine
open Microsoft.Xna.Framework.Input
open Microsoft.Xna.Framework.Graphics
open Microsoft.Xna.Framework

module Vector2 =
    let Multiply (v: Vector2, scale: float32) = Vector2(v.X * scale, v.Y * scale)

type GameState =
    | StartScreen
    | Playing
    | Paused
    | Finished

type Actor = {
    Id: int64
    mutable Position: Vector2
    mutable Speed: Vector2
    mutable Sprite: Texture2D
} with
    static member Empty () = {
        Id          = 0
        Position    = Vector2.Zero
        Speed       = Vector2.Zero
        Sprite      = null }

type SlimePong (input: InputHandler) =
    let ball = Actor.Empty ()
    let left = Actor.Empty ()
    let rite = Actor.Empty ()

    let maxX = 640f - 16f
    let maxY = 360f - 16f

    let mutable paddleBoundsX    = 0f
    let mutable sinceLastAiInput = TimeSpan.Zero

    member _.Initialize () =
        ball.Position <- Vector2(32f, 64f)
        ball.Speed <- Vector2(50f,50f)

        left.Position <- Vector2(16f, 180f)
        rite.Position <- Vector2(608f, 180f)

    member _.LoadContent (content: Content.ContentManager) =
        ball.Sprite <- content.Load<Texture2D>("Sprites/blue_slime")
        left.Sprite <- content.Load<Texture2D>("Sprites/paddle")
        rite.Sprite <- left.Sprite
        paddleBoundsX <- 360 - left.Sprite.Height |> float32

    member _.Update (time: GameTime) =
        let timeScale = float32 time.ElapsedGameTime.TotalSeconds

        // input
        if input.HasBeenPressed Keys.W then left.Speed.Y <- min -200f left.Speed.Y - 10f
        else if input.HasBeenReleased Keys.W then left.Speed.Y <- 0f
        else if input.HasBeenPressed Keys.S then left.Speed.Y <- max 200f left.Speed.Y + 10f
        else if input.HasBeenReleased Keys.S then left.Speed.Y <- 0f

        // ai
        if time.TotalGameTime.TotalSeconds - sinceLastAiInput.TotalSeconds > 0.25 then
            sinceLastAiInput <- time.TotalGameTime

            if ball.Speed.X > 0f then
                if ball.Position.Y - 4f < rite.Position.Y then
                    rite.Speed.Y <- min -200f rite.Speed.Y - 10f
                else if ball.Position.Y > rite.Position.Y + float32 rite.Sprite.Height - 4f then
                    rite.Speed.Y <- max 200f rite.Speed.Y + 10f
                else rite.Speed.Y <- 0f
            else rite.Speed.Y <- 0f

        // ball movement
        ball.Position <- ball.Position + Vector2.Multiply(ball.Speed, timeScale)

        if ball.Position.X > maxX || ball.Position.X < 0f then
            ball.Speed.X <- clamp -1280f 1280f ball.Speed.X * -1.1f
            ball.Speed.Y <- clamp -720f 720f ball.Speed.Y * 1.1f
        if ball.Position.Y > maxY || ball.Position.Y < 0f then
            ball.Speed.Y <- clamp -720f 720f ball.Speed.Y * -1.1f
            ball.Speed.X <- clamp -1280f 1280f ball.Speed.X * 1.1f

        // paddle movement
        let newLeft = Single.FusedMultiplyAdd(left.Speed.Y, timeScale, left.Position.Y)
        let newRite = Single.FusedMultiplyAdd(rite.Speed.Y, timeScale, rite.Position.Y)
        left.Position.Y <- clamp 0f paddleBoundsX newLeft
        rite.Position.Y <- clamp 0f paddleBoundsX newRite

        // collision
        let bc = Rectangle(int32 ball.Position.X, int32 ball.Position.Y, 16, 16)
        let lc = Rectangle(int32 left.Position.X, int32 left.Position.Y, 16, 64)
        let rc = Rectangle(int32 rite.Position.X, int32 rite.Position.Y, 16, 64)

        if lc.Intersects bc then
            ball.Speed.X <- clamp -680f 680f ball.Speed.X * -1.1f
            ball.Speed.Y <- ball.Speed.Y + left.Speed.Y
        else if rc.Intersects bc then
            ball.Speed.X <- clamp -680f 680f ball.Speed.X * -1.1f
            ball.Speed.Y <- ball.Speed.Y + rite.Speed.Y

        // todo: points,  etc
        // remove hardcoded resolution references

    member _.Draw (sb: SpriteBatch, time: GameTime) =
        let r = Rectangle((time.TotalGameTime.Seconds % 2) * 16, 0, 16,16)
        sb.Draw(ball.Sprite, ball.Position, r, Color.White)
        sb.Draw(left.Sprite, left.Position, Palette.Solarized.Red)
        sb.Draw(rite.Sprite, rite.Position, Palette.Solarized.Green)
