using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class ParcelamentoEdicaoViewModel
    {
        public Guid Codigo { get; set; }
        public List<SelectListItem> Categorias { get; set; } = new();

        [Display(Name = "Categoria da Movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoMovimentacaoCategoria { get; set; }

        [Display(Name = "Descrição da Movimentação")]
        public string Descricao { get; set; }

        [Display(Name = "Quantidade de Parcelas")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public short QuantidadeParcela { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public decimal Valor { get; set; }

        [Display(Name = "Data da Primeira Parcela")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public DateOnly DataPrimeiraParcela { get; set; }
    }
}
