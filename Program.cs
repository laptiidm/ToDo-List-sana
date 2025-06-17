using Microsoft.Extensions.Options;
using Todo_List_3.Configurations;
using Todo_List_3.Repositories;
using Todo_List_3.Enums;
using Todo_List_3.Models;
using Todo_List_3.Services;
using GraphQL;
using GraphQL.Types;
using Todo_List_3.GraphQL;
using GraphQL.NewtonsoftJson;

var builder = WebApplication.CreateBuilder(args);

// Конфігурація StorageOptions
builder.Services.Configure<StorageOptions>(
	builder.Configuration.GetSection("StorageOptions"));

builder.Services.Configure<StorageOptions>(options =>
{
	options.DBConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
});

// Додавання сервісів
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// Налаштування сесії
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// Реєстрація сервісів для роботи зі сховищами
builder.Services.AddScoped<IStorageSelectionService, StorageSelectionService>();
builder.Services.AddScoped<ITaskRepository>(provider =>
{
	var storageService = provider.GetRequiredService<IStorageSelectionService>();
	var options = provider.GetRequiredService<IOptions<StorageOptions>>().Value;

	if (string.IsNullOrEmpty(options.DBConnectionString))
	{
		throw new InvalidOperationException("The database connection string is not configured.");
	}

	return storageService.GetCurrentStorageType() switch
	{
		StorageType.Xml => new XmlTaskRepository(options.XmlFilePath
			?? throw new InvalidOperationException("The XML file path is not configured.")),
		StorageType.Database => new DbTaskRepository(options.DBConnectionString),
		_ => throw new NotSupportedException(
			$"Storage type '{storageService.GetCurrentStorageType()}' is not supported")
	};
});

// Реєстрація GraphQL компонентів (без middleware)
builder.Services.AddTransient<TaskType>();
builder.Services.AddTransient<TaskInputType>();
builder.Services.AddTransient<GraphQLStorageType>();
//builder.Services.AddSingleton<TaskType>();
//builder.Services.AddSingleton<TaskInputType>();
//builder.Services.AddSingleton<GraphQLStorageType>();
builder.Services.AddScoped<TaskQuery>();
builder.Services.AddScoped<TaskMutation>();
builder.Services.AddScoped<ISchema, TaskSchema>();
builder.Services.AddSingleton<IDocumentExecuter, DocumentExecuter>();

// <-- Заміна IDocumentSerializer на IGraphQLTextSerializer -->
builder.Services.AddSingleton<IGraphQLTextSerializer, GraphQLSerializer>();

builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
	{
		policy
		  .AllowAnyOrigin()
		  .AllowAnyMethod()
		  .AllowAnyHeader();
	});
});


var app = builder.Build();

// Конфігурація пайплайну
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();
