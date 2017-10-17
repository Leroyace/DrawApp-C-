using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DrawApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.SetStyle(
                System.Windows.Forms.ControlStyles.UserPaint | 
                System.Windows.Forms.ControlStyles.AllPaintingInWmPaint | 
                System.Windows.Forms.ControlStyles.OptimizedDoubleBuffer, 
                true);

            
        
        }
        class Line
        {
            public Point Start { get; set; }
            public Point End { get; set; }
        }
        private bool mouseDown = false;
        bool penPress = false;
        bool linePress = false;
        bool rectPress = false;
        bool ellipsePress = false;
       
        int colIndex = 0;
       
        private static Color col = Color.Black;
     
        static int penSize = 5;
        private const LineCap START_CAP = LineCap.ArrowAnchor;
        private const LineCap END_CAP = LineCap.ArrowAnchor;
        Point startPoint = new Point(0, 0);
        Point previousPoint = Point.Empty;
        Point currentPos,touchP;    // current mouse position
        List<Rectangle> rectangles = new List<Rectangle>();  // previous rectangles
                                                             // our collection of strokes for drawing
        List<List<Point>> _strokes = new List<List<Point>>();
        List<Color> color = new List<Color>();
       
        // the current stroke being drawn
        List<Point> _currStroke;
        private Stack<Line> lines = new Stack<Line>();
        private Stack<Line> ellipses = new Stack<Line>();

        // our pen
      
        Pen _pen = new Pen(col,penSize);
        

        private Rectangle getRectangle()
        {
            return new Rectangle(
                Math.Min(startPoint.X, currentPos.X),
                Math.Min(startPoint.Y, currentPos.Y),
                Math.Abs(startPoint.X - currentPos.X),
                Math.Abs(startPoint.Y - currentPos.Y));
        }


        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            
            mouseDown = true;
            currentPos = startPoint = e.Location;
            // mouse is down, starting new stroke
            _currStroke = new List<Point>();
            // add the initial point to the new stroke
            _currStroke.Add(e.Location);
            // add the new stroke collection to our strokes collection
            _strokes.Add(_currStroke);
            if(linePress)
                lines.Push(new Line { Start = e.Location });
            if (ellipsePress)
                ellipses.Push(new Line { Start = e.Location });
            
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                mouseDown = false;
                var rc = getRectangle();
                if (rc.Width > 0 && rc.Height > 0) rectangles.Add(rc);
               
                //panel1.Invalidate();
            }
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            toolStripStatusLabel1.Text = e.X + " , " + e.Y;
            touchP = e.Location;
           
            if (penPress == true && mouseDown == true)
            {
                // record stroke point if we're in drawing mode
                
                _currStroke.Add(e.Location);
                pictureBox1.Invalidate(); // refresh the drawing to see the latest section
                
  
            }

            if (ellipsePress == true && mouseDown == true)
            {
                if (ellipses.Count > 0 && e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    ellipses.Peek().End = e.Location;
                    pictureBox1.Invalidate();
                }
            }
            if(linePress == true&& mouseDown == true)
            {
                if (lines.Count > 0 && e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    lines.Peek().End = e.Location;
                    pictureBox1.Invalidate();
                }

               


            }
            if (rectPress == true && mouseDown == true)
            {
                currentPos = e.Location;
                pictureBox1.Invalidate();
            }

        }
       
        private void button1_Click(object sender, EventArgs e)
        {
            Graphics g1 = pictureBox1.CreateGraphics();
            g1.Clear(pictureBox1.BackColor);
            rectangles.Clear();
            lines.Clear();
            ellipses.Clear();
            _strokes.Clear();
            color.Clear();
            Invalidate();
            color.Add(col);
            colIndex = 0;
        }
       
        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            colorDialog1.ShowDialog();
            col = colorDialog1.Color;
            color.Add(col);
          
    
        }

        private void button3_Click(object sender, EventArgs e)
        {
            penPress = true;
            linePress = false;
            rectPress = false;
            ellipsePress = false;
            color.Add(col);
            colIndex = 0;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            penPress = false;
            linePress = false;
            rectPress = true;
            ellipsePress = false;
            color.Add(col);
           

        }

        private void button2_Click(object sender, EventArgs e)
        {
            linePress = true;
            penPress = false;
            rectPress = false;
            ellipsePress = false;
           
        }

        private void btnEllipse_Click(object sender, EventArgs e)
        {
            ellipsePress = true;
            linePress = false;
            penPress = false;
            rectPress = false;
          
           
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {


            var fd = new SaveFileDialog();
            fd.Filter = "Bmp(*.BMP;)|*.BMP;| Jpg(*Jpg)|*.jpg";
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                switch (Path.GetExtension(fd.FileName))
                {
                    case ".BMP":
                        pictureBox1.Image.Save(fd.FileName, System.Drawing.Imaging.ImageFormat.Bmp);
                        break;
                    case ".Jpg":
                        pictureBox1.Image.Save(fd.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
                        break;
                    default:
                        break;
                }
            }

        }

        private void panel1_Paint(object sender, PaintEventArgs e)
        {

            colIndex = 0;
            foreach (var line in lines)
            {
               
                
              
                e.Graphics.DrawLine(new Pen(col, penSize), line.Start, line.End);
              
            }
            
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;


          
                foreach (List<Point> stroke in _strokes.Where(x => x.Count > 1))
                {
                    if (color.Count - 1 == colIndex)
                    {
                        color.Add(col);
                    }
                    else
                    {
                        colIndex++;
                    }
                    _pen.Color = color[colIndex];
                    e.Graphics.DrawLines(_pen, stroke.ToArray());
                   
                    
                }

           
            if (rectangles.Count > 0) e.Graphics.DrawRectangles(new Pen(col, penSize), rectangles.ToArray());
            
            if (mouseDown && rectPress) e.Graphics.DrawRectangle(new Pen(col, penSize), getRectangle());
            

          
                foreach (var ellipse in ellipses)
                {   
                    
                    
                    e.Graphics.DrawEllipse(new Pen(col,penSize), ellipse.Start.X - (ellipse.End.X / 2), ellipse.Start.Y - (ellipse.End.Y / 2), ellipse.End.X, ellipse.End.Y);
                }
            
        }

        
    }
}
