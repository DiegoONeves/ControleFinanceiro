using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoEdicaoViewModel
    {
        public Guid Codigo { get; set; }
        public Guid? CodigoParcelamento { get; set; } = null;
        public Guid? CodigoMovimentacaoRecorrente { get; set; } = null;
 
        public List<SelectListItem> CartoesDePagamento { get; set; } = new();
        public List<SelectListItem> Tipos { get; set; } = new();
        public List<SelectListItem> Categorias { get; set; } = new();

        [Display(Name = "Tipo de movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoTipo { get; set; }

        [Display(Name = "Categoria da movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoCategoria { get; set; }
        [Display(Name = "Cartão de pagamento")]
        public Guid? CodigoCartao { get; set; } = null;

        [Display(Name = "Descrição da movimentação")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public decimal Valor { get; set; }

        [Display(Name = "Data da compra")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public DateOnly DataDaCompra { get; set; }
        [Display(Name = "Data de movimentação/vencimento")]
        public DateOnly DataMovimentacao { get; set; }
        public bool EstadoDoCampoCodigoCartaoDeCredito { get; set; }


    }
}
