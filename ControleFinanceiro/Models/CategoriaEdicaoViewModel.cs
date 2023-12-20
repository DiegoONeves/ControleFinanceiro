using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class CategoriaEdicaoViewModel
    {
        [Display(Name = "Código")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public Guid Codigo { get; set; }

        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Ativo?")]
        public bool Ativo { get; set; }

        public List<SelectListItem> CategoriasPais { get; set; } = new();
        [Display(Name = "Categoria pai")]
        public Guid? CodigoCategoriaPai { get; set; } = null;
    }
}
