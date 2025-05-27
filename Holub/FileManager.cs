using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.Windows.Forms;

namespace SLARSolver
{
    /// <summary>
    /// Improved FileManager class for SLAR file operations
    /// Enhanced handling of floating-point numbers in various formats
    /// </summary>
    public static class FileManager
    {
        /// <summary>
        /// Saves the solution results to a text file
        /// </summary>
        /// <param name="A">Coefficient matrix of the equation system</param>
        /// <param name="b">Right-hand side vector of the equation system</param>
        /// <param name="iterations">List of iteration results from the solver</param>
        /// <param name="methodName">Name of the solution method used</param>
        /// <returns>True if save was successful, false otherwise</returns>
        public static bool SaveResultsToFile(double[,] A, double[] b, List<SolverMethods.IterationResult> iterations, string methodName)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Save SLAR Solution Results",
                DefaultExt = "txt",
                FileName = $"SLAR_Solution_{methodName}_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter writer = new StreamWriter(saveFileDialog.FileName, false, Encoding.UTF8))
                    {
                        // Write system information
                        int n = b.Length;
                        writer.WriteLine($"Solution of SLAR using {methodName} method");
                        writer.WriteLine($"Date: {DateTime.Now}");
                        writer.WriteLine();

                        writer.WriteLine("System of equations:");
                        for (int i = 0; i < n; i++)
                        {
                            StringBuilder equation = new StringBuilder();
                            for (int j = 0; j < n; j++)
                            {
                                if (j > 0)
                                    equation.Append(A[i, j] >= 0 ? " + " : " - ");
                                else if (A[i, j] < 0)
                                    equation.Append("-");

                                equation.Append($"{Math.Abs(A[i, j]).ToString("F6", CultureInfo.InvariantCulture)}x{j + 1}");
                            }
                            equation.Append($" = {b[i].ToString("F6", CultureInfo.InvariantCulture)}");
                            writer.WriteLine(equation.ToString());
                        }
                        writer.WriteLine();

                        writer.WriteLine("Iteration Results:");
                        writer.WriteLine("Number\tError\tTime (ms)\tSolution");

                        foreach (var iteration in iterations)
                        {
                            StringBuilder solutionStr = new StringBuilder();
                            for (int i = 0; i < iteration.Solution.Length; i++)
                            {
                                solutionStr.Append($"x{i + 1}={iteration.Solution[i].ToString("F6", CultureInfo.InvariantCulture)}");
                                if (i < iteration.Solution.Length - 1)
                                    solutionStr.Append(", ");
                            }

                            writer.WriteLine($"{iteration.IterationNumber}\t{iteration.Error.ToString("E6", CultureInfo.InvariantCulture)}\t{iteration.ComputationTime.ToString("F2", CultureInfo.InvariantCulture)}\t{solutionStr}");
                        }

                        writer.WriteLine();

                        var finalIteration = iterations[iterations.Count - 1];
                        writer.WriteLine("Final Solution:");
                        for (int i = 0; i < finalIteration.Solution.Length; i++)
                        {
                            writer.WriteLine($"x{i + 1} = {finalIteration.Solution[i].ToString("F6", CultureInfo.InvariantCulture)}");
                        }

                        double[] residual = SolverMethods.CalculateResidual(A, finalIteration.Solution, b);
                        double residualNorm = SolverMethods.CalculateNorm(residual);
                        writer.WriteLine();
                        writer.WriteLine($"Residual Norm: {residualNorm.ToString("E6", CultureInfo.InvariantCulture)}");
                        writer.WriteLine($"Number of Iterations: {iterations.Count}");
                        writer.WriteLine($"Total Computation Time: {finalIteration.ComputationTime.ToString("F2", CultureInfo.InvariantCulture)} ms");
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Loads a system of linear equations from a file
        /// </summary>
        /// <param name="A">Output parameter: Coefficient matrix of the equation system</param>
        /// <param name="b">Output parameter: Right-hand side vector of the equation system</param>
        /// <returns>True if load was successful, false otherwise</returns>
        public static bool LoadSLARFromFile(out double[,] A, out double[] b)
        {
            A = new double[0, 0];
            b = Array.Empty<double>();

            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
                Title = "Load SLAR from file"
            };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string[] lines = File.ReadAllLines(openFileDialog.FileName);

                    // Skip comments and empty lines at the beginning of the file
                    int startLine = 0;
                    while (startLine < lines.Length &&
                           (string.IsNullOrWhiteSpace(lines[startLine]) || lines[startLine].TrimStart().StartsWith("Sol") ||
                            lines[startLine].TrimStart().StartsWith("Date") || lines[startLine].TrimStart().StartsWith("System")))
                    {
                        startLine++;
                    }

                    // Check if this is a result file or a matrix file
                    bool isResultFile = false;
                    int n = 0;

                    if (startLine < lines.Length)
                    {
                        // Try to read as a matrix file
                        if (int.TryParse(lines[startLine].Trim(), out n) && n > 0)
                        {
                            // This is a matrix file
                            isResultFile = false;
                        }
                        else
                        {
                            // This might be a result file, look for equation lines
                            isResultFile = true;

                            // Count equations to determine the size
                            int equationCount = 0;
                            for (int i = startLine; i < lines.Length; i++)
                            {
                                if (lines[i].Contains("x1") && !lines[i].Contains("Results"))
                                {
                                    equationCount++;
                                }
                                else if (lines[i].Contains("Results"))
                                {
                                    break;
                                }
                            }

                            n = equationCount;
                        }
                    }

                    if (n <= 0)
                    {
                        MessageBox.Show("Could not determine the size of the SLAR from the file.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    if (n < 2 || n > 10)
                    {
                        MessageBox.Show($"Invalid matrix dimension: {n}. Allowed range is 2-10.",
                            "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }

                    // Initialize matrix and vector
                    A = new double[n, n];
                    b = new double[n];

                    if (!isResultFile)
                    {
                        // Read coefficients from matrix file
                        for (int i = 0; i < n; i++)
                        {
                            if (startLine + 1 + i >= lines.Length)
                            {
                                MessageBox.Show($"Invalid file format: not enough lines (expected {n + 1}).",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            string[] values = lines[startLine + 1 + i].Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                            if (values.Length != n + 1)
                            {
                                MessageBox.Show($"Invalid file format: line {startLine + 2 + i} should contain {n + 1} numbers.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            for (int j = 0; j < n; j++)
                            {
                                if (!TryParseInvariant(values[j], out A[i, j]))
                                {
                                    MessageBox.Show($"Invalid number format in line {startLine + 2 + i}, position {j + 1}: '{values[j]}'",
                                        "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    return false;
                                }

                                // Round to 6 decimal places if needed
                                A[i, j] = Math.Round(A[i, j], 6);
                            }

                            if (!TryParseInvariant(values[n], out b[i]))
                            {
                                MessageBox.Show($"Invalid number format in line {startLine + 2 + i}, position {n + 1}: '{values[n]}'",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            // Round to 6 decimal places if needed
                            b[i] = Math.Round(b[i], 6);
                        }
                    }
                    else
                    {
                        // Read coefficients from results file
                        int equationLine = startLine;
                        for (int i = 0; i < n; i++)
                        {
                            // Find the next equation
                            while (equationLine < lines.Length &&
                                  (!lines[equationLine].Contains("x1") || lines[equationLine].Contains("Results")))
                            {
                                equationLine++;
                            }

                            if (equationLine >= lines.Length)
                            {
                                MessageBox.Show($"Invalid file format: could not find equation {i + 1}.",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            string equation = lines[equationLine];

                            // Parse the equation
                            if (!ParseEquation(equation, i, n, ref A, ref b))
                            {
                                MessageBox.Show($"Failed to parse equation {i + 1}: {equation}",
                                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return false;
                            }

                            equationLine++;
                        }
                    }

                    // Check that no diagonal element is zero
                    for (int i = 0; i < n; i++)
                    {
                        if (Math.Abs(A[i, i]) < 1e-14)
                        {
                            MessageBox.Show($"Error: Zero diagonal element at position [{i + 1},{i + 1}]. Division by zero is not allowed.",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading file: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }
            }

            return false;
        }

        /// <summary>
        /// Helper method to parse an equation in the format "a1x1 + a2x2 + ... = b"
        /// </summary>
        /// <param name="equation">Equation string to parse</param>
        /// <param name="row">Row index in the matrix</param>
        /// <param name="n">Size of the system</param>
        /// <param name="A">Reference to the coefficient matrix</param>
        /// <param name="b">Reference to the right-hand side vector</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        private static bool ParseEquation(string equation, int row, int n, ref double[,] A, ref double[] b)
        {
            try
            {
                // Split the equation into left and right parts
                string[] parts = equation.Split('=');
                if (parts.Length != 2)
                    return false;

                // Parse the right part (b)
                if (!TryParseInvariant(parts[1].Trim(), out b[row]))
                    return false;

                // Round to 6 decimal places if needed
                b[row] = Math.Round(b[row], 6);

                // Parse the left part (coefficients A)
                string leftPart = parts[0].Trim();

                // Split into terms (separated by + or -)
                List<string> terms = new List<string>();
                int startPos = 0;

                // Process the first term separately (might not have a sign in front)
                if (!leftPart.StartsWith("+") && !leftPart.StartsWith("-"))
                {
                    // Find the next + or - sign
                    int nextSign = FindNextSignPosition(leftPart, 1);
                    if (nextSign == -1)
                    {
                        // This is the only term
                        terms.Add(leftPart);
                    }
                    else
                    {
                        terms.Add(leftPart.Substring(0, nextSign).Trim());
                        startPos = nextSign;
                    }
                }

                // Process the remaining terms
                while (startPos < leftPart.Length)
                {
                    int nextSign = FindNextSignPosition(leftPart, startPos + 1);
                    if (nextSign == -1)
                    {
                        // Last term
                        terms.Add(leftPart.Substring(startPos).Trim());
                        break;
                    }
                    else
                    {
                        terms.Add(leftPart.Substring(startPos, nextSign - startPos).Trim());
                        startPos = nextSign;
                    }
                }

                // Initialize coefficients with zeros
                for (int j = 0; j < n; j++)
                {
                    A[row, j] = 0;
                }

                // Parse each term and fill the corresponding coefficient
                foreach (string term in terms)
                {
                    if (string.IsNullOrWhiteSpace(term))
                        continue;

                    // Determine the sign
                    double sign = 1.0;
                    string termToProcess = term;

                    if (term.StartsWith("-"))
                    {
                        sign = -1.0;
                        termToProcess = term.Substring(1).Trim();
                    }
                    else if (term.StartsWith("+"))
                    {
                        termToProcess = term.Substring(1).Trim();
                    }

                    // Find the variable index (x1, x2, ...)
                    int xIndex = termToProcess.IndexOf('x');
                    if (xIndex == -1)
                        return false;

                    // Extract the coefficient
                    string coeffStr = termToProcess.Substring(0, xIndex).Trim();
                    if (string.IsNullOrWhiteSpace(coeffStr))
                        coeffStr = "1";

                    double coeff;
                    if (!TryParseInvariant(coeffStr, out coeff))
                        return false;

                    // Extract the variable index
                    string varIndexStr = termToProcess.Substring(xIndex + 1).Trim();
                    if (!int.TryParse(varIndexStr, out int varIndex) || varIndex < 1 || varIndex > n)
                        return false;

                    // Set the coefficient
                    A[row, varIndex - 1] = Math.Round(sign * coeff, 6); // Round to 6 decimal places
                }

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method to find the next + or - sign in a string
        /// Handles scientific notation (ignores + or - after 'e' or 'E')
        /// </summary>
        /// <param name="input">Input string</param>
        /// <param name="startFrom">Position to start searching from</param>
        /// <returns>Position of the next sign or -1 if not found</returns>
        private static int FindNextSignPosition(string input, int startFrom)
        {
            for (int i = startFrom; i < input.Length; i++)
            {
                if ((input[i] == '+' || input[i] == '-') && (i == 0 || (input[i - 1] != 'E' && input[i - 1] != 'e')))
                    return i;
            }
            return -1;
        }

        /// <summary>
        /// Enhanced method for parsing floating-point numbers with support for various formats
        /// Supports invariant culture and scientific notation
        /// </summary>
        /// <param name="input">String to parse</param>
        /// <param name="result">Parsing result</param>
        /// <returns>True if parsing was successful, false otherwise</returns>
        private static bool TryParseInvariant(string input, out double result)
        {
            // Support different formats: decimal point, comma, scientific notation
            return double.TryParse(input.Replace(',', '.'),
                NumberStyles.Float | NumberStyles.AllowExponent,
                CultureInfo.InvariantCulture,
                out result);
        }
    }
}