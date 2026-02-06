using ecfrInsights.Data;
using ecfrInsights.Data.Entities;
using ecfrInsights.DisplayModels;
using ecfrInsights.Services;

using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<EcfrContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddHttpClient<DataRetrievalService>();
builder.Services.AddHttpClient<XmlRetrievalService>();

builder.Services.AddScoped<DataRetrievalService>();
builder.Services.AddScoped<XmlRetrievalService>();
builder.Services.AddSingleton<TaskProgressService>();
builder.Services.AddScoped<DataAnalyticsService>();
builder.Services.AddScoped<AgencyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();


//add api endpoint for getting title complexity
app.MapGet("/api/analytics/compute-titles/{date?}", async (DateTime? date, DataAnalyticsService dataAnalyticsService) =>
{
    List<CfrTitleComplexity> complexities = await dataAnalyticsService.CalculateTitleComplexities(date);
    return Results.Ok(complexities);
});

//add api endpoint for getting title complexity
app.MapGet("/api/analytics/titles/{date?}", async (DateTime? date, DataAnalyticsService dataAnalyticsService) =>
{
    List<CfrTitleComplexity> complexities = await dataAnalyticsService.GetTitleComplexities(date);
    return Results.Ok(complexities);
});
//add api endpoint for getting title complexity
app.MapGet("/api/analytics/compute-agency/{date?}", async (DateTime? date, DataAnalyticsService dataAnalyticsService) =>
{
    List<AgencyStatistics> complexities = await dataAnalyticsService.CalculateAgencyStatistics(date);
    return Results.Ok(complexities);
});

//add api endpoint for getting title complexity
app.MapGet("/api/analytics/agencies/{date?}", async (DateTime? date, DataAnalyticsService dataAnalyticsService) =>
{
    List<AgencyStatistics> complexities = await dataAnalyticsService.GetAgencyStatistics(date);
    return Results.Ok(complexities);
});

app.MapGet("/api/taskprogress/{taskId}", async (string taskId, TaskProgressService taskProgressService) =>
{
    TaskProgress? progress = taskProgressService.GetTaskProgress(taskId);
    if (progress == null)
    {
        return Results.NotFound();
    }
    return Results.Ok(progress);
});

app.MapGet("/api/taskprogress/active", async (TaskProgressService taskProgressService) =>
{
    List<TaskProgress> tasks = taskProgressService.GetAllActiveTasks();
    return Results.Ok(tasks);
});
app.MapGet("/api/taskprogress/all", async (TaskProgressService taskProgressService) =>
{
    List<TaskProgress> tasks = taskProgressService.GetAllTasks();
    return Results.Ok(tasks);
});
//check if there are pending migrations and apply them
using (IServiceScope scope = app.Services.CreateScope())
{
    EcfrContext dbContext = scope.ServiceProvider.GetRequiredService<EcfrContext>();
    if (dbContext.Database.GetPendingMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
}

app.Run();
