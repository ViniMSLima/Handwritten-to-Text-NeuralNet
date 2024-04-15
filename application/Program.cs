using System.Drawing;
using System.Windows.Forms;
using WriteOnScreen;

ApplicationConfiguration.Initialize();
Draw draw = Draw.GetInstance();
Application.Run(draw);