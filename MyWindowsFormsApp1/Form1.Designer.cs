using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tartı;
using static WebPackageV_1.SendCargo;

namespace WebPackageV_1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        ArkadasWebSiteDataContext db = new ArkadasWebSiteDataContext();
        MarketPlaceDataContext markt = new MarketPlaceDataContext();
        public int IDInvoice;
        System.Windows.Forms.Timer _typingTimer;
        public void AutoSize()
        {
            this.dataGridView1.Columns[0].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridView1.Columns[1].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            this.dataGridView1.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        }
        private void GetDetails2(int Id)
        {
            WebOrdersNew order = (from x in db.WebOrdersNews
                                  where x.ID == Id
                                  select x).FirstOrDefault();

            List<string> statusList = new List<string>();
            statusList.Add("Kabul Edilmiş");
            statusList.Add("Satıcının kargo bilgilerini sisteme girmesi bekleniyor");
            statusList.Add("Packaged");
            statusList.Add("Kabul Edilmiş Ama Zamanında Kargoya Verilmemiş");
            //🤣❤❤
            List<WebOrderComponentsNew> orderCompo = (from x in db.WebOrderComponentsNews
                                                      where x.IDOrder == Id && (statusList.Contains(x.OrderStatus) || x.OrderStatus == null && x.IDInvoice != null && x.IDInvoice != "" && x.IDInvoice != "0")
                                                      select x).ToList();
            CargoNameSurname.Text = order.CargoNameSurname;
            CargoNameSurnameDumy.Text = order.CargoNameSurname;
            InvoiceNameSurname.Text = order.InvoiceNameSurname;
            CargoAdress.Text = order.CargoAddress + " " + order.CargoCountry + " " + order.CargoDistrict + "/" + order.CargoCity;
            CargoAdressDumy.Text = order.CargoAddress + " " + order.CargoCountry + " " + order.CargoDistrict + "/" + order.CargoCity;
            textBox1.Text = order.CargoAddress + " " + order.CargoCountry + " " + order.CargoCity;
            label18.Text = (order.CargoDistrict == null ? order.CargoTown : order.CargoDistrict) + "/" + order.CargoCity;
            label15.Text = order.CargoTelNo;
            label16.Text = order.Email;
            label29.Text = DateTime.Now.ToShortDateString();
            dateLabel.Text = DateTime.Now.ToShortDateString();
            label30.Text = order.CargoNameSurname;
            //InvoiceAdress.Text = order.InvoiceAddress + " " + order.InvoiceCountry + " " + order.InvoiceCity;
            Phone.Text = order.CargoTelNo;
            PhoneDumy.Text = order.CargoTelNo;
            CargoName.Text = order.CargoFirm;
            byte[] barcodeImageNew;
            byte[] barcodeImageNew2;
            byte[] barcodeImageNewDumy;
            if (order.OrderSystemsId == null)//bizim sipariş
            {
                string InvoiceId = orderCompo.Where(x => x.IDInvoice != null && x.IDInvoice != "" && x.IDInvoice != "0").FirstOrDefault() == null ? "" : orderCompo.Where(x => x.IDInvoice != null && x.IDInvoice != "" && x.IDInvoice != "0").FirstOrDefault().IDInvoice.ToString();
                barcodeImageNew = new BarcodeGenerator().CreateBarcode(InvoiceId, 80, 2);
                barcodeImageNew2 = new BarcodeGenerator().CreateBarcode(InvoiceId, 20, 2);
                barcodeImageNewDumy = new BarcodeGenerator().CreateBarcode(order.OrderNumber, 20, 2);

            }
            else
            {
                barcodeImageNew = new BarcodeGenerator().CreateBarcode(order.Barkod, 80, 2);
                barcodeImageNew2 = new BarcodeGenerator().CreateBarcode(order.Barkod, 20, 2);
                barcodeImageNewDumy = new BarcodeGenerator().CreateBarcode(order.OrderNumber, 20, 2);
            }

            using (MemoryStream ms = new MemoryStream(barcodeImageNew))
            {
                pictureBox1.Show();
                pictureBox1.Image = Image.FromStream(ms);
            }
            using (MemoryStream ms = new MemoryStream(barcodeImageNew2))
            {
                pictureBox2.Image = Image.FromStream(ms);
            }
            using (MemoryStream ms = new MemoryStream(barcodeImageNewDumy))
            {
                pictureBox7.Image = Image.FromStream(ms);
                pictureBox5.Image = Image.FromStream(ms);
            }

            OrderSystem systemName = (from x in markt.OrderSystems where x.Id == order.OrderSystemsId select x).FirstOrDefault();
            if (systemName == null)
            {
                Platform.Text = "Web Siparişi";
            }
            else
            {
                Platform.Text = systemName.SystemName;
            }

            decimal totalPrice = 0;
            if (order.OrderSystemsId == 5)
            {
                totalPrice = orderCompo.Sum(x => x.SalesPrice * x.ItemAmount);
            }
            else
            {
                totalPrice = orderCompo.Sum(x => x.SalesPrice);
            }

            if (systemName != null)
            {
                if (systemName.Id == 2)
                {
                    CargoOdemeBilgisi.Text = "ÜA";
                    OdemeBilgisi.Text = order.CargoPayment;


                }
                if (systemName.Id == 3)
                {
                    if (order.CargoPayment == "Şartlı Kargo Ücretsiz (Satıcı Öder)")
                    {
                        CargoOdemeBilgisi.Text = "PÖ";
                    }
                    else if (order.CargoPayment == "N11 Öder")
                    {
                        CargoOdemeBilgisi.Text = "ÜA";
                    }

                    OdemeBilgisi.Text = order.CargoPayment;

                }
                if (systemName.Id == 5)
                {
                    if (order.CargoPayment == "Seller")
                    {
                        OdemeBilgisi.Text = "Hepsiburada öder";
                        CargoOdemeBilgisi.Text = "ÜA";
                    }
                }
            }



            //TotalPrice.Text = totalPrice.ToString();
            //dataGridView2.Visible = true;
            //dataGridView2.DataSource = orderCompo.Select(x => new PrintClass { ItemStockCode = x.ItemStockCode, ItemName = x.ItemName, ItemAmount = x.ItemAmount }).ToList();
        }


        public class PrintClass
        {
            public string ItemStockCode { get; set; }
            public string ItemName { get; set; }
            public int ItemAmount { get; set; }
        }


        private void button1_Click(object sender, EventArgs e)
        {
            blackbox.Visible = true;
            string invno = textBoxInvoiceNumber.Text.Trim();



            ClearAll();

            try
            {
                bool isPacking = false;

                string selectedInvoiceNumber = invno;

                if (String.IsNullOrEmpty(selectedInvoiceNumber))
                {
                    MessageBox.Show("Lütfen fatura numarası giriniz.");
                }
                else
                {

                    if (!checkBox1.Checked)
                    {
                        if (!Char.IsDigit(selectedInvoiceNumber[0]))
                        {
                            selectedInvoiceNumber = selectedInvoiceNumber.Substring(1);
                        }
                    }
                    DateTime nowmonth = DateTime.Now;
                    nowmonth = nowmonth.AddMonths(-2);

                    invoicenumberlabel.Text = selectedInvoiceNumber;
                    List<WebGetInvoiceArkadasPropertiesInvoiceIDResult> ListCompanyName = new List<WebGetInvoiceArkadasPropertiesInvoiceIDResult>();
                    int InvoiceIdDK = 0;
                    if (checkBox1.Checked)
                    {
                        List<WebGetInvoiceArkadasPropertiesResult> ListCompanyName2 = db.WebGetInvoiceArkadasProperties(selectedInvoiceNumber).Where(x => x.InvoiceDate > nowmonth).ToList();
                        if (ListCompanyName2.Count == 0)
                        {
                            //MessageBox.Show("Lütfen geçerli bir fatura numarası giriniz.");
                        }
                        else
                        {
                            InvoiceIdDK = ListCompanyName2.FirstOrDefault().Id;
                            if (InvoiceIdDK > 0)
                            {
                                ListCompanyName = db.WebGetInvoiceArkadasPropertiesInvoiceID(Convert.ToInt32(InvoiceIdDK)).Where(x => x.InvoiceDate > nowmonth).ToList();
                            }
                        }
                    }
                    else
                    {
                        ListCompanyName = db.WebGetInvoiceArkadasPropertiesInvoiceID(Convert.ToInt32(selectedInvoiceNumber)).Where(x => x.InvoiceDate > nowmonth).ToList();



                    }
                    if (ListCompanyName.Count == 0)
                    {
                        MessageBox.Show("Lütfen geçerli bir fatura numarası giriniz.");
                    }
                    else
                    {

                        if (ListCompanyName.Count > 1)
                        {
                            dataGridView1.DataSource = ListCompanyName;


                            dataGridView1.Visible = true;
                            //setVisible(false);

                        }
                        else
                        {
                            int HiddenIDInvoice = ListCompanyName.FirstOrDefault().Id;
                            IDInvoice = HiddenIDInvoice;

                            List<WebInvoiceControl> invoiceControls = (from i in db.WebInvoiceControls
                                                                       where
                                                                           i.IDInvoice == HiddenIDInvoice &&
                                                                           i.ItemAmount > 0
                                                                       select i).ToList();
                            dataGridView1.Visible = false;

                            //TODO Olası Hata Blokları Gerekirse tüm yoğunluk veritabanına verilebilir.

                            WebOrdersNew webOrder = (from woc in db.WebOrderComponentsNews
                                                     from wo in db.WebOrdersNews
                                                     where
                                                         woc.IDInvoice == HiddenIDInvoice.ToString() &&
                                                         wo.ID == woc.IDOrder
                                                     select wo).FirstOrDefault();
                            WebAdminOrderNote note = (from i in db.WebAdminOrderNotes where i.OrderNumber == webOrder.OrderNumber select i).FirstOrDefault();

                            if (note != null)
                            {
                                MessageBox.Show(note.Note, "Yönetici Sipariş Notu", MessageBoxButtons.OK);
                            }
                            if (webOrder != null)
                            {
                                try
                                {
                                    if (webOrder.OrderNote != null)
                                    {
                                        if (webOrder.OrderNote.Length > 0)
                                        {
                                            MessageBox.Show(webOrder.OrderNote, "Müşteri Sipariş Notu", MessageBoxButtons.OK);
                                            // MessageBox.Show(webOrder.OrderNote);
                                        }

                                    }
                                    GetDetails2(webOrder.ID);
                                }
                                catch (Exception)
                                {

                                }

                                LabelOrderId.Text = webOrder.ID.ToString();
                                //if (!String.IsNullOrEmpty(webOrder.OrderNote))
                                //{
                                //    RadTextBoxOrderNote.Text = webOrder.OrderNote;
                                //    LabelOrderNote.Visible = true;
                                //    RadTextBoxOrderNote.Visible = true;
                                //}
                                //else
                                //{
                                //    LabelOrderNote.Visible = false;
                                //    RadTextBoxOrderNote.Visible = false;
                                //}
                                //ups yapıldı??
                                //webOrder.IDCargo = 1;
                                //webOrder.IDCargo = 2;
                                ComboBoxCargo.SelectedValue = webOrder.IDCargo;
                                if (ComboBoxCargo.SelectedIndex == 7)
                                {
                                    blackbox.Visible = false;
                                }
                                db.SubmitChanges();

                                //tradminnote.Visible = true;

                                WebAdminOrderNote adminOrderNote = (from i in db.WebAdminOrderNotes
                                                                    where i.OrderNumber == webOrder.OrderNumber
                                                                    select i).FirstOrDefault();
                                //if (adminOrderNote != null)
                                //{
                                //    if (adminOrderNote.Note != null)
                                //    {
                                //        tradminnote.Visible = true;
                                //        LabelNote.Visible = true;
                                //        LabelAdminOrderNote.Visible = true;
                                //        LabelAdminOrderNote.Text = adminOrderNote.Note;
                                //    }
                                //}
                            }

                            if (invoiceControls.Count == 0)
                            {
                                List<WebGetInvoicePropertiesResult> invoiceProperties = db.WebGetInvoiceProperties(HiddenIDInvoice).ToList();

                                foreach (WebGetInvoicePropertiesResult item in invoiceProperties)
                                {
                                    WebInvoiceControl tempInvoiceControl = new WebInvoiceControl();
                                    tempInvoiceControl.ItemStockCode = item.ItemStockCode;
                                    tempInvoiceControl.ItemName = item.ProductName;
                                    tempInvoiceControl.ItemAmount = item.ItemAmount;
                                    tempInvoiceControl.ReadingAmount = item.ReadingAmount;
                                    tempInvoiceControl.EntryDate = DateTime.Now;

                                    tempInvoiceControl.Operator = "Windows Form";

                                    tempInvoiceControl.InvoiceNumber = ListCompanyName.FirstOrDefault().InvoiceNumber;
                                    tempInvoiceControl.IDInvoice = HiddenIDInvoice;
                                    tempInvoiceControl.PackingWeight = 0m;
                                    tempInvoiceControl.InvoiceDate = ListCompanyName.FirstOrDefault().InvoiceDate;
                                    if (webOrder != null)
                                    {
                                        tempInvoiceControl.OrderDate = webOrder.OrderDate;
                                    }
                                    tempInvoiceControl.PackingDate = new DateTime(1900, 1, 1);
                                    tempInvoiceControl.IsPacking = false;
                                    tempInvoiceControl.PackingOperator = String.Empty;

                                    db.WebInvoiceControls.InsertOnSubmit(tempInvoiceControl);
                                    db.SubmitChanges();
                                }
                                db.SubmitChanges();

                                dataGridView1.DataSource = invoiceProperties.Select(x => new { x.ItemStockCode, x.ProductName, x.ItemAmount, x.ReadingAmount }).ToList();
                                dataGridView1.Visible = true;
                                AutoSize();
                            }
                            else
                            {
                                isPacking = invoiceControls.FirstOrDefault().IsPacking ?? false;

                                if (isPacking)
                                {
                                    MessageBox.Show("Bu fatura daha önce paketlendi.");
                                }
                                else
                                {
                                    dataGridView1.DataSource = invoiceControls.OrderByDescending(y => y.EntryDate).Select(x => new { x.ItemStockCode, x.ItemName, x.ItemAmount, x.ReadingAmount }).ToList();
                                    dataGridView1.Visible = true;
                                }
                            }
                        }
                    }
                }

                //TextBoxProduct.Focus();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Lütfen Fatura Numarasını Kontrol Edin.Detay: " + exc.Message);
                //Logger.CommonExceptionAction(exc);
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {
            FillCargoList();




            if (PrinterSettings.InstalledPrinters.Count <= 0)
            {
                MessageBox.Show("Printer not found!");
                return;
            }

            //Get all available printers and add them to the combo box  
            foreach (String printer in PrinterSettings.InstalledPrinters)
            {

                comboBox1.Items.Add(printer.ToString());
            }
            //opening the subkey  
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\ArkadasSettings");

            //if it does exist, retrieve the stored values  
            if (key != null)
            {
                comboBox1.SelectedItem = key.GetValue("SelectPrinter");
                key.Close();
            }
        }
        private void button1_Click_1(object sender, EventArgs e)
        {

            blackbox.Visible = true;
            if (comboBox1.SelectedItem != null)
            {

                WebOrdersNew webOrder = new WebOrdersNew();
                if (checkBox1.Checked)
                {
                    int invoiceId = db.WebInvoiceInformations.Where(x => x.InvoiceNumber == invoicenumberlabel.Text).Select(x => x.Id).FirstOrDefault();
                    webOrder = (from woc in db.WebOrderComponentsNews
                                from wo in db.WebOrdersNews
                                where
                                    woc.IDInvoice == invoiceId.ToString() &&
                                    wo.ID == woc.IDOrder
                                select wo).FirstOrDefault();
                }
                else
                {
                    webOrder = (from woc in db.WebOrderComponentsNews
                                from wo in db.WebOrdersNews
                                where
                                    woc.IDInvoice == invoicenumberlabel.Text.ToString() &&
                                    wo.ID == woc.IDOrder
                                select wo).FirstOrDefault();
                }
                if (ComboBoxCargo.SelectedIndex == 7)
                {
                    blackbox.Visible = false;
                }
                if (ComboBoxCargo.SelectedIndex == 7 && webOrder.OrderSystemsId == null)
                {
                    int invoiceno = 0;
                    if (checkBox1.Checked)
                    {
                        invoiceno = db.WebInvoiceInformations.Where(x => x.InvoiceNumber == invoicenumberlabel.Text).Select(x => x.Id).FirstOrDefault();
                    }
                    else
                    {
                        invoiceno = Convert.ToInt32(invoicenumberlabel.Text);
                    }
                    OrderArasCargo arasc = db.OrderArasCargos.Where(x => x.InvoiceId == invoiceno).FirstOrDefault();
                    RawPrinterHelper.SendStringToPrinter(comboBox1.SelectedItem.ToString(), arasc.KargoZPL);
                }
                else
                {
                    //Create a PrintDocument object  
                    PrintDocument pd = new PrintDocument();
                    //Set PrinterName as the selected printer in the printers list  
                    pd.PrinterSettings.PrinterName =
                    comboBox1.SelectedItem.ToString();
                    //Add PrintPage event handler  
                    pd.PrintPage += new PrintPageEventHandler(printDocument1_PrintPage);
                    //Print the document  
                    pd.Print();
                }
            }
            else
            {
                MessageBox.Show("Lütfen yazıcı seçiniz ediniz.");
            }

        }
        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            float x = 10;// e.MarginBounds.Left;
            float y = 10;// e.MarginBounds.Top;
            Bitmap bmp = new Bitmap(panel1.Width, panel1.Height);

            panel1.DrawToBitmap(bmp, new Rectangle(0, 0, 360, 650));
            //e.Graphics.DrawImage((Image)bmp, x, y);
            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            e.Graphics.PageScale = 3f;
            e.Graphics.DrawImage((Image)bmp, new Rectangle(-1, 0, 33, 33));

        }
        private void printDocument2_PrintPage(object sender, PrintPageEventArgs e)
        {
            float x = 10;// e.MarginBounds.Left;
            float y = 10;// e.MarginBounds.Top;
            Bitmap bmp = new Bitmap(panel2.Width, panel2.Height);

            panel2.DrawToBitmap(bmp, new Rectangle(0, 0, panel2.Width, panel2.Height));
            //e.Graphics.DrawImage((Image)bmp, x, y);
            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            e.Graphics.PageScale = 3f;
            e.Graphics.DrawImage((Image)bmp, new Rectangle(0, 0, 45, 33));
            ////Get the Graphics object  




        }
        private void buttonAdd_Click(object sender, EventArgs e)
        {
            try
            {
                bool isCompleted = true;
                int tempIDInvoice = IDInvoice;

                WebInvoiceControl invoiceControl = (from i in db.WebInvoiceControls
                                                    where
                                                       i.IDInvoice == tempIDInvoice &&
                                                       i.ItemStockCode == textBoxProduct.Text.Trim()
                                                    select i).FirstOrDefault();

                List<WebInvoiceControl> tempInvoiceControl = (from i in db.WebInvoiceControls
                                                              where
                                                                 i.IDInvoice == tempIDInvoice &&
                                                                 i.ItemStockCode == textBoxProduct.Text.Trim()
                                                              select i).ToList();
                if (tempInvoiceControl.Count == 1)
                {



                    if (String.IsNullOrEmpty(textBoxProduct.Text.Trim()))
                    {
                        MessageBox.Show("Lütfen bir ürün okutunuz.");
                    }
                    else
                    {
                        if (invoiceControl == null)
                        {
                            MessageBox.Show("Okuttuğunuz ürün bu faturada yer almamaktadır.");
                        }
                        else
                        {
                            if (invoiceControl.ItemAmount == invoiceControl.ReadingAmount)
                            {
                                MessageBox.Show("Bu üründen daha fazla okutamazsınız.");
                            }
                            else
                            {
                                invoiceControl.ReadingAmount++;
                                invoiceControl.EntryDate = DateTime.Now;

                                //if (admininfo != null)
                                //    invoiceControl.Operator = admininfo.Name + " " + admininfo.Surname;
                                //else
                                invoiceControl.Operator = "Windows Form";

                                db.SubmitChanges();

                                dataGridView1.DataSource = (from i in db.WebInvoiceControls where i.IDInvoice == tempIDInvoice select i).OrderByDescending(x => x.EntryDate).Select(x => new { x.ItemStockCode, x.ItemName, x.ItemAmount, x.ReadingAmount }).ToList();

                            }
                        }
                    }
                }
                else if (tempInvoiceControl.Count > 1)
                {
                    if (String.IsNullOrEmpty(textBoxProduct.Text.Trim()))
                    {
                        MessageBox.Show("Lütfen bir ürün okutunuz.");
                    }
                    else
                    {
                        if (tempInvoiceControl == null)
                        {
                            MessageBox.Show("Okuttuğunuz ürün bu faturada yer almamaktadır.");
                        }
                        else
                        {

                            if ((tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim()).FirstOrDefault().ItemAmount == tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim()).FirstOrDefault().ReadingAmount) && (tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault() == null))
                            {
                                MessageBox.Show("Bu üründen daha fazla okutamazsınız.");
                            }
                            else
                            {
                                //if (admininfo != null)
                                //    tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault().Operator = admininfo.Name + " " + admininfo.Surname;
                                //else
                                invoiceControl.Operator = "Debug Mod";
                                tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault().EntryDate = DateTime.Now;
                                tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault().ReadingAmount++;

                                db.SubmitChanges();

                                dataGridView1.DataSource = (from i in db.WebInvoiceControls where i.IDInvoice == tempIDInvoice select i).OrderByDescending(x => x.EntryDate).Select(x => new { x.ItemStockCode, x.ItemName, x.ItemAmount, x.ReadingAmount }).ToList();

                            }
                        }
                    }

                }
                else if (tempInvoiceControl.Count() == 0)
                {
                    if (String.IsNullOrEmpty(textBoxProduct.Text.Trim()))
                    {
                        MessageBox.Show("Lütfen bir ürün okutunuz.");
                    }
                    else
                    {
                        if (tempInvoiceControl.Count() == 0)
                        {
                            MessageBox.Show("Okuttuğunuz ürün bu faturada yer almamaktadır.");
                        }
                        else
                        {

                            if ((tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim()).FirstOrDefault().ItemAmount == tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim()).FirstOrDefault().ReadingAmount) && (tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault() == null))
                            {
                                MessageBox.Show("Bu üründen daha fazla okutamazsınız.");
                            }
                            else
                            {
                                //if (admininfo != null)
                                //    tempInvoiceControl.Where(x => x.ItemStockCode == TextBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault().Operator = admininfo.Name + " " + admininfo.Surname;
                                //else
                                invoiceControl.Operator = "Debug Mod";
                                tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault().EntryDate = DateTime.Now;
                                tempInvoiceControl.Where(x => x.ItemStockCode == textBoxProduct.Text.Trim() && x.ReadingAmount == 0).FirstOrDefault().ReadingAmount++;

                                db.SubmitChanges();

                                dataGridView1.DataSource = (from i in db.WebInvoiceControls where i.IDInvoice == tempIDInvoice select i).OrderByDescending(x => x.EntryDate).Select(x => new { x.ItemStockCode, x.ItemName, x.ItemAmount, x.ReadingAmount }).ToList();

                            }
                        }
                    }

                }



                List<WebInvoiceControl> tempInvoiceControls = (
                                                               from i in db.WebInvoiceControls
                                                               where
                                                                   i.IDInvoice == tempIDInvoice
                                                               select i
                                                           ).ToList();

                foreach (WebInvoiceControl item in tempInvoiceControls)
                {
                    if (item.ItemAmount != 0)
                    {
                        if (item.ItemAmount != item.ReadingAmount)
                        {
                            isCompleted = false;
                        }
                    }
                    else
                    {
                        isCompleted = false;
                    }
                }

                if (isCompleted)
                {
                    MessageBox.Show("Tüm ürünler okutuldu. Ürünleri paketleyebilirsiniz.");
                }

                textBoxProduct.Text = String.Empty;
                textBoxProduct.Focus();
            }
            catch (Exception exc)
            {
                //Logger.CommonExceptionAction(exc);
            }
        }
        public void FillCargoList()
        {
            List<WebCargoFirm> cargoList = (from i in db.WebCargoFirms
                                            select i).ToList();

            ComboBoxCargo.Items.Clear();
            WebCargoFirm firm = new WebCargoFirm();
            firm.Id = 0;
            firm.CargoName = "Kargo Seçiniz";
            cargoList.Add(firm);
            ComboBoxCargo.DataSource = cargoList.OrderBy(x => x.Id).ToList();
            ComboBoxCargo.DisplayMember = "CargoName";
            ComboBoxCargo.ValueMember = "Id";

        }
        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBox1.SelectedItem != null)
                {
                    ArkadasWebSiteDataContext webConn = new ArkadasWebSiteDataContext();
                    List<WebOrderComponentsNew> compCheckList = webConn.WebOrderComponentsNews.Where(y => y.IDInvoice == IDInvoice.ToString()).ToList();
                    WebOrdersNew checkOrder = webConn.WebOrdersNews.Where(x => x.ID == compCheckList[0].IDOrder).FirstOrDefault();
                    webConn.Dispose();

                    if (checkOrder.IsOpen == true && compCheckList.Where(x => x.IsCancelled == false).FirstOrDefault() != null)
                    {
                        bool isCompleted = true, isPacking = true;
                        int tempIDInvoice = IDInvoice;
                        List<WebInvoiceControl> invoiceControls = (
                                                  from i in db.WebInvoiceControls
                                                  where
                                                      i.IDInvoice == tempIDInvoice &&
                                                      i.ItemAmount > 0
                                                  select i
                                              ).ToList();

                        foreach (WebInvoiceControl item in invoiceControls)
                        {
                            if (item.ItemAmount != 0)
                            {
                                if (item.ItemAmount != item.ReadingAmount)
                                {
                                    isCompleted = false;
                                }
                            }
                            else
                            {
                                isCompleted = false;
                            }
                        }
                        if (isCompleted)
                        {
                            isPacking = invoiceControls.FirstOrDefault().IsPacking ?? false;

                            if (isPacking)
                            {
                                MessageBox.Show("Bu fatura daha önce paketlendi.");
                            }
                            else
                            {
                                if ((
                                       String.IsNullOrEmpty(PackingReal.Text) ||
                                       Convert.ToInt32(PackingReal.Text) == 0
                                   ) &&
                                   (
                                       String.IsNullOrEmpty(PackingDecimal.Text) ||
                                       Convert.ToInt32(PackingDecimal.Text) == 0
                                   ))
                                {
                                    MessageBox.Show("Lütfen sıfırdan büyük bir paket ağırlığı giriniz.");
                                }
                                else
                                {
                                    if (Convert.ToInt32(PackingReal.Text) > 5)
                                    {
                                        DialogResult dialogResult = MessageBox.Show("Paket ağırlığı 5 Kg dan fazladır. Devam Etmek istiyormusunuz?", "Uyarı!!!", MessageBoxButtons.YesNo);
                                        if (dialogResult == DialogResult.Yes)
                                        {
                                            decimal realNumber = String.IsNullOrEmpty
                                                         (
                                                            PackingReal.Text.Trim()) ? 0m
                                                            :
                                                            Convert.ToDecimal(PackingReal.Text.Trim()
                                                         );

                                            decimal decimalNumber = String.IsNullOrEmpty
                                                                    (
                                                                        PackingDecimal.Text.Trim()) ? 0m
                                                                        :
                                                                        Convert.ToDecimal(PackingDecimal.Text.Trim()
                                                                    );
                                            decimal tempDecimalNumber = 0, tempWeight = 0;

                                            if (decimalNumber < 10)
                                            {
                                                tempDecimalNumber = (decimalNumber * 100) / 1000;
                                            }
                                            else if (decimalNumber < 100)
                                            {
                                                tempDecimalNumber = (decimalNumber * 10) / 1000;
                                            }
                                            else
                                            {
                                                tempDecimalNumber = decimalNumber / 1000;
                                            }

                                            tempWeight = realNumber + tempDecimalNumber;

                                            WebOrdersNew webOrder = (from woc in db.WebOrderComponentsNews
                                                                     from wo in db.WebOrdersNews
                                                                     where
                                                                         woc.IDInvoice == tempIDInvoice.ToString() &&
                                                                         wo.ID == woc.IDOrder
                                                                     select wo).FirstOrDefault();

                                            decimal totalPrice = getOrderTotalPrice(webOrder);

                                            WebUserDetail userDetail = (from i in db.WebUserDetails
                                                                        where
                                                                             i.Id == webOrder.IDUser
                                                                        select i).FirstOrDefault();

                                            int upsCargoID = 1;
                                            int yurticiCargoID = 2;
                                            int mngCargoID = 6;
                                            if (webOrder.IDCargo == 0)
                                            {
                                                MessageBox.Show("Lütfen bir kargo firması seçip, kargo değiştir e tıklayınız.");
                                            }
                                            else
                                            {

                                                foreach (WebInvoiceControl item in invoiceControls)
                                                {
                                                    item.PackingWeight = tempWeight;
                                                    item.IsPacking = true;

                                                    item.PackingOperator = "Windows Form";

                                                    item.PackingDate = DateTime.Now;
                                                }

                                                List<string> stockCodes = (from i in invoiceControls
                                                                           select i.ItemStockCode).ToList();

                                                List<WebOrderComponentsNew> webOrderComponentList =
                                                (
                                                    from i in db.WebOrderComponentsNews
                                                    where
                                                        i.IDInvoice == tempIDInvoice.ToString() &&
                                                        stockCodes.Contains(i.ItemStockCode)
                                                    select i
                                                ).ToList();

                                                //string packingDate = (invoiceControls.FirstOrDefault().PackingDate ?? DateTime.Now).ToShortDateString();

                                                AddPackingInfo(webOrderComponentList, tempIDInvoice);

                                                db.SubmitChanges();
                                                #region PTT Entegrasyonu
                                                if (ComboBoxCargo.SelectedIndex == 6 && webOrder.OrderSystemsId == null)//demekki ptt ve iç web
                                                {
                                                    string cargoBarcode = new SendCargo().SendCargoPTT(webOrder.OrderNumber);
                                                    byte[] barcodeImageNew = new BarcodeGenerator().CreateBarcode(cargoBarcode, 100, 2);
                                                    using (MemoryStream ms = new MemoryStream(barcodeImageNew))
                                                    {
                                                        pictureBox2.Image = Image.FromStream(ms);
                                                    }

                                                    webOrder.Barkod = cargoBarcode;
                                                    db.SubmitChanges();
                                                }
                                                #endregion
                                                #region Yurtiçi Kargo Entegrasyonu
                                                //if (ComboBoxCargo.SelectedIndex == 2 && webOrder.OrderSystemsId == null)//demekki yurtiçi ve iç web
                                                //{
                                                //    string cargoBarcode = new SendCargo().SendCargoYK(webOrder.OrderNumber);
                                                //    if (cargoBarcode.Length > 8)
                                                //    {
                                                //        MessageBox.Show("Paketleme işlemi tamamlandı.Fakat Yurtiçi Kargo sistemine yükelenemedi. IT Departmanına başvurunuz. Hata : " + cargoBarcode);
                                                //    }
                                                //    else
                                                //    {
                                                //        byte[] barcodeImageNew = new BarcodeGenerator().CreateBarcode(cargoBarcode, 100, 2);
                                                //        using (MemoryStream ms = new MemoryStream(barcodeImageNew))
                                                //        {
                                                //            pictureBox1.Image = Image.FromStream(ms);
                                                //        }

                                                //        webOrder.Barkod = cargoBarcode;
                                                //        db.SubmitChanges();
                                                //    }
                                                //}
                                                #endregion
                                                #region Aras Kargo Entegrasyonu
                                                if (ComboBoxCargo.SelectedIndex == 7 && webOrder.OrderSystemsId == null)//demekki Aras ve iç web
                                                {
                                                    string invoiceId = webOrderComponentList.Where(x => x.IDInvoice != null).Select(x => x.IDInvoice).FirstOrDefault();
                                                    WebInvoiceInformation invoice = db.WebInvoiceInformations.Where(x => x.Id == Convert.ToInt32(invoiceId)).FirstOrDefault();
                                                    bool IsCorporate = db.WebUserDetails.Where(x => x.Id == invoice.CustomerId).Select(x => x.IsCorporate).FirstOrDefault();
                                                    if (!IsCorporate)
                                                    {
                                                        CargoBarcodeReturn rtrn = new SendCargo().SendYayincilikCargoAras(webOrder, invoice);
                                                        if (rtrn != null)
                                                        {
                                                            if (rtrn.IsSuccess == false)
                                                            {
                                                                PrintDocument pd = new PrintDocument();
                                                                pd.PrinterSettings.PrinterName = comboBox1.SelectedItem.ToString();
                                                                pd.PrintPage += new PrintPageEventHandler(printDocument3_PrintPage);
                                                                pd.Print();
                                                            }
                                                            else
                                                            {
                                                                OrderArasCargo arasc = new OrderArasCargo();
                                                                arasc.InvoiceId = Convert.ToInt32(invoiceId);
                                                                arasc.KargoZPL = rtrn.ZPL;
                                                                arasc.KargoEPL = rtrn.EPL;
                                                                db.OrderArasCargos.InsertOnSubmit(arasc);
                                                                db.SubmitChanges();
                                                            }
                                                        }
                                                    }
                                                }
                                                #endregion
                                                MessageBox.Show("Paketleme işlemi tamamlandı.");
                                                button1_Click_1(sender, e);
                                                ClearAll();
                                                //ResetAll();
                                                //}
                                            }
                                        }
                                        else if (dialogResult == DialogResult.No)
                                        {
                                            PackingReal.Focus();
                                        }
                                    }
                                    else
                                    {
                                        decimal realNumber = String.IsNullOrEmpty
                                                        (
                                                           PackingReal.Text.Trim()) ? 0m
                                                           :
                                                           Convert.ToDecimal(PackingReal.Text.Trim()
                                                        );

                                        decimal decimalNumber = String.IsNullOrEmpty
                                                                (
                                                                    PackingDecimal.Text.Trim()) ? 0m
                                                                    :
                                                                    Convert.ToDecimal(PackingDecimal.Text.Trim()
                                                                );
                                        decimal tempDecimalNumber = 0, tempWeight = 0;

                                        if (decimalNumber < 10)
                                        {
                                            tempDecimalNumber = (decimalNumber * 100) / 1000;
                                        }
                                        else if (decimalNumber < 100)
                                        {
                                            tempDecimalNumber = (decimalNumber * 10) / 1000;
                                        }
                                        else
                                        {
                                            tempDecimalNumber = decimalNumber / 1000;
                                        }

                                        tempWeight = realNumber + tempDecimalNumber;

                                        WebOrdersNew webOrder = (from woc in db.WebOrderComponentsNews
                                                                 from wo in db.WebOrdersNews
                                                                 where
                                                                     woc.IDInvoice == tempIDInvoice.ToString() &&
                                                                     wo.ID == woc.IDOrder
                                                                 select wo).FirstOrDefault();

                                        decimal totalPrice = getOrderTotalPrice(webOrder);

                                        WebUserDetail userDetail = (from i in db.WebUserDetails
                                                                    where
                                                                         i.Id == webOrder.IDUser
                                                                    select i).FirstOrDefault();

                                        int upsCargoID = 1;
                                        int yurticiCargoID = 2;
                                        int mngCargoID = 6;
                                        if (webOrder.IDCargo == 0)
                                        {
                                            MessageBox.Show("Lütfen bir kargo firması seçip, kargo değiştir e tıklayınız.");
                                        }
                                        else
                                        {

                                            foreach (WebInvoiceControl item in invoiceControls)
                                            {
                                                item.PackingWeight = tempWeight;
                                                item.IsPacking = true;

                                                item.PackingOperator = "Windows Form";

                                                item.PackingDate = DateTime.Now;
                                            }

                                            List<string> stockCodes = (from i in invoiceControls
                                                                       select i.ItemStockCode).ToList();

                                            List<WebOrderComponentsNew> webOrderComponentList =
                                            (
                                                from i in db.WebOrderComponentsNews
                                                where
                                                    i.IDInvoice == tempIDInvoice.ToString() &&
                                                    stockCodes.Contains(i.ItemStockCode)
                                                select i
                                            ).ToList();

                                            AddPackingInfo(webOrderComponentList, tempIDInvoice);

                                            db.SubmitChanges();

                                            #region Aras Kargo Entegrasyonu
                                            if (ComboBoxCargo.SelectedIndex == 7 && webOrder.OrderSystemsId == null)//demekki Aras ve iç web
                                            {
                                                string invoiceId = webOrderComponentList.Where(x => x.IDInvoice != null).Select(x => x.IDInvoice).FirstOrDefault();
                                                WebInvoiceInformation invoice = db.WebInvoiceInformations.Where(x => x.Id == Convert.ToInt32(invoiceId)).FirstOrDefault();
                                                bool IsCorporate = db.WebUserDetails.Where(x => x.Id == invoice.CustomerId).Select(x => x.IsCorporate).FirstOrDefault();
                                                if (!IsCorporate)
                                                {
                                                    CargoBarcodeReturn rtrn = new SendCargo().SendYayincilikCargoAras(webOrder, invoice);
                                                    if (rtrn != null)
                                                    {
                                                        if (rtrn.IsSuccess == false)
                                                        {
                                                            PrintDocument pd = new PrintDocument();
                                                            pd.PrinterSettings.PrinterName = comboBox1.SelectedItem.ToString();
                                                            pd.PrintPage += new PrintPageEventHandler(printDocument3_PrintPage);
                                                            pd.Print();
                                                        }
                                                        else
                                                        {
                                                            OrderArasCargo arasc = new OrderArasCargo();
                                                            arasc.InvoiceId = Convert.ToInt32(invoiceId);
                                                            arasc.KargoZPL = rtrn.ZPL;
                                                            arasc.KargoEPL = rtrn.EPL;
                                                            db.OrderArasCargos.InsertOnSubmit(arasc);
                                                            db.SubmitChanges();
                                                        }
                                                    }
                                                }
                                            }
                                            #endregion
                                            #region PTT Kargo Entegrasyonu
                                            if (ComboBoxCargo.SelectedIndex == 6 && webOrder.OrderSystemsId == null)//demekki ptt ve iç web
                                            {
                                                string cargoBarcode = new SendCargo().SendCargoPTT(webOrder.OrderNumber);
                                                byte[] barcodeImageNew = new BarcodeGenerator().CreateBarcode(cargoBarcode, 100, 2);
                                                using (MemoryStream ms = new MemoryStream(barcodeImageNew))
                                                {
                                                    pictureBox2.Image = Image.FromStream(ms);
                                                }

                                                webOrder.Barkod = cargoBarcode;
                                                db.SubmitChanges();
                                            }
                                            #endregion
                                            MessageBox.Show("Paketleme işlemi tamamlandı.");

                                            if (ComboBoxCargo.SelectedIndex == 6 && webOrder.OrderSystemsId == null)//demekki ptt ve iç web
                                            {
                                                button6_Click(sender, e);
                                            }
                                            else
                                            {
                                                button1_Click_1(sender, e);
                                            }

                                            ClearAll();
                                            //ResetAll();
                                            //}
                                        }
                                    }

                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Okutmadığız ürünler var. Lütfen kontrol ediniz.");
                            //setVisible(true);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Bu Sipariş Iptal Edilmiştir. Lütfen Kontrol Ediniz.");
                    }
                    textBoxInvoiceNumber.Focus();
                }
                else
                {
                    MessageBox.Show("Lütfen yazıcı seçiniz ediniz.");
                }

            }
            catch (Exception exc)
            {

            }
        }
        public decimal getOrderTotalPrice(WebOrdersNew order)
        {
            decimal totalamount = 0;
            decimal cargoprice = 0;
            decimal giftprice = 0;
            decimal scoreprice = 0;
            List<WebOrderComponentsNew> components = db.WebOrderComponentsNews.Where(c => c.IDOrder == order.ID).ToList();

            foreach (WebOrderComponentsNew item in components)
            {
                totalamount = totalamount + item.SalesPrice * item.ItemAmount;
            }
            cargoprice = order.CargoPrice;
            WebGiftCheck cek = (from i in db.WebGiftChecks where i.CheckID == order.CheckNumber select i).FirstOrDefault();
            if (cek != null)
                giftprice = cek.CheckAmount;
            order.UsedScore = order.UsedScore == null ? 0 : order.UsedScore;
            scoreprice = (decimal)order.UsedScore / 100;

            totalamount = totalamount - (giftprice + scoreprice);

            return totalamount;
        }

        public void AddPackingInfo(List<WebOrderComponentsNew> webOrderComponents, int tempIDInvoice)
        {
            WebOrdersNew order = db.WebOrdersNews.FirstOrDefault(w => w.ID == webOrderComponents[0].IDOrder);
            // SendMailPackingInformation(order, tempIDInvoice);

            foreach (WebOrderComponentsNew item in webOrderComponents)
            {
                item.DescriptionComponent = "Paketlendi";
            }

            List<OrderShelfMatch> matchList = new List<OrderShelfMatch>();
            HBOrderMatch HBMatch = db.HBOrderMatches.Where(x => x.PackageNumber == order.SystemCargoNo).FirstOrDefault();


            if (HBMatch != null)//Demekki paketlenemeden rafa dizilmiş.
            {

                matchList = db.OrderShelfMatches.Where(x => x.OrderId == HBMatch.HBOrderId && x.Status == 1 && x.Prefix == "HB").ToList();
            }
            else
            {
                matchList = db.OrderShelfMatches.Where(x => x.OrderId == order.ID && x.Status == 1 && x.Prefix != "HB").ToList();

            }
            if (matchList.Count > 0)
            {
                foreach (OrderShelfMatch item in matchList)
                {
                    item.Status = 2;//Rafı boşalttık
                }
                if (order.OrderSystemsId != null)
                {
                    order.IsOpen = false; // Açık siparişi kapattık
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)
        {
            MainForm mform = new MainForm();
            mform.Show();
            this.Close();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            ClearAll();
        }
        public void ClearAll()
        {
            textBoxInvoiceNumber.Text = "";
            pictureBox1.Hide();
            //TotalPrice.Text = "";
            textBoxProduct.Clear();
            PackingDecimal.Text = "0";
            PackingReal.Text = "0";
            ComboBoxCargo.SelectedItem = 0;
            CargoAdress.Clear();
            CargoAdressDumy.Clear();
            CargoName.Text = "";
            CargoNameSurname.Text = "";
            CargoNameSurnameDumy.Text = "";
            InvoiceNameSurname.Text = "";
            CargoOdemeBilgisi.Text = "";
            //InvoiceAdress.Clear();
            OdemeBilgisi.Text = "";
            Phone.Text = "";
            PhoneDumy.Text = "";
            Platform.Text = "";
            LabelOrderId.Text = "";
            dataGridView1.DataSource = "";
            //dataGridView2.DataSource = "";
            //dataGridView2.Visible = false;
        }

        private void textBoxProduct_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                buttonAdd_Click(sender, e);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                int selectedIDInvoice = IDInvoice;
                int selectedCargoID = Convert.ToInt32(ComboBoxCargo.SelectedValue);

                if (selectedCargoID != 0)
                {

                    WebOrdersNew webOrder = (from woc in db.WebOrderComponentsNews
                                             from wo in db.WebOrdersNews
                                             where
                                                 woc.IDInvoice == selectedIDInvoice.ToString() &&
                                                 wo.ID == woc.IDOrder
                                             select wo).FirstOrDefault();

                    webOrder.IDCargo = selectedCargoID;
                    db.SubmitChanges();

                    MessageBox.Show("Kargo firması değiştirildi.");
                }
                else
                {
                    MessageBox.Show("Lütfen bir kargo seçiniz.");
                }
            }
            catch (Exception exc)
            {
            }
        }








        private void textBoxInvoiceNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                button1_Click(sender, e);
                textBoxProduct.Focus();
            }

        }

        private void PackingDecimal_Click(object sender, EventArgs e)
        {
            PackingDecimal.SelectAll();
        }

        private void PackingReal_Click(object sender, EventArgs e)
        {
            PackingReal.SelectAll();

        }

        private void button6_Click(object sender, EventArgs e)
        {
            label22.Text = PackingReal.Text + "," + PackingDecimal.Text + " (gr)";
            //Create a PrintDocument object  
            PrintDocument pd = new PrintDocument();

            //Set PrinterName as the selected printer in the printers list  
            pd.PrinterSettings.PrinterName =
            comboBox1.SelectedItem.ToString();
            //Add PrintPage event handler  
            pd.PrintPage += new PrintPageEventHandler(printDocument2_PrintPage);

            //Print the document  
            pd.Print();
        }

        private void label24_Click(object sender, EventArgs e)
        {

        }

        private void button7_Click(object sender, EventArgs e)
        {
            int tempIDInvoice = IDInvoice;
            WebOrdersNew webOrder = (from woc in db.WebOrderComponentsNews
                                     from wo in db.WebOrdersNews
                                     where
                                         woc.IDInvoice == tempIDInvoice.ToString() &&
                                         wo.ID == woc.IDOrder
                                     select wo).FirstOrDefault();
            if (ComboBoxCargo.SelectedIndex == 6 && webOrder.OrderSystemsId == null)//demekki ptt ve iç web
            {
                string cargoBarcode = new SendCargo().SendCargoPTT(webOrder.OrderNumber);
                byte[] barcodeImageNew = new BarcodeGenerator().CreateBarcode(cargoBarcode, 80, 2);
                using (MemoryStream ms = new MemoryStream(barcodeImageNew))
                {
                    pictureBox1.Image = Image.FromStream(ms);
                }

                webOrder.Barkod = cargoBarcode;
                db.SubmitChanges();
            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                labelInvoiceNumber.Text = "Fatura No:";
            }
            else
            {
                labelInvoiceNumber.Text = "Fatura ID:";
            }
        }

        private void printDocument3_PrintPage(object sender, PrintPageEventArgs e)
        {

            float x = 10;// e.MarginBounds.Left;
            float y = 10;// e.MarginBounds.Top;
            Bitmap bmp = new Bitmap(panel7.Width, panel7.Height);

            panel7.DrawToBitmap(bmp, new Rectangle(0, 0, panel7.Width, panel7.Height));
            //e.Graphics.DrawImage((Image)bmp, x, y);
            e.Graphics.PageUnit = GraphicsUnit.Millimeter;
            e.Graphics.PageScale = 3f;
            e.Graphics.DrawImage((Image)bmp, new Rectangle(-1, 0, 33, 33));
        }

        private void AgirlikAl_Click(object sender, EventArgs e)
        {
            Tarti v = new Tarti();
            v.Show();

            //if (v._serialPort.IsOpen != true)
            //{
            //    v._serialPort.Open();
            //}
            //int dataLength = v._serialPort.BytesToRead;
            //byte[] data = new byte[dataLength];
            //int nbrDataRead = v._serialPort.Read(data, 0, dataLength);
            //if (nbrDataRead == 0)
            //    return;
            //string str = System.Text.Encoding.UTF8.GetString(data);

            //double number;

            //if (Double.TryParse(str, out number))
            //{
            //    v.Text = string.Format("{0:0.000}", str);
            //}
            //else
            //{
            //    var doubleArray = Regex.Split(str, @"[^0-9\.]+")
            //    .Where(c => c != "." && c.Trim() != "");

            //    string[] str1 = ((System.Collections.IEnumerable)doubleArray)
            //  .Cast<object>()
            //  .Select(x => x.ToString())
            //  .ToArray();
            //    if (str1 != null && str1.Length > 0)
            //    {
            //        v.Text = string.Format("{0:0.000}", str1[0]);
            //        var tr = v.Text.ToList();
            //    }
            //}

            string a = v.weight;
            string[] split = a.Split('.');
            PackingReal.Text = split[0].ToString();
            PackingDecimal.Text = split[1].ToString();

        }
    }
}


