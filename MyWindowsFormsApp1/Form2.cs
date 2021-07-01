using System.Windows.Forms;

namespace MyWindowsFormsApp1
{
    public partial class Form2 : Form
    {
        public Form2()
        {
            InitializeComponent();
        }

        public void MainFormTextBoxChanged(string text)
        {
            this.textBox1.Text = text;
        }
    }
}
