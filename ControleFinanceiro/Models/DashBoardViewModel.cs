namespace ControleFinanceiro.Models
{
    public class DashBoardViewModel
    {
        public decimal DividaTotal { get; set; }
        public short ParcelamentosAtivos { get; set; }
        public List<DashboardDividaPorCategoriaViewModel> DividaPorCategoria { get; set; } = new();
    }
}
