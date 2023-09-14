namespace ControleFinanceiro.Entities
{
    public class Movimentacao
    {
        public Guid Codigo { get; set; }
        public Guid? CodigoCartaoDeCredito { get; set; } = null;
        public Guid? CodigoParcelamento { get; set; }
        public Guid CodigoMovimentacaoTipo { get; set; }
        public Guid CodigoMovimentacaoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public DateTime DataHora { get; set; }
        public DateTime DataDaCompra { get; set; }
        public MovimentacaoCategoria MovimentacaoCategoria { get; set; } = new();
        public MovimentacaoTipo MovimentacaoTipo { get; set; } = new();
        public CartaoDeCredito? CartaoDeCredito { get; set; } = null;

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Valor: {Valor.ToString("C")}";
        }

    }
}
