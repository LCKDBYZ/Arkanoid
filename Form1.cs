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
        private int paddleSpeed = 8;

        // Bricks
        private List<Brick> bricks = new List<Brick>();

        // Keyboard status
        private bool leftPressed = false;
        private bool rightPressed = false;

        // Timer
        private System.Windows.Forms.Timer gameTimer;

        //Game Status
        private bool isGameOver = false;
        private bool isGameWon = false;
        private bool isGamePaused = false;

        public Form1() {
            InitializeComponent();

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
            if (isGameOver) return;
            if (isGameWon) return;

            // Moving the platform
            if (leftPressed) paddleX -= paddleSpeed;
            if (rightPressed) paddleX += paddleSpeed;
            paddleX = Math.Clamp(paddleX, 0, ClientSize.Width - paddleWidth);

            // Moving the ball
            ballX += ballDX * ballSpeedMultiplier;
            ballY += ballDY * ballSpeedMultiplier;

            CollisionCheck();

            // Win check
            if (bricks.All(b => !b.IsAlive)) {
                isGameWon = true;
                gameTimer.Stop();
            }

            Invalidate(); // Repaint
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = true;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = true;

            if (e.KeyCode == Keys.Enter && (isGameOver || isGameWon)) {
                ResetGame();
            }
            if (e.KeyCode == Keys.Escape) {
                if (!isGamePaused) {
                    isGamePaused = true;
                    gameTimer.Stop();
                }
                else {
                    isGamePaused = false;
                    gameTimer.Start();
                }
                Invalidate(); // Repaint
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.A) leftPressed = false;
            if (e.KeyCode == Keys.Right || e.KeyCode == Keys.D) rightPressed = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;

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
            if (isGameOver) {
                string text = "GAME OVER";
                using (Font font = new Font("Arial", 40, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;
                    g.DrawString(text, font, Brushes.Red, x, y);
                }
                DrawRestartHint(g);

                leftPressed = false;
                rightPressed = false;
            }

            // Game Won text
            if (isGameWon) {
                string text = "YOU WIN!";
                using (Font font = new Font("Arial", 40, FontStyle.Bold)) {
                    SizeF textSize = g.MeasureString(text, font);
                    float x = (ClientSize.Width - textSize.Width) / 2;
                    float y = (ClientSize.Height - textSize.Height) / 2;
                    g.DrawString(text, font, Brushes.Yellow, x, y);
                }
                DrawRestartHint(g);
                leftPressed = false;
                rightPressed = false;
            }
            
            // Pause game
            if (!isGameWon && !isGameOver && isGamePaused) {
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
            }
        }

        private void CreateBricks() {
            int rows = 3;
            int cols = 9;
            int brickWidth = 80;
            int brickHeight = 25;
            int padding = 8;   // Gap between bricks
            int offsetTop = 50; // Gap from the top

            bricks.Clear();

            for (int row = 0; row < rows; row++) {
                for (int col = 0; col < cols; col++) {
                    float x = col * (brickWidth + padding) + padding;
                    float y = row * (brickHeight + padding) + offsetTop;
                    bricks.Add(new Brick(x, y, brickWidth, brickHeight));
                }
            }
        }

        private void CollisionCheck() {
            // Ball hits walls
            if (ballX <= 0 || ballX + ballSize >= ClientSize.Width) {
                ballDX *= -1;
            }

            // Ball hits ceiling
            if (ballY <= 0) {
                ballDY *= -1;
            }

            // Ball hits platform
            if (ballY + ballSize >= paddleY &&
                ballY + ballSize <= paddleY + paddleHeight &&
                ballX + ballSize >= paddleX &&
                ballX <= paddleX + paddleWidth) {
                ballDY *= -1;
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

                    break;
                }
            }

            // Ball hits ground
            if (ballY + ballSize >= ClientSize.Height) {
                isGameOver = true;
                gameTimer.Stop();
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

            // State flags
            isGameOver = false;
            isGameWon = false;
            isGamePaused = false;

            // Bricks
            CreateBricks();

            gameTimer.Start();
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
    }
}
