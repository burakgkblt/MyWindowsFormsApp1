using System;
using System.Windows.Forms;

namespace MyWindowsFormsApp1
{
    public partial class Form1 : Form
    {
        private Form2 form2;
        public Form1()
        {
            InitializeComponent();
            form2 = new Form2();
        }

        private void btnShowForms_Click(object sender, EventArgs e)
        {
            if (form2 == null) form2 = new Form2();
            if (!form2.Visible)
            form2.Show();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            form2.MainFormTextBoxChanged(textBox1.Text);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            form2.Close();
        }
    }
}
