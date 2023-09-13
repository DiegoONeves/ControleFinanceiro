using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoEdicaoViewModel
    {
        public Guid Codigo { get; set; }
        public List<SelectListItem> Tipos { get; set; } = new();
        public List<SelectListItem> Categorias { get; set; } = new();

        [Display(Name = "Tipo de Movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoMovimentacaoTipo { get; set; }

        [Display(Name = "Categoria da Movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoMovimentacaoCategoria { get; set; }

        [Display(Name = "Descrição da Movimentação")]
        public string Descricao { get; set; }

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public decimal Valor { get; set; }

        [Display(Name = "Data da Movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public DateOnly DataMovimentacao { get; set; }

    }
}
