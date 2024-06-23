using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace BallSimulation
{
    public class Ball
    {
        public float Radius { get; set; }
        public PointF Position { get; set; }
        public Color Color { get; set; }
        public PointF Velocity { get; set; }

        public Ball(float radius, PointF position, Color color, PointF velocity)
        {
            Radius = radius;
            Position = position;
            Color = color;
            Velocity = velocity;
        }

        public virtual void Update()
        {
            Position = new PointF(Position.X + Velocity.X, Position.Y + Velocity.Y);
        }

        public virtual void ResolveCollision(Ball other)
        {
         
        }
    }

    public class RegularBall : Ball
    {
        public RegularBall(float radius, PointF position, Color color, PointF velocity)
            : base(radius, position, color, velocity) { }

        public override void ResolveCollision(Ball other)
        {
            if (other is RegularBall)
            {
                if (this.Radius > other.Radius)
                {
                    this.Radius += other.Radius;
                    this.Color = CombineColors(this.Color, this.Radius, other.Color, other.Radius);
                    other.Radius = 0;
                }
                else
                {
                    other.Radius += this.Radius;
                    other.Color = CombineColors(other.Color, other.Radius, this.Color, this.Radius);
                    this.Radius = 0;
                }
            }
            else if (other is MonsterBall)
            {
                other.Radius += this.Radius;
                this.Radius = 0;
            }
            else if (other is RepelentBall)
            {
                other.Color = this.Color;
                this.Velocity = new PointF(-this.Velocity.X, -this.Velocity.Y);
            }
        }

        private Color CombineColors(Color c1, float r1, Color c2, float r2)
        {
            int r = (int)((c1.R * r1 + c2.R * r2) / (r1 + r2));
            int g = (int)((c1.G * r1 + c2.G * r2) / (r1 + r2));
            int b = (int)((c1.B * r1 + c2.B * r2) / (r1 + r2));
            return Color.FromArgb(r, g, b);
        }
    }

    public class MonsterBall : Ball
    {
        public MonsterBall(float radius, PointF position, Color color)
            : base(radius, position, color, new PointF(0, 0)) { }

        public override void ResolveCollision(Ball other)
        {
            if (other is RegularBall || other is RepelentBall)
            {
                this.Radius += other.Radius;
                other.Radius = 0;
            }
        }
    }

    public class RepelentBall : Ball
    {
        public RepelentBall(float radius, PointF position, Color color, PointF velocity)
            : base(radius, position, color, velocity) { }

        public override void ResolveCollision(Ball other)
        {
            if (other is RepelentBall)
            {
                Color tempColor = this.Color;
                this.Color = other.Color;
                other.Color = tempColor;
            }
            else if (other is MonsterBall)
            {
                this.Radius /= 2;
            }
            else if (other is RegularBall)
            {
                this.Color = other.Color;
                other.Velocity = new PointF(-other.Velocity.X, -other.Velocity.Y);
            }
        }
    }

    public class Simulation
    {
        private List<Ball> balls;
        private int canvasWidth;
        private int canvasHeight;
        private Form1 form;
        private System.Windows.Forms.Timer timer;

        public Simulation(Form1 form, int canvasWidth, int canvasHeight, int numRegularBalls, int numMonsterBalls, int numRepelentBalls)
        {
            this.form = form;
            this.canvasWidth = canvasWidth;
            this.canvasHeight = canvasHeight;
            balls = new List<Ball>();

            Random random = new Random();

            for (int i = 0; i < numRegularBalls; i++)
            {
                balls.Add(new RegularBall(
                    random.Next(10, 30),
                    new PointF(random.Next(0, canvasWidth), random.Next(0, canvasHeight)),
                    Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)),
                    new PointF((float)(random.NextDouble() * 4 - 2), (float)(random.NextDouble() * 4 - 2))
                ));
            }

            for (int i = 0; i < numMonsterBalls; i++)
            {
                balls.Add(new MonsterBall(
                    random.Next(10, 30),
                    new PointF(random.Next(0, canvasWidth), random.Next(0, canvasHeight)),
                    Color.Black
                ));
            }

            for (int i = 0; i < numRepelentBalls; i++)
            {
                balls.Add(new RepelentBall(
                    random.Next(10, 30),
                    new PointF(random.Next(0, canvasWidth), random.Next(0, canvasHeight)),
                    Color.FromArgb(random.Next(256), random.Next(256), random.Next(256)),
                    new PointF((float)(random.NextDouble() * 4 - 2), (float)(random.NextDouble() * 4 - 2))
                ));
            }
        }

        public void Run()
        {
            timer = new System.Windows.Forms.Timer();
            timer.Interval = 20; 
            timer.Tick += (sender, e) => Turn();
            timer.Start();

            Application.Run(form);
        }

        private void Turn()
        {
            foreach (var ball in balls)
            {
                ball.Update();
                HandleCollisions(ball);
                HandleWallCollisions(ball);
            }

            balls.RemoveAll(b => b.Radius <= 0);

            CheckForEndCondition();

            form.Invalidate();
        }

        private void HandleCollisions(Ball ball)
        {
            foreach (var other in balls)
            {
                if (ball != other && IsColliding(ball, other))
                {
                    ball.ResolveCollision(other);
                    other.ResolveCollision(ball);
                }
            }
        }

        private bool IsColliding(Ball a, Ball b)
        {
            float distance = (float)Math.Sqrt(Math.Pow(a.Position.X - b.Position.X, 2) + Math.Pow(a.Position.Y - b.Position.Y, 2));
            return distance < a.Radius + b.Radius;
        }

        private void HandleWallCollisions(Ball ball)
        {
            if (ball.Position.X - ball.Radius < 0 || ball.Position.X + ball.Radius > canvasWidth)
            {
                ball.Velocity = new PointF(-ball.Velocity.X, ball.Velocity.Y);
            }

            if (ball.Position.Y - ball.Radius < 0 || ball.Position.Y + ball.Radius > canvasHeight)
            {
                ball.Velocity = new PointF(ball.Velocity.X, -ball.Velocity.Y);
            }
        }

        private void CheckForEndCondition()
        {
            bool hasRegularBalls = balls.Any(b => b is RegularBall);

            if (!hasRegularBalls)
            {
                timer.Stop(); 
                MessageBox.Show("All regular balls have been eaten!", "Simulation ended!");
                Application.Exit();
            }
        }

        public List<Ball> GetBalls()
        {
            return balls;
        }
    }

    public class Form1 : Form
    {
        private Simulation simulation;

        public Form1()
        {
            this.DoubleBuffered = true; 
            this.Width = 800;
            this.Height = 600;
            this.Text = "Big Ball Game";

            simulation = new Simulation(this, 800, 600, 10, 2, 3);
            simulation.Run();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            foreach (var ball in simulation.GetBalls())
            {
                Brush brush = new SolidBrush(ball.Color);
                e.Graphics.FillEllipse(brush, ball.Position.X - ball.Radius, ball.Position.Y - ball.Radius, ball.Radius * 2, ball.Radius * 2);
            }
        }
    }

    public static class Program
    {
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}