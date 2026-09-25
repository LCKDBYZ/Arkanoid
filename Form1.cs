using System;

namespace Arkanoid
{
    public partial class Form1 : Form {
        // Ball
        private float ballX = 400, ballY = 300;
        private float ballDX = 4, ballDY = -4;
        private int ballSize = 15;
        private float ballSpeedMultiplier = 1.0f;
        private const float speedIncreasePerBrick = 0.03f; // 3% speed up

        // Platform
        private float paddleX = 350;
        private int paddleY = 550;
        private int paddleWidth = 100, paddleHeight = 15;
        private int paddleSpeed = 10;

        // Bricks
        private List<Brick> bricks = new List<Brick>();

        // Keyboard status
        private bool leftPressed = false;
        private bool rightPressed = false;

        private bool arrowLeftPressed = false;
        private bool arrowRightPressed = false;

        // Timer
        private System.Windows.Forms.Timer gameTimer;

        // Launch control
        private bool isLaunched = false;
        private float aimAngle = 0f;
        private const float maxAimAngle = 60f;
        private const float aimRotateSpeed = 4f;
        private const float baseBallSpeed = 6f;

        //Game Status
        private GameState currentState = GameState.Menu; // starts with menu
        private SaveData saveData;
        private int lives = 1;
        private int score = 0;


        public Form1() {
            InitializeComponent();

            saveData = SaveManager.Load();
            gameTimer = new System.Windows.Forms.Timer();
            gameTimer.Interval = 16; // about 60 FPS
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            this.DoubleBuffered = true;
            this.KeyPreview = true;
            this.ClientSize = new Size(800, 600);

            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;

            CreateBricks();

        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void GameTimer_Tick(object sender, EventArgs e) {
            if (currentState != GameState.Playing) return;

            // Moving the platform
            if (leftPressed) paddleX -= paddleSpeed;
            if (rightPressed) paddleX += paddleSpeed;
            paddleX = Math.Clamp(paddleX, 0, ClientSize.Width - paddleWidth);

            // Launch control
            if (!isLaunched) {
                // The ball stays on the platform
                ballX = paddleX + paddleWidth / 2f - ballSize / 2f;
                ballY = paddleY - ballSize;

                // Adjust aim angle
                if (arrowLeftPressed) aimAngle -= aimRotateSpeed;
                if (arrowRightPressed) aimAngle += aimRotateSpeed;
                aimAngle = Math.Clamp(aimAngle, -maxAimAngle, maxAimAngle);

                Invalidate();
                return; // if not started we shouldnt watch collisions and ball movement
            }

            // Moving the ball
            ballX += ballDX * ballSpeedMultiplier;
            ballY += ballDY * ballSpeedMultiplier;

            CollisionCheck();

            // Win check
            if (bricks.All(b => !b.IsAlive)) {
                currentState = GameState.Won;
                gameTimer.Stop();
            }

            Invalidate(); // Repaint
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            // Menu
            if (currentState == GameState.Menu) {
                if (e.KeyCode == Keys.Enter) {
                    ResetGame();
                }
                if (e.KeyCode == Keys.Escape) {
                    Application.Exit();
                }
                return; // nothing else should react while in the menu
            }

            // Game Over / Won
            if (currentState == GameState.GameOver || currentState == GameState.Won) {
                SaveHighScoreIfNeeded();
                if (e.KeyCode == Keys.Enter) {
                    ResetGame();
                }
                if (e.KeyCode == Keys.Escape) {
                    Application.Exit();
                }
                return;
            }

            // Pause
            if (e.KeyCode == Keys.Escape) {
                if (currentState == GameState.Playing) {
                    currentState = GameState.Paused;
                    gameTimer.Stop();
                }
                else if (currentState == GameState.Paused) {
                    currentState = GameState.Playing;
                    gameTimer.Start();
                }
                Invalidate();
                return;
            }

            // Only while playing
            if (currentState != GameState.Playing) return;

            if (e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.D) rightPressed = true;

            if (e.KeyCode == Keys.Left) { arrowLeftPressed = true; }
            if (e.KeyCode == Keys.Right) { arrowRightPressed = true; }

            if (e.KeyCode == Keys.Space && !isLaunched) {
                LaunchBall();
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.A) leftPressed = false;
            if (e.KeyCode == Keys.D) rightPressed = false;

            if (e.KeyCode == Keys.Left) { arrowLeftPressed = false; }
            if (e.KeyCode == Keys.Right) { arrowRightPressed = false; }
        }

        private void LaunchBall() {
            isLaunched = true;
            double rad = aimAngle * Math.PI / 180.0;
            ballDX = (float)(Math.Sin(rad) * baseBallSpeed);
            ballDY = (float)(-Math.Cos(rad) * baseBallSpeed);
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;

            // Menu - draw only the menu, nothing else
            if (currentState == GameState.Menu) {
                DrawMenu(g);
                return;
            }

            // we need to paint the bricks first so everything can go over them
            // Bricks
            foreach (Brick brick in bricks) {
                if (brick.IsAlive) {
                    g.FillRectangle(Brushes.SteelBlue, brick.X, brick.Y, brick.Width, brick.Height);
                }
            }

            // Platform
            g.FillRectangle(Brushes.Black, paddleX, paddleY, paddleWidth, paddleHeight);

            // Ball
            g.FillEllipse(Brushes.Red, ballX, ballY, ballSize, ballSize);

            

            // Game Over text
            if (currentState == GameState.GameOver) {
                string text = "GAME OVER";
                using (Font font = new Font("Arial", 40, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;
                    g.DrawString(text, font, Brushes.Red, x, y);
                }
                DrawRestartHint(g);
                DrawExitHint(g);

                leftPressed = false;
                rightPressed = false;
            }

            // Game Won text
            if (currentState == GameState.Won) {
                string text = "YOU WIN!";
                using (Font font = new Font("Arial", 40, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;
                    g.DrawString(text, font, Brushes.Yellow, x, y);
                }
                DrawRestartHint(g);
                DrawExitHint(g);
                leftPressed = false;
                rightPressed = false;
            }
            
            // Pause game
            if (currentState == GameState.Paused) {
                string text = "GAME PAUSED";
                using (Font font = new Font("Arial", 40, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;
                    g.DrawString(text, font, Brushes.Black, x, y);
                }
                string unPause = "Press ESCAPE to unpause";
                using (Font font = new Font("Arial", 20, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(unPause, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2 + 50;
                    g.DrawString(unPause, font, Brushes.Black, x, y);
                }
                {
                    float centerX = ballX + ballSize / 2f;
                    float centerY = ballY + ballSize / 2f;
                    float length = MathF.Sqrt(ballDX * ballDX + ballDY * ballDY);
                    float dirX = ballDX / length;
                    float dirY = ballDY / length;
                    float endX = centerX + dirX * 40;
                    float endY = centerY + dirY * 40;
                    g.DrawLine(Pens.Black, centerX, centerY, endX, endY);
                }

            }

            // Aim indicator
            if (!isLaunched && currentState == GameState.Playing) {
                float centerX = ballX + ballSize / 2f;
                float centerY = ballY + ballSize / 2f;
                double rad = aimAngle * Math.PI / 180.0;
                float endX = centerX + (float)(Math.Sin(rad) * 40);
                float endY = centerY - (float)(Math.Cos(rad) * 40);
                g.DrawLine(Pens.Black, centerX, centerY, endX, endY);

                string hint = "MOVE with A/D, AIM with <- ->, START with SPACE";
                using (Font font = new Font("Arial", 14)) {
                    g.DrawString(hint, font, Brushes.Black, 10, ClientSize.Height - 30);
                }
            }

            // Lives counter
            if (lives >= 0) {
                DrawLives(g);
                DrawScore(g);
            }
            
        }

        private void CreateBricks() {
            int rows = 4;
            int cols = 9;
            int brickWidth = 80;
            int brickHeight = 25;
            int padding = 8;   // Gap between bricks
            int offsetTop = 50; // Gap from the top
            float fillChance = 0.75f; // the chance to make a brick

            Random random = new Random();

            bricks.Clear();

            for (int row = 0; row < rows; row++) {
                int pointsForRow = (rows - row) * 10;
                for (int col = 0; col < cols; col++) {
                    if (random.NextDouble() > fillChance) continue;

                    float x = col * (brickWidth + padding) + padding;
                    float y = row * (brickHeight + padding) + offsetTop;
                    bricks.Add(new Brick(x, y, brickWidth, brickHeight, pointsForRow));
                }
            }

            // we dont want to have an empty map
            if (bricks.Count == 0) {
                CreateBricks();
            }
        }

        private void CollisionCheck() {
            // Ball hits walls
            if (ballX <= 0) {
                ballX = 0;
                ballDX *= -1;
            }
            else if (ballX + ballSize >= ClientSize.Width) {
                ballX = ClientSize.Width - ballSize;
                ballDX *= -1;
            }

            // Ball hits ceiling
            if (ballY <= 0) {
                ballY = 0;
                ballDY *= -1;
            }

            // Ball hits platform
            if (ballY + ballSize >= paddleY &&
                ballY + ballSize <= paddleY + paddleHeight &&
                ballX + ballSize >= paddleX &&
                ballX <= paddleX + paddleWidth) {

                // Where did the ball hit, based on the middle point (-1 = left, 0 = middle, +1 = right)
                float hitPos = (ballX + ballSize / 2f) - (paddleX + paddleWidth / 2f);
                float normalized = Math.Clamp(hitPos / (paddleWidth / 2f), -1f, 1f);

                float bounceAngleDeg = normalized * maxAimAngle;
                double rad = bounceAngleDeg * Math.PI / 180.0;

                float speed = MathF.Sqrt(ballDX * ballDX + ballDY * ballDY);
                ballDX = (float)(Math.Sin(rad) * speed);
                ballDY = (float)(-Math.Cos(rad) * speed);
            }

            // Ball hits bricks
            foreach (Brick brick in bricks) {
                if (!brick.IsAlive) continue;

                if (ballX + ballSize >= brick.X &&
                    ballX <= brick.X + brick.Width &&
                    ballY + ballSize >= brick.Y &&
                    ballY <= brick.Y + brick.Height) {
                    brick.IsAlive = false;

                    // How much overlap
                    float overlapLeft = (ballX + ballSize) - brick.X;
                    float overlapRight = (brick.X + brick.Width) - ballX;
                    float overlapTop = (ballY + ballSize) - brick.Y;
                    float overlapBottom = (brick.Y + brick.Height) - ballY;

                    float minOverlapX = Math.Min(overlapLeft, overlapRight);
                    float minOverlapY = Math.Min(overlapTop, overlapBottom);

                    if (minOverlapX < minOverlapY) {
                        ballDX *= -1; // Ball hit brick from the side
                    }
                    else {
                        ballDY *= -1; // Ball hit brick from the top/bottom
                    }
                    ballSpeedMultiplier += speedIncreasePerBrick;
                    ballSpeedMultiplier = Math.Min(ballSpeedMultiplier, 2.2f);
                    score += brick.Points;

                    break;
                }
            }

            // Ball hits ground
            if (ballY + ballSize >= ClientSize.Height) {
                lives--;
                if (lives < 0) {
                    currentState = GameState.GameOver;
                    gameTimer.Stop();
                }
                else {
                    ResetBallOnPaddle();
                }
            }
        }

        private void ResetGame() {
            // Ball
            ballX = 400;
            ballY = 300;
            ballDX = 4;
            ballDY = -4;
            ballSpeedMultiplier = 1.0f;

            // Paddle
            paddleX = 350;

            // Keyboard
            leftPressed = false;
            rightPressed = false;
            arrowLeftPressed = false;
            arrowRightPressed = false;

            // State flags
            currentState = GameState.Playing;
            isLaunched = false;

            if (lives < 0) {
                lives = 1;
            }
            score = 0;

            aimAngle = 0f;

            // Bricks
            CreateBricks();

            gameTimer.Start();
        }

        private void ResetBallOnPaddle() {
            isLaunched = false;
            aimAngle = 0f;
            ballDX = 4;
            ballDY = -4;
            ballSpeedMultiplier = 1.0f;
        }

        private void SaveHighScoreIfNeeded() {
            if (score > saveData.HighScore) {
                saveData.HighScore = score;
                SaveManager.Save(saveData);
            }
        }

        private void DrawRestartHint(Graphics g) {
            string newGame = "Press ENTER for a new game";
            using (Font font = new Font("Arial", 20, FontStyle.Bold)) {
                SizeF textSize = g.MeasureString(newGame, font);
                float x = (ClientSize.Width - textSize.Width) / 2;
                float y = (ClientSize.Height - textSize.Height) / 2 + 50;
                g.DrawString(newGame, font, Brushes.Black, x, y);
            }
        }

        private void DrawExitHint(Graphics g) {
            string newGame = "Press ESC to exit game";
            using (Font font = new Font("Arial", 20, FontStyle.Bold)) {
                SizeF textSize = g.MeasureString(newGame, font);
                float x = (ClientSize.Width - textSize.Width) / 2;
                float y = (ClientSize.Height - textSize.Height) / 2 + 80;
                g.DrawString(newGame, font, Brushes.Black, x, y);
            }
        }

        private void DrawLives(Graphics g) {
            string livesCount = "Remaning lives: " + lives;
            using (Font font = new Font("Arial", 10, FontStyle.Bold)) {
                SizeF textSize = g.MeasureString(livesCount, font);
                float x = (ClientSize.Width - textSize.Width);
                float y = (ClientSize.Height - textSize.Height);
                g.DrawString(livesCount, font, Brushes.Black, x, y);
            }
        }

        private void DrawScore(Graphics g) {
            string scoreText = "Score: " + score;
            string highScoreText = "High Score: " + saveData.HighScore;
            using (Font font = new Font("Arial", 14, FontStyle.Bold)) {
                g.DrawString(scoreText, font, Brushes.Black, 10, 10);
                g.DrawString(highScoreText, font, Brushes.Black, 10, 30);
            }
        }

        private void DrawMenu(Graphics g) {
            string title = "ARKANOID";
            using (Font titleFont = new Font("Arial", 50, FontStyle.Bold)) {
                SizeF titleSize = g.MeasureString(title, titleFont);
                float x = (ClientSize.Width - titleSize.Width) / 2;
                float y = ClientSize.Height / 3f;
                g.DrawString(title, titleFont, Brushes.SteelBlue, x, y);
            }

            string startHint = "Press ENTER to start";
            using (Font font = new Font("Arial", 20)) {
                SizeF hintSize = g.MeasureString(startHint, font);
                float x = (ClientSize.Width - hintSize.Width) / 2;
                float y = ClientSize.Height / 2f + 40;
                g.DrawString(startHint, font, Brushes.Black, x, y);
            }

            string exitHint = "Press ESC to exit";
            using (Font font = new Font("Arial", 20)) {
                SizeF hintSize = g.MeasureString(exitHint, font);
                float x = (ClientSize.Width - hintSize.Width) / 2;
                float y = ClientSize.Height / 2f + 80;
                g.DrawString(exitHint, font, Brushes.Black, x, y);
            }
        }
    }
}
