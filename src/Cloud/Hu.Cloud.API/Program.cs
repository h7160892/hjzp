using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// 添加服务
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS - 仅允许内网访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("InternalOnly", policy =>
    {
        policy.WithOrigins("http://localhost", "http://127.0.0.1")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 中间件管道
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("InternalOnly");
app.UseAuthorization();
app.MapControllers();

app.Run();
