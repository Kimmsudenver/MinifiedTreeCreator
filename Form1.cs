using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MinifiedTreeCreator.Objects;

namespace MinifiedTreeCreator
{
    /// <summary>
    /// Created by Will Patterson
    /// 4/29/2014
    /// 
    /// Allows input of points and calculates the shortest 
    /// Route to connect every point into a tree with no cycles.
    /// </summary>
    public partial class Form1 : Form
    {
        #region private constants
        private const string _fileDelimimter = "#_#";
        private const string _title = "Minified Tree Creator";
        private const int _pointRadius = 4;
        private const int _padding = 10;
        #endregion

        #region private defaults
        private string _fileName;
        private Color _bgColor = Color.LightSeaGreen;
        private Color _fgColor = Color.Red;
        #endregion

        #region private properties
        /// <summary>
        /// Current file name, setting will change the Form.Text to contain the file name.
        /// </summary>
        private string fileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                this.Text = _title;
                if(!String.IsNullOrEmpty(value))
                    this.Text += " - " + Path.GetFileNameWithoutExtension(_fileName);
            }
        }
        /// <summary>
        /// Set to true when the user is dragging the graph.
        /// </summary>
        private bool isDragging;
        /// <summary>
        /// List of user entered points
        /// </summary>
        private List<GraphPoint> points;
        /// <summary>
        /// List of generated line segments between two points.
        /// </summary>
        private List<Segment> segments;
        /// <summary>
        /// Integers used for transformation and naming
        /// </summary>
        private int zoomFactor, zoomX, zoomY, dragStartX, dragStartY, dragX, dragY,
            maxX = 0, maxY = 0, minX = 0, minY = 0, nextName;

        private int maxWidth
        {
            get
            {
                return panel1.Width - _padding;
            }
        }
        private int maxHeight
        {
            get
            {
                return panel1.Height - _padding;
            }
        }
        private Color bgColor
        {
            get
            {
                return _bgColor;
            }
            set
            {
                bgColorPanel.BackColor = _bgColor = value;
                panel1.Refresh();
            }
        }
        private Color fgColor
        {
            get
            {
                return _fgColor;
            }
            set
            {
                fgColorPanel.BackColor = _fgColor = value;
                panel1.Refresh();
            }
        }
        private Control lastFocused;
        #endregion

        public Form1()
        {
            InitializeComponent();
            points = new List<GraphPoint>();
            segments = new List<Segment>();
        }

        #region event handlers
        /// <summary>
        /// Set default values
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            textBoxX.Focus();
            textBoxX.SelectionStart = 0;
            textBoxX.SelectionLength = textBoxX.Text.Length;
            this.ActiveControl = textBoxX;
            this.AcceptButton = button1;

            panel1.MouseWheel += panel1_MouseWheel;

            bgColorPanel.BackColor = _bgColor;
            fgColorPanel.BackColor = _fgColor;
        }
        /// <summary>
        /// Add the point and do the calculations to draw it
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            double x = Convert.ToDouble(textBoxX.Text);
            double y = Convert.ToDouble(textBoxY.Text);

            textBoxX.Text = "";
            textBoxY.Text = "";

            textBoxX.Focus();
            textBoxX.SelectionStart = 0;
            textBoxX.SelectionLength = textBoxX.Text.Length;

            AddPoint(new GraphPoint(textBoxName.Text, x, y));

            textBoxName.Text = nextName.ToString();

            PerformCalculations();
        }
        /// <summary>
        /// Give a random set of values for x & y
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            textBoxX.Text = r.Next(Math.Min(minX, 0), Math.Max(maxX, panel1.Width)).ToString();
            textBoxY.Text = r.Next(Math.Min(minY, 0), Math.Max(maxY, panel1.Height)).ToString();
        }
        /// <summary>
        /// Validate user input to ensure it's numeric
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox_TextChanged(object sender, EventArgs e)
        {
            if (!Util.IsNumeric(((TextBox)sender).Text))
            {
                ((TextBox)sender).Text = "0";
            }
        }
        /// <summary>
        /// Give color dialog for graph's background color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void bgColorPanel_MouseUp(object sender, MouseEventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ((Panel)sender).BackColor = bgColor = dialog.Color;
                }
            }
        }
        /// <summary>
        /// Give color dialog for the foreground color
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void fgColorPanel_MouseUp(object sender, MouseEventArgs e)
        {
            using (ColorDialog dialog = new ColorDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ((Panel)sender).BackColor = fgColor = dialog.Color;
                }
            }
        }
        /// <summary>
        /// Iterate through the points and segments to draw everything.  Can be called with panel1.Refresh()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_Paint(object sender, PaintEventArgs e)
        {
            Panel panel = (Panel)sender;
            panel.BackColor = bgColor;
            List<Segment> DrawnSegments = new List<Segment>();
            using (Graphics g = e.Graphics)
            {
                using (Brush myBrush = new SolidBrush(fgColor))
                {
                    using (Pen myPen = new Pen(myBrush))
                    {
                        foreach (GraphPoint gp in points)
                        {
                            Rectangle rect = gp.Rectangle;

                            if (gp.Equals(listBox1.SelectedItem))//Double the circle size
                                rect = new Rectangle(rect.X - _pointRadius / 2, rect.Y - _pointRadius / 2, 
                                    rect.Width + _pointRadius, rect.Height + _pointRadius);

                            g.FillEllipse(myBrush, rect);
                        }
                        foreach (Segment s in segments.Where(x => x.IsFinal))
                        {
                            if (DrawnSegments.Contains(s)) continue;
                            g.DrawLine(myPen, s.A.Point, s.B.Point);
                            DrawnSegments.Add(s);
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Set values for zoom transformation and recalculate graphics
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseWheel(object sender, MouseEventArgs e)
        {
            zoomFactor = Math.Max(zoomFactor + e.Delta / 30, -5);
            zoomX = e.X;
            zoomY = e.Y;

            MakePointGraphics();
        }
        /// <summary>
        /// Need to make the panel the focus so mousewheel event is called
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseEnter(object sender, EventArgs e)
        {
            lastFocused = ActiveControl;
            if(!panel1.Focused)
                panel1.Focus();
        }
        /// <summary>
        /// Remove focus
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            lastFocused.Focus();
        }
        /// <summary>
        /// Start dragging transformation value calculation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            dragX = 0;
            dragY = 0;
            dragStartX = e.X;
            dragStartY = e.Y;
            isDragging = true;
        }
        /// <summary>
        /// End dragging transformation value calculation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isDragging = false;
        }
        /// <summary>
        /// If dragging, continue dragging transformation value calculation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                dragX = e.X - dragStartX;
                dragY = e.Y - dragStartY;
                MakePointGraphics();
                panel1.Refresh();
            }
        }
        /// <summary>
        /// Figure out which point was clicked on and highlight it in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void panel1_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (GraphPoint p in points)
            {
                if (p.IsWithinBounds(e.X, e.Y))
                {
                    listBox1.SelectedItem = p;
                    break;
                }
            }
        }
        /// <summary>
        /// Refresh panel so point is enlarged
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            panel1.Refresh();
        }
        /// <summary>
        /// If delete button pressed remove the item and recalculate
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_KeyUp(object sender, KeyEventArgs e)
        {
            ListBox lb = (ListBox)sender;
            if (lb.SelectedItem != null && e.KeyData == Keys.Delete)
            {
                points.Remove((GraphPoint)lb.SelectedItem);
                lb.Items.Remove(lb.SelectedItem);

                PerformCalculations();
            }
        }
        /// <summary>
        /// Wipe out everything
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void newToolStripMenuItem_Click(object sender, EventArgs e)
        {
            fileName = String.Empty;
            points.Clear();
            segments.Clear();
            listBox1.Items.Clear();
            PerformCalculations();
        }
        /// <summary>
        /// Save current points and colors to file, overwrite with current file name if not blank.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile();
        }
        /// <summary>
        /// Save to a new file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFile(true);
        }
        /// <summary>
        /// Open a .mt file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFile();
        }
        /// <summary>
        /// Shut it down
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        /// <summary>
        /// Allow user to scale the chart so large points will fit in the window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //clear zoom & drag so shifting makes sense
            zoomFactor = zoomX = zoomY = dragX = dragY = 0;
            MakePointGraphics();
        }
        #endregion

        #region reusable methods
        /// <summary>
        /// Add a point to the set
        /// </summary>
        /// <param name="point"></param>
        private void AddPoint(GraphPoint point)
        {
            //increment the name if it's numeric
            if (Util.IsNumeric(point.Name)) nextName = Convert.ToInt32(Math.Floor(Convert.ToDouble(point.Name) + 1.0));

            foreach (GraphPoint p in points)
            {
                if (point.Equals(p))
                {
                    return;
                }
            }

            points.Add(point);
            listBox1.Items.Add(point);
        }
        /// <summary>
        /// Create and calculate the final segments and calculate the actual transformed point coordinates for faster drawing
        /// </summary>
        private void PerformCalculations()
        {
            CalculateSegments();
            MakePointGraphics();
        }
        /// <summary>
        /// Create the segment list and the final segments
        /// </summary>
        private void CalculateSegments()
        {
            //disconnect everything
            foreach (GraphPoint p in points) p.Segments.Clear();
            segments.Clear();

            int totalFinalSegments = points.Count - 1;
            int finalSegments = 0;

            //create all segments
            points.ForEach(delegate(GraphPoint p)
            {
                Segment minSegment = null;
                GraphPoint minPoint = null;
                double minDistance = double.MaxValue;

                //iterate through the points to create segments
                points.Where(x => !x.Equals(p)).ToList().ForEach(delegate(GraphPoint p1)
                {
                    Segment s = new Segment(p, p1);
                    if (s.Distance < minDistance)
                    {
                        minPoint = p1;
                        minDistance = s.Distance;
                        minSegment = s;
                    }

                    if (!segments.Contains(minSegment)) segments.Add(minSegment);
                    else minSegment = segments.Single(x => x.Equals(minSegment));
                });

                //make sure the points exist and aren't already connected
                if (minPoint != null && !p.ConnectedTo(minPoint) && !minSegment.IsFinal)
                {
                    minSegment.MakeFinal();
                    finalSegments++;
                }
            });

            //only loop until we know we have the right number of segments
            if (finalSegments < totalFinalSegments)
            {
                //order all segments by distance ascending
                segments.Sort((seg1, seg2) => seg1.Distance.CompareTo(seg2.Distance));

                //keep looping until we've gotten all the segments we expect to get
                while (finalSegments < totalFinalSegments)
                {
                    //only get those that aren't connected
                    foreach (Segment s in segments.Where(x => !x.IsFinal && !x.IsRedundant))
                    {
                        //we have the right number so stop looping
                        if (finalSegments >= totalFinalSegments) 
                            break;

                        //call recursive function to see if the two points are already connected
                        if (!s.A.ConnectedTo(s.B))
                        {
                            s.MakeFinal();
                            finalSegments++;
                        }
                        else //these points are connected by other final segments so don't check them anymore
                            s.MakeRedundant();
                    }
                }
            }
        }
        /// <summary>
        /// Calculate the actual transformed pixels for drawing the points.
        /// </summary>
        private void MakePointGraphics()
        {
            bool needsXScaling = false, needsYScaling = false;

            if (checkBox1.Checked)
            {
                maxX = Convert.ToInt32(points.OrderByDescending(x => x.X).First().X);
                maxY = Convert.ToInt32(points.OrderByDescending(x => x.Y).First().Y);
                minX = Convert.ToInt32(points.OrderBy(x => x.X).First().X);
                minY = Convert.ToInt32(points.OrderBy(x => x.Y).First().Y);

                needsXScaling = maxX > maxWidth || minX < 0;
                needsYScaling = maxY > maxHeight || minY < 0;
            }

            foreach (GraphPoint gp in points)
            {
                int x = Convert.ToInt32(gp.X), y = Convert.ToInt32(gp.Y);
                int multiplier = 2;
                if (gp == listBox1.SelectedItem) multiplier = 4;

                //scale to fit
                if (needsXScaling) x = Util.Scale(minX, maxX, _padding, maxWidth, x);
                if (needsYScaling) y = Util.Scale(minY, maxY, _padding, maxHeight, y);

                //mousewheel zooming
                if (zoomFactor != 0)
                {
                    x -= (zoomX - x) * zoomFactor / 5;
                    y -= (zoomY - y) * zoomFactor / 5;
                }

                //dragging
                x += dragX;
                y -= dragY;

                Point p = new Point(x - (_pointRadius * multiplier / 2), panel1.Height - y - (_pointRadius * multiplier / 2));
                gp.Rectangle = new Rectangle(p, new Size(_pointRadius * multiplier, _pointRadius * multiplier));
                gp.Point = new Point(x, panel1.Height - y);
            }
            panel1.Refresh();
        }
        /// <summary>
        /// Get the filename from the user and write to a file
        /// </summary>
        /// <param name="b">If true, force new file name (save as)</param>
        private void SaveFile(bool b = false)
        {
            if (String.IsNullOrEmpty(fileName) || b)
                using (SaveFileDialog dialog = new SaveFileDialog())
                {
                    dialog.Filter = "Minified Tree (*.mt)|*.mt";
                    dialog.DefaultExt = "mt";
                    dialog.RestoreDirectory = true;

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        WriteFile((string)(fileName = dialog.FileName));
                    }
                }
            else
                WriteFile(fileName);
        }
        /// <summary>
        /// Get filename from user and open it
        /// </summary>
        private void OpenFile()
        {
            using (OpenFileDialog dialog = new OpenFileDialog())
            {
                dialog.Filter = "Minified Tree (*.mt)|*.mt";
                dialog.DefaultExt = "mt";
                dialog.RestoreDirectory = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    ExtractFile(dialog.FileName);
                }
            }
        }
        /// <summary>
        /// Write the file to the file system.
        /// </summary>
        /// <param name="fileName"></param>
        private void WriteFile(string fileName)
        {
            using (StreamWriter stream = new StreamWriter(fileName))
            {
                stream.WriteLine(String.Format("{1}{0}{2}{0}{3}", _fileDelimimter, bgColor.ToArgb(), fgColor.ToArgb(), checkBox1.Checked));
                foreach (GraphPoint p in points)
                {
                    stream.WriteLine(String.Format("{1}{0}{2}{0}{3}", _fileDelimimter, p.Name, p.X, p.Y));
                }
            }
        }
        /// <summary>
        /// Extract existing file into the application
        /// </summary>
        /// <param name="fileName"></param>
        private void ExtractFile(string fileName)
        {
            points.Clear();
            listBox1.Items.Clear();

            using (StreamReader stream = new StreamReader(fileName))
            {
                fileName = fileName;
                if (stream.Peek() != -1)
                {
                    string[] line = Regex.Split(stream.ReadLine(), _fileDelimimter);
                    bgColor = Color.FromArgb(Convert.ToInt32(line[0]));
                    fgColor = Color.FromArgb(Convert.ToInt32(line[1]));
                    if(line.Length > 2)
                        checkBox1.Checked = Convert.ToBoolean(line[2]);
                }
                while (stream.Peek() != -1)
                {
                    string[] line = Regex.Split(stream.ReadLine(), _fileDelimimter);
                    AddPoint(new GraphPoint(line[0], Convert.ToDouble(line[1]), Convert.ToDouble(line[2])));
                }
            }

            textBoxName.Text = nextName.ToString();
            PerformCalculations();
        }
        #endregion
    }
}
