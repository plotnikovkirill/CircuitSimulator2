using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace CircuitSimulator
{
    public class MainForm : Form
    {
        private TabControl mainTabs;
        private ComboBox cmbPresets;
        private Button btnSimulate, btnLoadPreset;
        private DataGridView dgvA, dgvB, dgvC, dgvD, dgvX0, dgvV;
        private TextBox txtStateNames, txtOutputNames, txtSourceNames;
        private NumericUpDown nudTimeStep, nudDuration;
        private Chart chartStates, chartOutputs;
        private Label lblStatus;

        private int stateCount = 2;
        private int sourceCount = 1;
        private int outputCount = 2;

        public MainForm()
        {
            InitializeComponents();
            LoadPresetSchemes();
        }

        private void InitializeComponents()
        {
            this.Text = "Универсальный симулятор электрических цепей - Метод переменных состояния";
            this.Size = new Size(1400, 900);
            this.StartPosition = FormStartPosition.CenterScreen;

            mainTabs = new TabControl
            {
                Dock = DockStyle.Fill,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };

            TabPage tabSetup = new TabPage("Настройка схемы");
            CreateSetupTab(tabSetup);
            mainTabs.TabPages.Add(tabSetup);

            TabPage tabResults = new TabPage("Результаты моделирования");
            CreateResultsTab(tabResults);
            mainTabs.TabPages.Add(tabResults);

            this.Controls.Add(mainTabs);
        }

        private void CreateSetupTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(10) };

            int yPos = 10;

            // Заголовок
            Label lblTitle = new Label
            {
                Text = "Задание параметров системы уравнений состояния",
                Font = new Font("Arial", 14, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblTitle);
            yPos += 40;

            // Выбор предустановленной схемы
            Label lblPreset = new Label
            {
                Text = "Выберите схему:",
                Location = new Point(10, yPos),
                Size = new Size(150, 25),
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            panel.Controls.Add(lblPreset);

            cmbPresets = new ComboBox
            {
                Location = new Point(170, yPos),
                Size = new Size(300, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Arial", 10)
            };
            panel.Controls.Add(cmbPresets);

            btnLoadPreset = new Button
            {
                Text = "Загрузить",
                Location = new Point(480, yPos - 2),
                Size = new Size(100, 30),
                BackColor = Color.LightBlue,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            btnLoadPreset.Click += BtnLoadPreset_Click;
            panel.Controls.Add(btnLoadPreset);
            yPos += 50;

            // Описание системы
            Label lblDesc = new Label
            {
                Text = "Система: dX/dt = A·X + B·V,  Y = C·X + D·V",
                Font = new Font("Consolas", 11, FontStyle.Bold),
                ForeColor = Color.DarkBlue,
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblDesc);
            yPos += 35;

            // Имена переменных
            CreateLabelAndTextBox(panel, "Имена состояний X:", ref txtStateNames, "uC, iL", ref yPos);
            CreateLabelAndTextBox(panel, "Имена выходов Y:", ref txtOutputNames, "i2, i3", ref yPos);
            CreateLabelAndTextBox(panel, "Имена источников V:", ref txtSourceNames, "J", ref yPos);
            yPos += 10;

            // Матрицы
            Label lblMatrices = new Label
            {
                Text = "Матрицы системы:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblMatrices);
            yPos += 35;

            CreateMatrixGroup(panel, "Матрица A (состояние → производная)", ref dgvA, ref yPos, 2, 2);
            CreateMatrixGroup(panel, "Матрица B (источники → производная)", ref dgvB, ref yPos, 2, 1);
            CreateMatrixGroup(panel, "Матрица C (состояние → выход)", ref dgvC, ref yPos, 2, 2);
            CreateMatrixGroup(panel, "Матрица D (источники → выход)", ref dgvD, ref yPos, 2, 1);

            // Начальные условия и источники
            yPos += 10;
            Label lblInitial = new Label
            {
                Text = "Начальные условия и параметры:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblInitial);
            yPos += 35;

            CreateMatrixGroup(panel, "Вектор X(0) - начальные условия", ref dgvX0, ref yPos, 2, 1);
            CreateMatrixGroup(panel, "Вектор V - источники воздействия", ref dgvV, ref yPos, 1, 1);

            // Параметры моделирования
            yPos += 10;
            Label lblSim = new Label
            {
                Text = "Параметры моделирования:",
                Font = new Font("Arial", 12, FontStyle.Bold),
                Location = new Point(10, yPos),
                AutoSize = true
            };
            panel.Controls.Add(lblSim);
            yPos += 35;

            CreateParameter(panel, "Шаг интегрирования h (с):", ref nudTimeStep, 0.0001, 0.00001, 0.01, ref yPos, 0.00001);
            CreateParameter(panel, "Время моделирования (с):", ref nudDuration, 0.05, 0.001, 10, ref yPos, 0.001);

            // Кнопка запуска
            yPos += 20;
            btnSimulate = new Button
            {
                Text = "▶ ЗАПУСТИТЬ МОДЕЛИРОВАНИЕ",
                Location = new Point(10, yPos),
                Size = new Size(400, 50),
                BackColor = Color.LightGreen,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            btnSimulate.Click += BtnSimulate_Click;
            panel.Controls.Add(btnSimulate);

            lblStatus = new Label
            {
                Location = new Point(420, yPos + 15),
                Size = new Size(400, 30),
                Text = "Готов к моделированию",
                ForeColor = Color.Blue,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            panel.Controls.Add(lblStatus);

            tab.Controls.Add(panel);
        }

        private void CreateResultsTab(TabPage tab)
        {
            Panel panel = new Panel { Dock = DockStyle.Fill };

            chartStates = CreateChart("Переменные состояния X(t)");
            chartOutputs = CreateChart("Выходные переменные Y(t)");

            SplitContainer split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal
            };

            chartStates.Dock = DockStyle.Fill;
            chartOutputs.Dock = DockStyle.Fill;

            split.Panel1.Controls.Add(chartStates);
            split.Panel2.Controls.Add(chartOutputs);

            panel.Controls.Add(split);
            tab.Controls.Add(panel);
        }

        private void CreateLabelAndTextBox(Panel panel, string labelText, ref TextBox textBox, string defaultValue, ref int yPos)
        {
            Label lbl = new Label
            {
                Text = labelText,
                Location = new Point(10, yPos),
                Size = new Size(250, 20),
                Font = new Font("Arial", 10)
            };
            panel.Controls.Add(lbl);

            textBox = new TextBox
            {
                Location = new Point(270, yPos - 3),
                Size = new Size(400, 25),
                Text = defaultValue,
                Font = new Font("Arial", 10)
            };
            panel.Controls.Add(textBox);
            yPos += 35;
        }

        private void CreateParameter(Panel panel, string label, ref NumericUpDown nud, double defaultValue, double min, double max, ref int yPos, double increment)
        {
            Label lbl = new Label
            {
                Text = label,
                Location = new Point(10, yPos),
                Size = new Size(250, 20),
                Font = new Font("Arial", 10)
            };
            panel.Controls.Add(lbl);

            nud = new NumericUpDown
            {
                Location = new Point(270, yPos - 3),
                Size = new Size(150, 25),
                Minimum = (decimal)min,
                Maximum = (decimal)max,
                Value = (decimal)defaultValue,
                DecimalPlaces = 6,
                Increment = (decimal)increment,
                Font = new Font("Arial", 10)
            };
            panel.Controls.Add(nud);
            yPos += 35;
        }

        private void CreateMatrixGroup(Panel panel, string title, ref DataGridView dgv, ref int yPos, int rows, int cols)
        {
            Label lbl = new Label
            {
                Text = title,
                Location = new Point(10, yPos),
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold)
            };
            panel.Controls.Add(lbl);
            yPos += 25;

            dgv = new DataGridView
            {
                Location = new Point(10, yPos),
                Size = new Size(cols * 100 + 50, rows * 30 + 40),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersWidth = 50,
                Font = new Font("Arial", 9)
            };

            for (int i = 0; i < cols; i++)
            {
                dgv.Columns.Add($"col{i}", $"[{i}]");
                dgv.Columns[i].Width = 100;
            }

            for (int i = 0; i < rows; i++)
            {
                dgv.Rows.Add();
                dgv.Rows[i].HeaderCell.Value = $"[{i}]";
            }

            panel.Controls.Add(dgv);
            yPos += dgv.Height + 15;
        }

        private Chart CreateChart(string title)
        {
            Chart chart = new Chart { BackColor = Color.White };

            ChartArea chartArea = new ChartArea
            {
                BackColor = Color.WhiteSmoke,
                AxisX = {
                    Title = "Время (с)",
                    TitleFont = new Font("Arial", 10, FontStyle.Bold),
                    LabelStyle = { Format = "0.####" }
                },
                AxisY = {
                    Title = "Значение",
                    TitleFont = new Font("Arial", 10, FontStyle.Bold),
                    LabelStyle = { Format = "0.####" }
                }
            };
            chart.ChartAreas.Add(chartArea);

            Legend legend = new Legend
            {
                Docking = Docking.Top,
                Font = new Font("Arial", 9)
            };
            chart.Legends.Add(legend);

            Title chartTitle = new Title
            {
                Text = title,
                Font = new Font("Arial", 12, FontStyle.Bold)
            };
            chart.Titles.Add(chartTitle);

            return chart;
        }

        private void LoadPresetSchemes()
        {
            cmbPresets.Items.Clear();
            cmbPresets.Items.Add("Схема 1: RLC цепь с источником тока (из лабы)");
            cmbPresets.Items.Add("Схема 2: RC цепь");
            cmbPresets.Items.Add("Схема 3: RL цепь");
            cmbPresets.Items.Add("Схема 4: Колебательный контур");
            cmbPresets.SelectedIndex = 0;
        }

        private void BtnLoadPreset_Click(object sender, EventArgs e)
        {
            int index = cmbPresets.SelectedIndex;
            switch (index)
            {
                case 0: LoadScheme1(); break;
                case 1: LoadScheme2(); break;
                case 2: LoadScheme3(); break;
                case 3: LoadScheme4(); break;
            }
            lblStatus.Text = $"Загружена схема: {cmbPresets.SelectedItem}";
            lblStatus.ForeColor = Color.Green;
        }

        private void LoadScheme1()
        {
            // Исходная схема из лабы: R1=100, R2=200, L=0.1, C=0.0001, J=0.05
            txtStateNames.Text = "uC, iL";
            txtOutputNames.Text = "i2, i3";
            txtSourceNames.Text = "J";

            double R1 = 100, R2 = 200, L = 0.1, C = 0.0001, J = 0.05;

            SetMatrixSize(dgvA, 2, 2);
            SetMatrixValue(dgvA, 0, 0, -1 / (C * R2));
            SetMatrixValue(dgvA, 0, 1, -1 / C);
            SetMatrixValue(dgvA, 1, 0, 1 / L);
            SetMatrixValue(dgvA, 1, 1, -R1 / L);

            SetMatrixSize(dgvB, 2, 1);
            SetMatrixValue(dgvB, 0, 0, 1 / C);
            SetMatrixValue(dgvB, 1, 0, 0);

            SetMatrixSize(dgvC, 2, 2);
            SetMatrixValue(dgvC, 0, 0, 1 / R2);
            SetMatrixValue(dgvC, 0, 1, 0);
            SetMatrixValue(dgvC, 1, 0, -1 / R2);
            SetMatrixValue(dgvC, 1, 1, -1);

            SetMatrixSize(dgvD, 2, 1);
            SetMatrixValue(dgvD, 0, 0, 0);
            SetMatrixValue(dgvD, 1, 0, 1);

            SetMatrixSize(dgvX0, 2, 1);
            SetMatrixValue(dgvX0, 0, 0, J * R2);
            SetMatrixValue(dgvX0, 1, 0, 0);

            SetMatrixSize(dgvV, 1, 1);
            SetMatrixValue(dgvV, 0, 0, J);
        }

        private void LoadScheme2()
        {
            // RC цепь: R=1000 Ом, C=0.001 Ф, V=5 В
            txtStateNames.Text = "uC";
            txtOutputNames.Text = "i, uR";
            txtSourceNames.Text = "Vin";

            double R = 1000, C = 0.001;

            SetMatrixSize(dgvA, 1, 1);
            SetMatrixValue(dgvA, 0, 0, -1 / (R * C));

            SetMatrixSize(dgvB, 1, 1);
            SetMatrixValue(dgvB, 0, 0, 1 / (R * C));

            SetMatrixSize(dgvC, 2, 1);
            SetMatrixValue(dgvC, 0, 0, 1 / R);
            SetMatrixValue(dgvC, 1, 0, -1);

            SetMatrixSize(dgvD, 2, 1);
            SetMatrixValue(dgvD, 0, 0, -1 / R);
            SetMatrixValue(dgvD, 1, 0, 1);

            SetMatrixSize(dgvX0, 1, 1);
            SetMatrixValue(dgvX0, 0, 0, 0);

            SetMatrixSize(dgvV, 1, 1);
            SetMatrixValue(dgvV, 0, 0, 5);

            nudDuration.Value = 5;
            nudTimeStep.Value = 0.001m;
        }

        private void LoadScheme3()
        {
            // RL цепь: R=100 Ом, L=0.5 Гн, V=10 В
            txtStateNames.Text = "iL";
            txtOutputNames.Text = "uL, uR";
            txtSourceNames.Text = "Vin";

            double R = 100, L = 0.5;

            SetMatrixSize(dgvA, 1, 1);
            SetMatrixValue(dgvA, 0, 0, -R / L);

            SetMatrixSize(dgvB, 1, 1);
            SetMatrixValue(dgvB, 0, 0, 1 / L);

            SetMatrixSize(dgvC, 2, 1);
            SetMatrixValue(dgvC, 0, 0, -R);
            SetMatrixValue(dgvC, 1, 0, R);

            SetMatrixSize(dgvD, 2, 1);
            SetMatrixValue(dgvD, 0, 0, 1);
            SetMatrixValue(dgvD, 1, 0, 0);

            SetMatrixSize(dgvX0, 1, 1);
            SetMatrixValue(dgvX0, 0, 0, 0);

            SetMatrixSize(dgvV, 1, 1);
            SetMatrixValue(dgvV, 0, 0, 10);

            nudDuration.Value = 0.05m;
            nudTimeStep.Value = 0.0001m;
        }

        private void LoadScheme4()
        {
            // Колебательный контур: L=0.01 Гн, C=0.0001 Ф, R=10 Ом
            txtStateNames.Text = "uC, iL";
            txtOutputNames.Text = "Энергия";
            txtSourceNames.Text = "нет";

            double L = 0.01, C = 0.0001, R = 10;

            SetMatrixSize(dgvA, 2, 2);
            SetMatrixValue(dgvA, 0, 0, 0);
            SetMatrixValue(dgvA, 0, 1, -1 / C);
            SetMatrixValue(dgvA, 1, 0, 1 / L);
            SetMatrixValue(dgvA, 1, 1, -R / L);

            SetMatrixSize(dgvB, 2, 1);
            SetMatrixValue(dgvB, 0, 0, 0);
            SetMatrixValue(dgvB, 1, 0, 0);

            SetMatrixSize(dgvC, 1, 2);
            SetMatrixValue(dgvC, 0, 0, 0);
            SetMatrixValue(dgvC, 0, 1, 1);

            SetMatrixSize(dgvD, 1, 1);
            SetMatrixValue(dgvD, 0, 0, 0);

            SetMatrixSize(dgvX0, 2, 1);
            SetMatrixValue(dgvX0, 0, 0, 10);
            SetMatrixValue(dgvX0, 1, 0, 0);

            SetMatrixSize(dgvV, 1, 1);
            SetMatrixValue(dgvV, 0, 0, 0);

            nudDuration.Value = 0.1m;
            nudTimeStep.Value = 0.00005m;
        }

        private void SetMatrixSize(DataGridView dgv, int rows, int cols)
        {
            dgv.Columns.Clear();
            dgv.Rows.Clear();

            for (int i = 0; i < cols; i++)
            {
                dgv.Columns.Add($"col{i}", $"[{i}]");
                dgv.Columns[i].Width = 100;
            }

            for (int i = 0; i < rows; i++)
            {
                dgv.Rows.Add();
                dgv.Rows[i].HeaderCell.Value = $"[{i}]";
            }
        }

        private void SetMatrixValue(DataGridView dgv, int row, int col, double value)
        {
            dgv.Rows[row].Cells[col].Value = value;
        }

        private double[,] GetMatrixFromGrid(DataGridView dgv)
        {
            int rows = dgv.Rows.Count;
            int cols = dgv.Columns.Count;
            double[,] matrix = new double[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    var value = dgv.Rows[i].Cells[j].Value;
                    matrix[i, j] = value != null ? Convert.ToDouble(value) : 0;
                }
            }

            return matrix;
        }

        private void BtnSimulate_Click(object sender, EventArgs e)
        {
            try
            {
                lblStatus.Text = "Моделирование...";
                lblStatus.ForeColor = Color.Orange;
                Application.DoEvents();

                // Получение матриц
                double[,] A = GetMatrixFromGrid(dgvA);
                double[,] B = GetMatrixFromGrid(dgvB);
                double[,] C = GetMatrixFromGrid(dgvC);
                double[,] D = GetMatrixFromGrid(dgvD);
                double[,] X0 = GetMatrixFromGrid(dgvX0);
                double[,] V = GetMatrixFromGrid(dgvV);

                double h = (double)nudTimeStep.Value;
                double duration = (double)nudDuration.Value;

                // Моделирование
                var result = SimulateSystem(A, B, C, D, X0, V, h, duration);

                // Построение графиков
                PlotResults(result);

                lblStatus.Text = "Моделирование завершено успешно!";
                lblStatus.ForeColor = Color.Green;

                mainTabs.SelectedIndex = 1;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                lblStatus.Text = "Ошибка моделирования";
                lblStatus.ForeColor = Color.Red;
            }
        }

        private SimulationResult SimulateSystem(double[,] A, double[,] B, double[,] C, double[,] D,
            double[,] X0, double[,] V, double h, double duration)
        {
            int n = A.GetLength(0);
            int m = V.GetLength(0);
            int k = C.GetLength(0);

            int N = (int)(duration / h);
            List<double> time = new List<double>();
            List<double[]> X_history = new List<double[]>();
            List<double[]> Y_history = new List<double[]>();

            double[] X = new double[n];
            for (int i = 0; i < n; i++)
                X[i] = X0[i, 0];

            double[] v = new double[m];
            for (int i = 0; i < m; i++)
                v[i] = V[i, 0];

            double t = 0;

            for (int step = 0; step <= N; step++)
            {
                time.Add(t);
                X_history.Add((double[])X.Clone());

                // Y = C*X + D*V
                double[] Y = new double[k];
                for (int i = 0; i < k; i++)
                {
                    Y[i] = 0;
                    for (int j = 0; j < n; j++)
                        Y[i] += C[i, j] * X[j];
                    for (int j = 0; j < m; j++)
                        Y[i] += D[i, j] * v[j];
                }
                Y_history.Add(Y);

                // dX/dt = A*X + B*V
                double[] dX = new double[n];
                for (int i = 0; i < n; i++)
                {
                    dX[i] = 0;
                    for (int j = 0; j < n; j++)
                        dX[i] += A[i, j] * X[j];
                    for (int j = 0; j < m; j++)
                        dX[i] += B[i, j] * v[j];
                }

                // Метод Эйлера: X(n+1) = X(n) + h*dX/dt
                for (int i = 0; i < n; i++)
                    X[i] += h * dX[i];

                t += h;
            }

            return new SimulationResult
            {
                Time = time,
                States = X_history,
                Outputs = Y_history
            };
        }

        private void PlotResults(SimulationResult result)
        {
            string[] stateNames = txtStateNames.Text.Split(',').Select(s => s.Trim()).ToArray();
            string[] outputNames = txtOutputNames.Text.Split(',').Select(s => s.Trim()).ToArray();

            Color[] colors = { Color.Blue, Color.Red, Color.Green, Color.Purple, Color.Orange, Color.Brown };

            // График состояний
            chartStates.Series.Clear();
            for (int i = 0; i < result.States[0].Length; i++)
            {
                string name = i < stateNames.Length ? stateNames[i] : $"X[{i}]";
                Series series = new Series(name)
                {
                    ChartType = SeriesChartType.Line,
                    Color = colors[i % colors.Length],
                    BorderWidth = 2
                };

                for (int t = 0; t < result.Time.Count; t++)
                    series.Points.AddXY(result.Time[t], result.States[t][i]);

                chartStates.Series.Add(series);
            }
            chartStates.ChartAreas[0].RecalculateAxesScale();

            // График выходов
            chartOutputs.Series.Clear();
            for (int i = 0; i < result.Outputs[0].Length; i++)
            {
                string name = i < outputNames.Length ? outputNames[i] : $"Y[{i}]";
                Series series = new Series(name)
                {
                    ChartType = SeriesChartType.Line,
                    Color = colors[i % colors.Length],
                    BorderWidth = 2
                };

                for (int t = 0; t < result.Time.Count; t++)
                    series.Points.AddXY(result.Time[t], result.Outputs[t][i]);

                chartOutputs.Series.Add(series);
            }
            chartOutputs.ChartAreas[0].RecalculateAxesScale();
        }

        private class SimulationResult
        {
            public List<double> Time { get; set; }
            public List<double[]> States { get; set; }
            public List<double[]> Outputs { get; set; }
        }


    }
}