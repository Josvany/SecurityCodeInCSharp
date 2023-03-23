var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

// las mejores practicas dicta que el valor debe ser 120 dias maximo 365
// al incluir  los subdominios indica que solo se puede acceder al site mediante https
app.UseHsts(opt => opt.MaxAge(365).IncludeSubdomains());

//esto crea encabezados con la opcion nosniff
app.UseXContentTypeOptions();

// con esta se previenen automaticamente la mayoria de ataques de secuencia de comando entres sitios
// al habilitar el blackmode esto habilita las notificaciones cuando se hayan evitado ataques cross-site scripting
app.UseXXssProtection(conf => conf.EnabledWithBlockMode());

//se ocupa de los marcos y evita que alguien los marque en nuestra aplicacion fuera del dominio
// conf deny => bloquea todos los marcos
app.UseXfo(conf => conf.SameOrigin());

//evita que las url se capture en registros y otros entornos en casos que se contengan datos de naturaleza confidencial
// ejemplo : cuando se incluye una especie de token o id de usuario cuando se hacemos una transicion de un sitio a otro
//           la opcion noreferrer evita que todos los encabezados de referencia esten disponibles pero tambien puede usar noreferrer
//           al no degradar para evitarlos solo cuando se pasa de https a http
app.UseReferrerPolicy(conf => conf.NoReferrer());

// USECSP: ocupa dos tipos de recursos usa nuestra aplicacion y donde se encuentra
// por ejemplo -> los estilos y las secuencias de comandos solo se pueden cargar desde el mismo dominio
//                y tambien permitimos que se ejecute js en linea
app.UseCsp(conf =>
                conf.DefaultSources(s => s.Self())
                    .StyleSources(s => s.Self().UnsafeInline())
                    .ScriptSources(s => s.Self().UnsafeInline().UnsafeEval())
           );

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
