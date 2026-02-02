var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<AccountAPI.Repositories.IAccountRepository, AccountAPI.Repositories.AccountRepository>();
builder.Services.AddScoped<AccountAPI.Repositories.IAccountFileRepository, AccountAPI.Repositories.AccountFileRepository>();
builder.Services.AddScoped<AccountAPI.Services.IAccountService, AccountAPI.Services.AccountService>();
builder.Services.AddScoped<AccountAPI.Services.IAccountFileService, AccountAPI.Services.AccountFileService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
