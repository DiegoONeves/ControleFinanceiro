using System.ComponentModel.DataAnnotations;

namespace ControleFinanceiro.Models
{
    public class MovimentacaoCategoriaCadastroViewModel
    {
        [Display(Name = "Descrição")]
        [Required(ErrorMessage = "O campo {0} é obrigatório")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Ativo?")]
        public bool Ativo { get; set; }
    }
}
