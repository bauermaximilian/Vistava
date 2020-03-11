using System.Windows.Forms;
using Vistava.Library;

namespace Vistava.GUI
{
    public partial class ConnectionTroubleshootingUnixDialog : Form
    {
        //https://docs.connectwise.com/ConnectWise_Control_Documentation/On-premises/Advanced_setup/SSL_certificate_installation/Install_an_SSL_certificate_on_OS_X_or_Linux

        private readonly Settings settings;

        public ConnectionTroubleshootingUnixDialog()
        {
            InitializeComponent();

            if (!Settings.TryLoad(out settings))
            {
                MessageBox.Show("The current configuration is invalid. " +
                    "Please review your configuration and try again.",
                    "Invalid configuration", MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
                Close();
            }

            firewallLabel.Text = firewallLabel.Text.Replace("%PORT%",
                settings.Port.ToString());
        }
    }
}
