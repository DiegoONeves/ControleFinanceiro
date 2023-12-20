using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoRecorrenteEdicaoViewModel
    {
        public Guid Codigo { get; set; }
        public List<SelectListItem> Tipos { get; set; } = new();
        public List<SelectListItem> Categorias { get; set; } = new();

        [Display(Name = "Tipo de movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoTipo { get; set; }

        [Display(Name = "Categoria da movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid CodigoCategoria { get; set; }

        [Display(Name = "Descrição da movimentação")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public decimal Valor { get; set; }


        [Display(Name = "Despesa fixa?")]
        public bool DespesaFixa { get; set; }

        [Display(Name = "Data da primeira movimentação")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public DateOnly DataDaPrimeiraMovimentacao { get; set; }

        [Display(Name = "Quantidade de movimentações")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public short QuantidadeMovimentacao { get; set; }
    }
}
