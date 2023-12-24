using ControleFinanceiro.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<MovimentacaoService>();
builder.Services.AddScoped<ParcelamentoService>();
builder.Services.AddScoped<TipoService>();
builder.Services.AddScoped<CategoriaService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<CartaoBandeiraService>();
builder.Services.AddScoped<CartaoService>();
builder.Services.AddScoped<MovimentacaoRecorrenteService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
