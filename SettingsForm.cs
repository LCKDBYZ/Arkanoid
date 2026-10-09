using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Arkanoid {
    public class SettingsForm : Form {
        private readonly SaveData data;
        private CheckBox chkSound;
        private TrackBar trackVolume;
        private Label lblVolume;

        internal SettingsForm(SaveData data) {
            this.data = data;

            Text = "Settings";
            ClientSize = new Size(320, 180);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition = FormStartPosition.CenterParent;
            MaximizeBox = false;
            MinimizeBox = false;

            chkSound = new CheckBox {
                Text = "Sound enabled",
                Checked = data.SoundEnabled,
                Left = 20,
                Top = 20,
                AutoSize = true
            };

            lblVolume = new Label {
                Text = "Volume: " + (int)(data.Volume * 100) + "%",
                Left = 20,
                Top = 60,
                AutoSize = true
            };

            trackVolume = new TrackBar {
                Minimum = 0,
                Maximum = 100,
                TickFrequency = 10,
                SmallChange = 5,
                LargeChange = 10,
                Value = (int)(data.Volume * 100),
                Left = 15,
                Top = 80,
                Width = 290
            };
            trackVolume.ValueChanged += (s, e) => lblVolume.Text = "Volume: " + trackVolume.Value + "%";

            Button btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 130, Top = 135, Width = 80 };
            Button btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 220, Top = 135, Width = 80 };

            AcceptButton = btnOk;
            CancelButton = btnCancel;

            Controls.AddRange(new Control[] { chkSound, lblVolume, trackVolume, btnOk, btnCancel });

            FormClosing += (s, e) => {
                if (DialogResult == DialogResult.OK) {
                    data.SoundEnabled = chkSound.Checked;
                    data.Volume = trackVolume.Value / 100f;
                }
            };
        }
    }
}
