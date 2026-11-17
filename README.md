# ComandaFácil

Sistema web de controle de mesas e comandas para lanchonetes, pensado para quem tem pouca familiaridade com tecnologia: botões grandes, cores por status da mesa e poucos toques por tarefa.

Projeto das disciplinas **Projeto de Desenvolvimento de Software I e II**, Engenharia de Software, UniFil.

**Autor:** Vinicius Bruno Pimenta

## Tecnologias

- C# / ASP.NET Core MVC (.NET 8)
- Entity Framework Core 8 + SQLite
- Razor + Bootstrap 5
- Login por cookie com senha em hash (`PasswordHasher`)
- Diagramas UML em PlantUML (`docs/diagramas`)

## Funcionalidades (casos de uso)

| Caso de uso | Ator |
|---|---|
| UC01 – Gerenciar produtos e categorias (CRUD) | Administrador |
| UC02 – Abrir comanda em uma mesa | Atendente |
| UC03 – Lançar/remover itens na comanda | Atendente |
| UC04 – Fechar comanda (total, forma de pagamento, libera a mesa) | Atendente |
| UC05 – Consultar vendas do dia | Administrador |

Status das mesas: **verde** livre, **vermelho** ocupada, **amarelo** aguardando pagamento.

## Como rodar

Pré-requisito: [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou mais novo (com o runtime do .NET 8 instalado).

```bash
git clone <url-do-repositorio>
cd ComandaFacil
dotnet run
```

Acesse `http://localhost:5080`. Na primeira execução, o banco `comandafacil.db` é criado pelas migrations da pasta `Migrations`, com 12 mesas, um cardápio de exemplo e dois usuários:

| Perfil | Usuário | Senha |
|---|---|---|
| Administrador | `admin` | `admin123` |
| Atendente | `atendente` | `atendente123` |

Para começar com o banco zerado, pare o sistema (Ctrl+C), apague os arquivos `comandafacil.db*` e rode `dotnet run` de novo.

Para alterar o modelo de dados, instale a ferramenta de migrations (`dotnet tool install --global dotnet-ef --version 8.0.11`) e crie uma nova migration com `dotnet ef migrations add NomeDaAlteracao`.

## Regras de negócio

- Só pode existir **uma comanda aberta por mesa** (validado no código e por índice único no banco).
- O item da comanda guarda o **preço unitário do momento do pedido**, então mudar o preço do produto não altera vendas antigas.
- Produto que já foi vendido não é apagado, só **desativado**, para manter o histórico.
- Só o Administrador acessa produtos, categorias e relatório.

## Estrutura

```
Controllers/   Conta, Mesas, Comandas, Categorias, Produtos, Relatorios
Data/          AppDbContext (mapeamento EF Core) e SeedData (dados iniciais)
Models/        Usuario, Mesa, Categoria, Produto, Comanda, ItemComanda e enums
ViewModels/    modelos das telas (painel, comanda, relatório, login)
Views/         telas Razor
wwwroot/css/   estilos (botões de no mínimo 48px, cores de status)
docs/          diagramas PlantUML (.puml) e imagens (.png)
```

## Usar PostgreSQL em vez de SQLite

1. `dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL --version 8.0.11`
2. Em `Program.cs`, troque `UseSqlite` por `UseNpgsql`.
3. Em `appsettings.json`, use `"Padrao": "Host=localhost;Database=comandafacil;Username=postgres;Password=suasenha"`.
4. No início do `Program.cs`, adicione `AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);`, porque o projeto grava datas com `DateTime.Now`.
5. Apague a pasta `Migrations` e rode `dotnet ef migrations add Inicial` de novo.
