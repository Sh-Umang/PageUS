# PageUS

`PageUS` is a lightweight offset based pagination utility created to simplify paginated data handling in ASP.NET MVC apps — especially when working with **stored procedures** and server-side data sources.

---

## ⚡ Why PageUS?

Other pagination libraries often require fetching **all records** just to compute total pages — slowing down performance for large datasets.

`PageUS` solves this with:

- Efficient calculation of `Skip` and `Take` values
- Compatibility with **stored procedures**
- Optional setting of total records (if returned from DB) for full UI controls
- AJAX-based pagination UI generation with minimal setup

---

## ⚡ What's new?

`PageUS` v2.0.0 now supports pagination for IQueryable sources.

## 📦 Installation

```bash
dotnet add package PageUS
```

---

## 💻 Backend Example 1 (StoredProcedure call)

````csharp
var pg = new PageUS(page, pageSize);

// Call your SP using Skip and Take
var results = obj.spGetPagedResults(search, pg.Skip, pg.Take);

// If SP returns total record count
pg.TotalCount = results.FirstOrDefault()?.TotalRecords ?? 0;

var viewModel = new MyViewModel
{
    pageUs = pg,
    Applications1 = results.ToPageUSList(pg),
    TotalApplications = pg.TotalCount
};


## 💻 Backend Example 2 (IQueryable call)

```csharp

// Call your SP using Skip and Take
var results = obj.Table.ToPageUSResult(page, pageSize);

var viewModel = new MyViewModel
{
    Applications2 = results,
    TotalApplications = obj.Table.Count();
};

````

---

## 🗃️ Sample Stored Procedure Logic

```sql
SELECT *, COUNT(*) OVER() AS TotalRecords
FROM Applications
WHERE IsActive = 1
ORDER BY CreatedDate DESC
OFFSET @Skip ROWS
FETCH NEXT @Take ROWS ONLY;
```

---

## 🖼 View Example For Stored Procedure (Razor)

```cshtml
@using USPage;
@model MyViewModel
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-ajax-unobtrusive/3.2.6/jquery.unobtrusive-ajax.min.js"></script>

<p>Total Applications: @Model.TotalApplications</p>
<div id = "results">
    <table>
    @foreach (var app in Model.Applications1)
    {
        <tr>
            <td>@app.property1</td>
            <td>@app.property2</td>
        </tr>
    }
    </table>
</div>

@Ajax.AjaxPagination(
    Model.pageUs,
    page => Url.Action("Index", new { page }),
    new AjaxOptions { HttpMethod = "GET", UpdateTargetId = "results" }
)
```

---

## 🖼 View Example For IQuerable (Razor)

```cshtml
@using USPage;
@model MyViewModel
<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery-ajax-unobtrusive/3.2.6/jquery.unobtrusive-ajax.min.js"></script>

<p>Total Applications: @Model.TotalApplications</p>
<div id = "results">
    <table>
    @foreach (var app in Model.Applications2.pageUSList)
    {
        <tr>
            <td>@app.property1</td>
            <td>@app.property2</td>
        </tr>
    }
    </table>
</div>

@Ajax.AjaxPagination(
    Model.Application2,
    page => Url.Action("Index", new { page }),
    new AjaxOptions { HttpMethod = "GET", UpdateTargetId = "results" }
)
```

---

## 📚 API Reference

### `PageUS` Class

| Property      | Description                                    |
| ------------- | ---------------------------------------------- |
| `Skip`        | How many records to skip (for pagination)      |
| `Take`        | PageSize + 1 (for next page detection)         |
| `CurrentPage` | The current page number                        |
| `PageSize`    | Items per page                                 |
| `NextPage`    | `true` if there are more pages                 |
| `TotalCount`  | Optional total records (to enable "last page") |
| `LastPage`    | Computed if `TotalCount` is set                |

### `PageUSResult` Class

| Property     | Description                                                     |
| ------------ | --------------------------------------------------------------- |
| `pageUS`     | Contains pagination metadata like CurrentPage, total count, etc |
| `pageUSList` | A list of data items for the current page                       |

---

## 📄 License

MIT License

---

## 🔗 Links

- 📦 NuGet: [PageUS on NuGet](https://www.nuget.org/packages/PageUS)
- 🗂 GitHub: [https://github.com/Sh-Umang/PageUS](https://github.com/Sh-Umang/PageUS)
