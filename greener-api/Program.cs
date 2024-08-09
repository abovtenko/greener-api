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

app.MapGet("/user/{id}", async (int id, NpgsqlConnection db) =>
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
            newUser
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

app.MapPut("/user/{id}", async (int id, GUser updatedUser, NpgsqlConnection db) =>
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

app.MapDelete("/user/{id}", async (int id, NpgsqlConnection db) =>
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

#region Account

app.MapGet("/account/{userId}", async (int userId, int? accountId, NpgsqlConnection db) =>
{
    var accountList = new List<Account>();

    try
    {
        var query = "select * from public.account where user_id = @UserId";
        var payload = new { UserId = userId, AccountId = 0 };

        if (accountId.HasValue)
        {
            query += " and id = @AccountId";
            payload = new { UserId = userId, AccountId = accountId.Value };
        }

        query += ";"; 
        accountList = (List<Account>)await db.QueryAsync<Account>(query, payload);
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }

    return Results.Ok(accountList);
});

app.MapPost("/account", async (Account updatedAccount, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync(
            "INSERT INTO public.account (user_id, provider, name, type) VALUES (@UserId, @Name, @Provider, @Type) RETURNING id;",
            updatedAccount
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

app.MapPut("/account/{id}", async (int id, Account pAccount, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync(
            "UPDATE public.account SET name = @Name, Provider = @Provider, @Type = type WHERE id = @Id;",
            pAccount
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

app.MapDelete("/account/{id}", async (int id, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.account WHERE id = @Id;", new { Id = id });

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

#region Transaction

app.MapGet("/transaction/{accountId}", async (int accountId, NpgsqlConnection db) =>
{
    var transactionList = new List<Transaction>();

    try
    {
        transactionList = (List<Transaction>)await db.QueryAsync<Transaction>("select * from public.transaction where account_id = @AccountId;",
            new { AccountId = accountId });
    }
    catch (Exception e)
    {
        Results.Problem(detail: e.Message);
    }

    return Results.Ok(transactionList);
});

app.MapPost("/transaction", async (Transaction pTransaction, NpgsqlConnection db) =>
{
    try
    {
        var insertedRowId = await db.QuerySingleAsync<int>("INSERT INTO public.transaction (account_id, description, credit, debit, date) " +
            " VALUES (@AccountId, @Description, @Credit, @Debit, @Date) RETURNING id;", pTransaction);

        pTransaction.Id = insertedRowId;

        return Results.Created($"/transaction/{insertedRowId}", pTransaction);
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }
});

app.MapPut("/transaction/{id}", async (int id, Transaction pTransaction, NpgsqlConnection db) =>
{
    try
    {
        var query = @"UPDATE public.transaction
            SET 
                description = @Description,
                credit = @credit,
                debit = @Debit,
                date = @Date
            WHERE 
                id = @Id;";

        var rowsAffected = await db.ExecuteAsync(query, pTransaction);

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

app.MapDelete("/transaction/{id}", async (int id, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.transaction WHERE id = @Id;", new { Id = id });

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

#region Category

app.MapGet("/category/{userId}", async (int userId, NpgsqlConnection db) =>
{
    var categoryList = new List<Category>();

    try
    {
        categoryList = (List<Category>)await db.QueryAsync<Category>("select * from public.category where user_id = @Id;",
            new { Id = userId });
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }

    return Results.Ok(categoryList);
});

app.MapPost("/category", async (Category pCategory, NpgsqlConnection db) =>
{
    try
    {
        var insertedId = await db.QuerySingleAsync<int>("INSERT INTO (user_id, category_id, name) VALUES (@UserId, @CategoryId, @Name) RETURNING id; ", pCategory);

        pCategory.Id = insertedId;

        return Results.Created($"/category/{pCategory.UserId}?categoryId={insertedId}", pCategory);
    }
    catch (Exception e)
    {
        return Results.Problem(detail: e.Message);
    }
});

app.MapPut("/category/{id}", async (int id, Category pCategory, NpgsqlConnection db) =>
{
    try
    {
        var query = @"UPDATE public.category
            SET 
                user_id = @UserId,
                category_id = @CategoryId,
                name = @Name
            WHERE 
                id = @Id;";

        var rowsAffected = await db.ExecuteAsync(query, pCategory);

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

app.MapDelete("/category/{id}", async (int id, NpgsqlConnection db) =>
{
    try
    {
        var rowsAffected = await db.ExecuteAsync("DELETE FROM public.category WHERE id = @Id;", new { Id = id });

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



app.Run();
