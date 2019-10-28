using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

using PostBoy.Helpers;
using PostBoy.Models;

namespace PostBoy
{
    public partial class MainWindow : Window
    {
        private string app_version;
        private string response_content_type = String.Empty;
        private WsdlParser? wsdl_parser;

        public MainWindow()
        {
            InitializeComponent();
            app_version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? String.Empty;
            this.Title = $"PostBoy v{app_version}";
        }

        void btnGoClick(object sender, RoutedEventArgs e)
        {
            void _Action()
            {
                string? method = null;
                string? url = null;
                string? request_header = null;
                string? request_body = null;
                string? content_type = null;
                string? charset = null;

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    response_content_type = String.Empty;
                    method = cbxMethod.Text;
                    url = tbxUrl.Text;
                    request_header = tbxRequestHeader.Text;
                    request_body = tbxRequestBody.Text;
                    content_type = cbxContentType.Text;
                    charset = cbxCharset.Text;
                }));

                var response = HttpHelper.Request(
                    method: method!,
                    url: url!,
                    header: request_header!,
                    content_type: content_type!,
                    charset: charset!,
                    body: request_body);

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    response_content_type = response.content_type;
                    tblResponseHeaderLeft.Text = $"HTTP: {response.status}";
                    tblResponseBodyLeft.Text = response_content_type;
                    tbxResponseHeader.Text = response.header;
                    tbxResponseBody.Text = response.body;
                }));
            }

            tblResponseHeaderLeft.Text = String.Empty;
            tblResponseBodyLeft.Text = String.Empty;
            tbxResponseHeader.Text = String.Empty;
            tbxResponseBody.Text = String.Empty;

            MakeAction(_Action);
        }

        void btnSaveClick(object sender, RoutedEventArgs e)
        {
            var state = new State()
            {
                config = new State.Config()
                {
                    version = app_version
                },
                request = new State.Transaction()
                {
                    method = cbxMethod.Text,
                    url = tbxUrl.Text,
                    content_type = cbxContentType.Text,
                    charset = cbxCharset.Text,
                    header = tbxRequestHeader.Text,
                    body = tbxRequestBody.Text
                },
                response = new State.Transaction()
                {
                    content_type = response_content_type,
                    header = tbxResponseHeader.Text,
                    body = tbxResponseBody.Text
                }
            };

            var dialog = new SaveFileDialog()
            {
                DefaultExt = ".json",
                Filter = "PostBoy Json (.json) | *.json"
            };

            if (dialog.ShowDialog() ?? false)
            {
                try
                {
                    using var file_stream = new FileStream(dialog.FileName, FileMode.Create);
                    using var json_stream = new Utf8JsonWriter(file_stream);
                    JsonSerializer.Serialize<State>(json_stream, state, new JsonSerializerOptions() { IgnoreNullValues = true });
                }
                catch (Exception exception)
                {
                    WriteLog(exception.Message);
                }
            }
        }

        void btnLoadClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                DefaultExt = ".json",
                Filter = "PostBoy Json (.json)|*.json|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog() ?? false)
            {
                try
                {
                    using var file_stream = new FileStream(dialog.FileName, FileMode.Open);
                    using var json_stream = new Utf8JsonStreamReader(file_stream);
                    json_stream.Read();
                    var state = json_stream.Deserialize<State>();
                    response_content_type = state?.response?.content_type ?? String.Empty;
                    cbxMethod.Text = state?.request?.method ?? String.Empty;
                    tbxUrl.Text = state?.request?.url ?? String.Empty;
                    cbxContentType.Text = state?.request?.content_type ?? String.Empty;
                    cbxCharset.Text = state?.request?.charset ?? String.Empty;
                    tbxRequestHeader.Text = state?.request?.header ?? String.Empty;
                    tbxRequestBody.Text = state?.request?.body ?? String.Empty;
                    tblResponseBodyLeft.Text = response_content_type;
                    tbxResponseHeader.Text = state?.response?.header ?? String.Empty;
                    tbxResponseBody.Text = state?.response?.body ?? String.Empty;
                }
                catch (Exception exception)
                {
                    WriteLog(exception.Message);
                }
            }
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
        }

        void btnWsdlParseClick(object sender, RoutedEventArgs e)
        {
            void _Action()
            {
                string? wsdl_content = null;
                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    wsdl_content = tbxWsdlContent.Text;
                }));
                var _wsdl_parser = new WsdlParser(wsdl_content!);
                if (_wsdl_parser.operations.Count > 0)
                {
                    var items = new string[_wsdl_parser.operations.Count + 1];
                    items[0] = "(select)";
                    Array.Copy(_wsdl_parser.operations.Keys.ToArray(), 0, items, 1, _wsdl_parser.operations.Count);
                    Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                    {
                        wsdl_parser = _wsdl_parser;
                        cbxWsdlOperation.ItemsSource = items;
                        cbxWsdlOperation.SelectedIndex = 0;
                        cbxWsdlOperation.IsEnabled = true;
                        tbxWsdlContent.Clear();
                    }));
                }
            }
            cbxWsdlOperation.ClearValue(ItemsControl.ItemsSourceProperty);
            cbxWsdlOperation.IsEnabled = false;
            MakeAction(_Action);
        }

        void cbxWsdlOperationDropDownClosed(object sender, EventArgs e)
        {
            void _Action()
            {
                string? wsdl_operation = null;
                string? soap_action = null;
                string? request_body = null;
                WsdlParser? _wsdl_parser = null;

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    wsdl_operation = cbxWsdlOperation.Text;
                    soap_action = wsdl_parser!.operations[wsdl_operation];
                    _wsdl_parser = wsdl_parser;
                }));

                request_body = _wsdl_parser!.BuildRequest(wsdl_operation!);

                Dispatcher.Invoke(DispatcherPriority.Normal, (Action)(() =>
                {
                    tbxRequestHeader.Text = $"SOAPAction: \"{soap_action}\"\n";
                    tbxRequestBody.Text = request_body;
                    cbxContentType.Text = "text/xml";
                    cbxCharset.Text = "utf-8";
                }));
            }
            if ((wsdl_parser != null) && (cbxWsdlOperation.IsEnabled) && (cbxWsdlOperation.SelectedIndex > 0))
            {
                MakeAction(_Action);
            }
        }

        void evtClearAlert(object sender, RoutedEventArgs e)
        {
            (sender as Control)?.ClearAlert();
        }

        void evtTextBoxSelectAll(object sender, InputEventArgs e)
        {
            (sender as TextBox)?.SelectAll();
        }

        void tbxUrlGotMouseCapture(object sender, MouseEventArgs e)
        {
            tbxUrl.SelectAll();
        }

        void evtTextBoxMenuItemClick(object sender, RoutedEventArgs eventArgs)
        {
            MenuItem menu_item = (MenuItem)sender;
            TextBox text_box = (TextBox)(((ContextMenu)(menu_item.Parent)).PlacementTarget);

            void _ProcessText(Func<string, string> enccode_proc)
            {
                try
                {
                    if (text_box.SelectionLength > 0)
                    {
                        text_box.SelectedText = enccode_proc(text_box.SelectedText);
                    }
                    else
                    {
                        text_box.Text = enccode_proc(text_box.Text);
                        text_box.SelectAll();
                    }
                }
                catch (Exception e)
                {
                    WriteLog(e.Message);
                }
            }

            switch (menu_item.Name)
            {
                case "mniCopyAll":
                    text_box.SelectAll();
                    text_box.Copy();
                    break;
                case "mniPasteOver":
                    text_box.SelectAll();
                    text_box.Paste();
                    text_box.SelectAll();
                    break;
                case "mniClean":
                    text_box.Clear();
                    break;
                case "mniB64Encode":
                    _ProcessText(x => { return Converter.ToBase64(x); });
                    break;
                case "mniB64Decode":
                    _ProcessText(x => { return Converter.FromBase64(x); });
                    break;
                case "mniUriEncode":
                    _ProcessText(x => { return Converter.ToUrlEncode(x); });
                    break;
                case "mniUriDecode":
                    _ProcessText(x => { return Converter.FromUrlEncode(x); });
                    break;
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

        private async void MakeAction(Action action)
        {
            IsEnabled = false;
            Mouse.OverrideCursor = Cursors.Wait;

            try
            {
                await Task.Run(() => { action(); });
            }
            catch (Exception exception)
            {
                WriteLog(exception.Message);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                IsEnabled = true;
            }
        }

        private void WriteLog(string log_text)
        {
            tbxLog.AppendText($"[{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] {log_text}\n");
            tbxLog.ScrollToEnd();
            tbiLog.Background = Brushes.Yellow;
        }
    }
}
