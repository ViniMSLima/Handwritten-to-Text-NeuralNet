using System;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.IO;
using System.Drawing.Imaging;
using Python.Runtime;

// dotnet add package pythonnet

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
            Runtime.PythonDLL = "python311.dll";
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

            if (extractedBitmap != null)
            {
                // Obtenha as coordenadas dos retângulos das letras
                List<Rectangle> letters = ImageProcessor.ProcessImage(extractedBitmap);

                if (letters != null)
                {
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

                    int i = 0;
                    // Agora você pode passar cada bitmap da letra para a rede neural
                    foreach (Bitmap croppedLetter in croppedLetters)
                    {
                        Bitmap resizedImage = ResizeImage(croppedLetter, 128, 128);
                        resizedImage.Save($"{i}.png", ImageFormat.Png);

                        // Converta o bitmap da letra redimensionado em um formato aceito pela rede neural
                        byte[] byteArray = BitmapToByteArray(resizedImage);

                        MessageBox.Show("nao morre");
                        // Faça a previsão para o bitmap atual
                        dynamic prediction = PredictWithNeuralNetwork($"{i}.png");
                        i++;

                        // Exiba o resultado da previsão em uma MessageBox
                        // MessageBox.Show(prediction);
                    }

                    // Apaga apenas o que foi desenhado dentro da área delimitada
                    using (Graphics clearGraphics = Graphics.FromImage(Bmp))
                    {
                        // Apaga a área dentro do retângulo
                        clearGraphics.FillRectangle(new SolidBrush(Color.White), HighlightRect);
                    }
                    Pb.Refresh();
                }
                else
                {
                    MessageBox.Show("Lista de retângulos das letras retornou nula.");
                }
            }
            else
            {
                MessageBox.Show("O bitmap extraído está nulo.");
            }
        }


        private Bitmap ResizeImage(Bitmap image, int width, int height)
        {
            Bitmap resizedImage = new Bitmap(width, height);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                graphics.DrawImage(image, 0, 0, width, height);
            }
            return resizedImage;
        }

        private static dynamic PredictWithNeuralNetwork(string imgPath)
        {
            dynamic result = null;
            try
            {
                PythonEngine.Initialize();

                dynamic tf = Py.Import("tensorflow");
                dynamic np = Py.Import("numpy");
                dynamic model = tf.keras.models.load_model("C:/Users/disrct/Desktop/caracterDetector/Handwritten-to-Text-NeuralNet/checkpoints/model.keras");

                // pip install pillow
                dynamic list = new PyList();

                dynamic img = tf.keras.utils.load_img("C:/Users/disrct/Desktop/caracterDetector/Handwritten-to-Text-NeuralNet/application/0.png");
                list.append(img);
                MessageBox.Show("Erro na previsão: " + model);

                result = model.predict(list);

                return result; // Retorna o resultado da previsão
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro na previsão: " + ex.Message);
                return null; // Retorna nulo em caso de erro
            }
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

        private void run_cmd()
        {

            string fileName = @"C:\sample_script.py";

            Process p = new Process();
            p.StartInfo = new ProcessStartInfo(@"C:\Python27\python.exe", fileName)
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
            p.Start();

            string output = p.StandardOutput.ReadToEnd();
            p.WaitForExit();

            Console.WriteLine(output);

            Console.ReadLine();

        }
    }

}


// C:\Program Files\Python311\python.exe