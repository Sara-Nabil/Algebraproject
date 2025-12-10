using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace RREFSolver
{
    public class InputForm : Form
    {
        private TextBox inputBox;
        private Button solveButton;

        public InputForm()
        {
            CreateLayout();
        }

        private void CreateLayout()
        {
            this.Text = "RREF Solver - Input";
            this.Width = 800;
            this.Height = 600;

            Label lbl = new Label
            {
                Text = "Enter equations as augmented matrix rows:",
                Top = 20,
                Left = 20,
                Width = 600
            };
            this.Controls.Add(lbl);

            inputBox = new TextBox
            {
                Multiline = true,
                Width = 740,
                Height = 100,
                Top = 50,
                Left = 20,
                ScrollBars = ScrollBars.Vertical,
                Font = new System.Drawing.Font("Arial", 12)
            };
            this.Controls.Add(inputBox);

            solveButton = new Button
            {
                Text = "Solve Equations",
                Top = 170,
                Left = 20,
                Width = 200,
                Height = 40
            };
            solveButton.Click += SolveButton_Click;
            this.Controls.Add(solveButton);
        }

        private void ShowResult(string text)
        {
            OutputForm form = new OutputForm(text);
            form.ShowDialog();
        }

        private void SolveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var rows = inputBox.Text.Trim()
                    .Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim().Split(' ').Select(double.Parse).ToArray())
                    .ToArray();

               
                var original = CloneMatrix(rows);

                
                string result = SolveRREF(rows);

               
                if (result.Contains("No solution"))
                {
                    result += "\n(Note: The system of equations is inconsistent, but Transpose/Inverse will still be attempted below.)\n";
                }
              
                result += "\n=== TRANSPOSE (Step-by-step) ===\n";
                result += TransposeSteps(original);

               
                if (original.Length == original[0].Length)
                {
                    try
                    {
                        result += "\n=== INVERSE (Step-by-step) ===\n";
                        result += InverseSteps(original);
                    }
                    catch (Exception ex)
                    {
                        result += $"\nCannot compute inverse: {ex.Message}\n";
                    }
                }
                else
                {
                    result += "\nMatrix is not square → cannot compute inverse.\n";
                }

                ShowResult(result);
            }
            catch
            {
                MessageBox.Show("Invalid input format. Please enter numbers separated by spaces.",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        
        private string SolveRREF(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            int lead = 0;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("=== Step-by-Step RREF Solution ===");

            for (int r = 0; r < rows; r++)
            {
                if (lead >= cols)
                    break;

                int i = r;
                while (Math.Abs(matrix[i][lead]) < 1e-10)
                {
                    i++;
                    if (i == rows)
                    {
                        i = r;
                        lead++;
                        if (lead == cols)
                            goto done;
                    }
                }

                if (i != r)
                {
                    var temp = matrix[r];
                    matrix[r] = matrix[i];
                    matrix[i] = temp;
                    sb.AppendLine($"Swapped Row {r + 1} with Row {i + 1}:");
                    PrintMatrix(matrix, sb);
                }

                double div = matrix[r][lead];
                if (Math.Abs(div) > 1e-10)
                {
                    for (int j = 0; j < cols; j++)
                        matrix[r][j] /= div;

                    sb.AppendLine($"Normalized Row {r + 1} (divided by {div:F2}):");
                    PrintMatrix(matrix, sb);
                }

                for (int k = 0; k < rows; k++)
                {
                    if (k != r)
                    {
                        double mult = matrix[k][lead];
                        for (int j = 0; j < cols; j++)
                            matrix[k][j] -= mult * matrix[r][j];

                        sb.AppendLine($"Eliminated Row {k + 1} using Row {r + 1} (multiplied by {mult:F2}):");
                        PrintMatrix(matrix, sb);
                    }
                }

                lead++;
            }

        done:
            sb.AppendLine("\n=== Final RREF Matrix ===");
            PrintMatrix(matrix, sb);

            sb.AppendLine(AnalyzeSolution(matrix));
            return sb.ToString();
        }

        private void PrintMatrix(double[][] matrix, StringBuilder sb)
        {
            for (int i = 0; i < matrix.Length; i++)
                sb.AppendLine(string.Join("\t", matrix[i].Select(x => x.ToString("F2"))));
            sb.AppendLine();
        }

        private string AnalyzeSolution(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            int variables = cols - 1;

            for (int i = 0; i < rows; i++)
            {
                bool allZero = true;
                for (int j = 0; j < variables; j++)
                {
                    if (Math.Abs(matrix[i][j]) > 1e-10)
                    {
                        allZero = false;
                        break;
                    }
                }
                if (allZero && Math.Abs(matrix[i][variables]) > 1e-10)
                    return "No solution.";
            }

            int rank = matrix.Count(row => row.Any(x => Math.Abs(x) > 1e-10));
            if (rank < variables)
                return "Infinite solutions.";

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Unique solution found:");
            for (int i = 0; i < variables; i++)
                sb.AppendLine($"x{i + 1} = {matrix[i][variables]:F4}");

            return sb.ToString();
        }

   
        private string TransposeSteps(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Original Matrix:");
            sb.AppendLine(MatrixToString(matrix));

            double[][] t = new double[cols][];
            for (int i = 0; i < cols; i++)
            {
                t[i] = new double[rows];
                for (int j = 0; j < rows; j++)
                {
                    sb.AppendLine($"t[{i},{j}] = matrix[{j},{i}] → {matrix[j][i]:F4}");
                    t[i][j] = matrix[j][i];
                }
            }

            sb.AppendLine("\nTranspose Result:");
            sb.AppendLine(MatrixToString(t));
            return sb.ToString();
        }

        private string InverseSteps(double[][] matrix)
        {
            int n = matrix.Length;
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("Creating augmented matrix [A | I]:");

            double[][] aug = new double[n][];
            for (int i = 0; i < n; i++)
            {
                aug[i] = new double[2 * n];
                for (int j = 0; j < n; j++)
                    aug[i][j] = matrix[i][j];
                aug[i][n + i] = 1;
            }

            sb.AppendLine(MatrixToString(aug));

            for (int i = 0; i < n; i++)
            {
                double pivot = aug[i][i];
                if (Math.Abs(pivot) < 1e-10)
                    throw new Exception("Matrix is singular");

                sb.AppendLine($"\nNormalize row {i + 1} by dividing with pivot {pivot:F4}");
                for (int j = 0; j < 2 * n; j++)
                    aug[i][j] /= pivot;

                sb.AppendLine(MatrixToString(aug));

                for (int r = 0; r < n; r++)
                {
                    if (r != i)
                    {
                        double mult = aug[r][i];
                        sb.AppendLine($"Row {r + 1} = Row {r + 1} – {mult:F4} × Row {i + 1}");
                        for (int c = 0; c < 2 * n; c++)
                            aug[r][c] -= mult * aug[i][c];

                        sb.AppendLine(MatrixToString(aug));
                    }
                }
            }

           
            double[][] inv = new double[n][];
            for (int i = 0; i < n; i++)
            {
                inv[i] = new double[n];
                for (int j = 0; j < n; j++)
                    inv[i][j] = aug[i][j + n];
            }

            sb.AppendLine("Inverse Matrix:");
            sb.AppendLine(MatrixToString(inv));
            return sb.ToString();
        }

        
        private double[][] CloneMatrix(double[][] m)
        {
            return m.Select(row => row.ToArray()).ToArray();
        }

        private string MatrixToString(double[][] matrix)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var row in matrix)
                sb.AppendLine(string.Join("\t", row.Select(x => x.ToString("F4"))));
            return sb.ToString();
        }
    }
}
