using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;

using PostBoy.Helpers;
using PostBoy.Models;

namespace PostBoy
{
    public partial class MainWindow : Window
    {
        private SynchronizationContext originalContext { get; }
        protected string app_version { get; }
        protected WsdlParser? wsdl_parser { get; set; }
        protected int? timeout { get; set; }
        protected string? proxy_address { get; set; }
        protected string response_content_type { get; set; } = String.Empty;

        private enum StateItem
        {
            Timeout,
            ProxyAddress,
            Method,
            Url,
            RequestContentType,
            RequestCharset,
            RequestHeader,
            RequestBody,
            ResponseStatus,
            ResponseContentType,
            ResponseCharset,
            ResponseHeader,
            ResponseBody,
            WsdlParser,
            WsdlContent,
            WsdlOperation,
            FileName,
            SaveState
        }

        private class AsyncState : Dictionary<StateItem, object?> { }

        public MainWindow()
        {
            InitializeComponent();
            originalContext = SynchronizationContext.Current!;
            app_version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? String.Empty;
            this.Title = $"PostBoy v{app_version}";
        }

        void btnGoClick(object sender, RoutedEventArgs e)
        {
            static void _PreAction(MainWindow mw, AsyncState async_state)
            {
                async_state[StateItem.Timeout] = mw.timeout;
                async_state[StateItem.ProxyAddress] = mw.proxy_address;
                async_state[StateItem.Method] = mw.cbxMethod.Text;
                async_state[StateItem.Url] = mw.tbxUrl.Text;
                async_state[StateItem.RequestHeader] = mw.tbxRequestHeader.Text;
                async_state[StateItem.RequestBody] = mw.tbxRequestBody.Text;
                async_state[StateItem.RequestContentType] = mw.cbxContentType.Text;
                async_state[StateItem.RequestCharset] = mw.cbxCharset.Text;
            }
            static void _Action(AsyncState async_state)
            {
                var http_helper = new HttpHelper()
                {
                    timeout = (int?)(async_state[StateItem.Timeout]),
                    proxy_address = (string?)(async_state[StateItem.ProxyAddress])
                };
                var response = http_helper.Request(
                    method: (async_state[StateItem.Method] as string)!,
                    url: (async_state[StateItem.Url] as string)!,
                    header: (async_state[StateItem.RequestHeader] as string)!,
                    content_type: (async_state[StateItem.RequestContentType] as string)!,
                    charset: (async_state[StateItem.RequestCharset] as string)!,
                    body: async_state[StateItem.RequestBody] as string);
                async_state[StateItem.ResponseStatus] = response.status;
                async_state[StateItem.ResponseHeader] = response.header;
                async_state[StateItem.ResponseContentType] = response.content_type;
                async_state[StateItem.ResponseBody] = response.body;
            }
            static void _PostAction(MainWindow mw, AsyncState async_state)
            {
                mw.response_content_type = (async_state[StateItem.ResponseContentType] as string)!;
                mw.tblResponseHeaderLeft.Text = $"HTTP: {async_state[StateItem.ResponseStatus]!}";
                mw.tblResponseBodyLeft.Text = mw.response_content_type;
                mw.tbxResponseHeader.Text = (async_state[StateItem.ResponseHeader] as string)!;
                mw.tbxResponseBody.Text = (async_state[StateItem.ResponseBody] as string)!;
            }

            tblResponseHeaderLeft.Text = String.Empty;
            tblResponseBodyLeft.Text = String.Empty;
            tbxResponseHeader.Text = String.Empty;
            tbxResponseBody.Text = String.Empty;

            var async_state = new AsyncState();
            _ = AsyncRun(async_state, _PreAction, _Action, _PostAction);
        }

        void btnSaveClick(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                DefaultExt = ".json",
                Filter = "PostBoy Json (.json) | *.json"
            };

            if (dialog.ShowDialog() ?? false)
            {
                SaveJson(dialog.FileName);
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
                LoadJson(dialog.FileName);
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

        void btnLogClearClick(object sender, RoutedEventArgs e)
        {
            tbxLog.Clear();
        }

        void btnWsdlParseClick(object sender, RoutedEventArgs e)
        {
            static void _PreAction(MainWindow mw, AsyncState async_state)
            {
                async_state[StateItem.WsdlContent] = mw.tbxWsdlContent.Text;
            }
            static void _Action(AsyncState async_state)
            {
                async_state[StateItem.WsdlParser] = new WsdlParser((async_state[StateItem.WsdlContent] as string)!);
            }
            static void _PostAction(MainWindow mw, AsyncState async_state)
            {
                var wsdl_parser = (async_state[StateItem.WsdlParser] as WsdlParser)!;
                if (wsdl_parser.operations.Count > 0)
                {
                    var items = new string[wsdl_parser.operations.Count + 1];
                    items[0] = "(select)";
                    Array.Copy(wsdl_parser.operations.Keys.ToArray(), 0, items, 1, wsdl_parser.operations.Count);
                    mw.wsdl_parser = wsdl_parser;
                    mw.cbxWsdlOperation.ItemsSource = items;
                    mw.cbxWsdlOperation.SelectedIndex = 0;
                    mw.cbxWsdlOperation.IsEnabled = true;
                    mw.tbxWsdlContent.Clear();
                }
            }
            cbxWsdlOperation.ClearValue(ItemsControl.ItemsSourceProperty);
            cbxWsdlOperation.IsEnabled = false;
            var async_state = new AsyncState();
            _ = AsyncRun(async_state, _PreAction, _Action, _PostAction);
        }

        void cbxWsdlOperationDropDownClosed(object sender, EventArgs e)
        {
            static void _PreAction(MainWindow mw, AsyncState async_state)
            {
                async_state[StateItem.WsdlParser] = mw.wsdl_parser!;
                async_state[StateItem.WsdlOperation] = mw.cbxWsdlOperation.Text;
            }
            void _Action(AsyncState async_state)
            {
                var wsdl_parser = (async_state[StateItem.WsdlParser] as WsdlParser)!;
                var wsdl_operation = (async_state[StateItem.WsdlOperation] as string)!;
                async_state[StateItem.RequestBody] = wsdl_parser.BuildRequest(wsdl_operation);
            }
            static void _PostAction(MainWindow mw, AsyncState async_state)
            {
                var wsdl_parser = (async_state[StateItem.WsdlParser] as WsdlParser)!;
                var wsdl_operation = (async_state[StateItem.WsdlOperation] as string)!;
                mw.tbxRequestHeader.Text = $"SOAPAction: \"{wsdl_parser.operations[wsdl_operation]}\"\n";
                mw.tbxRequestBody.Text = (async_state[StateItem.RequestBody] as string);
                mw.cbxContentType.Text = "text/xml";
                mw.cbxCharset.Text = "utf-8";
            }
            if ((wsdl_parser != null) && (cbxWsdlOperation.IsEnabled) && (cbxWsdlOperation.SelectedIndex > 0))
            {
                var async_state = new AsyncState();
                _ = AsyncRun(async_state, _PreAction, _Action, _PostAction);
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
                case "mniCutAll":
                    text_box.SelectAll();
                    text_box.Cut();
                    break;
                case "mniCopyAll":
                    text_box.SelectAll();
                    text_box.Copy();
                    break;
                case "mniPasteOver":
                    text_box.SelectAll();
                    text_box.Paste();
                    text_box.SelectAll();
                    break;
                case "mniClear":
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
                case "mniBeautifyXml":
                    _ProcessText(x => { return Beautifier.Xml(x); });
                    break;
                case "mniBeautifyJson":
                    _ProcessText(x => { return Beautifier.Json(x); });
                    break;
            }
        }

        void evtDropLoadJson(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var file_name = ((string[])(e.Data.GetData(DataFormats.FileDrop))).FirstOrDefault();
                LoadJson(file_name);
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

        private void LoadJson(string file_name)
        {
            static void _Action(AsyncState async_state)
            {
                using var file_stream = new FileStream((async_state[StateItem.FileName] as string)!, FileMode.Open);
                using var json_stream = new Utf8JsonStreamReader(file_stream);
                json_stream.Read();
                async_state[StateItem.SaveState] = json_stream.Deserialize<SaveState>();
            }
            static void _PostAction(MainWindow mw, AsyncState async_state)
            {
                var save_state = async_state[StateItem.SaveState] as SaveState;
                mw.timeout = save_state?.config?.timeout;
                mw.proxy_address = save_state?.config?.proxy_address;
                mw.response_content_type = save_state?.response?.content_type ?? String.Empty;
                mw.cbxMethod.Text = save_state?.request?.method ?? String.Empty;
                mw.tbxUrl.Text = save_state?.request?.url ?? String.Empty;
                mw.cbxContentType.Text = save_state?.request?.content_type ?? String.Empty;
                mw.cbxCharset.Text = save_state?.request?.charset ?? String.Empty;
                mw.tbxRequestHeader.Text = save_state?.request?.header ?? String.Empty;
                mw.tbxRequestBody.Text = save_state?.request?.body ?? String.Empty;
                mw.tblResponseBodyLeft.Text = mw.response_content_type;
                mw.tbxResponseHeader.Text = save_state?.response?.header ?? String.Empty;
                mw.tbxResponseBody.Text = save_state?.response?.body ?? String.Empty;
            }
            var async_state = new AsyncState() { [StateItem.FileName] = file_name };
            _ = AsyncRun(async_state, delegate {}, _Action, _PostAction);
        }

        private void SaveJson(string file_name)
        {
            static void _PreAction(MainWindow mw, AsyncState async_state)
            {
                async_state[StateItem.SaveState] = new SaveState()
                {
                    config = new SaveState.Config()
                    {
                        version = mw.app_version,
                        timeout = mw.timeout,
                        proxy_address = mw.proxy_address
                    },
                    request = new SaveState.Transaction()
                    {
                        method = mw.cbxMethod.Text,
                        url = mw.tbxUrl.Text,
                        content_type = mw.cbxContentType.Text,
                        charset = mw.cbxCharset.Text,
                        header = mw.tbxRequestHeader.Text,
                        body = mw.tbxRequestBody.Text
                    },
                    response = new SaveState.Transaction()
                    {
                        content_type = mw.response_content_type,
                        header = mw.tbxResponseHeader.Text,
                        body = mw.tbxResponseBody.Text
                    }
                };
            }
            static void _Action(AsyncState async_state)
            {
                var save_state = (async_state[StateItem.SaveState] as SaveState)!;
                using var file_stream = new FileStream((async_state[StateItem.FileName] as string)!, FileMode.Create);
                using var json_stream = new Utf8JsonWriter(file_stream);
                JsonSerializer.Serialize<SaveState>(json_stream, save_state, new JsonSerializerOptions() { IgnoreNullValues = true });
            }
            var async_state = new AsyncState() { [StateItem.FileName] = file_name };
            _ = AsyncRun(async_state, _PreAction, _Action, delegate { });
        }

        private async Task AsyncRun(
            AsyncState async_state,
            Action<MainWindow, AsyncState> pre_action,
            Action<AsyncState> action,
            Action<MainWindow, AsyncState> post_action)
        {
            void _PreWork()
            {
                IsEnabled = false;
                Mouse.OverrideCursor = Cursors.Wait;
            }
            void _PostWork()
            {
                Mouse.OverrideCursor = null;
                IsEnabled = true;
            }
            void _Exception(object? exception)
            {
                WriteLog((exception as Exception)!.Message);
            }
            //
            await Task.Delay(0).ConfigureAwait(false);
            originalContext.Send(delegate { _PreWork(); }, null);
            try
            {
                originalContext.Send(delegate { lock (async_state) { pre_action(this, async_state); } }, null);
                await Task.Run(() => { lock (async_state) { action(async_state); } }).ConfigureAwait(false);
                originalContext.Send(delegate { lock (async_state) { post_action(this, async_state); } }, null);
            }
            catch (Exception exception)
            {
                originalContext.Post(_Exception, exception);
            }
            finally
            {
                originalContext.Post(delegate { _PostWork(); }, null);
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
