using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class CategoriaViewModel
    {
        [Display(Name = "Código")]
        public Guid Codigo { get; set; }

        [Display(Name = "Categoria")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Categoria pai")]
        public string DescricaoCategoriaPai { get; set; } = null;

        [Display(Name = "Ativa?")]
        public string Ativo { get; set; } = "Inativo";
    }
}
