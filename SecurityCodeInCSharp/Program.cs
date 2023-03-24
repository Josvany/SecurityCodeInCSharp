using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecurityCodeInCSharp.Data;
using SecurityCodeInCSharp.Areas.Identity.Data;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("SecurityCodeInCSharpContextConnection") ?? throw new InvalidOperationException("Connection string 'SecurityCodeInCSharpContextConnection' not found.");

builder.Services.AddDbContext<SecurityCodeInCSharpContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<SecurityCodeInCSharpUser>(options =>
                                                               {
                                                                   options.User.RequireUniqueEmail = true; // requerir emailUnico
                                                                   options.User.AllowedUserNameCharacters = "abcdfghijklmnopqrstvwxyz123456789@-."; // caracteres que se pueden utilizar
                                                                                                                                      
                                                                   options.SignIn.RequireConfirmedAccount = true; // requiere confirmacion de cuenta
                                                                   options.SignIn.RequireConfirmedEmail = true; // requiere confirmacion de email
                                                                   options.SignIn.RequireConfirmedPhoneNumber = false; // no requiere confirmacion por numero de telefono

                                                                   options.Password.RequiredLength = 12; // minimo de caracteres en la contraseña
                                                                   options.Password.RequireLowercase = true; // requiere caracteres en minusculas
                                                                   options.Password.RequireUppercase = true; // requiere caracteres en mayusculas
                                                                   options.Password.RequireDigit = true; // requiere de numeros
                                                                   options.Password.RequireNonAlphanumeric = true; // no requiere caracteres alfanumericos
                                                                   options.Password.RequiredUniqueChars = 10; // cantidad de caracteres unicos requeridos

                                                                   //opciones de bloqueo
                                                                   options.Lockout.AllowedForNewUsers = true; // permitido para nuevos usuarios
                                                                   options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // bloquear 5 minutos cuando fallan la cantidad de intentos por autenticarse
                                                                   options.Lockout.MaxFailedAccessAttempts = 5; // bloquear usuario despues de 5 intentos por authenticarse
                                                               }
                                                              ).AddEntityFrameworkStores<SecurityCodeInCSharpContext>();

// Add services to the container.
builder.Services.AddRazorPages();

//valida automaticamente los tokens antiforgery en cualquier formulario dentro de nuestro site
builder.Services.AddControllersWithViews(conf =>
{
    conf.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});

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
