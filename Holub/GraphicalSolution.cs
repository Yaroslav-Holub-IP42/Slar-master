using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SLARSolver
{
    /// <summary>
    /// Form for visualizing graphical solution of 2x2 systems of linear equations.
    /// Displays the lines representing each equation and their intersection point (solution).
    /// </summary>
    public partial class GraphicalSolution : Form
    {
        private double[,] A; // Coefficient matrix
        private double[] b;  // Right-hand side vector
        private double[] solution; // Solution vector

        /// <summary>
        /// Constructor for the GraphicalSolution form
        /// </summary>
        /// <param name="A">Coefficient matrix (must be 2x2)</param>
        /// <param name="b">Right-hand side vector (must be length 2)</param>
        /// <param name="solution">Solution vector (must be length 2)</param>
        public GraphicalSolution(double[,] A, double[] b, double[] solution)
        {
            InitializeComponent();
            this.A = A;
            this.b = b;
            this.solution = solution;
        }

        /// <summary>
        /// Initializes the form components and sets up the chart
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Create chart
            Chart chart = new Chart();
            chart.Dock = DockStyle.Fill;
            chart.ChartAreas.Add(new ChartArea("MainArea"));
            chart.ChartAreas["MainArea"].AxisX.Title = "X";
            chart.ChartAreas["MainArea"].AxisY.Title = "Y";
            chart.ChartAreas["MainArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas["MainArea"].AxisY.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas["MainArea"].AxisX.Interval = 1;
            chart.ChartAreas["MainArea"].AxisY.Interval = 1;

            // Add series for equation lines and intersection point
            chart.Series.Add(new Series("Equation1"));
            chart.Series["Equation1"].ChartType = SeriesChartType.Line;
            chart.Series["Equation1"].Color = Color.Blue;

            chart.Series.Add(new Series("Equation2"));
            chart.Series["Equation2"].ChartType = SeriesChartType.Line;
            chart.Series["Equation2"].Color = Color.Red;

            chart.Series.Add(new Series("Solution"));
            chart.Series["Solution"].ChartType = SeriesChartType.Point;
            chart.Series["Solution"].Color = Color.Green;
            chart.Series["Solution"].MarkerStyle = MarkerStyle.Circle;
            chart.Series["Solution"].MarkerSize = 10;

            this.Controls.Add(chart);

            // Form settings
            this.Text = "Graphical Solution of 2x2 SLAR";
            this.Size = new Size(800, 600);
            this.Load += GraphicalSolution_Load;

            this.ResumeLayout(false);
        }

        /// <summary>
        /// Event handler for form load - calls the plotting method
        /// </summary>
        private void GraphicalSolution_Load(object? sender, EventArgs e)
        {
            Plot();
        }

        /// <summary>
        /// Creates the graphical representation of the system and its solution
        /// </summary>
        private void Plot()
        {
            // Check that this is a 2x2 system
            if (A.GetLength(0) != 2 || A.GetLength(1) != 2 || b.Length != 2)
            {
                MessageBox.Show("Graphical visualization is only available for 2x2 systems.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            Chart chart = (Chart)this.Controls[0];

            // Set the chart range based on the solution
            double minX = Math.Min(solution[0] - 5, -5);
            double maxX = Math.Max(solution[0] + 5, 5);

            // Set axis limits
            chart.ChartAreas["MainArea"].AxisX.Minimum = minX;
            chart.ChartAreas["MainArea"].AxisX.Maximum = maxX;
            chart.ChartAreas["MainArea"].AxisY.Minimum = Math.Min(solution[1] - 5, -5);
            chart.ChartAreas["MainArea"].AxisY.Maximum = Math.Max(solution[1] + 5, 5);

            // Plot the lines for each equation in the form y = mx + c
            PlotLine(chart.Series["Equation1"], A[0, 0], A[0, 1], b[0], minX, maxX);
            PlotLine(chart.Series["Equation2"], A[1, 0], A[1, 1], b[1], minX, maxX);

            // Add the solution point
            chart.Series["Solution"].Points.AddXY(solution[0], solution[1]);

            // Add legend
            chart.Legends.Add(new Legend("Legend"));
            chart.Series["Equation1"].LegendText = $"{A[0, 0]:F2}x + {A[0, 1]:F2}y = {b[0]:F2}";
            chart.Series["Equation2"].LegendText = $"{A[1, 0]:F2}x + {A[1, 1]:F2}y = {b[1]:F2}";
            chart.Series["Solution"].LegendText = $"Solution: ({solution[0]:F4}, {solution[1]:F4})";

            // Add coordinate axis lines
            AddAxisLines(chart);
        }

        /// <summary>
        /// Plots a line representing an equation in the form ax + by = c
        /// </summary>
        /// <param name="series">Chart series to plot the line on</param>
        /// <param name="a">Coefficient of x</param>
        /// <param name="b">Coefficient of y</param>
        /// <param name="c">Right-hand side constant</param>
        /// <param name="minX">Minimum X value for the chart</param>
        /// <param name="maxX">Maximum X value for the chart</param>
        private void PlotLine(Series series, double a, double b, double c, double minX, double maxX)
        {
            series.Points.Clear();

            Chart chart = (Chart)this.Controls[0];

            if (b == 0)
            {
                // Vertical line (x = const)
                double x = c / a;
                series.Points.AddXY(x, chart.ChartAreas["MainArea"].AxisY.Minimum);
                series.Points.AddXY(x, chart.ChartAreas["MainArea"].AxisY.Maximum);
            }
            else
            {
                // Normal line y = (-a/b)x + (c/b)
                double m = -a / b;  // Slope
                double intercept = c / b;  // Y-intercept

                // Add points for the boundary X values
                double y1 = m * minX + intercept;
                double y2 = m * maxX + intercept;

                series.Points.AddXY(minX, y1);
                series.Points.AddXY(maxX, y2);
            }
        }

        /// <summary>
        /// Adds coordinate axis lines to the chart
        /// </summary>
        /// <param name="chart">Chart to add the axis lines to</param>
        private void AddAxisLines(Chart chart)
        {
            Series xAxis = new Series("X-Axis");
            Series yAxis = new Series("Y-Axis");

            xAxis.ChartType = SeriesChartType.Line;
            yAxis.ChartType = SeriesChartType.Line;

            xAxis.Color = Color.Black;
            yAxis.Color = Color.Black;

            xAxis.BorderWidth = 1;
            yAxis.BorderWidth = 1;

            // Points for X-axis
            xAxis.Points.AddXY(chart.ChartAreas["MainArea"].AxisX.Minimum, 0);
            xAxis.Points.AddXY(chart.ChartAreas["MainArea"].AxisX.Maximum, 0);

            // Points for Y-axis
            yAxis.Points.AddXY(0, chart.ChartAreas["MainArea"].AxisY.Minimum);
            yAxis.Points.AddXY(0, chart.ChartAreas["MainArea"].AxisY.Maximum);

            // Hide series from legend
            xAxis.IsVisibleInLegend = false;
            yAxis.IsVisibleInLegend = false;

            // Add series to chart
            chart.Series.Add(xAxis);
            chart.Series.Add(yAxis);
        }
    }
}