var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers(option =>
{                                       // Return error if the content-type in header of request is not json (json is an default type in api)
    option.ReturnHttpNotAcceptable=true;// Return 406 Not Acceptable
}).AddNewtonsoftJson().AddXmlDataContractSerializerFormatters();//AddNewtonsoftJson() used for HttpPatch, AddXmlDataContractSerializerFormatters() means XML type is acceptable              
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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
