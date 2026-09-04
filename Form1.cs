namespace Arkanoid
{
    public partial class Form1 : Form {

        // Ball
        private float ballX = 400, ballY = 300;
        private float ballDX = 4, ballDY = -4;
        private int ballSize = 15;

        // Platform
        private float paddleX = 350;
        private int paddleY = 550;
        private int paddleWidth = 100, paddleHeight = 15;
        private int paddleSpeed = 8;

        // Keyboard status
        private bool leftPressed = false;
        private bool rightPressed = false;

        // Timer
        private System.Windows.Forms.Timer gameTimer;

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

        }

        private void Form1_Load(object sender, EventArgs e) {

        }

        private void GameTimer_Tick(object sender, EventArgs e) {
            // Moving the platform
            if (leftPressed) paddleX -= paddleSpeed;
            if (rightPressed) paddleX += paddleSpeed;
            paddleX = Math.Clamp(paddleX, 0, ClientSize.Width - paddleWidth);

            // Moving the ball
            ballX += ballDX;
            ballY += ballDY;

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

            Invalidate(); // Repaint
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) leftPressed = true;
            if (e.KeyCode == Keys.Right) rightPressed = true;
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e) {
            if (e.KeyCode == Keys.Left) leftPressed = false;
            if (e.KeyCode == Keys.Right) rightPressed = false;
        }

        protected override void OnPaint(PaintEventArgs e) {
            base.OnPaint(e);
            var g = e.Graphics;

            // Platform
            g.FillRectangle(Brushes.Black, paddleX, paddleY, paddleWidth, paddleHeight);

            // Ball
            g.FillEllipse(Brushes.Red, ballX, ballY, ballSize, ballSize);
        }
    }
}
