using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using PostBoy.Helpers;
using PostBoy.Models;

namespace PostBoy
{
    public partial class MainWindow : Window
    {
        private string response_content_type = String.Empty;
        private WsdlRoot? wsdl_obj;

        public MainWindow()
        {
            InitializeComponent();
        }

        void btnSendClick(object sender, RoutedEventArgs e)
        {
            void Request()
            {
                response_content_type = String.Empty;
                tblResponseHeaderLeft.Text = String.Empty;
                tblResponseBodyLeft.Text = String.Empty;
                tbxResponseHeader.Text = String.Empty;
                tbxResponseBody.Text = String.Empty;

                var response = HttpHelper.Request(
                    method: cbxMethod.Text,
                    url: tbxUrl.Text,
                    header: tbxRequestHeader.Text,
                    content_type: cbxContentType.Text,
                    charset: cbxCharset.Text,
                    body: tbxRequestBody.Text);

                response_content_type = response.content_type;
                tblResponseHeaderLeft.Text = $"HTTP: {response.status}";
                tblResponseBodyLeft.Text = response_content_type;
                tbxResponseHeader.Text = response.header;
                tbxResponseBody.Text = response.body;
            }
            MakeAction(Request);
        }

        void tgbResponseBodyWrapChecked(object sender, RoutedEventArgs e)
        {
            tbxResponseBody.TextWrapping = TextWrapping.Wrap;
            tbxResponseBody.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        }

        void tgbResponseBodyWrapUnchecked(object sender, RoutedEventArgs e)
        {
            tbxResponseBody.TextWrapping = TextWrapping.NoWrap;
            tbxResponseBody.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
        }

        void btnRequestBodyBeautifyClick(object sender, RoutedEventArgs e)
        {
            tbxRequestBody.Text = Beautify(cbxContentType.Text, tbxRequestBody.Text);
        }

        void btnResponseBodyBeautifyClick(object sender, RoutedEventArgs e)
        {
            if (!String.IsNullOrWhiteSpace(response_content_type))
            {
                tbxResponseBody.Text = Beautify(response_content_type, tbxResponseBody.Text);
            }
        }

        void btnLogClearClick(object sender, RoutedEventArgs e)
        {
            tbxLog.Clear();
            tbiLog.ClearValue(Control.BackgroundProperty);
        }

        void btnWsdlParseClick(object sender, RoutedEventArgs e)
        {
            void Process()
            {
                cbxWsdlOperation.Items.Clear();
                cbxWsdlOperation.IsEnabled = false;
                var actions = WsdlParser.Parse(tbxWsdlContent.Text, ref wsdl_obj);
                if (actions.Length > 0)
                {
                    var items = new string[actions.Length + 1];
                    items[0] = "(select)";
                    Array.Copy(actions, 0, items, 1, actions.Length);
                    cbxWsdlOperation.ItemsSource = items;
                    cbxWsdlOperation.SelectedIndex = 0;
                    cbxWsdlOperation.IsEnabled = true;
                }
            }
            MakeAction(Process);
        }

        void cbxWsdlOperationSelectionChanged(object sender, RoutedEventArgs e)
        {
            if ((wsdl_obj != null) && (cbxWsdlOperation.IsEnabled) && (cbxWsdlOperation.SelectedIndex > 0))
            {
                string soap_action;
                (tbxRequestBody.Text, soap_action) = WsdlParser.Build(wsdl_obj, cbxWsdlOperation.Text, cbxCharset.Text);
            }
        }

        private string Beautify(string content_type, string input)
        {
            try
            {
                if (content_type.IndexOf("/json") >= 0)
                {
                    return Beautifier.Json(input);
                }
                else if (content_type.IndexOf("/xml") >= 0)
                {
                    return Beautifier.Xml(input);
                }
                else
                {
                    return input;
                }
            }
            catch (Exception exception)
            {
                WriteLog(exception.Message);
                return input;
            }
        }

        private void MakeAction(Action action)
        {
            IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                action();
            }
            catch (Exception exception)
            {
                WriteLog(exception.Message);
            }

            Mouse.OverrideCursor = null;
            IsEnabled = true;
        }

        private void WriteLog(string log_text)
        {
            tbxLog.AppendText($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {log_text}\n");
            tbxLog.ScrollToEnd();
            tbiLog.Background = Brushes.Yellow;
        }
    }
}
