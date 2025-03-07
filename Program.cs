using CashoutServices.Partner;
using CashoutServices.Services;
using Serilog;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// Register Redis connection using "localhost:6379"
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect("localhost:6379") // Redis in Docker
);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day) // Logs to a file
    .WriteTo.MySQL(
        connectionString: "server=127.0.0.1;user id=charlie;Password=kickMyAss;persist security info=True;port=3306;database=cashoutservices;Connection Timeout=2000;Allow User Variables=True",
        tableName: "logs")
    .CreateLogger();


//builder.Host.UseSerilog(logger); // 🔥 Ensure .NET Core uses Serilog
builder.Host.UseSerilog(); // Use Serilog instead of default logger

// ✅ Register Services
builder.Services.AddScoped<ICashoutServices, CashoutService>();
builder.Services.AddScoped<IKredigramServices, KredigramServices>();

// ✅ Register Partner Implementations
builder.Services.AddScoped<DANACO>();
builder.Services.AddScoped<Gopay>();


var app = builder.Build();

app.UseSerilogRequestLogging(); // ✅ Log HTTP requests

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    Serilog.Debugging.SelfLog.Enable(Console.Error);
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
