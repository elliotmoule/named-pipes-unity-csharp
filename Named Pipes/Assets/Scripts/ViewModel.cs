using UnityEngine;
using UnityEngine.UI;

public class ViewModel : MonoBehaviour
{
    private ClientPipe _clientPipe;
    public Text TextArea;
    public Button ClientStartButton;
    public Button ClientStopButton;
    public Button ClientSendButton;
    public InputField UserInput;

    public ClientPipe ClientPipe
    {
        get { return _clientPipe; }
        set
        {
            _clientPipe = value;
            ClientStartButton.enabled = _clientPipe == null ? true : false;
            ClientStopButton.enabled = !ClientStartButton.enabled;
        }
    }

    public void SendButton_Click()
    {
        ClientPipe?.SendMessage(string.Copy(UserInput.text));
        UserInput.text = "";
    }

    public void ClientStartButton_Click()
    {
        ClientPipe = new ClientPipe(this);
        ClientPipe.ClientClosed += ClientPipe_ClientClosed;
    }

    private void ClientPipe_ClientClosed(object sender, System.EventArgs e)
    {
        ClientPipe.ClientClosed -= ClientPipe_ClientClosed;
        ClientPipe = null;
    }

    public void ClientStopButton_Click()
    {
        ClientPipe?.StopClient();
        ClientStopButton.enabled = false;
    }
}
