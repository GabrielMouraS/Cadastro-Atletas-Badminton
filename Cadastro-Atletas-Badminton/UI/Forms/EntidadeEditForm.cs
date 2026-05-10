using BadmintonCadastro.Models;

namespace BadmintonCadastro.UI.Forms;

internal class EntidadeEditForm : Form
{
    public Entidade Resultado { get; } = new();

    private readonly TextBox _txtSigla  = new() { MaxLength = 20 };
    private readonly TextBox _txtNome   = new() { MaxLength = 200 };
    private readonly TextBox _txtCidade = new() { MaxLength = 100 };

    public EntidadeEditForm(Entidade? existente = null)
    {
        if (existente != null)
        {
            Resultado.Id          = existente.Id;
            Resultado.Sigla       = existente.Sigla;
            Resultado.NomeCompleto = existente.NomeCompleto;
            Resultado.Cidade      = existente.Cidade;
        }

        Text = Resultado.Id == 0 ? "Nova Entidade" : "Editar Entidade";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(400, 190);

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 4,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        layout.Controls.Add(Rotulo("Sigla:"),   0, 0); layout.Controls.Add(_txtSigla,  1, 0);
        layout.Controls.Add(Rotulo("Nome:"),    0, 1); layout.Controls.Add(_txtNome,   1, 1);
        layout.Controls.Add(Rotulo("Cidade:"),  0, 2); layout.Controls.Add(_txtCidade, 1, 2);

        _txtSigla.Dock  = DockStyle.Fill;
        _txtNome.Dock   = DockStyle.Fill;
        _txtCidade.Dock = DockStyle.Fill;

        var btnSalvar   = new Button { Text = "Salvar",   Width = 90 };
        var btnCancelar = new Button { Text = "Cancelar", Width = 90, DialogResult = DialogResult.Cancel };

        btnSalvar.Click += Salvar_Click;

        var btnFlow = new FlowLayoutPanel { Anchor = AnchorStyles.Right | AnchorStyles.Bottom, AutoSize = true };
        btnFlow.Controls.AddRange(new Control[] { btnSalvar, btnCancelar });
        layout.Controls.Add(btnFlow, 1, 3);

        Controls.Add(layout);
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        // Preencher valores
        _txtSigla.Text  = Resultado.Sigla;
        _txtNome.Text   = Resultado.NomeCompleto;
        _txtCidade.Text = Resultado.Cidade ?? string.Empty;
    }

    private void Salvar_Click(object? s, EventArgs e)
    {
        Resultado.Sigla        = _txtSigla.Text.Trim();
        Resultado.NomeCompleto = _txtNome.Text.Trim();
        Resultado.Cidade       = string.IsNullOrWhiteSpace(_txtCidade.Text) ? null : _txtCidade.Text.Trim();

        if (string.IsNullOrWhiteSpace(Resultado.Sigla))      { Aviso("Sigla é obrigatória.");       return; }
        if (string.IsNullOrWhiteSpace(Resultado.NomeCompleto)) { Aviso("Nome é obrigatório."); return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    private void Aviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
