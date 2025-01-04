namespace ControleFinanceiro.Models
{
    public class MovimentacaoRecorrenteViewModel
    {
        public Guid Codigo { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Categoria { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataVencimento { get; set; }
        public bool Finalizado { get; set; }
    }
}
