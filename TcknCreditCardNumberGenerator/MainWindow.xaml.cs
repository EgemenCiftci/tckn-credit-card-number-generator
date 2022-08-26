using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace TcknCreditCardNumberGenerator
{
    public partial class MainWindow : Window
    {
        private BackgroundWorker bgw;

        public int? bin
        {
            get => (int?)GetValue(binProperty);
            set => SetValue(binProperty, value);
        }
        public static readonly DependencyProperty binProperty = DependencyProperty.Register("bin", typeof(int?), typeof(MainWindow), new UIPropertyMetadata(null));

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            bgw = new BackgroundWorker();
            bgw.DoWork += new DoWorkEventHandler(bgw_DoWork);
            bgw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw_RunWorkerCompleted);
            bgw.RunWorkerAsync(bin == null ? "" : bin.ToString());

            SetIsBusy(true);
        }

        private void bgw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            lbxTckns.ItemsSource = ((List<string>[])e.Result)[0];
            lbxCreditCardNumbers.ItemsSource = ((List<string>[])e.Result)[1];

            SetIsBusy(false);
        }

        private void bgw_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> tckns = GenerateTckns();
            List<string> creditCardNumbers = GenerateCreditCardNumbers((string)e.Argument);

            e.Result = new List<string>[] { tckns, creditCardNumbers };
        }

        private List<string> GenerateTckns()
        {
            List<string> tckns = new List<string>();
            while (tckns.Count < 5)
            {
                string number = GenerateRandomTckn().ToString();
                if (!tckns.Contains(number) && ValidateTckn(number))
                {
                    tckns.Add(number);
                }
            }

            return tckns;
        }

        private List<string> GenerateCreditCardNumbers(string bin)
        {
            List<string> creditCardNumbers = new List<string>();
            while (creditCardNumbers.Count < 5)
            {
                string number = GenerateRandomNumber(16, bin);
                if (number.StartsWith(bin) && !creditCardNumbers.Contains(number) && ValidateCreditCardNumber(number))
                {
                    creditCardNumbers.Add(number);
                }
            }
            return creditCardNumbers;
        }

        private void btnGenerate_Click(object sender, RoutedEventArgs e)
        {
            bgw.RunWorkerAsync(bin == null ? "" : bin.ToString());

            SetIsBusy(true);
        }

        private void lbxTckns_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxTckns.SelectedItem != null)
            {
                Clipboard.Clear();
                Clipboard.SetText((string)lbxTckns.SelectedItem);
            }
        }

        private void lbxCreditCardNumbers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbxCreditCardNumbers.SelectedItem != null)
            {
                Clipboard.Clear();
                Clipboard.SetText((string)lbxCreditCardNumbers.SelectedItem);
            }
        }

        private bool ValidateTckn(string tckn)
        {
            if (tckn.Length != 11)
            {
                return false;
            }

            long ATCNO, BTCNO, TcNo;
            long C1, C2, C3, C4, C5, C6, C7, C8, C9, Q1, Q2;

            TcNo = long.Parse(tckn);

            ATCNO = TcNo / 100;
            BTCNO = TcNo / 100;

            C1 = ATCNO % 10;
            ATCNO /= 10;
            C2 = ATCNO % 10;
            ATCNO /= 10;
            C3 = ATCNO % 10;
            ATCNO /= 10;
            C4 = ATCNO % 10;
            ATCNO /= 10;
            C5 = ATCNO % 10;
            ATCNO /= 10;
            C6 = ATCNO % 10;
            ATCNO /= 10;
            C7 = ATCNO % 10;
            ATCNO /= 10;
            C8 = ATCNO % 10;
            ATCNO /= 10;
            C9 = ATCNO % 10;
            Q1 = (10 - ((((C1 + C3 + C5 + C7 + C9) * 3) + C2 + C4 + C6 + C8) % 10)) % 10;
            Q2 = (10 - ((((C2 + C4 + C6 + C8 + Q1) * 3) + C1 + C3 + C5 + C7 + C9) % 10)) % 10;

            return (BTCNO * 100) + (Q1 * 10) + Q2 == TcNo;

        }

        private bool ValidateCreditCardNumber(string creditCardNumber)
        {
            int[] DELTAS = new int[] { 0, 1, 2, 3, 4, -4, -3, -2, -1, 0 };
            int checksum = 0;
            char[] chars = creditCardNumber.ToCharArray();
            for (int i = chars.Length - 1; i > -1; i--)
            {
                int j = chars[i] - 48;
                checksum += j;
                if (((i - chars.Length) % 2) == 0)
                {
                    checksum += DELTAS[j];
                }
            }

            return (checksum % 10) == 0;
        }

        private string GenerateRandomNumber(int lenght, string startsWith)
        {
            string number = startsWith;
            Random r = new Random();
            for (int i = startsWith.Length; i < lenght; i++)
            {
                number += r.Next(10);
            }
            return number;
        }

        private void SetIsBusy(bool isBusy)
        {
            if (isBusy)
            {
                grdBusy.Visibility = Visibility.Visible;
                pbrBusy.Visibility = Visibility.Visible;
                tbkBusy.Visibility = Visibility.Visible;
            }
            else
            {
                grdBusy.Visibility = Visibility.Hidden;
                pbrBusy.Visibility = Visibility.Hidden;
                tbkBusy.Visibility = Visibility.Hidden;
            }
        }

        private long GenerateRandomTckn()
        {
            Random r = new Random();
            byte[] buf = new byte[8];
            r.NextBytes(buf);
            long longRand = BitConverter.ToInt64(buf, 0);

            return Math.Abs(longRand % (99999999999 - 10000000000)) + 10000000000;
        }
    }
}
