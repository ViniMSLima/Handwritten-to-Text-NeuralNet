using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using System.IO;
using System.Drawing.Imaging;
using Python.Runtime;


using CharacterFinder;


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

        private Bitmap ExtractDrawingFromHighlightRect()
        {
            // Define o retângulo de recorte relativo à imagem inteira
            Rectangle relativeCropRect = new Rectangle(HighlightRect.X, HighlightRect.Y, HighlightRect.Width - HighlightRect.X, HighlightRect.Height - HighlightRect.Y);
            MessageBox.Show(HighlightRect.X.ToString());
            MessageBox.Show(HighlightRect.Y.ToString());
            MessageBox.Show((HighlightRect.Width - HighlightRect.X).ToString());
            MessageBox.Show((HighlightRect.Height - HighlightRect.X).ToString());

            // Cria um bitmap para armazenar a parte recortada da imagem
            Bitmap croppedBitmap = new Bitmap(relativeCropRect.Width, relativeCropRect.Height);

            // Realiza o recorte da parte desejada da imagem original
            using (Graphics g = Graphics.FromImage(croppedBitmap))
            {
                g.DrawImage(Bmp, new Rectangle(0, 0, croppedBitmap.Width, croppedBitmap.Height), relativeCropRect, GraphicsUnit.Pixel);
            }

            return croppedBitmap;
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
            G.DrawRectangle(new Pen(Color.Green, 2), HighlightRect.X - 20, HighlightRect.Y - 10, HighlightRect.Width, HighlightRect.Height);
        }

        private void ClearScreen()
        {
            // Extract drawing from the highlighted rectangle
            Bitmap extractedBitmap = ExtractDrawingFromHighlightRect();

            // Obtenha as coordenadas dos retângulos das letras
            List<Rectangle> letters = ImageProcessor.ProcessImage(extractedBitmap);

            // Lista para armazenar os bitmaps recortados de cada letra
            List<Bitmap> croppedLetters = new List<Bitmap>();

            // Recorte cada letra da imagem original
            foreach (Rectangle letterRect in letters)
            {
                // Crie um bitmap para armazenar a parte recortada da imagem original
                Bitmap croppedLetterBitmap = new Bitmap(letterRect.Width, letterRect.Height);

                // Realize o recorte da parte desejada da imagem original
                using (Graphics g = Graphics.FromImage(croppedLetterBitmap))
                {
                    g.DrawImage(extractedBitmap, new Rectangle(0, 0, croppedLetterBitmap.Width, croppedLetterBitmap.Height),
                                letterRect, GraphicsUnit.Pixel);
                }

                // Adicione o bitmap recortado à lista
                croppedLetters.Add(croppedLetterBitmap);
            }

            // Agora você pode passar cada bitmap da letra para a rede neural
            foreach (Bitmap croppedLetter in croppedLetters)
            {
                // Converta o bitmap da letra em um formato aceito pela rede neural
                byte[] byteArray = BitmapToByteArray(croppedLetter);

                // Faça a previsão para o bitmap atual
                dynamic prediction = PredictWithNeuralNetwork(byteArray);

                // Exiba o resultado da previsão em uma MessageBox
                MessageBox.Show(prediction.ToString());
            }

            // Apaga apenas o que foi desenhado dentro da área delimitada
            using (Graphics clearGraphics = Graphics.FromImage(Bmp))
            {
                // Apaga a área dentro do retângulo
                clearGraphics.FillRectangle(new SolidBrush(Color.White), HighlightRect);
            }
            Pb.Refresh();
        }


        // Método para fazer a previsão com a rede neural
        private static dynamic PredictWithNeuralNetwork(byte[] data)
        {
            Runtime.PythonDLL = "python311.dll";

            // Inicialize o PythonEngine
            PythonEngine.Initialize();

            // Importe os módulos necessários
            dynamic tf = Py.Import("tensorflow");
            dynamic np = Py.Import("numpy");
            dynamic model = tf.keras.models.load_model("C:/Users/disrct/Desktop/VC_Projeto/checkpoints/model2.keras");

            // Converta os dados para um formato aceito pelo modelo
            dynamic dataArray = np.array(data);

            // Faça a previsão
            dynamic result = model.predict(dataArray);

            // Desligue o PythonEngine
            PythonEngine.Shutdown();

            return result;
        }


        // Método auxiliar para converter um bitmap em um array de bytes
        private static byte[] BitmapToByteArray(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
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
