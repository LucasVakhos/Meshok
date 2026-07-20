namespace CsvToExcel;

public sealed class MainForm : Form
{
    private readonly Button _convertButton;
    private readonly Label _statusLabel;

    public MainForm()
    {
        Text = "CSV → Excel";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(520, 220);
        MinimumSize = new Size(460, 230);
        MaximizeBox = false;
        Font = new Font("Segoe UI", 10F);
        BackColor = Color.FromArgb(244, 247, 245);

        Label titleLabel = new()
        {
            AutoSize = true,
            Text = "Конвертер CSV в Excel",
            Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold),
            ForeColor = Color.FromArgb(23, 32, 42),
            Margin = new Padding(0, 0, 0, 8)
        };

        Label descriptionLabel = new()
        {
            AutoSize = true,
            Text = "Выберите CSV-файл — приложение предложит сохранить его как .xlsx.",
            ForeColor = Color.FromArgb(102, 114, 127),
            Margin = new Padding(0, 0, 0, 18)
        };

        _convertButton = new Button
        {
            Text = "Выбрать CSV и конвертировать",
            AutoSize = false,
            Height = 46,
            Dock = DockStyle.Top,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(10, 127, 120),
            ForeColor = Color.White,
            Cursor = Cursors.Hand,
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold)
        };
        _convertButton.FlatAppearance.BorderSize = 0;
        _convertButton.Click += ConvertButton_Click;

        _statusLabel = new Label
        {
            AutoSize = true,
            Text = "Поддерживаются CSV в UTF-8 и Windows-1251.",
            ForeColor = Color.FromArgb(102, 114, 127),
            Margin = new Padding(0, 12, 0, 0)
        };

        TableLayoutPanel content = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(34, 28, 34, 24),
            ColumnCount = 1,
            RowCount = 4
        };
        content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        content.Controls.Add(titleLabel, 0, 0);
        content.Controls.Add(descriptionLabel, 0, 1);
        content.Controls.Add(_convertButton, 0, 2);
        content.Controls.Add(_statusLabel, 0, 3);
        Controls.Add(content);

        AcceptButton = _convertButton;
    }

    private async void ConvertButton_Click(object? sender, EventArgs e)
    {
        using OpenFileDialog openDialog = new()
        {
            Title = "Выберите CSV-файл",
            Filter = "CSV-файлы (*.csv)|*.csv|Все файлы (*.*)|*.*",
            CheckFileExists = true,
            Multiselect = false,
            RestoreDirectory = true
        };
        if (openDialog.ShowDialog(this) != DialogResult.OK)
            return;

        using SaveFileDialog saveDialog = new()
        {
            Title = "Сохранить Excel-файл",
            Filter = "Книга Excel (*.xlsx)|*.xlsx",
            AddExtension = true,
            DefaultExt = "xlsx",
            FileName = Path.GetFileNameWithoutExtension(openDialog.FileName) + ".xlsx",
            InitialDirectory = Path.GetDirectoryName(openDialog.FileName),
            OverwritePrompt = true,
            RestoreDirectory = true
        };
        if (saveDialog.ShowDialog(this) != DialogResult.OK)
            return;

        SetBusy(true, "Конвертация…");
        try
        {
            ConversionResult result = await Task.Run(
                () => CsvExcelConverter.Convert(openDialog.FileName, saveDialog.FileName));

            _statusLabel.Text = $"Готово: {result.Rows:N0} строк, {result.Columns} столбцов.";
            MessageBox.Show(
                this,
                $"Excel-файл успешно создан.\n\n{saveDialog.FileName}\n\n" +
                $"Строк: {result.Rows:N0}\nСтолбцов: {result.Columns}\nКодировка: {result.EncodingName}",
                "CSV → Excel",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Не удалось выполнить конвертацию.";
            MessageBox.Show(
                this,
                ex.Message,
                "Ошибка конвертации",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false, _statusLabel.Text);
        }
    }

    private void SetBusy(bool busy, string status)
    {
        _convertButton.Enabled = !busy;
        _convertButton.Text = busy ? "Подождите…" : "Выбрать CSV и конвертировать";
        _statusLabel.Text = status;
        UseWaitCursor = busy;
    }
}
