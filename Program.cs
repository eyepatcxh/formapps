using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace ModernImageProcessingApp
{
    public partial class MainForm : Form
    {
        private List<Tuple<string, string>> imageValueList = new List<Tuple<string, string>>();
        private PictureBox pictureBox;
        private Button btnSelectPhoto;
        private Button btnRunScript;
        private Button btnReset;
        private Button btnChangeValue; // New button for changing values
        private Button btnChooseScript; // New button for choosing the script
        private TextBox txtInputValue;
        private TextBox txtChangeValue;
        private Label lblResults;
        private ListBox listBoxImages;
        private Process pythonProcess;
        private ContextMenuStrip contextMenuStrip; // Context menu for the ListBox

        public MainForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            // Form Settings
            this.Text = "Image Processing App";
            this.Size = new System.Drawing.Size(1000, 800);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.BackColor = System.Drawing.Color.LightGray;

            // PictureBox
            pictureBox = new PictureBox
            {
                Name = "pictureBox",
                Location = new System.Drawing.Point(20, 20),
                Size = new System.Drawing.Size(500, 500),
                SizeMode = PictureBoxSizeMode.Zoom,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = System.Drawing.Color.White
            };
            this.Controls.Add(pictureBox);

            // Button (Select Photo)
            btnSelectPhoto = new Button
            {
                Name = "btnSelectPhoto",
                Location = new System.Drawing.Point(550, 20),
                Size = new System.Drawing.Size(120, 40),
                Text = "Select Photo",
                BackColor = System.Drawing.Color.FromArgb(0, 123, 255),
                ForeColor = System.Drawing.Color.White
            };
            btnSelectPhoto.Click += new EventHandler(btnSelectPhoto_Click);
            this.Controls.Add(btnSelectPhoto);

            // Button (Run Script)
            btnRunScript = new Button
            {
                Name = "btnRunScript",
                Location = new System.Drawing.Point(700, 20),
                Size = new System.Drawing.Size(120, 40),
                Text = "Run Script",
                BackColor = System.Drawing.Color.FromArgb(40, 167, 69),
                ForeColor = System.Drawing.Color.White
            };
            btnRunScript.Click += new EventHandler(btnRunScript_Click);
            this.Controls.Add(btnRunScript);

            // Button (Reset)
            btnReset = new Button
            {
                Name = "btnReset",
                Location = new System.Drawing.Point(850, 20),
                Size = new System.Drawing.Size(120, 40),
                Text = "Reset",
                BackColor = System.Drawing.Color.FromArgb(220, 53, 69),
                ForeColor = System.Drawing.Color.White
            };
            btnReset.Click += new EventHandler(btnReset_Click);
            this.Controls.Add(btnReset);

            // TextBox for input description
            txtInputValue = new TextBox
            {
                Name = "txtInputValue",
                Location = new System.Drawing.Point(550, 80),
                Size = new System.Drawing.Size(420, 40),
                PlaceholderText = "Enter a description...",
                BackColor = System.Drawing.Color.WhiteSmoke
            };
            this.Controls.Add(txtInputValue);

            // TextBox for changing values
            txtChangeValue = new TextBox
            {
                Name = "txtChangeValue",
                Location = new System.Drawing.Point(550, 650),
                Size = new System.Drawing.Size(320, 40),
                PlaceholderText = "Enter a new value...",
                BackColor = System.Drawing.Color.WhiteSmoke
            };
            this.Controls.Add(txtChangeValue);

            // Button for changing values
            btnChangeValue = new Button
            {
                Name = "btnChangeValue",
                Location = new System.Drawing.Point(880, 650),
                Size = new System.Drawing.Size(90, 40),
                Text = "Change Value",
                BackColor = System.Drawing.Color.FromArgb(255, 193, 7),
                ForeColor = System.Drawing.Color.Black
            };
            btnChangeValue.Click += new EventHandler(btnChangeValue_Click);
            this.Controls.Add(btnChangeValue);

            // Button for choosing the script
            btnChooseScript = new Button
            {
                Name = "btnChooseScript",
                Location = new System.Drawing.Point(550, 200),
                Size = new System.Drawing.Size(120, 40),
                Text = "Choose Script",
                BackColor = System.Drawing.Color.FromArgb(255, 193, 7),
                ForeColor = System.Drawing.Color.Black
            };
            btnChooseScript.Click += new EventHandler(btnChooseScript_Click);
            this.Controls.Add(btnChooseScript);

            // Label (Results)
            lblResults = new Label
            {
                Name = "lblResults",
                Location = new System.Drawing.Point(550, 140),
                Size = new System.Drawing.Size(420, 200),
                BorderStyle = BorderStyle.FixedSingle,
                Text = "Results will be displayed here.",
                BackColor = System.Drawing.Color.White
            };
            this.Controls.Add(lblResults);

            // ListBox (Images and Values)
            listBoxImages = new ListBox
            {
                Name = "listBoxImages",
                Location = new System.Drawing.Point(550, 350),
                Size = new System.Drawing.Size(420, 200),
                BackColor = System.Drawing.Color.White
            };
            listBoxImages.SelectedIndexChanged += new EventHandler(listBoxImages_SelectedIndexChanged);
            this.Controls.Add(listBoxImages);

            // Initialize imageValueList
            imageValueList = new List<Tuple<string, string>>();

            // Context menu for the ListBox
            contextMenuStrip = new ContextMenuStrip();

            // Option 1: Delete Picture
            ToolStripMenuItem deleteMenuItem = new ToolStripMenuItem("Delete Picture");
            deleteMenuItem.Click += new EventHandler(deleteMenuItem_Click);
            contextMenuStrip.Items.Add(deleteMenuItem);

            // Option 2: Change Value
            ToolStripMenuItem changeValueMenuItem = new ToolStripMenuItem("Change Value");
            changeValueMenuItem.Click += new EventHandler(changeValueMenuItem_Click);
            contextMenuStrip.Items.Add(changeValueMenuItem);

            // Option 3: Go to Path
            ToolStripMenuItem goToPathMenuItem = new ToolStripMenuItem("Go to Path");
            goToPathMenuItem.Click += new EventHandler(goToPathMenuItem_Click);
            contextMenuStrip.Items.Add(goToPathMenuItem);

            // Set the context menu for the ListBox
            listBoxImages.ContextMenuStrip = contextMenuStrip;
        }

        private void btnSelectPhoto_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Photo";
                openFileDialog.Filter = "Image Files|*.jpg;*.jpeg;*.png";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    pictureBox.Image = null;
                    pictureBox.ImageLocation = openFileDialog.FileName;

                    int existingIndex = imageValueList.FindIndex(tuple => tuple.Item1 == pictureBox.ImageLocation);

                    if (existingIndex != -1)
                    {
                        // If the image is already in the list, update its description
                        txtInputValue.Text = imageValueList[existingIndex].Item2;
                    }
                    else
                    {
                        // If it's a new image, clear the TextBox
                        txtInputValue.Clear();
                        // Add the image to the list
                        imageValueList.Add(new Tuple<string, string>(pictureBox.ImageLocation, ""));
                    }

                    UpdateListBox();
                }
            }
        }

        private void btnRunScript_Click(object sender, EventArgs e)
        {
            if (imageValueList.Count == 0)
            {
                MessageBox.Show("Please add images and descriptions before running the script.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Choose the script file
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Select Script File";
                openFileDialog.Filter = "Python Scripts|*.py|All Files|*.*";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string scriptPath = openFileDialog.FileName;

                    foreach (var imageValueTuple in imageValueList)
                    {
                        // Add your logic here to run the selected script for each image and description
                        ExecuteScript(scriptPath, imageValueTuple.Item1, imageValueTuple.Item2);
                    }
                }
            }
        }

        private void ExecuteScript(string scriptPath, string imagePath, string description)
        {
            // Construct the command to run the Python script with arguments
            string command = $"python \"{scriptPath}\" \"{imagePath}\" \"{description}\"";

            // Create a process to run the command
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (Process process = new Process { StartInfo = startInfo })
            {
                process.Start();

                // Pass the command to the cmd process
                process.StandardInput.WriteLine(command);
                process.StandardInput.Flush();
                process.StandardInput.Close();

                // Wait for the process to complete
                process.WaitForExit();

                // Get the output and error messages
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();

                // Handle the output and error messages as needed
                Console.WriteLine($"Output: {output}");
                Console.WriteLine($"Error: {error}");
            }
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            imageValueList.Clear();
            UpdateListBox();
            pictureBox.Image = null;
            txtInputValue.Clear();
            txtChangeValue.Clear();
            lblResults.Text = "Results will be displayed here.";
        }

        private void btnChangeValue_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxImages.SelectedIndex;

            if (selectedIndex != -1)
            {
                // Change the value of the selected image
                imageValueList[selectedIndex] = new Tuple<string, string>(
                    imageValueList[selectedIndex].Item1,
                    txtChangeValue.Text
                );

                UpdateListBox();
            }
        }

        private void btnChooseScript_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Title = "Choose Script File";
                openFileDialog.Filter = "Python Scripts|*.py|All Files|*.*";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the selected script file path
                    string selectedScriptPath = openFileDialog.FileName;

                    // Display a message or perform any additional actions based on the selected script file path
                    MessageBox.Show($"Script file selected: {selectedScriptPath}", "Script Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void listBoxImages_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBoxImages.SelectedIndex;

            if (selectedIndex != -1)
            {
                // Display the selected image in the PictureBox
                pictureBox.ImageLocation = imageValueList[selectedIndex].Item1;

                // Display the selected image's description in the TextBox
                txtInputValue.Text = imageValueList[selectedIndex].Item2;
                txtChangeValue.Text = imageValueList[selectedIndex].Item2; // Set the change value TextBox as well
            }
        }

        private void UpdateListBox()
        {
            listBoxImages.Items.Clear();

            foreach (var imageValueTuple in imageValueList)
            {
                listBoxImages.Items.Add($"{Path.GetFileName(imageValueTuple.Item1)} - {imageValueTuple.Item2}");
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopPythonScript();
        }

        private void StopPythonScript()
        {
            try
            {
                if (pythonProcess != null && !pythonProcess.HasExited)
                {
                    pythonProcess.Kill();
                    pythonProcess.WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error stopping Python script: {ex.Message}");
            }
        }

        private void deleteMenuItem_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxImages.SelectedIndex;
            if (selectedIndex != -1)
            {
                // Remove the selected image from the list
                imageValueList.RemoveAt(selectedIndex);
                UpdateListBox();
            }
        }

        private void changeValueMenuItem_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxImages.SelectedIndex;
            if (selectedIndex != -1)
            {
                // Change the value of the selected image
                imageValueList[selectedIndex] = new Tuple<string, string>(
                    imageValueList[selectedIndex].Item1,
                    txtChangeValue.Text
                );

                UpdateListBox();
            }
        }

        private void goToPathMenuItem_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBoxImages.SelectedIndex;
            if (selectedIndex != -1)
            {
                // Open the file explorer at the path of the selected image
                string directoryPath = Path.GetDirectoryName(imageValueList[selectedIndex].Item1);
                Process.Start("explorer.exe", directoryPath);
            }
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Application.Run(new MainForm());
        }
    }
}
