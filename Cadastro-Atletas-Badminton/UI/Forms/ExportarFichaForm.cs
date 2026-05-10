using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class ExportarFichaForm : Form
{
    private readonly Torneio  _torneio;
    private readonly ComboBox _cmbEntidade = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox _cmbTipoExp  = new() { DropDownStyle = ComboBoxStyle.DropDownList };

    public ExportarFichaForm(Torneio torneio)
    {
        _torneio = torneio;

        Text = $"Exportar Ficha — {torneio.Nome}";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(440, 210);

        ConfigurarComboBoxes();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 2,
            RowCount = 5,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var txtTorneio = new TextBox { Text = torneio.Nome, ReadOnly = true, Dock = DockStyle.Fill };
        _cmbEntidade.Dock = DockStyle.Fill;
        _cmbTipoExp.Dock  = DockStyle.Fill;

        layout.Controls.Add(Rotulo("Torneio:"),   0, 0); layout.Controls.Add(txtTorneio,   1, 0);
        layout.Controls.Add(Rotulo("Entidade:"),  0, 1); layout.Controls.Add(_cmbEntidade, 1, 1);
        layout.Controls.Add(Rotulo("Tipo ficha:"),0, 2); layout.Controls.Add(_cmbTipoExp,  1, 2);

        var lblInfo = new Label
        {
            Text = "O arquivo será salvo onde você escolher.\nAs fórmulas e a aba ATLETAS nunca são alteradas.",
            Dock = DockStyle.Fill,
            ForeColor = System.Drawing.SystemColors.GrayText,
            Font = new System.Drawing.Font(System.Drawing.SystemFonts.DefaultFont!.FontFamily, 8f)
        };
        layout.Controls.Add(lblInfo, 0, 3);
        layout.SetColumnSpan(lblInfo, 2);

        var btnExportar = new Button { Text = "Exportar…", Width = 100 };
        var btnCancelar = new Button { Text = "Cancelar",  Width = 90, DialogResult = DialogResult.Cancel };
        btnExportar.Click += BtnExportar_Click;

        var btnFlow = new FlowLayoutPanel { Anchor = AnchorStyles.Right | AnchorStyles.Bottom, AutoSize = true };
        btnFlow.Controls.AddRange(new Control[] { btnExportar, btnCancelar });
        layout.Controls.Add(btnFlow, 1, 4);

        Controls.Add(layout);
        CancelButton = btnCancelar;
    }

    private void ConfigurarComboBoxes()
    {
        // Entidades
        var entidades = EntidadesService.Listar().ToList();
        _cmbEntidade.DataSource    = entidades;
        _cmbEntidade.DisplayMember = "Sigla";
        _cmbEntidade.ValueMember   = "Id";

        // Tipo de exportação — depende do tipo_ficha do torneio
        if (_torneio.TipoFicha == "REGIONAL")
        {
            _cmbTipoExp.Items.Add(new TipoExportItem("Regional Classificatório", TipoExportPlanilha.RegionalClassificatorio));
            _cmbTipoExp.Items.Add(new TipoExportItem("Regional Amistoso",        TipoExportPlanilha.RegionalAmistoso));
        }
        else
        {
            _cmbTipoExp.Items.Add(new TipoExportItem("Simples + Duplas (Estadual/Interescolar)", TipoExportPlanilha.SimplesDuplas));
        }
        _cmbTipoExp.SelectedIndex = 0;
    }

    private void BtnExportar_Click(object? s, EventArgs e)
    {
        if (_cmbEntidade.SelectedItem is not Entidade entidade)
        {
            MessageBox.Show("Selecione uma entidade.", "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_cmbTipoExp.SelectedItem is not TipoExportItem tipoItem) return;

        using var dlg = new SaveFileDialog
        {
            Title       = "Salvar ficha de inscrição",
            Filter      = "Planilha Excel (*.xlsx)|*.xlsx",
            FileName    = $"Ficha_{entidade.Sigla}_{_torneio.Nome}_{DateTime.Now:yyyyMMdd}.xlsx",
            DefaultExt  = "xlsx"
        };

        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            PlanilhaService.ExportarFicha(_torneio, entidade, tipoItem.Tipo, dlg.FileName);
            MessageBox.Show(
                $"Ficha exportada com sucesso!\n\n{dlg.FileName}",
                "Exportação concluída",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Abrir a pasta do arquivo
            System.Diagnostics.Process.Start("explorer.exe", $"/select,\"{dlg.FileName}\"");
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Erro ao exportar:\n\n{ex.Message}", "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    // Auxiliar para ComboBox item com valor tipado
    private sealed class TipoExportItem(string texto, TipoExportPlanilha tipo)
    {
        public TipoExportPlanilha Tipo { get; } = tipo;
        public override string ToString() => texto;
    }
}
