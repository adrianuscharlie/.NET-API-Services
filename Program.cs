using CashoutServices.Partner;
using CashoutServices.Services;
using Serilog;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//var logger = new LoggerConfiguration()
//    .Enrich.FromLogContext()
//    .WriteTo.Console()  // Log to Console for debugging
//    .WriteTo.MySQL(
//        connectionString: builder.Configuration.GetConnectionString("connectionString"),
//        tableName: "applicationlogs",
//        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
//    )
//    .CreateLogger();

//builder.Host.UseSerilog(logger); // 🔥 Ensure .NET Core uses Serilog


// ✅ Register Services
builder.Services.AddScoped<ICashoutServices, CashoutService>();
builder.Services.AddScoped<IKredigramServices, KredigramServices>();

// ✅ Register Partner Implementations
builder.Services.AddScoped<DANACO>();
builder.Services.AddScoped<Gopay>();


var app = builder.Build();

//app.UseSerilogRequestLogging(); // ✅ Log HTTP requests

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
