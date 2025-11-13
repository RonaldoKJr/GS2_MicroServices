using Repository;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddScoped<IPromptRepository, PromptRepository>(provider =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Server=localhost;Database=fiap;User=root;Password=123;Port=3306;";

    return new PromptRepository(connectionString);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
