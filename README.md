# PageUS

`PageUS` is a lightweight pagination utility created to simplify paginated data handling in ASP.NET MVC apps — especially when working with **stored procedures** and server-side data sources.

---

## ⚡ Why PageUS?

Other pagination libraries often require fetching **all records** just to compute total pages — which slows down performance for large datasets.

`PageUS` solves this with:

- Efficient calculation of `Skip` and `Take` values
- Compatibility with **stored procedures**
- Optional setting of total records (if returned from DB) for full UI controls
- AJAX-based pagination UI generation with minimal setup

---

## 📦 Installation

```bash
dotnet add package PageUS
```

---

## 🧠 Typical Use Case

> You want to paginate large datasets using a stored procedure that accepts `@Skip` and `@Take` — and return results efficiently **without fetching all records**.

---

## 💻 Backend Example (Controller)

```csharp
public ActionResult Index(string search = "", int page = 1, int pageSize = 10)
{
    var pg = new PageUS(page, pageSize);

    // Call your SP using Skip and Take
    var results = _myService.GetPagedResults(search, pg.Skip, pg.Take);

    // If SP returns total record count
    pg.TotalCount = results.FirstOrDefault()?.TotalRecords ?? 0;

    var viewModel = new MyViewModel
    {
        pageUs = pg,
        Applications = results.ToPageUSList(pg),
        TotalApplications = pg.TotalCount
    };

    return View(viewModel);
}
```

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

## 🖼 View Example (Razor)

```cshtml
@using USPage;
@model MyViewModel

<p>Total Applications: @Model.TotalApplications</p>
<div id = "results">
    <table>
    @foreach (var app in Model.Applications)
    {
        <tr>
            <td>@app.ApplicationNo</td>
            <td>@app.ApplicantName</td>
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

---

## 📄 License

MIT License

---

## 🔗 Links

- 📦 NuGet: [PageUS on NuGet](https://www.nuget.org/packages/PageUS)
- 🗂 GitHub: [https://github.com/Sh-Umang/PageUS](https://github.com/Sh-Umang/PageUS)
