using BadmintonCadastro.Models;

namespace BadmintonCadastro.UI.Forms;

internal class TorneioEditForm : Form
{
    public Torneio Resultado { get; } = new();

    private readonly TextBox         _txtNome     = new() { MaxLength = 200 };
    private readonly ComboBox        _cmbTipo     = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DateTimePicker  _dtpData     = new() { Format = DateTimePickerFormat.Short, ShowCheckBox = true, Checked = false };
    private readonly TextBox         _txtLocal    = new() { MaxLength = 200 };
    private readonly TextBox         _txtRespNome = new() { MaxLength = 200 };
    private readonly TextBox         _txtRespTel  = new() { MaxLength = 50  };

    public TorneioEditForm(Torneio? existente = null)
    {
        if (existente != null)
        {
            Resultado.Id             = existente.Id;
            Resultado.Nome           = existente.Nome;
            Resultado.TipoFicha      = existente.TipoFicha;
            Resultado.DataInicio     = existente.DataInicio;
            Resultado.Local          = existente.Local;
            Resultado.ResponsavelNome = existente.ResponsavelNome;
            Resultado.ResponsavelTel  = existente.ResponsavelTel;
        }

        Text = Resultado.Id == 0 ? "Novo Torneio" : "Editar Torneio";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(440, 280);

        _cmbTipo.Items.AddRange(new[] { "ESTADUAL", "REGIONAL", "INTERESCOLAR" });

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 7,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 6; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _txtNome.Dock     = DockStyle.Fill;
        _cmbTipo.Dock     = DockStyle.Fill;
        _dtpData.Dock     = DockStyle.Fill;
        _txtLocal.Dock    = DockStyle.Fill;
        _txtRespNome.Dock = DockStyle.Fill;
        _txtRespTel.Dock  = DockStyle.Fill;

        layout.Controls.Add(Rotulo("Nome:"),         0, 0); layout.Controls.Add(_txtNome,     1, 0);
        layout.Controls.Add(Rotulo("Tipo Ficha:"),   0, 1); layout.Controls.Add(_cmbTipo,     1, 1);
        layout.Controls.Add(Rotulo("Data Início:"),  0, 2); layout.Controls.Add(_dtpData,     1, 2);
        layout.Controls.Add(Rotulo("Local:"),        0, 3); layout.Controls.Add(_txtLocal,    1, 3);
        layout.Controls.Add(Rotulo("Responsável:"),  0, 4); layout.Controls.Add(_txtRespNome, 1, 4);
        layout.Controls.Add(Rotulo("Telefone:"),     0, 5); layout.Controls.Add(_txtRespTel,  1, 5);

        var btnSalvar   = new Button { Text = "Salvar",   Width = 90 };
        var btnCancelar = new Button { Text = "Cancelar", Width = 90, DialogResult = DialogResult.Cancel };
        btnSalvar.Click += Salvar_Click;

        var btnFlow = new FlowLayoutPanel { Anchor = AnchorStyles.Right | AnchorStyles.Bottom, AutoSize = true };
        btnFlow.Controls.AddRange(new Control[] { btnSalvar, btnCancelar });
        layout.Controls.Add(btnFlow, 1, 6);

        Controls.Add(layout);
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        PreencherValores();
    }

    private void PreencherValores()
    {
        _txtNome.Text          = Resultado.Nome;
        _cmbTipo.SelectedItem  = string.IsNullOrEmpty(Resultado.TipoFicha) ? null : Resultado.TipoFicha;
        _txtLocal.Text         = Resultado.Local ?? string.Empty;
        _txtRespNome.Text      = Resultado.ResponsavelNome ?? string.Empty;
        _txtRespTel.Text       = Resultado.ResponsavelTel ?? string.Empty;

        if (Resultado.DataInicio.HasValue)
        {
            _dtpData.Checked = true;
            _dtpData.Value   = Resultado.DataInicio.Value;
        }
        else
        {
            _dtpData.Checked = false;
        }
    }

    private void Salvar_Click(object? s, EventArgs e)
    {
        Resultado.Nome           = _txtNome.Text.Trim();
        Resultado.TipoFicha      = _cmbTipo.SelectedItem?.ToString() ?? string.Empty;
        Resultado.DataInicio     = _dtpData.Checked ? _dtpData.Value.Date : null;
        Resultado.Local          = string.IsNullOrWhiteSpace(_txtLocal.Text)    ? null : _txtLocal.Text.Trim();
        Resultado.ResponsavelNome = string.IsNullOrWhiteSpace(_txtRespNome.Text) ? null : _txtRespNome.Text.Trim();
        Resultado.ResponsavelTel  = string.IsNullOrWhiteSpace(_txtRespTel.Text)  ? null : _txtRespTel.Text.Trim();

        if (string.IsNullOrWhiteSpace(Resultado.Nome))      { Aviso("Nome é obrigatório.");         return; }
        if (string.IsNullOrEmpty(Resultado.TipoFicha))      { Aviso("Selecione o tipo de ficha.");  return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    private void Aviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
