namespace ControleFinanceiro.Entities
{
    public class MovimentacaoRecorrente
    {
        public Guid Codigo { get; set; }
        public Guid CodigoTipo { get; set; }
        public Guid CodigoCategoria { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public DateTime DataHora { get; set; }
        public Categoria Categoria { get; set; } = new();
        public Tipo Tipo { get; set; } = new();
        public DateTime DataDaPrimeiraMovimentacao { get; set; }
        public short QuantidadeMovimentacao { get; set; }
        public DateTime DataDaUltimaMovimentacao { get; set; }

        public override string ToString()
        {
            return $"Codigo: {Codigo} - Valor: {Valor.ToString("C")}";
        }

    }
}
