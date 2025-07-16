using Todo_List_3.Configurations;
using Todo_List_3.Repositories;
using Todo_List_3.Services;
using Todo_List_3.GraphQL;
using GraphQL;
using GraphQL.Types;
using GraphQL.NewtonsoftJson;


var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<StorageOptions>(options =>
{
	options.DBConnectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	options.XmlFilePath = builder.Configuration.GetSection("StorageOptions:XmlFilePath").Get<string>();
});

// Додавання сервісів
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();

// NEWTONSOFT.JSON ОКРЕМО
builder.Services.AddControllersWithViews()
	.AddNewtonsoftJson();

// settings providers, role: provide configuration values '​​in isolation'
builder.Services.AddScoped<IXmlRepositorySettingsProvider, XmlRepositorySettingsProvider>();
builder.Services.AddScoped<IDatabaseRepositorySettingsProvider, DbRepositorySettingsProvider>();

// session
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30);
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

// automatically created and provided where needed
builder.Services.AddScoped<XmlTaskRepository>();
builder.Services.AddScoped<DbTaskRepository>();

// factory
builder.Services.AddScoped<IStorageSelectionService, StorageSelectionService>();


builder.Services.AddTransient<TaskType>();
builder.Services.AddTransient<TaskInputType>();
builder.Services.AddSingleton<TaskQuery>();
builder.Services.AddSingleton<TaskMutation>();
builder.Services.AddSingleton<ISchema>(services =>
	new TaskSchema(
		services.GetRequiredService<IServiceProvider>()
	));

//builder.Services.AddScoped<ISchema, TaskSchema>(); // constructor injection -> GraphQLController


builder.Services.AddGraphQL(configure =>
{
	configure
		.AddSystemTextJson() // or .AddNewtonsoftJson()
		.AddSchema<TaskSchema>()
		.AddUserContextBuilder<CustomUserContextBuilder>();
});

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

app.UseGraphQL<TaskSchema>("/graphql");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Tasks}/{action=Index}/{id?}");

app.Run();
