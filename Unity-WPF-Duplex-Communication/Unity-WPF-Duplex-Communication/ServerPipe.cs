using Shared.Utilities;
using System;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;

namespace Unity_WPF_Duplex_Communication
{
    public class ServerPipe
    {
        private MainWindow _parent;
        private StreamString _streamString;
        private NamedPipeServerStream _serverPipe;
        public event EventHandler ServerClosed;
        public event EventHandler ClientDisconnected;

        public ServerPipe(MainWindow parent)
        {
            this._parent = parent;
            StartServer();
        }

        private void OnServerClosed(EventArgs e)
        {
            EventHandler handler = ServerClosed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnClientDisconnect(EventArgs e)
        {
            EventHandler handler = ClientDisconnected;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public bool SendMessage(string message)
        {
            if (_serverPipe != null && _serverPipe.IsConnected)
            {
                PipeUtilities.SendPipedMessage(_streamString, message);
                return true;
            }
            return false;
        }

        public void StopServer()
        {
            SendMessage(Constants.DisconnectKeyword);
        }

        private void StartServer()
        {
            try
            {
                PipeSecurity security = new PipeSecurity();
                security.AddAccessRule(
                    new PipeAccessRule($"{Environment.UserDomainName}\\{Environment.UserName}",
                    PipeAccessRights.ReadWrite,
                    System.Security.AccessControl.AccessControlType.Allow));
                _serverPipe = new NamedPipeServerStream(
                    Constants.PipeName,
                    Constants.PipeDirection,
                    1,
                    Constants.PipeTransmissionMode,
                    Constants.PipeOptions,
                    4096,
                    4096,
                    security);

                Task.Factory.StartNew(async () =>
                {
                    await RunServerAsync();
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task RunServerAsync()
        {
            try
            {
                if (_serverPipe != null)
                {
                    await Task.Factory.FromAsync(
                                                (cb, state) => _serverPipe.BeginWaitForConnection(cb, state),
                                                ar => _serverPipe.EndWaitForConnection(ar),
                                                null);

                    bool clientDisconnected = false;
                    _streamString = new StreamString(_serverPipe);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        _parent.TextArea.Text = "";
                    });

                    SendMessage("initialised");

                    while (true)
                    {
                        if (_serverPipe != null && _serverPipe.IsConnected)
                        {
                            string line = string.Empty;
                            try
                            {
                                line = _streamString.ReadString();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex);
                            }

                            if (!string.IsNullOrWhiteSpace(line))
                            {
                                if (line == Constants.DisconnectKeyword)
                                {
                                    break;
                                }
                                else
                                {
                                    Application.Current.Dispatcher.Invoke(() =>
                                    {
                                        _parent.TextArea.Text = line;
                                    });
                                }
                            }
                            else if (string.IsNullOrWhiteSpace(line))
                            {
                                break;
                            }
                        }
                        else if (!_serverPipe.IsConnected)
                        {
                            clientDisconnected = true;
                            break;
                        }
                    }

                    if (clientDisconnected)
                    {
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            OnClientDisconnect(EventArgs.Empty);
                        });
                        clientDisconnected = false;
                    }

                    PipeFlushClose(_serverPipe);
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnServerClosed(EventArgs.Empty);
                    });
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void PipeFlushClose(NamedPipeServerStream namedPipeServerStream)
        {
            try
            {
                if (namedPipeServerStream != null)
                {
                    if (!namedPipeServerStream.IsConnected)
                    {
                        using (NamedPipeClientStream npcs = new NamedPipeClientStream(Constants.PipeName))
                        {
                            npcs.Connect(100);
                        }
                    }
                    namedPipeServerStream.WaitForPipeDrain();
                    namedPipeServerStream.Flush();
                    namedPipeServerStream.Disconnect();
                    namedPipeServerStream.Close();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
