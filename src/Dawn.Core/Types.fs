namespace Dawn.Game.Types

open System
open Dawn.Core.Engine
open FontStashSharp
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

type SlimePong (fonts: FontSystem, input: InputHandler) =
    let ball = Actor.Empty ()
    let left = Actor.Empty ()
    let rite = Actor.Empty ()

    let mutable state = GameState.StartScreen

    let mutable font48 = Unchecked.defaultof<DynamicSpriteFont>
    let mutable font36 = Unchecked.defaultof<DynamicSpriteFont>

    let mutable startLength     = 0f
    let mutable finishLength    = 0f
    let mutable finishMessage   = ""

    let maxX = 640f - 16f
    let maxY = 360f - 16f

    let mutable paddleBoundsX    = 0f
    let mutable sinceLastAiInput = TimeSpan.Zero

    let mutable score       = "0 | 0"
    let mutable pointsLeft  = 0
    let mutable pointsRite  = 0
    let mutable bonus       = 0f

    let resetBall xSpeed =
        ball.Position   <- Vector2(320f, 120f)
        ball.Speed      <- Vector2(xSpeed, 75f)

    member _.Initialize () =
        resetBall 175f
        left.Position <- Vector2(16f, 180f)
        rite.Position <- Vector2(608f, 180f)

        font48  <- fonts.GetFont 48f
        font36 <- fonts.GetFont 36f

        startLength <- font36.MeasureString("Press any key to start").X

    member _.LoadContent (content: Content.ContentManager) =
        ball.Sprite <- content.Load<Texture2D>("Sprites/blue_slime")
        left.Sprite <- content.Load<Texture2D>("Sprites/paddle")
        rite.Sprite <- left.Sprite
        paddleBoundsX <- 360 - left.Sprite.Height |> float32

    member _.Update (time: GameTime) =
        match state with
        | GameState.StartScreen
        | GameState.Finished ->
            if input.Pressed().Length > 0 then
                resetBall 175f
                pointsLeft  <- 0
                pointsRite  <- 0
                bonus       <- 0f
                state       <- GameState.Playing
        | GameState.Paused ->
            if input.HasBeenPressed Keys.Space then state <- GameState.Playing
        | GameState.Playing ->
            let timeScale = float32 time.ElapsedGameTime.TotalSeconds

            if input.HasBeenPressed Keys.Space then state <- GameState.Paused

            // input
            if input.HasBeenPressed Keys.W || input.IsKeyDown Keys.W then left.Speed.Y <- min -200f left.Speed.Y - 15f
            else if input.HasBeenReleased Keys.W then left.Speed.Y <- 0f
            else if input.HasBeenPressed Keys.S || input.IsKeyDown Keys.S then left.Speed.Y <- max 200f left.Speed.Y + 15f
            else if input.HasBeenReleased Keys.S then left.Speed.Y <- 0f

            // ai
            if time.TotalGameTime.TotalSeconds - sinceLastAiInput.TotalSeconds > (0.25 - (float bonus / 10.0)) then
                sinceLastAiInput <- time.TotalGameTime

                if ball.Speed.X > 0f then
                    if ball.Position.Y - 4f < rite.Position.Y then
                        rite.Speed.Y <- min -200f rite.Speed.Y - (12f + bonus)
                    else if ball.Position.Y > rite.Position.Y + float32 rite.Sprite.Height - 4f then
                        rite.Speed.Y <- max 200f rite.Speed.Y + 12f + bonus
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

            if lc.Intersects bc && ball.Speed.X < 0f then
                ball.Speed.X <- clamp -680f 680f ball.Speed.X * -1.1f
                ball.Speed.Y <- ball.Speed.Y + left.Speed.Y
            else if rc.Intersects bc && ball.Speed.X > 0f then
                ball.Speed.X <- clamp -680f 680f ball.Speed.X * -1.1f
                ball.Speed.Y <- ball.Speed.Y + rite.Speed.Y

            // scoring
            if ball.Position.X < 15f then
                pointsRite <- pointsRite + 1
                if pointsRite = 9 then
                    finishMessage   <- "You lose!"
                    finishLength    <- font36.MeasureString(finishMessage).X
                    state           <- GameState.Finished
                else
                    resetBall -175f
                    score <- String.Format("{0} | {1}", pointsLeft, pointsRite)
                    bonus <- if pointsLeft > pointsRite then float32 (pointsLeft - pointsRite) else 0f
            else if ball.Position.X > 624f then
                pointsLeft <- pointsLeft + 1
                if pointsLeft = 9 then
                    finishMessage   <- "Congratulations!"
                    finishLength    <- font36.MeasureString(finishMessage).X
                    state           <- GameState.Finished
                else
                    resetBall 175f
                    score <- String.Format("{0} | {1}", pointsLeft, pointsRite)
                    bonus <- if pointsLeft > pointsRite then float32 (pointsLeft - pointsRite) else 0f

    member _.Draw (sb: SpriteBatch, time: GameTime) =
        let alpha = time.TotalGameTime.Milliseconds / 3
        match state with
        | GameState.StartScreen ->
            let c = Palette.Solarized.Cyan |> Color.WithAlpha alpha
            sb.DrawString(font36, @"Press any key to start", Vector2(320f - startLength / 2f, 100f), c)  |> ignore
        | GameState.Finished ->
            let c = if pointsLeft = 9 then
                        Palette.Solarized.Magenta |> Color.WithAlpha alpha
                    else Palette.Solarized.Orange |> Color.WithAlpha alpha
            sb.DrawString(font36, finishMessage, Vector2(320f - finishLength / 2f, 100f), c) |> ignore
        | _ ->
            let r = Rectangle((time.TotalGameTime.Seconds % 2) * 16, 0, 16,16)
            sb.Draw(ball.Sprite, ball.Position, r, Color.White)
            sb.Draw(left.Sprite, left.Position, Palette.Solarized.Red)
            sb.Draw(rite.Sprite, rite.Position, Palette.Solarized.Green)

            // if I was smort I'd use a monospaced font so I could align this properly
            sb.DrawString(font48, score, Vector2(272f, 16f), Palette.Solarized.Content) |> ignore

            if state = GameState.Paused then
                let c = Palette.Solarized.Yellow |> Color.WithAlpha alpha
                let textLength = font48.MeasureString("Paused")
                sb.DrawString(font48, "Paused", Vector2(320f - textLength.X / 2f, 100f), c) |> ignore

