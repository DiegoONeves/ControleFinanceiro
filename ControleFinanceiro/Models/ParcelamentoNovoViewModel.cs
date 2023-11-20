using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class ParcelamentoNovoViewModel
    {
        public List<SelectListItem> Categorias { get; set; } = new();
        public List<SelectListItem> CartoesDePagamento { get; set; } = new();

        [Display(Name = "Categoria da movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoMovimentacaoCategoria { get; set; }

        [Display(Name = "Cartão de pagamento")]
        public Guid? CodigoCartao { get; set; } = null;

        [Display(Name = "Descrição da movimentação")]
        public string Descricao { get; set; }

        [Display(Name = "Quantidade de parcelas")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public short QuantidadeParcela { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public decimal Valor { get; set; }

        [Display(Name = "Data da compra")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public DateOnly DataDaCompra { get; set; }

        [Display(Name = "Essencial")]
        public bool Essencial { get; set; }


    }
}
