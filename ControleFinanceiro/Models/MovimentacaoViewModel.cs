using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoViewModel
    {
        [Display(Name = "Período")]
        public string Periodo { get; set; } = string.Empty;
        public string Direcao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public IEnumerable<MovimentacaoItemViewModel> Movimentacoes { get; set; }
    }
}
