using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class AtletaEditForm : Form
{
    public Atleta Resultado { get; } = new();

    private readonly ComboBox        _cmbEntidade     = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox         _txtNome         = new() { MaxLength = 200 };
    private readonly NumericUpDown   _nudAno          = new();
    private readonly ComboBox        _cmbSexo         = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox         _txtCodFederacao = new() { MaxLength = 50 };

    public AtletaEditForm(Atleta? existente = null)
    {
        if (existente != null)
        {
            Resultado.Id              = existente.Id;
            Resultado.EntidadeId      = existente.EntidadeId;
            Resultado.NomeCompleto    = existente.NomeCompleto;
            Resultado.AnoNascimento   = existente.AnoNascimento;
            Resultado.Sexo            = existente.Sexo;
            Resultado.CodigoFederacao = existente.CodigoFederacao;
        }

        Text = Resultado.Id == 0 ? "Novo Atleta" : "Editar Atleta";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(420, 230);

        ConfigurarControles();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 6,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 5; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _cmbEntidade.Dock     = DockStyle.Fill;
        _txtNome.Dock         = DockStyle.Fill;
        _cmbSexo.Dock         = DockStyle.Fill;
        _txtCodFederacao.Dock = DockStyle.Fill;

        layout.Controls.Add(Rotulo("Entidade:"),      0, 0); layout.Controls.Add(_cmbEntidade,     1, 0);
        layout.Controls.Add(Rotulo("Nome completo:"), 0, 1); layout.Controls.Add(_txtNome,         1, 1);
        layout.Controls.Add(Rotulo("Ano nascimento:"),0, 2); layout.Controls.Add(_nudAno,          1, 2);
        layout.Controls.Add(Rotulo("Sexo:"),          0, 3); layout.Controls.Add(_cmbSexo,         1, 3);
        layout.Controls.Add(Rotulo("Cód. Federação:"),0, 4); layout.Controls.Add(_txtCodFederacao, 1, 4);

        var btnSalvar   = new Button { Text = "Salvar",   Width = 90 };
        var btnCancelar = new Button { Text = "Cancelar", Width = 90, DialogResult = DialogResult.Cancel };
        btnSalvar.Click += Salvar_Click;

        var btnFlow = new FlowLayoutPanel { Anchor = AnchorStyles.Right | AnchorStyles.Bottom, AutoSize = true };
        btnFlow.Controls.AddRange(new Control[] { btnSalvar, btnCancelar });
        layout.Controls.Add(btnFlow, 1, 5);

        Controls.Add(layout);
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        PreencherValores();
    }

    private void ConfigurarControles()
    {
        // Entidade
        var entidades = EntidadesService.Listar().ToList();
        _cmbEntidade.DataSource    = entidades;
        _cmbEntidade.DisplayMember = "Sigla";
        _cmbEntidade.ValueMember   = "Id";

        // Sexo
        _cmbSexo.Items.Add("M");
        _cmbSexo.Items.Add("F");

        // Ano nascimento
        _nudAno.Minimum = 1950;
        _nudAno.Maximum = DateTime.Now.Year;
        _nudAno.Value   = DateTime.Now.Year - 15;
    }

    private void PreencherValores()
    {
        if (Resultado.EntidadeId > 0)
            _cmbEntidade.SelectedValue = Resultado.EntidadeId;
        else if (_cmbEntidade.Items.Count > 0)
            _cmbEntidade.SelectedIndex = 0;

        _txtNome.Text          = Resultado.NomeCompleto;
        _nudAno.Value          = Resultado.AnoNascimento > 0 ? Resultado.AnoNascimento : (int)_nudAno.Value;
        _cmbSexo.SelectedItem  = string.IsNullOrEmpty(Resultado.Sexo) ? null : Resultado.Sexo;
        _txtCodFederacao.Text  = Resultado.CodigoFederacao ?? string.Empty;
    }

    private void Salvar_Click(object? s, EventArgs e)
    {
        Resultado.EntidadeId      = _cmbEntidade.SelectedValue is int id ? id : 0;
        Resultado.NomeCompleto    = _txtNome.Text.Trim();
        Resultado.AnoNascimento   = (int)_nudAno.Value;
        Resultado.Sexo            = _cmbSexo.SelectedItem?.ToString() ?? string.Empty;
        Resultado.CodigoFederacao = string.IsNullOrWhiteSpace(_txtCodFederacao.Text) ? null : _txtCodFederacao.Text.Trim();

        if (Resultado.EntidadeId <= 0)          { Aviso("Selecione uma entidade.");    return; }
        if (string.IsNullOrWhiteSpace(Resultado.NomeCompleto)) { Aviso("Nome é obrigatório."); return; }
        if (string.IsNullOrEmpty(Resultado.Sexo))              { Aviso("Selecione o sexo.");   return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    private void Aviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
