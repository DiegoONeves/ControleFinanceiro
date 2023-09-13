namespace ControleFinanceiro.Services
{
    public class BaseService
    {
        IConfiguration _config;
        public string ConnectionString { get; set; } = "";
        public BaseService(IConfiguration config)
        {
            _config = config;
            ConnectionString = _config.GetConnectionString("DefaultConnection") ?? "";
        }

    }
}
