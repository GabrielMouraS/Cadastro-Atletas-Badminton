using BadmintonCadastro.Models;
using BadmintonCadastro.Services;

namespace BadmintonCadastro.UI.Forms;

internal class InscricaoEditForm : Form
{
    public Inscricao Resultado { get; } = new();

    private readonly ComboBox      _cmbCategoria  = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox      _cmbAtleta1    = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly ComboBox      _cmbAtleta2    = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly NumericUpDown _nudRkInterno  = new() { Minimum = 0, Maximum = 9999, Value = 0 };
    private readonly NumericUpDown _nudRkEstadual = new() { Minimum = 0, Maximum = 9999, Value = 0 };
    private readonly CheckBox      _chkRemanej    = new() { Text = string.Empty };
    private readonly TextBox       _txtObsRemanej = new() { MaxLength = 500 };
    private readonly NumericUpDown _nudValor      = new() { DecimalPlaces = 2, Minimum = 0, Maximum = 99999, Increment = 1 };
    private readonly CheckBox      _chkPago       = new() { Text = string.Empty };

    private readonly List<Atleta> _atletas;

    public InscricaoEditForm(Torneio torneio, Inscricao? existente = null)
    {
        Resultado.TorneioId = torneio.Id;

        if (existente != null)
        {
            Resultado.Id                 = existente.Id;
            Resultado.TorneioId          = existente.TorneioId;
            Resultado.CategoriaId        = existente.CategoriaId;
            Resultado.Atleta1Id          = existente.Atleta1Id;
            Resultado.Atleta2Id          = existente.Atleta2Id;
            Resultado.RkInterno          = existente.RkInterno;
            Resultado.RkEstadual         = existente.RkEstadual;
            Resultado.AceitaRemanejamento = existente.AceitaRemanejamento;
            Resultado.ObsRemanejamento   = existente.ObsRemanejamento;
            Resultado.Valor              = existente.Valor;
            Resultado.Pago               = existente.Pago;
        }

        Text = Resultado.Id == 0 ? "Nova Inscrição" : "Editar Inscrição";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        ClientSize = new System.Drawing.Size(460, 360);

        _atletas = AtletasService.ListarComEntidade().ToList();

        ConfigurarComboBoxes();

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            ColumnCount = 2,
            RowCount = 10,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < 9; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _cmbCategoria.Dock  = DockStyle.Fill;
        _cmbAtleta1.Dock    = DockStyle.Fill;
        _cmbAtleta2.Dock    = DockStyle.Fill;
        _txtObsRemanej.Dock = DockStyle.Fill;

        layout.Controls.Add(Rotulo("Categoria:"),       0, 0); layout.Controls.Add(_cmbCategoria,  1, 0);
        layout.Controls.Add(Rotulo("Atleta 1:"),        0, 1); layout.Controls.Add(_cmbAtleta1,    1, 1);
        layout.Controls.Add(Rotulo("Atleta 2:"),        0, 2); layout.Controls.Add(_cmbAtleta2,    1, 2);
        layout.Controls.Add(Rotulo("RK Interno:"),      0, 3); layout.Controls.Add(_nudRkInterno,  1, 3);
        layout.Controls.Add(Rotulo("RK Estadual:"),     0, 4); layout.Controls.Add(_nudRkEstadual, 1, 4);
        layout.Controls.Add(Rotulo("Remanejamento:"),   0, 5); layout.Controls.Add(_chkRemanej,    1, 5);
        layout.Controls.Add(Rotulo("Obs. Remanej.:"),   0, 6); layout.Controls.Add(_txtObsRemanej, 1, 6);
        layout.Controls.Add(Rotulo("Valor (R$):"),      0, 7); layout.Controls.Add(_nudValor,      1, 7);
        layout.Controls.Add(Rotulo("Pago:"),            0, 8); layout.Controls.Add(_chkPago,       1, 8);

        var btnSalvar   = new Button { Text = "Salvar",   Width = 90 };
        var btnCancelar = new Button { Text = "Cancelar", Width = 90, DialogResult = DialogResult.Cancel };
        btnSalvar.Click += Salvar_Click;

        var btnFlow = new FlowLayoutPanel { Anchor = AnchorStyles.Right | AnchorStyles.Bottom, AutoSize = true };
        btnFlow.Controls.AddRange(new Control[] { btnSalvar, btnCancelar });
        layout.Controls.Add(btnFlow, 1, 9);

        Controls.Add(layout);
        AcceptButton = btnSalvar;
        CancelButton = btnCancelar;

        PreencherValores();
    }

    private void ConfigurarComboBoxes()
    {
        // Categorias
        var categorias = CategoriasService.Listar().ToList();
        _cmbCategoria.DataSource    = categorias;
        _cmbCategoria.DisplayMember = "Codigo";
        _cmbCategoria.ValueMember   = "Id";

        // Atleta 1
        _cmbAtleta1.DataSource    = _atletas.ToList();
        _cmbAtleta1.DisplayMember = "NomeCompleto";
        _cmbAtleta1.ValueMember   = "Id";

        // Atleta 2 — inclui opção "nenhum"
        var atletasComNenhum = new List<Atleta> { new() { Id = 0, NomeCompleto = "(nenhum)" } };
        atletasComNenhum.AddRange(_atletas);
        _cmbAtleta2.DataSource    = atletasComNenhum;
        _cmbAtleta2.DisplayMember = "NomeCompleto";
        _cmbAtleta2.ValueMember   = "Id";
        _cmbAtleta2.SelectedIndex = 0;
    }

    private void PreencherValores()
    {
        if (Resultado.CategoriaId > 0) _cmbCategoria.SelectedValue = Resultado.CategoriaId;
        if (Resultado.Atleta1Id   > 0) _cmbAtleta1.SelectedValue   = Resultado.Atleta1Id;

        _cmbAtleta2.SelectedValue = Resultado.Atleta2Id ?? 0;

        _nudRkInterno.Value  = Resultado.RkInterno  ?? 0;
        _nudRkEstadual.Value = Resultado.RkEstadual ?? 0;
        _chkRemanej.Checked  = Resultado.AceitaRemanejamento;
        _txtObsRemanej.Text  = Resultado.ObsRemanejamento ?? string.Empty;
        _nudValor.Value      = (decimal)Resultado.Valor;
        _chkPago.Checked     = Resultado.Pago;
    }

    private void Salvar_Click(object? s, EventArgs e)
    {
        Resultado.CategoriaId         = _cmbCategoria.SelectedValue is int cid ? cid : 0;
        Resultado.Atleta1Id           = _cmbAtleta1.SelectedValue  is int a1  ? a1  : 0;
        int atleta2Id                 = _cmbAtleta2.SelectedValue  is int a2  ? a2  : 0;
        Resultado.Atleta2Id           = atleta2Id > 0 ? atleta2Id : null;
        Resultado.RkInterno           = (int)_nudRkInterno.Value  > 0 ? (int)_nudRkInterno.Value  : null;
        Resultado.RkEstadual          = (int)_nudRkEstadual.Value > 0 ? (int)_nudRkEstadual.Value : null;
        Resultado.AceitaRemanejamento = _chkRemanej.Checked;
        Resultado.ObsRemanejamento    = string.IsNullOrWhiteSpace(_txtObsRemanej.Text) ? null : _txtObsRemanej.Text.Trim();
        Resultado.Valor               = (double)_nudValor.Value;
        Resultado.Pago                = _chkPago.Checked;

        if (Resultado.CategoriaId <= 0)   { Aviso("Selecione uma categoria.");  return; }
        if (Resultado.Atleta1Id   <= 0)   { Aviso("Selecione o Atleta 1.");     return; }
        if (Resultado.Atleta2Id == Resultado.Atleta1Id) { Aviso("Atleta 1 e Atleta 2 não podem ser o mesmo."); return; }

        DialogResult = DialogResult.OK;
        Close();
    }

    private static Label Rotulo(string texto) =>
        new() { Text = texto, TextAlign = System.Drawing.ContentAlignment.MiddleRight, Dock = DockStyle.Fill };

    private void Aviso(string msg) =>
        MessageBox.Show(msg, "Atenção", MessageBoxButtons.OK, MessageBoxIcon.Warning);
}
