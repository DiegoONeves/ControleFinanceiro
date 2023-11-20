namespace ControleFinanceiro.Entities
{
    public class Movimentacao
    {
        public Guid Codigo { get; set; }
        public Guid? CodigoCartao { get; set; } = null;
        public Guid? CodigoParcelamento { get; set; }
        public Guid CodigoMovimentacaoTipo { get; set; }
        public Guid CodigoMovimentacaoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataMovimentacao { get; set; }
        public DateTime DataHora { get; set; }
        public DateTime DataDaCompra { get; set; }
        public bool Baixado { get; set; } = false;
        public MovimentacaoCategoria MovimentacaoCategoria { get; set; } = new();
        public MovimentacaoTipo MovimentacaoTipo { get; set; } = new();
        public Cartao? Cartao { get; set; } = null;
        public Parcelamento Parcelamento { get; set; } = null;
        public bool Essencial { get; set; } = false;

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Valor: {Valor.ToString("C")}";
        }

    }
}
