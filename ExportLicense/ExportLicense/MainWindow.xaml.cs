using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ExportLicense
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            combo.SelectedIndex = 0;
        }
        
        // Model
        public string SQLServer { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Database { get; set; }
        public SQLType ConnectionType { get; set; }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string table;
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder();
                builder.DataSource = SQLServer;
                if (string.IsNullOrEmpty(Database))
                {
                    builder.InitialCatalog = "master";
                    table = "[$ndo$srvproperty]";
                }
                else
                {
                    builder.UserID = UserName;
                    builder.Password = Password;
                    builder.InitialCatalog = Database;
                    table = "[$ndo$dbproperty]";
                }
                if (ConnectionType == SQLType.Windows)
                    builder.IntegratedSecurity = true;
                SqlConnection Con = new SqlConnection(builder.ConnectionString);
                Con.Open();
                SqlCommand cmd = new SqlCommand("SELECT License from "+table, Con);
                SqlDataReader data = cmd.ExecuteReader();
                while (data.Read())
                {
                    MemoryStream ms = new MemoryStream();
                    data.GetStream(0).CopyTo(ms);
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "License|*.flf";
                    if (sfd.ShowDialog() == true)
                    {
                        FileStream fs = new FileStream(sfd.FileName, FileMode.CreateNew);
                        ms.Position = 0;
                        ms.CopyTo(fs);
                        fs.Close();
                        MessageBox.Show("Succes, exported as " + sfd.FileName);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }
    }
    public enum SQLType
    {
        Windows = 0,
        Database = 1
    }
}
