using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

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
        private int currentThickness = 5;
        private PointF markerPosition = new PointF(20, 40); // Posição inicial do marcador

        // Fator de suavização
        private readonly float smoothFactor = 0.3f;

        // Lista de pontos suavizados
        private List<PointF> smoothedPoints = new List<PointF>();

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
            this.Cursor = new Cursor("a.cur");
            this.KeyDown += KeyBoardDown;
        }

        private Bitmap ExtractDrawingFromHighlightRect()
        {
            // Define o retângulo de recorte relativo à imagem inteira
            Rectangle relativeCropRect = new(HighlightRect.X, HighlightRect.Y, HighlightRect.Width - HighlightRect.X, HighlightRect.Height - HighlightRect.Y);

            Bitmap croppedBitmap = new(relativeCropRect.Width, relativeCropRect.Height);

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
            Pb.MouseWheel += MouseWheelMoved;
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
                    List<string> paths = new();

                    // Agora você pode passar cada bitmap da letra para a rede neural
                    foreach (Bitmap croppedLetter in croppedLetters)
                    {
                        Bitmap resizedImage = ResizeImage(croppedLetter, 128, 128);
                        resizedImage.Save($"{i}.png", ImageFormat.Png);

                        paths.Add($"{i}.png");
                        i++;
                    }
                    RunCmdAsync(paths);


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

        private Bitmap ResizeImage(Bitmap image, int targetWidth, int targetHeight, double paddingPercentage = 0.5)
        {
            // Calcular o novo tamanho da imagem com base na porcentagem de preenchimento
            int paddedWidth = (int)(targetWidth * (1 - paddingPercentage));
            int paddedHeight = (int)(targetHeight * (1 - paddingPercentage));

            // Criar a nova imagem com a borda em branco
            Bitmap resizedImage = new Bitmap(targetWidth, targetHeight);
            using (Graphics graphics = Graphics.FromImage(resizedImage))
            {
                // Preencher o fundo com branco
                graphics.FillRectangle(Brushes.White, 0, 0, targetWidth, targetHeight);

                // Calcular as coordenadas para centralizar a imagem original
                int x = (targetWidth - paddedWidth) / 2;
                int y = (targetHeight - paddedHeight) / 2;

                // Configurar a qualidade de interpolação
                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;

                // Desenhar a imagem original centralizada
                graphics.DrawImage(image, x, y, paddedWidth, paddedHeight);
            }
            return resizedImage;
        }

        // Método auxiliar para converter um bitmap em um array de bytes
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
                    UpdateMouseCursor();
                    break;
                case Keys.Enter:
                    markerPosition.X = 20;
                    markerPosition.Y += 15;
                    break;
            }
        }

        private void MouseWheelMoved(object sender, MouseEventArgs e)
        {
            // Ajusta o tamanho da borracha com base na direção da rotação da roda do mouse
            if (e.Delta > 0)
            {
                // Aumenta o tamanho da borracha, mas limita para não ultrapassar um valor máximo
                currentThickness = Math.Min(currentThickness + 1, 20); // Valor máximo definido como 20
            }
            else
            {
                // Diminui o tamanho da borracha, mas limita para não ficar menor que um valor mínimo
                currentThickness = Math.Max(currentThickness - 1, 1); // Valor mínimo definido como 1
            }
        }

        private void UpdateMouseCursor()
        {
            Cursor newCursor;
            IsEraser = !IsEraser;

            if (IsEraser)
                newCursor = new Cursor("b.cur");
            else
                newCursor = new Cursor("a.cur"); // Substitua "caminho/para/lapis.cur" pelo caminho do arquivo do cursor do lápis

            this.Cursor = newCursor;
        }

        private void MouseClickDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsDrawing = true;
                PrevMouse = e.Location;
                smoothedPoints.Add(PrevMouse); // Adiciona o primeiro ponto
                DrawPoint(e.Location, Color.Black);
            }

            if (e.Button == MouseButtons.Right)
            {
                UpdateMouseCursor();
            }
        }

        private void DrawPoint(Point location, Color color)
        {
            using (var pen = new Pen(color, currentThickness))
            {
                G.DrawEllipse(pen, new Rectangle(location, new Size(currentThickness, currentThickness)));
            }
        }

        private void MouseClickUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                IsDrawing = false;
                smoothedPoints.Clear(); // Limpa os pontos suavizados ao terminar o desenho
            }
        }

        private void MouseMoved(object sender, MouseEventArgs e)
        {
            if (IsDrawing)
            {
                if (DrawingArea.Contains(e.Location))
                {
                    PointF currentPoint = e.Location;

                    // Calcula o ponto suavizado
                    PointF smoothedPoint = SmoothPoint(currentPoint);

                    // Desenha a linha suavizada
                    if (IsEraser)
                        DrawLineSmooth(PrevMouse, smoothedPoint, Color.White, currentThickness);
                    else
                        DrawLineSmooth(PrevMouse, smoothedPoint, Color.Black, currentThickness);

                    // Atualiza o ponto anterior
                    PrevMouse = smoothedPoint;
                }

                Pb.Refresh();
            }
        }

        private PointF SmoothPoint(PointF currentPoint)
        {
            // Se não houver pontos anteriores, retorna o ponto atual
            if (smoothedPoints.Count == 0)
                return currentPoint;

            // Calcula o ponto suavizado como a média ponderada entre o ponto atual e o ponto anterior
            PointF smoothedPoint = new(
                currentPoint.X * smoothFactor + smoothedPoints[^1].X * (1 - smoothFactor),
                currentPoint.Y * smoothFactor + smoothedPoints[^1].Y * (1 - smoothFactor)
            );

            // Adiciona o ponto suavizado à lista
            smoothedPoints.Add(smoothedPoint);
            return smoothedPoint;
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

        private async Task<List<string>> RunCmdAsync(
            List<string> paths,
            string cmd = "C:/Program Files/Python311/python.exe",
            string scriptPath = "predict.py"
        )
        {
            List<string> outputLines = new();

            ProcessStartInfo start = new() { FileName = cmd };

            string args = $"{scriptPath} {paths.Count}";

            foreach (string path in paths)
            {
                args += $" {path}";
            }

            start.Arguments = args;

            // MessageBox.Show(args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;

            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string line;
                    while ((line = await reader.ReadLineAsync()) != null)
                        outputLines.Add(line);
                }
            }
            
            // MessageBox.Show(outputLines[^1]);
            WriteTextOnScreen(outputLines[^1]);

            return outputLines;
        }

        // Método para escrever o texto retornado pelo Python na tela
        private void WriteTextOnScreen(string text)
        {
            using (Graphics graphics = Graphics.FromImage(Bmp))
            {
                // Define a fonte e o tamanho
                Font font = new Font("Arial", 12, FontStyle.Regular);

                // Desenha o texto na posição indicada pelo marcador
                graphics.DrawString(text, font, Brushes.Black, markerPosition);

                // Adiciona um espaço após a palavra escrita
                markerPosition.X += graphics.MeasureString(text, font).Width + 1;
            }

            Pb.Refresh();
        }

    }
}
