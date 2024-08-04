using Npgsql;
using Dapper;
using greener_api.models;
using System.Security.Principal;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<NpgsqlConnection>((serviceProvider) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var connectionString = configuration.GetConnectionString("DefaultConnection");
    return new NpgsqlConnection(connectionString);
});

var app = builder.Build();


#region GUser

app.MapGet("/users", async (NpgsqlConnection db) =>
{
    var accountList = new List<GUser>();

    try
    {
        accountList = (List<GUser>)await db.QueryAsync<GUser>("select * from public.g_user;");
    }
    catch (Exception e)
    {
    }

    return Results.Ok(accountList);
});

app.MapGet("/users/{id}", async (int id, NpgsqlConnection db) =>
{
    GUser user = null;

    try
    {
        user = await db.QueryFirstOrDefaultAsync<GUser>("SELECT * FROM public.g_user WHERE id = @Id;", new { Id = id });
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }

    if (user == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(user);
});

app.MapPost("/user", async (GUser newUser, NpgsqlConnection db) =>
{
    try
    {
        var insertedId = await db.QuerySingleAsync<int>(
            "INSERT INTO public.g_user (username) VALUES (@Username) RETURNING id;",
            new { newUser }
        );

        // Optionally, you can return the created entity with its new ID
        newUser.Id = insertedId;
        return Results.Created($"/user/{insertedId}", newUser);
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }
});


app.MapPut("/users/{id}", async (int id, GUser updatedUser, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync(
            "UPDATE public.g_user SET username = @Username WHERE id = @Id;",
            new { updatedUser.Username, Id = id }
        );

        if (rowsAffected == 0)
        {
            return Results.NotFound();
        }
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }

    return Results.NoContent();
});

app.MapDelete("/users/{id}", async (int id, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.g_user WHERE id = @Id;", new { Id = id });

        if (rowsAffected == 0)
        {
            return Results.NotFound();
        }
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }

    return Results.NoContent();
});

#endregion

//#region Account

//app.MapGet("/account", async (NpgsqlConnection db) =>
//{
//    var accountList = new List<Account>();

//    try
//    {
//        accountList = (List<Account>)await db.QueryAsync<Account>("select * from public.account;");
//    }
//    catch (Exception e)
//    {
//    }

//    return Results.Ok(accountList);
//});

//app.MapGet("/account/{id}", async (int id, NpgsqlConnection db) =>
//{
//    GUser user = null;

//    try
//    {
//        user = await db.QueryFirstOrDefaultAsync<Account>("SELECT * FROM public.g_user WHERE id = @Id;", new { Id = id });
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    if (user == null)
//    {
//        return Results.NotFound();
//    }

//    return Results.Ok(user);
//});

//app.MapPut("/account/{id}", async (int id, GUser updatedUser, NpgsqlConnection db) =>
//{
//    try
//    {
//        var rowsAffected = await db.ExecuteAsync(
//            "UPDATE public.g_user SET name = @Name, email = @Email WHERE id = @Id;",
//            new { Name = updatedUser.Name, Email = updatedUser.Email, Id = id }
//        );

//        if (rowsAffected == 0)
//        {
//            return Results.NotFound();
//        }
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    return Results.NoContent();
//});

//app.MapDelete("/account/{id}", async (int id, NpgsqlConnection db) =>
//{
//    try
//    {
//        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.g_user WHERE id = @Id;", new { Id = id });

//        if (rowsAffected == 0)
//        {
//            return Results.NotFound();
//        }
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    return Results.NoContent();
//});

//#endregion

//#region Transaction

//app.MapGet("/account", async (NpgsqlConnection db) =>
//{
//    var accountList = new List<Account>();
//    try
//    {
//        accountList = (List<Account>)await db.QueryAsync<Account>("select * from public.account;");
//    }
//    catch (Exception e)
//    {
//    }

//    return Results.Ok(accountList);
//});

//app.MapGet("/account/{id}", async (int id, NpgsqlConnection db) =>
//{
//    GUser user = null;

//    try
//    {
//        user = await db.QueryFirstOrDefaultAsync<Account>("SELECT * FROM public.g_user WHERE id = @Id;", new { Id = id });
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    if (user == null)
//    {
//        return Results.NotFound();
//    }

//    return Results.Ok(user);
//});

//app.MapPut("/account/{id}", async (int id, GUser updatedUser, NpgsqlConnection db) =>
//{
//    try
//    {
//        var rowsAffected = await db.ExecuteAsync(
//            "UPDATE public.g_user SET name = @Name, email = @Email WHERE id = @Id;",
//            new { Name = updatedUser.Name, Email = updatedUser.Email, Id = id }
//        );

//        if (rowsAffected == 0)
//        {
//            return Results.NotFound();
//        }
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    return Results.NoContent();
//});

//app.MapDelete("/account/{id}", async (int id, NpgsqlConnection db) =>
//{
//    try
//    {
//        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.g_user WHERE id = @Id;", new { Id = id });

//        if (rowsAffected == 0)
//        {
//            return Results.NotFound();
//        }
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    return Results.NoContent();
//});

//#endregion

//#region Category

//app.MapGet("/account", async (NpgsqlConnection db) =>
//{
//    var accountList = new List<Account>();

//    try
//    {
//        accountList = (List<Account>)await db.QueryAsync<Account>("select * from public.account;");
//    }
//    catch (Exception e)
//    {
//    }

//    return Results.Ok(accountList);
//});

//app.MapGet("/account/{id}", async (int id, NpgsqlConnection db) =>
//{
//    GUser user = null;

//    try
//    {
//        user = await db.QueryFirstOrDefaultAsync<Account>("SELECT * FROM public.g_user WHERE id = @Id;", new { Id = id });
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    if (user == null)
//    {
//        return Results.NotFound();
//    }

//    return Results.Ok(user);
//});

//app.MapPut("/account/{id}", async (int id, GUser updatedUser, NpgsqlConnection db) =>
//{
//    try
//    {
//        var rowsAffected = await db.ExecuteAsync(
//            "UPDATE public.g_user SET name = @Name, email = @Email WHERE id = @Id;",
//            new { Name = updatedUser.Name, Email = updatedUser.Email, Id = id }
//        );

//        if (rowsAffected == 0)
//        {
//            return Results.NotFound();
//        }
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    return Results.NoContent();
//});

//app.MapDelete("/account/{id}", async (int id, NpgsqlConnection db) =>
//{
//    try
//    {
//        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.g_user WHERE id = @Id;", new { Id = id });

//        if (rowsAffected == 0)
//        {
//            return Results.NotFound();
//        }
//    }
//    catch (Exception e)
//    {
//        return Results.Problem(detail: e.Message);
//    }

//    return Results.NoContent();
//});

//#endregion



app.Run();
