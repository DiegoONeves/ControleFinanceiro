namespace ControleFinanceiro.Models
{
    public class DashboardDividaPorCategoriaViewModel
    {
        public string Categoria { get; set; } = string.Empty;
        public decimal Valor { get; set; } = 0;
        public decimal ValorMesAnterior { get; set; } = 0;
    }
}
