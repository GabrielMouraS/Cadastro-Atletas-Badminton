using BadmintonCadastro.Models;

namespace BadmintonCadastro.UI.Forms;

internal class CategoriaEditForm : Form
{
    public Categoria Resultado { get; } = new();

    private readonly TextBox       _txtCodigo  = new() { MaxLength = 20 };
    private readonly TextBox       _txtDesc    = new() { MaxLength = 200 };
    private readonly ComboBox      _cmbTipo    = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _nudChave   = new() { DecimalPlaces = 1, Minimum = 0, Maximum = 9999, Increment = 1 };

    public CategoriaEditForm(Categoria? existente = null)
    {
        if (existente != null)
        {
            Resultado.Id            = existente.Id;
            Resultado.Codigo        = existente.Codigo;
            Resultado.Descricao     = existente.Descricao;
            Resultado.Tipo          = existente.Tipo;
            Resultado.ChavePlanilha = existente.ChavePlanilha;
        }

        Text = Resultado.Id == 0 ? "Nova Categoria" : "Editar Categoria";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(400, 220);

        _cmbTipo.Items.AddRange(new[] { "SIMPLES", "DUPLA", "MISTA" });

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 5,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 4; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _txtCodigo.Dock = DockStyle.Fill;
        _txtDesc.Dock   = DockStyle.Fill;
        _cmbTipo.Dock   = DockStyle.Fill;

        layout.Controls.Add(Rotulo("Código:"),         0, 0); layout.Controls.Add(_txtCodigo, 1, 0);
        layout.Controls.Add(Rotulo("Descrição:"),      0, 1); layout.Controls.Add(_txtDesc,   1, 1);
        layout.Controls.Add(Rotulo("Tipo:"),           0, 2); layout.Controls.Add(_cmbTipo,   1, 2);
        layout.Controls.Add(Rotulo("Chave Planilha:"), 0, 3); layout.Controls.Add(_nudChave,  1, 3);

        var btnSalvar   = new Button { Text = "Salvar",   Width = 90 };
        var btnCancelar = new Button { Text = "Cancelar", Width = 90, DialogResult = DialogResult.Cancel };
        btnSalvar.Click += Salvar_Click;

        var btnFlow = new FlowLayoutPanel { Anchor = AnchorStyles.Right | AnchorStyles.Bottom, AutoSize = true };
        btnFlow.Controls.AddRange(new Control[] { btnSalvar, btnCancelar });
        layout.Controls.Add(btnFlow, 1, 4);

        Controls.Add(layout);
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        _txtCodigo.Text         = Resultado.Codigo;
        _txtDesc.Text           = Resultado.Descricao ?? string.Empty;
        _cmbTipo.SelectedItem   = string.IsNullOrEmpty(Resultado.Tipo) ? null : Resultado.Tipo;
        _nudChave.Value         = (decimal)Resultado.ChavePlanilha;
    }

    private void Salvar_Click(object? s, EventArgs e)
    {
        Resultado.Codigo        = _txtCodigo.Text.Trim();
        Resultado.Descricao     = string.IsNullOrWhiteSpace(_txtDesc.Text) ? null : _txtDesc.Text.Trim();
        Resultado.Tipo          = _cmbTipo.SelectedItem?.ToString() ?? string.Empty;
        Resultado.ChavePlanilha = (double)_nudChave.Value;

        if (string.IsNullOrWhiteSpace(Resultado.Codigo))   { Aviso("Código é obrigatório.");             return; }
        if (string.IsNullOrEmpty(Resultado.Tipo))          { Aviso("Selecione o tipo.");                  return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    private void Aviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
