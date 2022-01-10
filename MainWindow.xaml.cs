using System;
using System.Collections.Generic;
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
using System.IO;
using System.Data.SQLite;
using System.Data;

namespace kacperfilipbazydanych
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        const string folderBazyDanych = @"C:\bazy\test_sqlite\";
        const string plikBazyDanych = "test.sqlite";

        string strConnection = @"Data Source=" + folderBazyDanych + plikBazyDanych + ";Version=3;";
        SQLiteConnection cn;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void tw_baza_Click(object sender, RoutedEventArgs e)
        {
            CreateSQLiteDatabaseFile();
        }

        private void tw_tabela_Click(object sender, RoutedEventArgs e)
        {
            CreateSQLiteTables();
        }

        private void dodaj_rekord_Click(object sender, RoutedEventArgs e)
        {
            AddNewTask();   
        }


        private void usun_Click(object sender, RoutedEventArgs e)
        {
            RemoveSelectedTask(num_usun.Text);
        }

        private void odczytaj_jeden_Click(object sender, RoutedEventArgs e)
        {
            readOne(num_usun.Text);
        }


        private void CreateSQLiteDatabaseFile()
        {

            DirectoryInfo di = new DirectoryInfo(folderBazyDanych);

            if (!di.Exists)
            {
                try
                {
                    di.Create();
                    MessageBox.Show("Utworzono folder bazy danych", "Informacja");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.HResult + "\n" + ex.Message, "Błąd programu");
                }
            }

            FileInfo fi = new FileInfo(folderBazyDanych + plikBazyDanych);

            if (!fi.Exists)
            {
                //utworzenie pliku bazy danych SQLite
                SQLiteConnection.CreateFile(folderBazyDanych + plikBazyDanych);

                MessageBox.Show("Utworzono bazę danych");
            }
            else
            {
                MessageBox.Show("Istnieje już plik bazy danych");
            }

        }

        private void CreateSQLiteTables()
        {
            using (cn = new SQLiteConnection(strConnection))
            {
                string sql = "CREATE TABLE 'todoevents'('id' INTEGER PRIMARY KEY AUTOINCREMENT, 'date' TEXT, 'title' TEXT, 'description' TEXT);";

                Console.WriteLine(sql);

                try
                {
                    cn.Open();
                    SQLiteCommand cmd = new SQLiteCommand(sql, cn);

                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Utworzono tabelę w bazie danych", "Informacja");
                    cn.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.HResult + "\n" + ex.Message, "Błąd programu");
                }
            }
        }

        private void AddNewTask()
        {


            if (tb_tytul.Text.Length > 0 && tb_opis.Text.Length > 0)
            {
                using (cn = new SQLiteConnection(strConnection))
                {
                    cn.Open();

                    string sql = "INSERT INTO todoevents(date, title, description) VALUES(@param1, @param2, @param3)";

                    SQLiteParameter param1 = new SQLiteParameter("param1", DbType.String);
                    SQLiteParameter param2 = new SQLiteParameter("param2", DbType.String);
                    SQLiteParameter param3 = new SQLiteParameter("param3", DbType.String);

                    SQLiteCommand cmd = new SQLiteCommand(sql, cn);
                    cmd.Parameters.Add(param1);
                    cmd.Parameters.Add(param2);
                    cmd.Parameters.Add(param3);

                    param1.Value = DateTime.Now.ToString();
                    param2.Value = tb_tytul.Text;
                    param3.Value = tb_opis.Text;

                    try
                    {
                        cmd.ExecuteNonQuery();

                        tb_tytul.Text = string.Empty;
                        tb_opis.Text = string.Empty;

                        RefreshListView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.HResult + "\n" + ex.Message, "Błąd programu");
                    }
                }
            }
            else
            {
                MessageBox.Show("Niepoprawne dane", "Informacja");
            }
        }

        private void RefreshListView()
        {
            using (cn = new SQLiteConnection(strConnection))
            {
                try
                {

                    cn.Open();

                    string Komenda_SQL = "SELECT * FROM todoevents";
                    SQLiteCommand Zapytanie_do_bazy = new SQLiteCommand(Komenda_SQL, cn);
                    Zapytanie_do_bazy.ExecuteNonQuery();
                    SQLiteDataAdapter pobrane_dane = new SQLiteDataAdapter(Zapytanie_do_bazy);
                    DataTable dt = new DataTable("todoevents");
                    pobrane_dane.Fill(dt); // zapisanie danych w tabeli dt
                    dataGrid1.ItemsSource = dt.DefaultView;
                    pobrane_dane.Update(dt);

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.HResult + "\n" + ex.Message, "Błąd programu");

                }
                cn.Close();
            }

        }

        private void RemoveSelectedTask(string num)
        {
            using (cn = new SQLiteConnection(strConnection))
            {
                SQLiteParameter param1 = new SQLiteParameter("param1", DbType.Int16);

                string sql = "DELETE FROM todoevents WHERE id=@param1";

                SQLiteCommand cmd = new SQLiteCommand(sql, cn);
                cmd.Parameters.Add(param1);

                param1.Value = num;

                cn.Open();

                try
                {
                    cmd.ExecuteNonQuery();

                    RefreshListView();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.HResult + "\n" + ex.Message, "Błąd programu");
                }

            }
        }

        private void readOne(string num)
        {
            using (cn = new SQLiteConnection(strConnection))
            {
                SQLiteParameter param1 = new SQLiteParameter("param1", DbType.Int16);

                string sql = "SELECT title, description FROM todoevents WHERE id=@param1";

                SQLiteCommand cmd = new SQLiteCommand(sql, cn);

                cmd.Parameters.Add(param1);

                param1.Value = num;
                cn.Open();

                using SQLiteDataReader rdr = cmd.ExecuteReader();

                tb_tytul.Text = $"{rdr.GetName(0)}"; // tytuł okna
                tb_opis.Text = $"{rdr.GetName(1)}";
                rdr.Read();

                tb_tytul.Text = $@"{rdr.GetString(0)}";
                tb_opis.Text = $@"{rdr.GetString(1)}";

            

            }
        }

    }
}
