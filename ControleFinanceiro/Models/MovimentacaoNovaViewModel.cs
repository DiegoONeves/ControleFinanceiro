using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoNovaViewModel
    {
        public List<SelectListItem> CartoesDeCredito { get; set; } = new();
        public List<SelectListItem> Tipos { get; set; } = new();
        public List<SelectListItem> Categorias { get; set; } = new();

        [Display(Name = "Tipo de movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoMovimentacaoTipo { get; set; }

        [Display(Name = "Categoria da movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoMovimentacaoCategoria { get; set; }

        [Display(Name = "Cartão de crédito")]
        public Guid? CodigoCartaoDeCredito { get; set; } = null;

        [Display(Name = "Descrição da movimentação")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public decimal Valor { get; set; }

        [Display(Name = "Data da compra")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public DateOnly DataDaCompra { get; set; }
        public DateOnly DataMovimentacao { get; set; }

        public Guid? CodigoParcelamento { get; set; }

    }
}
