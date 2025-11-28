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
    public class OutputForm : Form
    {
        private TextBox outputBox;

        public OutputForm(string result)
        {
            this.Text = "RREF Output";
            this.Width = 800;
            this.Height = 600;

            outputBox = new TextBox
            {
                Multiline = true,
                Width = 740,
                Height = 520,
                Top = 20,
                Left = 20,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true,
                Font = new System.Drawing.Font("Arial", 12)
            };

            outputBox.Text = result;

            this.Controls.Add(outputBox);
        }
    }
}
