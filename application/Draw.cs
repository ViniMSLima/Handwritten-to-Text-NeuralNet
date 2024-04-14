using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D; // Adicionando a diretiva para o namespace System.Drawing.Drawing2D

namespace WriteOnScreen
{
    public class Draw : Form
    {
        private PictureBox Pb { get; set; }
        private Bitmap Bmp { get; set; }
        private Graphics G { get; set; }
        private bool IsEraser { get; set; }
        private static PointF PrevMouse { get; set; }
        private static bool IsDrawing { get; set; }
        private Rectangle DrawingArea { get; set; } // Retângulo delimitador
        private Rectangle HighlightRect { get; set; } // Retângulo de destaque

        public static Draw _instance { get; set; }

        public static Draw GetInstance()
            => _instance ??= new Draw();

        private Draw()
        {
            InitializeForm();
            InitializePictureBox();
            InitializeDrawing();
            EnterFullScreen();
        }

        private void InitializeForm()
        {
            this.Text = "Write On Screen";
            this.FormBorderStyle = FormBorderStyle.None; // Remove bordas
            this.WindowState = FormWindowState.Maximized; // Maximiza janela
            this.KeyPreview = true;
            this.KeyDown += KeyBoardDown;
        }

        private void InitializePictureBox()
        {
            Pb = new PictureBox { Dock = DockStyle.Fill };
            this.Controls.Add(Pb);
        }

        private void InitializeDrawing()
        {
            int screenWidth = Screen.PrimaryScreen.Bounds.Width;
            int screenHeight = Screen.PrimaryScreen.Bounds.Height;
            Bmp = new Bitmap(screenWidth, screenHeight); // Usar o tamanho da tela
            G = Graphics.FromImage(Bmp);
            G.Clear(Color.White);

            Pb.Image = Bmp;
            Pb.MouseDown += MouseClickDown;
            Pb.MouseUp += MouseClickUp;
            Pb.MouseMove += MouseMoved;

            // Define a área de desenho como toda a tela
            DrawingArea = new Rectangle(0, 0, screenWidth, screenHeight);

            // Define o retângulo de destaque
            HighlightRect = new Rectangle((int)(Screen.PrimaryScreen.Bounds.Width * 0.6), (int)(Screen.PrimaryScreen.Bounds.Height * 0.7), Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            // Desenha a borda do retângulo de destaque
            G.DrawRectangle(new Pen(Color.Red, 2), HighlightRect);
        }

        private void ClearScreen()
        {
            // Apaga apenas o que foi desenhado dentro da área delimitada
            using (Graphics clearGraphics = Graphics.FromImage(Bmp))
            {

                // Apaga a área dentro do retângulo
                clearGraphics.FillRectangle(new SolidBrush(Color.White), HighlightRect);
            }
            Pb.Refresh();
        }


        private void KeyBoardDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Application.Exit();
                    break;
                case Keys.Space:
                    ClearScreen();
                    break;
                case Keys.B:
                    IsEraser = !IsEraser;
                    break;
            }
        }

        private void MouseClickDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsDrawing = true;
                PrevMouse = e.Location;
                DrawPoint(e.Location, Color.Black, 5);
            }
        }

        private void DrawPoint(Point location, Color color, int size)
        {

            using (var pen = new Pen(color, size))
            {
                G.DrawEllipse(pen, new Rectangle(location, new Size(size, size)));
            }

        }

        private void MouseClickUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsDrawing = false;
            }
        }

        private void MouseMoved(object sender, MouseEventArgs e)
        {
            if (IsDrawing)
            {
                if (IsEraser)
                {
                    // Aumenta a área de apagamento
                    Rectangle eraseRect = new Rectangle(e.X - 10, e.Y - 10, 20, 20);

                    // Apaga dentro da área delimitada
                    if (DrawingArea.Contains(e.Location))
                    {
                        using (GraphicsPath path = new GraphicsPath())
                        {
                            path.AddEllipse(eraseRect);
                            G.SetClip(path);
                            G.Clear(Color.White);
                            G.ResetClip();
                        }
                    }
                }
                else
                {
                    // Desenha apenas dentro da área delimitada
                    if (DrawingArea.Contains(e.Location))
                    {
                        DrawLineSmooth(PrevMouse, e.Location, Color.Black, 5);
                        PrevMouse = e.Location;
                    }
                }

                Pb.Refresh();
            }
        }

        private void DrawLineSmooth(PointF p1, PointF p2, Color color, int size)
        {
            using (var pen = new Pen(color, size))
            {
                // Calculate distance and direction
                float dx = p2.X - p1.X;
                float dy = p2.Y - p1.Y;
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);

                // Calculate unit direction vector
                float unitX = dx / distance;
                float unitY = dy / distance;

                // Draw line segment by segment
                for (float i = 0; i < distance; i += 0.5f)
                {
                    float x = p1.X + unitX * i;
                    float y = p1.Y + unitY * i;
                    G.DrawEllipse(pen, new RectangleF(x, y, size, size));
                }
            }
        }

        private void EnterFullScreen()
        {
            // Desativa ALT+F4 para sair do modo de tela cheia
            this.ControlBox = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.StartPosition = FormStartPosition.CenterScreen;
        }
    }

}
