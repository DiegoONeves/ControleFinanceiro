using ControleFinanceiro.Entities;
using ControleFinanceiro.Models;
using Dapper;
using System.Data.SqlClient;
using System.Text;
using System.Transactions;

namespace ControleFinanceiro.Services
{
    public class MovimentacaoService : BaseService
    {
        private readonly MovimentacaoTipoService _movimentacaoTipoService;
        private readonly CategoriaService _movimentacaoCategoriaService;
        private readonly CartaoDeCreditoService _cartaoDeCreditoService;

        public MovimentacaoService(IConfiguration config,
            MovimentacaoTipoService movimentacaoTipoService,
            CategoriaService movimentacaoCategoriaService,
            CartaoDeCreditoService cartaoDeCreditoService) : base(config)
        {
            _movimentacaoTipoService = movimentacaoTipoService;
            _movimentacaoCategoriaService = movimentacaoCategoriaService;
            _cartaoDeCreditoService = cartaoDeCreditoService;
        }

        public void AbrirTransacaoParaInserirNovaMovimentacao(MovimentacaoNovaViewModel model)
        {
            using TransactionScope scope = new();
            var tipo = _movimentacaoTipoService.Obter(model.CodigoMovimentacaoTipo).First();
            model.Valor = tipo.Descricao.ToLower() != "entrada" ? model.Valor * -1 : model.Valor;
            model.DataMovimentacao = model.CodigoCartaoDeCredito is null ? model.DataDaCompra : _cartaoDeCreditoService.ObterDataDaParcela(model.CodigoCartaoDeCredito.Value, model.DataDaCompra);
            InserirMovimentacao(model);
            scope.Complete();
        }
        public void InserirMovimentacao(MovimentacaoNovaViewModel model)
        {
            Movimentacao movimentacao = new()
            {
                Codigo = Guid.NewGuid(),
                CodigoCartaoDeCredito = model.CodigoCartaoDeCredito,
                DataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra),
                CodigoParcelamento = model.CodigoParcelamento,
                Descricao = model.Descricao,
                CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria,
                CodigoMovimentacaoTipo = model.CodigoMovimentacaoTipo,
                DataMovimentacao = CommonHelper.ConverterDateOnlyParaDateTime(model.DataMovimentacao),
                Valor = model.Valor,
                DataHora = DateTime.Now
            };
            InsertSQL(movimentacao);

        }

        public MovimentacaoViewModel BuscarMovimentacoes(MovimentacaoPesquisaViewModel? model = null)
        {
            MovimentacaoViewModel resultado = new()
            {
                Movimentacoes = new List<MovimentacaoItemViewModel> { }
            };
            string mes = DateTime.Now.Month.ToString().PadLeft(2, '0');
            string ano = DateTime.Now.Year.ToString();

            if (model?.Periodo is not null)
            {
                if (model.Periodo.Length is 7)
                {
                    mes = model.Periodo.Split("/")[0].PadLeft(2, '0');
                    ano = model.Periodo.Split("/")[1];

                    if (model.Direcao is not null)
                    {
                        var d = new DateTime(int.Parse(ano), int.Parse(mes), 1);
                        int direcaoInt = 1;

                        if (model.Direcao == "Anterior")
                            direcaoInt = -1;

                        d = d.AddMonths(direcaoInt);

                        mes = d.Month.ToString().PadLeft(2, '0');
                        ano = d.Year.ToString();
                    }

                }
            }

            var movimentacoes = SelectSQL(ano: short.Parse(ano), mes: short.Parse(mes));
            resultado.Movimentacoes = DePara(movimentacoes).OrderByDescending(x => x.DataDaCompra);
            resultado.Periodo = $"{mes}/{ano}";
            resultado.ValorTotalSaida = movimentacoes.Where(x => x.Valor < 0).Select(x => x.Valor).Sum();
            resultado.ValorTotalEntrada = movimentacoes.Where(x => x.Valor > 0).Select(x => x.Valor).Sum();
            resultado.ValorTotalDeParcelamento = movimentacoes.Where(x => x.Valor < 0 && x.CodigoParcelamento is not null).Select(x => x.Valor).Sum();

            resultado.Valor = resultado.Movimentacoes.Select(x => x.Valor).Sum();

            var categorias = movimentacoes.Where(x => x.MovimentacaoTipo.Descricao == "Saída").Select(x => x.MovimentacaoCategoria.Descricao).Distinct();

            foreach (var item in categorias)
            {
                resultado.TotaisPorCategoria.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = movimentacoes.Where(x => x.MovimentacaoCategoria.Descricao == item).Select(x => x.Valor).Sum()
                });
            }

            var categoriasParcelamentos = movimentacoes.Where(x => x.CodigoParcelamento is not null && x.MovimentacaoTipo.Descricao == "Saída").Select(x => x.MovimentacaoCategoria.Descricao).Distinct();

            foreach (var item in categoriasParcelamentos)
            {
                resultado.TotaisPorCategoriaParcelamentos.Add(new DashboardDividaPorCategoriaViewModel
                {
                    Categoria = item,
                    Valor = movimentacoes.Where(x => x.CodigoParcelamento is not null && x.MovimentacaoCategoria.Descricao == item).Select(x => x.Valor).Sum()
                });
            }

            resultado.TotaisPorCategoria = resultado.TotaisPorCategoria.OrderBy(x => x.Valor).ToList();
            resultado.TotaisPorCategoriaParcelamentos = resultado.TotaisPorCategoriaParcelamentos.OrderBy(x => x.Valor).ToList();
            return resultado;
        }


        private IEnumerable<MovimentacaoItemViewModel> DePara(List<Movimentacao> movimentacoes)
        {
            foreach (var item in movimentacoes)
            {
                yield return new MovimentacaoItemViewModel
                {
                    Codigo = item.Codigo,
                    CorDoTextDeValor = item.Valor < 0 ? "text-danger" : "text-success",
                    MeioDeParcelamento = item.Valor > 0 ? "Não se aplica" : item.CodigoCartaoDeCredito is null ? "Outro" : $"Crédito/{item?.CartaoDeCredito?.BandeiraCartao.Descricao}/{item?.CartaoDeCredito?.NumeroCartao.Substring(12, 4)}/{((item?.CartaoDeCredito?.Virtual ?? false) ? "Virtual" : "Físico")}",
                    Categoria = item.MovimentacaoCategoria.Descricao,
                    Tipo = item.MovimentacaoTipo.Descricao,
                    DataDaCompra = item.DataDaCompra,
                    DataMovimentacao = item.DataMovimentacao,
                    Descricao = item.Descricao,
                    Valor = item.Valor
                };
            }
        }


       

        public void AbrirTransacaoParaAtualizarMovimentacao(MovimentacaoEdicaoViewModel model)
        {
            if (model.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            using TransactionScope scope = new();
            var movimentacaoParaEditar = SelectSQL(model.Codigo).First();
            var tipoCadastradoNaBase = _movimentacaoTipoService.Obter(model.CodigoMovimentacaoTipo).First();
            if (movimentacaoParaEditar.CodigoParcelamento is null)
            {
                if (model.CodigoMovimentacaoTipo == Guid.Empty)
                    throw new Exception("O código do tipo de movimentação não foi informado");

                if (model.CodigoMovimentacaoCategoria == Guid.Empty)
                    throw new Exception("O código da categoria movimentação não foi informado");

                movimentacaoParaEditar.CodigoMovimentacaoTipo = tipoCadastradoNaBase.Codigo;
                movimentacaoParaEditar.CodigoMovimentacaoCategoria = model.CodigoMovimentacaoCategoria;
                if (tipoCadastradoNaBase.Descricao.ToLower() == "saída")
                {
                    movimentacaoParaEditar.CodigoCartaoDeCredito = model.CodigoCartaoDeCredito;
                    movimentacaoParaEditar.DataMovimentacao = CommonHelper.ConverterDateOnlyParaDateTime(model.CodigoCartaoDeCredito is null ? model.DataDaCompra : _cartaoDeCreditoService.ObterDataDaParcela(model.CodigoCartaoDeCredito.Value, model.DataDaCompra));
                }
                movimentacaoParaEditar.Valor = tipoCadastradoNaBase.Descricao.ToLower() != "entrada" ? model.Valor * -1 : model.Valor;
                movimentacaoParaEditar.DataDaCompra = CommonHelper.ConverterDateOnlyParaDateTime(model.DataDaCompra);
            }
            else
            {
                //se for parcelamento
                movimentacaoParaEditar.Valor = model.Valor * -1;
            }
            movimentacaoParaEditar.Descricao = model.Descricao;

            EditarMovimentacao(movimentacaoParaEditar);
            scope.Complete();
        }

        public void EditarMovimentacao(Movimentacao movimentacao)
        {
            if (movimentacao.Codigo == Guid.Empty)
                throw new Exception("O código da movimentação não foi informado");

            if (movimentacao.CodigoMovimentacaoTipo == Guid.Empty)
                throw new Exception("O código do tipo de movimentação não foi informado");

            if (movimentacao.CodigoMovimentacaoCategoria == Guid.Empty)
                throw new Exception("O código da categoria movimentação não foi informado");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("UPDATE Movimentacao SET DataDaCompra = @DataDaCompra, CodigoCartaoDeCredito = @CodigoCartaoDeCredito, DataMovimentacao = @DataMovimentacao,Valor = @Valor,CodigoMovimentacaoCategoria = @CodigoMovimentacaoCategoria,CodigoMovimentacaoTipo = @CodigoMovimentacaoTipo,Descricao = @Descricao WHERE Codigo = @Codigo", movimentacao);
        }

        public MovimentacaoEdicaoViewModel ObterParaEditar(Guid codigo)
        {
            var movimentacaoParaEditar = SelectSQL(codigo).First();
            return new MovimentacaoEdicaoViewModel
            {
                Codigo = movimentacaoParaEditar.Codigo,
                CodigoParcelamento = movimentacaoParaEditar.CodigoParcelamento,
                DataDaCompra = DateOnly.FromDateTime(movimentacaoParaEditar.DataDaCompra),
                CodigoCartaoDeCredito = movimentacaoParaEditar.CodigoCartaoDeCredito,
                CodigoMovimentacaoCategoria = movimentacaoParaEditar.CodigoMovimentacaoCategoria,
                CodigoMovimentacaoTipo = movimentacaoParaEditar.CodigoMovimentacaoTipo,
                DataMovimentacao = DateOnly.FromDateTime(movimentacaoParaEditar.DataMovimentacao),
                Descricao = movimentacaoParaEditar.Descricao,
                Valor = Math.Abs(movimentacaoParaEditar.Valor)
            };
        }

        #region Crud

        public List<Movimentacao> SelectSQL(
           Guid? codigo = null,
           Guid? codigoTipo = null,
           Guid? codigoCategoria = null,
           DateTime? dataMaiorOuIgualA = null,
           bool somenteParcelamentos = false,
           short? mes = null,
           short? ano = null)
        {
            StringBuilder sql = new();
            sql.AppendLine("SELECT A.*,B.*,C.*,D.*,E.* FROM Movimentacao A (NOLOCK) ");
            sql.AppendLine("INNER JOIN MovimentacaoCategoria B (NOLOCK) ON A.CodigoMovimentacaoCategoria = B.Codigo ");
            sql.AppendLine("INNER JOIN MovimentacaoTipo C (NOLOCK) ON A.CodigoMovimentacaoTipo = C.Codigo ");
            sql.AppendLine("LEFT JOIN CartaoDeCredito D (NOLOCK) ON A.CodigoCartaoDeCredito = D.Codigo ");
            sql.AppendLine("LEFT JOIN BandeiraCartao E (NOLOCK) ON D.CodigoBandeiraCartao = E.Codigo ");
            sql.AppendLine("WHERE A.Codigo = ISNULL(@Codigo,A.Codigo) ");
            sql.AppendLine("AND A.CodigoMovimentacaoTipo = ISNULL(@CodigoMovimentacaoTipo,A.CodigoMovimentacaoTipo) ");
            sql.AppendLine("AND A.CodigoMovimentacaoCategoria = ISNULL(@CodigoMovimentacaoCategoria,A.CodigoMovimentacaoCategoria)  ");
            sql.AppendLine("AND A.DataMovimentacao >= ISNULL(@DataMaiorOuIgualA,A.DataMovimentacao) ");

            if (somenteParcelamentos)
                sql.AppendLine("AND A.CodigoParcelamento IS NOT NULL");

            if (mes is not null)
                sql.AppendLine("AND MONTH(A.DataMovimentacao) = ISNULL(@Mes,MONTH(A.DataMovimentacao))");

            if (ano is not null)
                sql.AppendLine("AND YEAR(A.DataMovimentacao) = ISNULL(@Ano,YEAR(A.DataMovimentacao))");

            sql.AppendLine("ORDER BY A.DataMovimentacao DESC");

            using var conn = new SqlConnection(ConnectionString);
            return conn.Query<Movimentacao, MovimentacaoCategoria, MovimentacaoTipo, CartaoDeCredito, BandeiraCartao, Movimentacao>(sql.ToString(),
                (movimentacao, catagoria, tipo, cartaoDeCredito, bandeiraCartao) =>
                {
                    movimentacao.MovimentacaoCategoria = catagoria;
                    movimentacao.MovimentacaoTipo = tipo;

                    if (cartaoDeCredito is not null)
                    {
                        cartaoDeCredito.BandeiraCartao = bandeiraCartao;
                        movimentacao.CartaoDeCredito = cartaoDeCredito;
                    }

                    return movimentacao;

                }, new
                {
                    @Codigo = codigo,
                    @CodigoMovimentacaoTipo = codigoTipo,
                    @CodigoMovimentacaoCategoria = codigoCategoria,
                    @DataMaiorOuIgualA = dataMaiorOuIgualA,
                    @Mes = mes,
                    @Ano = ano
                }, splitOn: "Codigo").ToList();

        }
        public void DeleteSQL(Guid? codigo = null, Guid? codigoParcelamento = null)
        {
            if (codigo is null && codigoParcelamento is null)
                throw new Exception("Nenhum parâmetro foi fornecido para peração de exclusão de movimentação");

            using var conn = new SqlConnection(ConnectionString);
            conn.Execute("delete from Movimentacao where Codigo = ISNULL(@Codigo,Codigo) and CodigoParcelamento = ISNULL(@CodigoParcelamento,CodigoParcelamento)", new { @Codigo = codigo, @CodigoParcelamento = codigoParcelamento });
        }

        private void InsertSQL(Movimentacao movimentacao)
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Execute("insert into Movimentacao (Codigo,DataDaCompra,CodigoCartaoDeCredito,CodigoParcelamento,DataMovimentacao,DataHora,Valor,CodigoMovimentacaoCategoria,CodigoMovimentacaoTipo,Descricao) values (@Codigo,@DataDaCompra,@CodigoCartaoDeCredito,@CodigoParcelamento,@DataMovimentacao,@DataHora,@Valor,@CodigoMovimentacaoCategoria,@CodigoMovimentacaoTipo,@Descricao)", movimentacao);
        }

        #endregion

    }
}
