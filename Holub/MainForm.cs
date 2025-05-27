using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace SLARSolver
{
    /// <summary>
    /// Main form of the application for solving systems of linear algebraic equations.
    /// </summary>
    public partial class MainForm : Form
    {
        private double[,] A; // Coefficient matrix
        private double[] b; // Right-hand side vector
        private int size; // System dimension
        private List<SolverMethods.IterationResult> lastIterations; // Results of the last solution
        private string currentMethod; // Current solution method

        // Chart controls as class fields
        private Chart chartConvergence = null!;
        private Chart chartTime = null!;
        private Chart chartCombined = null!;

        /// <summary>
        /// Constructor for the main form
        /// </summary>
        public MainForm()
        {
            InitializeComponent();
            CreateControls(); // Call method to create UI controls

            // Initialize non-nullable fields
            A = new double[0, 0];
            b = new double[0];
            lastIterations = new List<SolverMethods.IterationResult>();
            currentMethod = string.Empty;
        }

        /// <summary>
        /// Initializes form components
        /// </summary>
        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MainForm
            // 
            ClientSize = new Size(949, 729);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "System of Linear Algebraic Equations - Solver";
            ResumeLayout(false);
        }

        /// <summary>
        /// Creates and initializes all UI controls for the form
        /// </summary>
        private void CreateControls()
        {
            // Group box for system dimension
            GroupBox gbDimension = new GroupBox();
            gbDimension.Text = "System Dimension";
            gbDimension.Location = new Point(10, 10);
            gbDimension.Size = new Size(200, 60);

            Label lblSize = new Label();
            lblSize.Text = "Dimension (n):";
            lblSize.Location = new Point(10, 25);
            lblSize.AutoSize = true;

            NumericUpDown numSize = new NumericUpDown();
            numSize.Minimum = 2;
            numSize.Maximum = 10;
            numSize.Value = 3;
            numSize.Location = new Point(120, 23);
            numSize.Size = new Size(60, 20);
            numSize.ValueChanged += NumSize_ValueChanged;

            gbDimension.Controls.Add(lblSize);
            gbDimension.Controls.Add(numSize);

            // Create matrix button
            Button btnCreateMatrix = new Button();
            btnCreateMatrix.Text = "Create Matrix";
            btnCreateMatrix.Location = new Point(220, 27);
            btnCreateMatrix.Size = new Size(120, 30);
            btnCreateMatrix.Click += BtnCreateMatrix_Click;

            // Group box for solution method selection
            GroupBox gbMethod = new GroupBox();
            gbMethod.Text = "Solution Method";
            gbMethod.Location = new Point(350, 10);
            gbMethod.Size = new Size(500, 60);

            RadioButton rbJacobi = new RadioButton();
            rbJacobi.Text = "Jacobi Method";
            rbJacobi.Location = new Point(10, 25);
            rbJacobi.AutoSize = true;
            rbJacobi.Checked = true;

            RadioButton rbGaussSeidel = new RadioButton();
            rbGaussSeidel.Text = "Gauss-Seidel Method";
            rbGaussSeidel.Location = new Point(130, 25);
            rbGaussSeidel.AutoSize = true;

            RadioButton rbGradient = new RadioButton();
            rbGradient.Text = "Steepest Descent Method";
            rbGradient.Location = new Point(280, 25);
            rbGradient.AutoSize = true;

            gbMethod.Controls.Add(rbJacobi);
            gbMethod.Controls.Add(rbGaussSeidel);
            gbMethod.Controls.Add(rbGradient);

            // Panel for coefficient matrix and right-hand side vector
            Panel pnlMatrix = new Panel();
            pnlMatrix.Location = new Point(10, 80);
            pnlMatrix.Size = new Size(770, 300);
            pnlMatrix.AutoScroll = true;
            pnlMatrix.BorderStyle = BorderStyle.FixedSingle;
            pnlMatrix.Name = "pnlMatrix";

            // Buttons for operations
            Button btnSolve = new Button();
            btnSolve.Text = "Solve";
            btnSolve.Location = new Point(10, 390);
            btnSolve.Size = new Size(100, 30);
            btnSolve.Click += BtnSolve_Click;

            Button btnGraphical = new Button();
            btnGraphical.Text = "Graphical Solution";
            btnGraphical.Location = new Point(120, 390);
            btnGraphical.Size = new Size(140, 30);
            btnGraphical.Enabled = false; // Initially inactive
            btnGraphical.Click += BtnGraphical_Click;
            btnGraphical.Name = "btnGraphical";

            Button btnSaveResult = new Button();
            btnSaveResult.Text = "Save Result";
            btnSaveResult.Location = new Point(270, 390);
            btnSaveResult.Size = new Size(130, 30);
            btnSaveResult.Enabled = false; // Initially inactive
            btnSaveResult.Click += BtnSaveResult_Click;
            btnSaveResult.Name = "btnSaveResult";

            Button btnLoadFromFile = new Button();
            btnLoadFromFile.Text = "Load From File";
            btnLoadFromFile.Location = new Point(410, 390);
            btnLoadFromFile.Size = new Size(130, 30);
            btnLoadFromFile.Click += BtnLoadFromFile_Click;

            Button btnRandom = new Button();
            btnRandom.Text = "Random Values";
            btnRandom.Location = new Point(550, 390);
            btnRandom.Size = new Size(130, 30);
            btnRandom.Click += BtnRandom_Click;

            // Tab control for displaying results
            TabControl tabResults = new TabControl();
            tabResults.Location = new Point(10, 430);
            tabResults.Size = new Size(770, 300);  // Increased height from 120 to 300
            tabResults.Name = "tabResults";
            tabResults.Dock = DockStyle.Bottom;   // Anchor to bottom of the form
            tabResults.Height = 300;              // Fixed height

            // Tab for displaying solution
            TabPage tabSolution = new TabPage("Solution");
            TextBox txtSolution = new TextBox();
            txtSolution.Multiline = true;
            txtSolution.ScrollBars = ScrollBars.Vertical;
            txtSolution.Dock = DockStyle.Fill;
            txtSolution.ReadOnly = true;
            txtSolution.Name = "txtSolution";
            tabSolution.Controls.Add(txtSolution);

            // Tab for convergence chart
            TabPage tabConvergence = new TabPage("Convergence");
            chartConvergence = new Chart();
            chartConvergence.Dock = DockStyle.Fill;
            chartConvergence.ChartAreas.Add(new ChartArea("MainArea"));
            chartConvergence.ChartAreas["MainArea"].AxisX.Title = "Iteration";
            chartConvergence.ChartAreas["MainArea"].AxisY.Title = "Error";
            chartConvergence.ChartAreas["MainArea"].AxisY.IsLogarithmic = true;
            chartConvergence.ChartAreas["MainArea"].AxisY.LogarithmBase = 10;
            chartConvergence.ChartAreas["MainArea"].AxisX.LabelStyle.Format = "N0";
            chartConvergence.ChartAreas["MainArea"].AxisY.LabelStyle.Format = "E2";

            // Grid settings for better readability
            chartConvergence.ChartAreas["MainArea"].AxisX.MajorGrid.Enabled = true;
            chartConvergence.ChartAreas["MainArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartConvergence.ChartAreas["MainArea"].AxisY.MajorGrid.Enabled = true;
            chartConvergence.ChartAreas["MainArea"].AxisY.MajorGrid.LineColor = Color.LightGray;

            // Chart enhancement
            chartConvergence.Series.Add(new Series("ErrorSeries"));
            chartConvergence.Series["ErrorSeries"].ChartType = SeriesChartType.Line;
            chartConvergence.Series["ErrorSeries"].Color = Color.Blue;
            chartConvergence.Series["ErrorSeries"].BorderWidth = 2;
            chartConvergence.Series["ErrorSeries"].MarkerStyle = MarkerStyle.Circle;
            chartConvergence.Series["ErrorSeries"].MarkerSize = 6;
            chartConvergence.Series["ErrorSeries"].MarkerColor = Color.Red;
            chartConvergence.Name = "chartConvergence";

            // Add legend
            Legend legend1 = new Legend("Legend1");
            legend1.Title = "Error";
            legend1.Docking = Docking.Top;
            chartConvergence.Legends.Add(legend1);
            chartConvergence.Series["ErrorSeries"].Legend = "Legend1";
            chartConvergence.Series["ErrorSeries"].LegendText = "Error per iteration";

            tabConvergence.Controls.Add(chartConvergence);

            // Tab for execution time
            TabPage tabTime = new TabPage("Computation Time");
            chartTime = new Chart();
            chartTime.Dock = DockStyle.Fill;
            chartTime.ChartAreas.Add(new ChartArea("MainArea"));
            chartTime.ChartAreas["MainArea"].AxisX.Title = "Iteration";
            chartTime.ChartAreas["MainArea"].AxisY.Title = "Time (ms)";
            chartTime.ChartAreas["MainArea"].AxisX.LabelStyle.Format = "N0";
            chartTime.ChartAreas["MainArea"].AxisY.LabelStyle.Format = "N2";

            // Grid settings for better readability
            chartTime.ChartAreas["MainArea"].AxisX.MajorGrid.Enabled = true;
            chartTime.ChartAreas["MainArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartTime.ChartAreas["MainArea"].AxisY.MajorGrid.Enabled = true;
            chartTime.ChartAreas["MainArea"].AxisY.MajorGrid.LineColor = Color.LightGray;

            // Chart enhancement
            chartTime.Series.Add(new Series("TimeSeries"));
            chartTime.Series["TimeSeries"].ChartType = SeriesChartType.Line;
            chartTime.Series["TimeSeries"].Color = Color.Green;
            chartTime.Series["TimeSeries"].BorderWidth = 2;
            chartTime.Series["TimeSeries"].MarkerStyle = MarkerStyle.Diamond;
            chartTime.Series["TimeSeries"].MarkerSize = 6;
            chartTime.Series["TimeSeries"].MarkerColor = Color.DarkGreen;
            chartTime.Name = "chartTime";

            // Add legend
            Legend legend2 = new Legend("Legend2");
            legend2.Title = "Time";
            legend2.Docking = Docking.Top;
            chartTime.Legends.Add(legend2);
            chartTime.Series["TimeSeries"].Legend = "Legend2";
            chartTime.Series["TimeSeries"].LegendText = "Computation time per iteration";

            tabTime.Controls.Add(chartTime);

            // Tab for combined chart
            TabPage tabCombined = new TabPage("Combined");
            chartCombined = new Chart();
            chartCombined.Dock = DockStyle.Fill;
            chartCombined.ChartAreas.Add(new ChartArea("MainArea"));
            chartCombined.ChartAreas["MainArea"].AxisX.Title = "Iteration";
            chartCombined.ChartAreas["MainArea"].AxisY.Title = "Error";
            chartCombined.ChartAreas["MainArea"].AxisY.IsLogarithmic = true;
            chartCombined.ChartAreas["MainArea"].AxisY.LogarithmBase = 10;
            chartCombined.ChartAreas["MainArea"].AxisX.LabelStyle.Format = "N0";
            chartCombined.ChartAreas["MainArea"].AxisY.LabelStyle.Format = "E2";

            // Grid settings
            chartCombined.ChartAreas["MainArea"].AxisX.MajorGrid.Enabled = true;
            chartCombined.ChartAreas["MainArea"].AxisX.MajorGrid.LineColor = Color.LightGray;
            chartCombined.ChartAreas["MainArea"].AxisY.MajorGrid.Enabled = true;
            chartCombined.ChartAreas["MainArea"].AxisY.MajorGrid.LineColor = Color.LightGray;

            // Add second Y-axis for time
            chartCombined.ChartAreas["MainArea"].AxisY2.Enabled = AxisEnabled.True;
            chartCombined.ChartAreas["MainArea"].AxisY2.Title = "Time (ms)";
            chartCombined.ChartAreas["MainArea"].AxisY2.LabelStyle.Format = "N2";

            // First series for errors
            chartCombined.Series.Add(new Series("ErrorCombinedSeries"));
            chartCombined.Series["ErrorCombinedSeries"].ChartType = SeriesChartType.Line;
            chartCombined.Series["ErrorCombinedSeries"].Color = Color.Blue;
            chartCombined.Series["ErrorCombinedSeries"].BorderWidth = 2;
            chartCombined.Series["ErrorCombinedSeries"].MarkerStyle = MarkerStyle.Circle;
            chartCombined.Series["ErrorCombinedSeries"].MarkerSize = 6;
            chartCombined.Series["ErrorCombinedSeries"].MarkerColor = Color.Red;

            // Second series for time
            chartCombined.Series.Add(new Series("TimeCombinedSeries"));
            chartCombined.Series["TimeCombinedSeries"].ChartType = SeriesChartType.Line;
            chartCombined.Series["TimeCombinedSeries"].Color = Color.Green;
            chartCombined.Series["TimeCombinedSeries"].BorderWidth = 2;
            chartCombined.Series["TimeCombinedSeries"].MarkerStyle = MarkerStyle.Diamond;
            chartCombined.Series["TimeCombinedSeries"].MarkerSize = 6;
            chartCombined.Series["TimeCombinedSeries"].MarkerColor = Color.DarkGreen;
            chartCombined.Series["TimeCombinedSeries"].YAxisType = AxisType.Secondary;
            chartCombined.Name = "chartCombined";

            // Add legend
            Legend legend3 = new Legend("Legend3");
            legend3.Docking = Docking.Top;
            chartCombined.Legends.Add(legend3);
            chartCombined.Series["ErrorCombinedSeries"].Legend = "Legend3";
            chartCombined.Series["ErrorCombinedSeries"].LegendText = "Error";
            chartCombined.Series["TimeCombinedSeries"].Legend = "Legend3";
            chartCombined.Series["TimeCombinedSeries"].LegendText = "Time (ms)";

            tabCombined.Controls.Add(chartCombined);

            // Add all tabs to TabControl
            tabResults.TabPages.Add(tabSolution);
            tabResults.TabPages.Add(tabConvergence);
            tabResults.TabPages.Add(tabTime);
            tabResults.TabPages.Add(tabCombined); // Add the combined tab

            // Add all elements to the form
            this.Controls.Add(gbDimension);
            this.Controls.Add(btnCreateMatrix);
            this.Controls.Add(gbMethod);
            this.Controls.Add(pnlMatrix);
            this.Controls.Add(btnSolve);
            this.Controls.Add(btnGraphical);
            this.Controls.Add(btnSaveResult);
            this.Controls.Add(btnLoadFromFile);
            this.Controls.Add(btnRandom);
            this.Controls.Add(tabResults);

            // Set initial value for dimension
            size = (int)numSize.Value;
        }

        /// <summary>
        /// Event handler for loading SLAR from file
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnLoadFromFile_Click(object? sender, EventArgs e)
        {
            if (FileManager.LoadSLARFromFile(out A, out b))
            {
                // Update dimension and display
                size = b.Length;
                NumericUpDown? numSize = null;

                // Find the dimension control
                foreach (Control ctrl in this.Controls)
                {
                    if (ctrl is GroupBox gb && gb.Text == "System Dimension")
                    {
                        foreach (Control gbCtrl in gb.Controls)
                        {
                            if (gbCtrl is NumericUpDown nud)
                            {
                                numSize = nud;
                                break;
                            }
                        }
                        break;
                    }
                }

                if (numSize != null)
                {
                    numSize.Value = size;
                    BtnCreateMatrix_Click(this, EventArgs.Empty); // Create fields

                    // Fill textboxes with values
                    Panel pnlMatrix = (Panel)this.Controls.Find("pnlMatrix", true)[0];
                    foreach (Control ctrl in pnlMatrix.Controls)
                    {
                        if (ctrl is TextBox txtBox && txtBox.Tag != null)
                        {
                            string tag = txtBox.Tag.ToString()!;

                            if (tag.StartsWith("A_"))
                            {
                                string[] indices = tag.Substring(2).Split('_');
                                int i = int.Parse(indices[0]);
                                int j = int.Parse(indices[1]);
                                txtBox.Text = A[i, j].ToString();
                            }
                            else if (tag.StartsWith("b_"))
                            {
                                int i = int.Parse(tag.Substring(2));
                                txtBox.Text = b[i].ToString();
                            }
                        }
                    }

                    MessageBox.Show("SLAR successfully loaded from file.", "Information",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        /// <summary>
        /// Event handler for filling the matrix with random values
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnRandom_Click(object? sender, EventArgs e)
        {
            Panel pnlMatrix = (Panel)this.Controls.Find("pnlMatrix", true)[0];
            Random rnd = new Random();

            // Generate random values with varying decimal places
            foreach (Control ctrl in pnlMatrix.Controls)
            {
                if (ctrl is TextBox txtBox && txtBox.Tag != null)
                {
                    string tag = txtBox.Tag.ToString()!;

                    if (tag.StartsWith("A_"))
                    {
                        // For diagonal elements - larger values to ensure diagonal dominance
                        string[] indices = tag.Substring(2).Split('_');
                        int i = int.Parse(indices[0]);
                        int j = int.Parse(indices[1]);

                        if (i == j)
                        {
                            // Diagonal elements - generate value between 50 and 100
                            double value = 50 + rnd.NextDouble() * 50;
                            // Format with random number of decimal places between 0 and 6
                            int decimalPlaces = rnd.Next(7);
                            txtBox.Text = value.ToString($"F{decimalPlaces}");
                        }
                        else
                        {
                            // Non-diagonal elements - generate value between -10 and 10
                            double value = -10 + rnd.NextDouble() * 20;
                            // Format with random number of decimal places between 0 and 6
                            int decimalPlaces = rnd.Next(7);
                            txtBox.Text = value.ToString($"F{decimalPlaces}");
                        }
                    }
                    else if (tag.StartsWith("b_"))
                    {
                        // Right-hand side elements - generate value between -50 and 50
                        double value = -50 + rnd.NextDouble() * 100;
                        // Format with random number of decimal places between 0 and 6
                        int decimalPlaces = rnd.Next(7);
                        txtBox.Text = value.ToString($"F{decimalPlaces}");
                    }
                }
            }
        }

        /// <summary>
        /// Event handler for creating the matrix UI
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnCreateMatrix_Click(object? sender, EventArgs e)
        {
            Panel pnlMatrix = (Panel)this.Controls.Find("pnlMatrix", true)[0];
            pnlMatrix.Controls.Clear();

            int cellSize = 60;
            int margin = 5;
            int labelWidth = 25;

            // Create textboxes for matrix A
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    TextBox txt = new TextBox();
                    txt.Size = new Size(cellSize, 25);
                    txt.Location = new Point(j * (cellSize + margin) + labelWidth, i * (25 + margin) + labelWidth);
                    txt.Tag = $"A_{i}_{j}"; // For identification
                    txt.Text = "0";
                    pnlMatrix.Controls.Add(txt);

                    // Column headers (top)
                    if (i == 0)
                    {
                        Label lblCol = new Label();
                        lblCol.Text = $"x{j + 1}";
                        lblCol.AutoSize = true;
                        lblCol.Location = new Point(j * (cellSize + margin) + labelWidth + cellSize / 2 - 5, 5);
                        pnlMatrix.Controls.Add(lblCol);
                    }
                }

                // Row labels (left)
                Label lblRow = new Label();
                lblRow.Text = $"{i + 1}:";
                lblRow.AutoSize = true;
                lblRow.Location = new Point(5, i * (25 + margin) + labelWidth + 5);
                pnlMatrix.Controls.Add(lblRow);

                // "=" label
                Label lblEqual = new Label();
                lblEqual.Text = "=";
                lblEqual.AutoSize = true;
                lblEqual.Location = new Point(size * (cellSize + margin) + labelWidth, i * (25 + margin) + labelWidth + 5);
                pnlMatrix.Controls.Add(lblEqual);

                // Textbox for right-hand side (b)
                TextBox txtB = new TextBox();
                txtB.Size = new Size(cellSize, 25);
                txtB.Location = new Point(size * (cellSize + margin) + labelWidth + 20, i * (25 + margin) + labelWidth);
                txtB.Tag = $"b_{i}"; // For identification
                txtB.Text = "0";
                pnlMatrix.Controls.Add(txtB);
            }
        }

        /// <summary>
        /// Event handler for dimension value change
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void NumSize_ValueChanged(object? sender, EventArgs e)
        {
            size = (int)((NumericUpDown)sender!).Value;

            // Disable "Graphical Solution" button if dimension != 2
            Button btnGraphical = (Button)this.Controls.Find("btnGraphical", true)[0];
            btnGraphical.Enabled = (size == 2);
        }

        /// <summary>
        /// Reads matrix values from UI controls
        /// </summary>
        /// <returns>True if successful, false if error occurred</returns>
        private bool ReadMatrixFromUI()
        {
            // First validate the input values
            if (!ValidateMatrixInput())
                return false;

            A = new double[size, size];
            b = new double[size];

            Panel pnlMatrix = (Panel)this.Controls.Find("pnlMatrix", true)[0];

            try
            {
                foreach (Control ctrl in pnlMatrix.Controls)
                {
                    if (ctrl is TextBox txtBox && txtBox.Tag != null)
                    {
                        string tag = txtBox.Tag.ToString()!;

                        if (tag.StartsWith("A_"))
                        {
                            string[] indices = tag.Substring(2).Split('_');
                            int i = int.Parse(indices[0]);
                            int j = int.Parse(indices[1]);
                            A[i, j] = double.Parse(txtBox.Text);
                        }
                        else if (tag.StartsWith("b_"))
                        {
                            int i = int.Parse(tag.Substring(2));
                            b[i] = double.Parse(txtBox.Text);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading data: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }
        /// <summary>
        /// Gets the selected solution method
        /// </summary>
        /// <returns>Method name as string identifier</returns>
        private string GetSelectedMethod()
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is GroupBox gb && gb.Text == "Solution Method")
                {
                    foreach (Control gbCtrl in gb.Controls)
                    {
                        if (gbCtrl is RadioButton rb && rb.Checked)
                        {
                            if (rb.Text.Contains("Jacobi"))
                                return "Jacobi";
                            else if (rb.Text.Contains("Gauss-Seidel"))
                                return "GaussSeidel";
                            else if (rb.Text.Contains("Steepest Descent"))
                                return "Gradient";
                        }
                    }
                }
            }
            return "Jacobi"; // Default
        }

        /// <summary>
        /// Event handler for solving the system
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnSolve_Click(object? sender, EventArgs e)
        {
            if (!ReadMatrixFromUI())
                return;

            string method = GetSelectedMethod();
            currentMethod = method;

            // Check for potential numerical issues - pass the method name
            string stabilityWarning = SolverMethods.CheckNumericalStability(A, method);
            if (!string.IsNullOrEmpty(stabilityWarning))
            {
                // Show warning with option to continue
                DialogResult result = MessageBox.Show(
                    $"{stabilityWarning}\nDo you want to continue with the solution?",
                    "Numerical Stability Warning",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.No)
                    return;
            }

            try
            {
                List<SolverMethods.IterationResult>? iterations = null;

                switch (method)
                {
                    case "Jacobi":
                        iterations = SolverMethods.JacobiMethod(A, b);
                        break;
                    case "GaussSeidel":
                        iterations = SolverMethods.GaussSeidelMethod(A, b);
                        break;
                    case "Gradient":
                        iterations = SolverMethods.GradientDescentMethod(A, b);
                        break;
                }

                if (iterations != null && iterations.Count > 0)
                {
                    lastIterations = iterations;
                    DisplayResults(iterations);
                    UpdateCharts(iterations); // Call the UpdateCharts method

                    // Enable buttons
                    Button btnSaveResult = (Button)this.Controls.Find("btnSaveResult", true)[0];
                    btnSaveResult.Enabled = true;

                    Button btnGraphical = (Button)this.Controls.Find("btnGraphical", true)[0];
                    btnGraphical.Enabled = size == 2;

                    // Add check for potentially slow convergence
                    if (iterations.Count > 50)
                    {
                        MessageBox.Show(
                            $"The method required {iterations.Count} iterations to converge. This might indicate slow convergence or a poorly conditioned system.",
                            "Convergence Information",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error solving SLAR: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        /// <summary>
        /// Display method for showing the results of the solution
        /// </summary>
        /// <param name="iterations">List of iteration results</param>
        private void DisplayResults(List<SolverMethods.IterationResult> iterations)
        {
            // Get final solution
            var finalIteration = iterations[iterations.Count - 1];

            // Display solution in text field
            TextBox txtSolution = (TextBox)this.Controls.Find("txtSolution", true)[0];
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"Method: {GetMethodName(currentMethod)}");
            sb.AppendLine($"Number of iterations: {iterations.Count}");
            sb.AppendLine($"Total time: {finalIteration.ComputationTime:F2} ms");
            sb.AppendLine();
            sb.AppendLine("Solution:");

            // Display solution values with exactly 6 decimal places
            for (int i = 0; i < finalIteration.Solution.Length; i++)
            {
                sb.AppendLine($"x{i + 1} = {finalIteration.Solution[i]:F6}");
            }

            sb.AppendLine();

            // Calculate and display residual
            double[] residual = SolverMethods.CalculateResidual(A, finalIteration.Solution, b);
            double residualNorm = SolverMethods.CalculateNorm(residual);

            // Display residual norm and error in scientific notation
            sb.AppendLine($"Residual norm: {residualNorm:E6}");
            sb.AppendLine($"Final error: {finalIteration.Error:E6}");

            txtSolution.Text = sb.ToString();

            // Display convergence chart
            Chart chartConvergence = (Chart)this.Controls.Find("chartConvergence", true)[0];
            chartConvergence.Series["ErrorSeries"].Points.Clear();

            for (int i = 0; i < iterations.Count; i++)
            {
                chartConvergence.Series["ErrorSeries"].Points.AddXY(i, iterations[i].Error);
            }

            // Display execution time chart
            Chart chartTime = (Chart)this.Controls.Find("chartTime", true)[0];
            chartTime.Series["TimeSeries"].Points.Clear();

            for (int i = 0; i < iterations.Count; i++)
            {
                chartTime.Series["TimeSeries"].Points.AddXY(i, iterations[i].ComputationTime);
            }
        }

        /// <summary>
        /// Gets the human-readable name of the solution method
        /// </summary>
        /// <param name="method">Method identifier</param>
        /// <returns>Human-readable method name</returns>
        private string GetMethodName(string method)
        {
            switch (method)
            {
                case "Jacobi": return "Jacobi Method";
                case "GaussSeidel": return "Gauss-Seidel Method";
                case "Gradient": return "Steepest Descent Method";
                default: return method;
            }
        }

        /// <summary>
        /// Event handler for showing graphical solution
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnGraphical_Click(object? sender, EventArgs e)
        {
            if (size != 2 || lastIterations == null || lastIterations.Count == 0)
            {
                MessageBox.Show("Graphical solution is only available for 2x2 systems after successful solution.",
                    "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            double[] solution = lastIterations[lastIterations.Count - 1].Solution;
            GraphicalSolution graphForm = new GraphicalSolution(A, b, solution);
            graphForm.ShowDialog();
        }

        /// <summary>
        /// Event handler for saving results to file
        /// </summary>
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments</param>
        private void BtnSaveResult_Click(object? sender, EventArgs e)
        {
            if (lastIterations == null || lastIterations.Count == 0)
            {
                MessageBox.Show("Solve the SLAR first.", "Warning",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (FileManager.SaveResultsToFile(A, b, lastIterations, GetMethodName(currentMethod)))
            {
                MessageBox.Show("Results successfully saved to file.", "Information",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        /// <summary>
        /// Updates all chart controls with iteration data
        /// </summary>
        /// <param name="iterations">List of iteration results</param>
        public void UpdateCharts(List<SolverMethods.IterationResult> iterations)
        {
            // Clear all chart series
            chartConvergence.Series["ErrorSeries"].Points.Clear();
            chartTime.Series["TimeSeries"].Points.Clear();
            chartCombined.Series["ErrorCombinedSeries"].Points.Clear();
            chartCombined.Series["TimeCombinedSeries"].Points.Clear();

            double minError = double.MaxValue;
            double maxError = double.MinValue;
            double minTime = double.MaxValue;
            double maxTime = double.MinValue;

            // Fill with data
            foreach (var iteration in iterations)
            {
                // Error chart
                chartConvergence.Series["ErrorSeries"].Points.AddXY(iteration.IterationNumber, iteration.Error);

                // Time chart
                chartTime.Series["TimeSeries"].Points.AddXY(iteration.IterationNumber, iteration.ComputationTime);

                // Combined chart
                chartCombined.Series["ErrorCombinedSeries"].Points.AddXY(iteration.IterationNumber, iteration.Error);
                chartCombined.Series["TimeCombinedSeries"].Points.AddXY(iteration.IterationNumber, iteration.ComputationTime);

                // Update min and max values
                minError = Math.Min(minError, iteration.Error);
                maxError = Math.Max(maxError, iteration.Error);
                minTime = Math.Min(minTime, iteration.ComputationTime);
                maxTime = Math.Max(maxTime, iteration.ComputationTime);
            }

            // Automatic scaling for error chart
            if (minError > 0 && !double.IsInfinity(minError) && !double.IsNaN(minError))
            {
                chartConvergence.ChartAreas["MainArea"].AxisY.Minimum = minError / 10;
                chartConvergence.ChartAreas["MainArea"].AxisY.Maximum = maxError * 10;
                chartCombined.ChartAreas["MainArea"].AxisY.Minimum = minError / 10;
                chartCombined.ChartAreas["MainArea"].AxisY.Maximum = maxError * 10;
            }

            // Automatic scaling for time chart
            if (minTime < maxTime && !double.IsInfinity(minTime) && !double.IsNaN(minTime))
            {
                chartTime.ChartAreas["MainArea"].AxisY.Minimum = minTime > 0 ? minTime * 0.9 : 0;
                chartTime.ChartAreas["MainArea"].AxisY.Maximum = maxTime * 1.1;
                chartCombined.ChartAreas["MainArea"].AxisY2.Minimum = minTime > 0 ? minTime * 0.9 : 0;
                chartCombined.ChartAreas["MainArea"].AxisY2.Maximum = maxTime * 1.1;
            }

            // If few iterations, show all labels
            if (iterations.Count <= 20)
            {
                chartConvergence.ChartAreas["MainArea"].AxisX.Interval = 1;
                chartTime.ChartAreas["MainArea"].AxisX.Interval = 1;
                chartCombined.ChartAreas["MainArea"].AxisX.Interval = 1;
            }
            else
            {
                chartConvergence.ChartAreas["MainArea"].AxisX.Interval = iterations.Count / 10;
                chartTime.ChartAreas["MainArea"].AxisX.Interval = iterations.Count / 10;
                chartCombined.ChartAreas["MainArea"].AxisX.Interval = iterations.Count / 10;
            }
        }

        /// <summary>
        /// Validates the input values in matrix textboxes to ensure they are valid numbers
        /// </summary>
        /// <returns>True if all values are valid, false otherwise</returns>
        private bool ValidateMatrixInput()
        {
            Panel pnlMatrix = (Panel)this.Controls.Find("pnlMatrix", true)[0];

            foreach (Control ctrl in pnlMatrix.Controls)
            {
                if (ctrl is TextBox txtBox && txtBox.Tag != null)
                {
                    string tag = txtBox.Tag.ToString()!;

                    if (tag.StartsWith("A_") || tag.StartsWith("b_"))
                    {
                        // Check if the value can be parsed as a double
                        if (!double.TryParse(txtBox.Text, out double value))
                        {
                            MessageBox.Show($"Invalid numeric value in {GetElementDescription(tag)}",
                                "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtBox.Focus();
                            return false;
                        }

                        // Only check for extremely large values that could cause overflow
                        if (double.IsInfinity(value) || Math.Abs(value) > 1e100)
                        {
                            MessageBox.Show($"Value in {GetElementDescription(tag)} is too large and may cause calculation errors",
                                "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            txtBox.Focus();
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a human-readable description of a matrix element based on its tag
        /// </summary>
        /// <param name="tag">Element tag (e.g., "A_0_1" or "b_2")</param>
        /// <returns>Human-readable description (e.g., "Matrix A[1,2]" or "Vector b[3]")</returns>
        private string GetElementDescription(string tag)
        {
            if (tag.StartsWith("A_"))
            {
                string[] indices = tag.Substring(2).Split('_');
                int i = int.Parse(indices[0]) + 1;
                int j = int.Parse(indices[1]) + 1;
                return $"Matrix A[{i},{j}]";
            }
            else if (tag.StartsWith("b_"))
            {
                int i = int.Parse(tag.Substring(2)) + 1;
                return $"Vector b[{i}]";
            }

            return tag;
        }
    }
}