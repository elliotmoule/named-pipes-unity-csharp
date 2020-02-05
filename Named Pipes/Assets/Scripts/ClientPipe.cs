using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

public class ClientPipe
{
    private ViewModel _parent;
    private NamedPipeClientStream _clientPipe;
    private StreamString _streamString;
    private bool _serverClose;
    public event EventHandler ClientClosed;
    private string _messageToSend;

    public ClientPipe(ViewModel parent)
    {
        this._parent = parent;
        _serverClose = false;
        Task.Factory.StartNew(() => { StartClient(); });
    }

    private void OnClientClosed(EventArgs e)
    {
        EventHandler handler = ClientClosed;
        if (handler != null)
        {
            handler(this, e);
        }
    }

    public void SendMessage(string message)
    {
        if (_clientPipe != null && _clientPipe.IsConnected)
        {
            Task.Run(() =>
            {
                PipeUtilities.SendPipedMessage(_streamString, message);
            });
        }
    }

    public void StopClient()
    {
        Task.Run(() =>
        {
            if (_clientPipe != null && _clientPipe.IsConnected)
            {
                _clientPipe.Close();
                _serverClose = true;
            }
        });
    }

    private void StartClient()
    {
        try
        {
            Task.Factory.StartNew(() =>
            {
                RunClient();
            });
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }

    private void RunClient()
    {
        try
        {
            _clientPipe = new NamedPipeClientStream(".", Constants.PipeName, Constants.PipeDirection, Constants.PipeOptions);
            _clientPipe.Connect();
            _clientPipe.Flush();
            _streamString = new StreamString(_clientPipe);

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                _parent.TextArea.text = "";
            });

            do
            {
                if (_clientPipe != null && _clientPipe.IsConnected)
                {
                    string line = _streamString.ReadString();
                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        if (line == Constants.DisconnectKeyword)
                        {
                            SendMessage(Constants.DisconnectKeyword);
                        }
                        else
                        {
                            UnityMainThreadDispatcher.Instance().Enqueue(() =>
                            {
                                _parent.TextArea.text = string.Copy(line);
                            });
                        }
                    }
                    else
                    {
                        _serverClose = true;
                    }
                }

            } while (!_serverClose);

            _clientPipe.Close();

            UnityMainThreadDispatcher.Instance().Enqueue(() =>
            {
                OnClientClosed(EventArgs.Empty);
            });
        }
        catch (IOException)
        {
            _serverClose = true;
            _clientPipe.Flush();
            _clientPipe.Close();
        }
        catch (Exception ex)
        {
            throw ex;
        }
    }
}
