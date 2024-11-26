using API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDataContext>();

builder.Services.AddCors(
    options => options.AddPolicy("Total Acess",
        configs => configs
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod())
);

var app = builder.Build();


app.MapGet("/", () => "Prova A1");

//ENDPOINTS DE CATEGORIA
//GET: http://localhost:5273/api/categoria/listar
app.MapGet("/api/categoria/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Categorias.Any())
    {
        return Results.Ok(ctx.Categorias.ToList());
    }
    return Results.NotFound("Nenhuma categoria encontrada");
});

//POST: http://localhost:5273/api/categoria/cadastrar
app.MapPost("/api/categoria/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Categoria categoria) =>
{
    ctx.Categorias.Add(categoria);
    ctx.SaveChanges();
    return Results.Created("", categoria);
});

//ENDPOINTS DE TAREFA
//GET: http://localhost:5273/api/tarefas/listar
app.MapGet("/api/tarefas/listar", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Tarefas.Any())
    {
        return Results.Ok(ctx.Tarefas.Include(x => x.Categoria).ToList());
    }
    return Results.NotFound("Nenhuma tarefa encontrada");
});

//POST: http://localhost:5273/api/tarefas/cadastrar
app.MapPost("/api/tarefas/cadastrar", ([FromServices] AppDataContext ctx, [FromBody] Tarefa tarefa) =>
{
    Categoria? categoria = ctx.Categorias.Find(tarefa.CategoriaId);
    if (categoria == null)
    {
        return Results.NotFound("Categoria não encontrada");
    }
    tarefa.Categoria = categoria;
    ctx.Tarefas.Add(tarefa);
    ctx.SaveChanges();
    return Results.Created("", tarefa);
});

//PUT: http://localhost:5273/tarefas/alterar/{id}
app.MapPut("/api/tarefas/alterar/{id}", async ([FromServices] AppDataContext ctx, [FromBody] Tarefa TarefaAtualizada, [FromRoute] string id) =>
{
    var tarefa = await ctx.Tarefas
        .Include(t => t.Categoria)
        .FirstOrDefaultAsync(t => t.TarefaId == id);

    if (tarefa == null) return Results.NotFound("Tarefa não encontrada.");

    Categoria? categoria = ctx.Categorias.Find(tarefa.CategoriaId);

    // Atualizar os valores da tarefa
    tarefa.Titulo = TarefaAtualizada.Titulo ?? tarefa.Titulo;
    tarefa.Descricao = TarefaAtualizada.Descricao ?? tarefa.Descricao;
    if (categoria != null) {
        tarefa.Categoria = TarefaAtualizada.Categoria ?? tarefa.Categoria;
        tarefa.CategoriaId = TarefaAtualizada.CategoriaId ?? tarefa.CategoriaId;
    }

    if (tarefa.Status.Equals("Não iniciada")) {
        tarefa.Status = "Em andamento";
    } else tarefa.Status = "Concluído";

    // Salvar as alterações
    await ctx.SaveChangesAsync();

    return Results.Ok("Tarefa atualizada.");
});

//GET: http://localhost:5273/tarefas/naoconcluidas
app.MapGet("/api/tarefas/naoconcluidas", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Tarefas.Any())
    {
        List<Tarefa> tarefasNaoConluidas = new List<Tarefa>();

        foreach (var tarefa in ctx.Tarefas.Include(x => x.Categoria)) {
            if (!tarefa.Status.Equals("Concluído")) {
                tarefasNaoConluidas.Add(tarefa);
            }
        }
        return Results.Ok(tarefasNaoConluidas);
    }
    return Results.NotFound("Nenhuma tarefa encontrada");
});

//GET: http://localhost:5273/tarefas/concluidas
app.MapGet("/api/tarefas/concluidas", ([FromServices] AppDataContext ctx) =>
{
    if (ctx.Tarefas.Any())
    {
        List<Tarefa> tarefasConcluidas = new List<Tarefa>();

        foreach (var tarefa in ctx.Tarefas.Include(x => x.Categoria)) {
            if (tarefa.Status.Equals("Concluído")) {
                tarefasConcluidas.Add(tarefa);
            }
        }
        return Results.Ok(tarefasConcluidas);
    }
    return Results.NotFound("Nenhuma tarefa encontrada");
});

app.UseCors("Total Acess");
app.Run();
