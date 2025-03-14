using CashoutServices;
using CashoutServices.Partner;
using CashoutServices.Services;
using Serilog;
using StackExchange.Redis;
var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
    ConnectionMultiplexer.Connect(Function.GetConfiguration("ApplicationSettings:redisServices")) 
);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.MySQL(
        connectionString: Function.GetConfiguration("ApplicationSettings:connectionString"),
        tableName: "logs")
    .CreateLogger();


builder.Host.UseSerilog(); 

// ✅ Register Services
builder.Services.AddScoped<ICashoutServices, CashoutService>();
builder.Services.AddScoped<IKredigramServices, KredigramServices>();

// ✅ Register Partner Implementations
builder.Services.AddScoped<DANACO>();
builder.Services.AddScoped<GopayCO>();
builder.Services.AddScoped<ISakuCO>();
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never;
});



var app = builder.Build();

app.UseSerilogRequestLogging(); 

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
