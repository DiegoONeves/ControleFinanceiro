namespace ControleFinanceiro.Entities
{
    public class Movimentacao
    {
        public Guid Codigo { get; set; }
        public Guid? CodigoCartao { get; set; } = null;
        public Guid? CodigoParcelamento { get; set; } = null;
        public Guid? CodigoMovimentacaoRecorrente { get; set; } = null;
        public Guid CodigoTipo { get; set; }
        public Guid CodigoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public DateTime DataHora { get; set; }
        public DateTime DataDaCompra { get; set; }
        public bool Baixado { get; set; } = false;
        public Categoria Categoria { get; set; } = new();
        public Tipo Tipo { get; set; } = new();
        public Cartao? Cartao { get; set; } = null;
        public Parcelamento? Parcelamento { get; set; } = null;
        public MovimentacaoRecorrente? MovimentacaoRecorrente { get; set; } = null;
        public bool DespesaFixa { get; set; } = false;

        public bool MovimentacaoIsAvulsa() => CodigoParcelamento is null && CodigoMovimentacaoRecorrente is null;

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Valor: {Valor.ToString("C")}";
        }

    }
}
