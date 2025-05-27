using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SLARSolver
{
    /// <summary>
    /// Form for visualizing the results of solving systems of linear algebraic equations.
    /// Provides tabbed interface with tables of results, convergence charts, and computation time charts.
    /// </summary>
    public partial class ResultsVisualization : Form
    {
        private double[,] A; // Coefficient matrix
        private double[] b;  // Right-hand side vector
        private List<SolverMethods.IterationResult> iterations; // List of iteration results
        private string methodName; // Name of the solution method used

        /// <summary>
        /// Constructor for the ResultsVisualization form
        /// </summary>
        /// <param name="A">Coefficient matrix</param>
        /// <param name="b">Right-hand side vector</param>
        /// <param name="iterations">List of iteration results</param>
        /// <param name="methodName">Name of the solution method used</param>
        public ResultsVisualization(double[,] A, double[] b, List<SolverMethods.IterationResult> iterations, string methodName)
        {
            this.A = A;
            this.b = b;
            this.iterations = iterations;
            this.methodName = methodName;
            InitializeComponent();
        }

        /// <summary>
        /// Initializes form components and creates the tabbed interface
        /// </summary>
        private void InitializeComponent()
        {
            this.SuspendLayout();

            // Create controls
            TabControl tabControl = new TabControl();
            tabControl.Dock = DockStyle.Fill;

            // Tab for results table
            TabPage tabResults = new TabPage("Results Table");
            DataGridView dataGridView = CreateResultsGrid();
            dataGridView.Dock = DockStyle.Fill;
            tabResults.Controls.Add(dataGridView);

            // Tab for convergence chart
            TabPage tabConvergence = new TabPage("Convergence Chart");
            Chart chartConvergence = CreateConvergenceChart();
            chartConvergence.Dock = DockStyle.Fill;
            tabConvergence.Controls.Add(chartConvergence);

            // Tab for computation time chart
            TabPage tabTime = new TabPage("Computation Time");
            Chart chartTime = CreateTimeChart();
            chartTime.Dock = DockStyle.Fill;
            tabTime.Controls.Add(chartTime);

            // Tab for solution information
            TabPage tabSolution = new TabPage("Solution Information");
            Panel solutionPanel = CreateSolutionPanel();
            solutionPanel.Dock = DockStyle.Fill;
            tabSolution.Controls.Add(solutionPanel);

            // Add tabs
            tabControl.TabPages.Add(tabResults);
            tabControl.TabPages.Add(tabConvergence);
            tabControl.TabPages.Add(tabTime);
            tabControl.TabPages.Add(tabSolution);

            // Add button panel
            Panel buttonPanel = new Panel();
            buttonPanel.Dock = DockStyle.Bottom;
            buttonPanel.Height = 50;

            Button btnSave = new Button();
            btnSave.Text = "Save Results";
            btnSave.Width = 150;
            btnSave.Location = new Point(10, 10);
            btnSave.Click += BtnSave_Click;

            Button btnClose = new Button();
            btnClose.Text = "Close";
            btnClose.Width = 100;
            btnClose.Location = new Point(170, 10);
            btnClose.Click += BtnClose_Click;

            Button btnGraphical = new Button();
            btnGraphical.Text = "Graphical Solution";
            btnGraphical.Width = 150;
            btnGraphical.Location = new Point(280, 10);
            btnGraphical.Click += BtnGraphical_Click;
            // Enable button only for 2x2 systems
            btnGraphical.Enabled = (A.GetLength(0) == 2);

            buttonPanel.Controls.Add(btnSave);
            buttonPanel.Controls.Add(btnClose);
            buttonPanel.Controls.Add(btnGraphical);

            // Add elements to the form
            this.Controls.Add(tabControl);
            this.Controls.Add(buttonPanel);

            // Form settings
            this.Text = $"SLAR Solution Results - {methodName}";
            this.Size = new Size(800, 600);

            this.ResumeLayout(false);
        }

        /// <summary>
        /// Creates a data grid view for displaying iteration results
        /// </summary>
        /// <returns>Configured DataGridView control</returns>
        private DataGridView CreateResultsGrid()
        {
            DataGridView grid = new DataGridView();
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.ReadOnly = true;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersWidth = 50;
            grid.AlternatingRowsDefaultCellStyle.BackColor = Color.LightGray;

            // Add columns
            grid.Columns.Add("IterationNumber", "Iteration #");
            grid.Columns.Add("Error", "Error");
            grid.Columns.Add("Time", "Time (ms)");

            int n = A.GetLength(0);
            for (int i = 0; i < n; i++)
            {
                grid.Columns.Add($"X{i + 1}", $"x{i + 1}");
            }

            // Fill data
            foreach (var iteration in iterations)
            {
                int rowIndex = grid.Rows.Add();
                grid.Rows[rowIndex].Cells["IterationNumber"].Value = iteration.IterationNumber;
                grid.Rows[rowIndex].Cells["Error"].Value = iteration.Error.ToString("E6");
                grid.Rows[rowIndex].Cells["Time"].Value = iteration.ComputationTime.ToString("F2");

                for (int i = 0; i < n; i++)
                {
                    grid.Rows[rowIndex].Cells[$"X{i + 1}"].Value = iteration.Solution[i].ToString("F8");
                }
            }

            return grid;
        }

        /// <summary>
        /// Creates a chart for visualizing convergence (error vs. iteration)
        /// </summary>
        /// <returns>Configured Chart control</returns>
        private Chart CreateConvergenceChart()
        {
            Chart chart = new Chart();
            chart.ChartAreas.Add(new ChartArea("MainArea"));
            chart.ChartAreas["MainArea"].AxisX.Title = "Iteration";
            chart.ChartAreas["MainArea"].AxisY.Title = "Error";
            chart.ChartAreas["MainArea"].AxisY.IsLogarithmic = true;
            chart.ChartAreas["MainArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas["MainArea"].AxisY.MajorGrid.LineColor = Color.LightGray;

            Series series = new Series("Error");
            series.ChartType = SeriesChartType.Line;
            series.Color = Color.Blue;
            series.BorderWidth = 2;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 5;

            // Add data
            foreach (var iteration in iterations)
            {
                series.Points.AddXY(iteration.IterationNumber, iteration.Error);
            }

            chart.Series.Add(series);

            // Add legend
            chart.Legends.Add(new Legend("Legend"));
            series.LegendText = "Error";

            return chart;
        }

        /// <summary>
        /// Creates a chart for visualizing computation time
        /// </summary>
        /// <returns>Configured Chart control</returns>
        private Chart CreateTimeChart()
        {
            Chart chart = new Chart();
            chart.ChartAreas.Add(new ChartArea("MainArea"));
            chart.ChartAreas["MainArea"].AxisX.Title = "Iteration";
            chart.ChartAreas["MainArea"].AxisY.Title = "Time (ms)";
            chart.ChartAreas["MainArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            chart.ChartAreas["MainArea"].AxisY.MajorGrid.LineColor = Color.LightGray;

            Series series = new Series("Time");
            series.ChartType = SeriesChartType.Line;
            series.Color = Color.Green;
            series.BorderWidth = 2;
            series.MarkerStyle = MarkerStyle.Circle;
            series.MarkerSize = 5;

            // Add data
            foreach (var iteration in iterations)
            {
                series.Points.AddXY(iteration.IterationNumber, iteration.ComputationTime);
            }

            chart.Series.Add(series);

            // Add legend
            chart.Legends.Add(new Legend("Legend"));
            series.LegendText = "Computation Time";

            return chart;
        }

        /// <summary>
        /// Creates a panel with detailed solution information
        /// </summary>
        /// <returns>Configured Panel control</returns>
        private Panel CreateSolutionPanel()
        {
            Panel panel = new Panel();

            // Get final solution
            var finalSolution = iterations[iterations.Count - 1].Solution;
            double[] residual = SolverMethods.CalculateResidual(A, finalSolution, b);
            double residualNorm = SolverMethods.CalculateNorm(residual);

            // Create information elements
            Label lblMethod = new Label();
            lblMethod.Text = $"Solution Method: {methodName}";
            lblMethod.AutoSize = true;
            lblMethod.Location = new Point(20, 20);
            lblMethod.Font = new Font(lblMethod.Font.FontFamily, 12);

            Label lblIterations = new Label();
            lblIterations.Text = $"Number of Iterations: {iterations.Count}";
            lblIterations.AutoSize = true;
            lblIterations.Location = new Point(20, 50);
            lblIterations.Font = new Font(lblIterations.Font.FontFamily, 12);

            Label lblTime = new Label();
            lblTime.Text = $"Total Computation Time: {iterations[iterations.Count - 1].ComputationTime:F2} ms";
            lblTime.AutoSize = true;
            lblTime.Location = new Point(20, 80);
            lblTime.Font = new Font(lblTime.Font.FontFamily, 12);

            Label lblResidual = new Label();
            lblResidual.Text = $"Residual Norm: {residualNorm:E10}";
            lblResidual.AutoSize = true;
            lblResidual.Location = new Point(20, 110);
            lblResidual.Font = new Font(lblResidual.Font.FontFamily, 12);

            Label lblSolution = new Label();
            lblSolution.Text = "Final Solution:";
            lblSolution.AutoSize = true;
            lblSolution.Location = new Point(20, 140);
            lblSolution.Font = new Font(lblSolution.Font.FontFamily, 12);

            // Create list for displaying solution
            ListBox listSolution = new ListBox();
            listSolution.Location = new Point(20, 170);
            listSolution.Size = new Size(300, 150);
            listSolution.Font = new Font(lblSolution.Font.FontFamily, 10);

            for (int i = 0; i < finalSolution.Length; i++)
            {
                listSolution.Items.Add($"x{i + 1} = {finalSolution[i]:F10}");
            }

            // Create list for displaying residual
            Label lblResidualVector = new Label();
            lblResidualVector.Text = "Residual Vector:";
            lblResidualVector.AutoSize = true;
            lblResidualVector.Location = new Point(350, 140);
            lblResidualVector.Font = new Font(lblResidualVector.Font.FontFamily, 12);

            ListBox listResidual = new ListBox();
            listResidual.Location = new Point(350, 170);
            listResidual.Size = new Size(300, 150);
            listResidual.Font = new Font(listResidual.Font.FontFamily, 10);

            for (int i = 0; i < residual.Length; i++)
            {
                listResidual.Items.Add($"r{i + 1} = {residual[i]:E10}");
            }

            // Add elements to panel
            panel.Controls.Add(lblMethod);
            panel.Controls.Add(lblIterations);
            panel.Controls.Add(lblTime);
            panel.Controls.Add(lblResidual);
            panel.Controls.Add(lblSolution);
            panel.Controls.Add(listSolution);
            panel.Controls.Add(lblResidualVector);
            panel.Controls.Add(listResidual);

            return panel;
        }

        /// <summary>
        /// Event handler for Save button click
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnSave_Click(object? sender, EventArgs e)
        {
            // Save results to file
            FileManager.SaveResultsToFile(A, b, iterations, methodName);
        }

        /// <summary>
        /// Event handler for Close button click
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnClose_Click(object? sender, EventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// Event handler for Graphical Solution button click
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnGraphical_Click(object? sender, EventArgs e)
        {
            // Open graphical visualization form (only for 2x2 systems)
            if (A.GetLength(0) == 2)
            {
                var finalSolution = iterations[iterations.Count - 1].Solution;
                GraphicalSolution graphicalForm = new GraphicalSolution(A, b, finalSolution);
                graphicalForm.Show();
            }
        }
    }
}