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

        private void SolveButton_Click(object sender, EventArgs e)
        {
            try
            {
                var rows = inputBox.Text.Trim()
                    .Split('\n')
                    .Where(line => !string.IsNullOrWhiteSpace(line))
                    .Select(line => line.Trim().Split(' ').Select(double.Parse).ToArray())
                    .ToArray();

                string result = SolveRREF(rows);

                // فتح OutputForm
                OutputForm outputForm = new OutputForm(result);
                outputForm.ShowDialog();
            }
            catch
            {
                MessageBox.Show("Invalid input format. Please enter numbers separated by spaces.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region RREF Logic (لمسح أي تعديل)
        private string SolveRREF(double[][] matrix)
        {
            int rows = matrix.Length;
            int cols = matrix[0].Length;
            int variables = cols - 1;
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
            {
                sb.AppendLine(string.Join("\t", matrix[i].Select(x => x.ToString("F2"))));
            }
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
            {
                double value = matrix[i][variables];
                sb.AppendLine($"x{i + 1} = {value:F4}");
            }

            return sb.ToString();
        }
        #endregion
    }
}

