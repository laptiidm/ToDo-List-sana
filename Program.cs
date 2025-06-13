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
		StorageType.Xml => new XmlTaskRepository(options.XmlFilePath ?? throw new InvalidOperationException("The XML file path is not configured.")),
		StorageType.Database => new DbTaskRepository(options.DBConnectionString),
		_ => throw new NotSupportedException($"Storage type '{storageService.GetCurrentStorageType()}' is not supported")
	};
});

// Реєстрація GraphQL компонентів (без middleware)
builder.Services.AddSingleton<TaskType>();
builder.Services.AddSingleton<TaskInputType>();
builder.Services.AddSingleton<GraphQLStorageType>();
builder.Services.AddScoped<TaskQuery>();
builder.Services.AddScoped<TaskMutation>();
builder.Services.AddScoped<ISchema, TaskSchema>();
builder.Services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
builder.Services.AddSingleton<IDocumentSerializer, GraphQL.NewtonsoftJson.DocumentSerializer>();

var app = builder.Build();

// Конфігурація пайплайну
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();



// *1 
//	As a result, StorageOptions receives data from two sources:
// 1. Automatically from appsettings.json ("XmlFilePath")
// 2. Manually from the connection string section ("DBConnectionString")

//*2
//	This block registers the ITaskRepository interface with the Dependency 
//	Injection (DI) container using a factory pattern.Instead of directly 
//	mapping ITaskRepository to a single concrete implementation 
//	(like DBTaskRepository or XmlTaskRepository), it dynamically decides
//	which concrete repository to instantiate based on the StorageType 
//	determined by IStorageSelectionService.// Ensure the required GraphQL package is installed in your project.  
// You can install it using the following command in the terminal:  
// dotnet add package GraphQL.Server  


