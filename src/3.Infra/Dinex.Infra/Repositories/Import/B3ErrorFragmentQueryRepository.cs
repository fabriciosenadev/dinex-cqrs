using Microsoft.Extensions.Logging;

namespace Dinex.Infra;



public sealed class B3ErrorFragmentQueryRepository : IB3ErrorFragmentQueryRepository
{
    private readonly DinexApiContext _db;
    private readonly ILogger<B3ErrorFragmentQueryRepository> _log;

    public B3ErrorFragmentQueryRepository(DinexApiContext db, ILogger<B3ErrorFragmentQueryRepository> log)
    {
        _db = db; _log = log;
    }

    public async Task<PagedResult<B3ErrorFragmentReadModel>> GetErrorFragmentsByJobAsync(
        Guid importJobId, int page, int pageSize,
        string? search, string? orderBy, bool desc, bool includeRaw)
    {
        if (page < 1) throw new ArgumentException("Page deve ser >= 1.");
        if (pageSize < 1) throw new ArgumentException("PageSize deve ser >= 1.");

        // ✅ SQL canônico: um ÚNICO alias (frag) e nomes explícitos
        var baseSql = @"
            SELECT
              r.""Id""         AS ""RowId"",
              r.""ImportJobId"",
              r.""RowNumber"",
              r.""RawLineJson"",
              r.""CreatedAt"",
              frag.""ErrorIndex"",
              trim(both ' ' from frag.""Value"") AS ""Error""
            FROM public.""B3StatementRows"" r
            CROSS JOIN LATERAL
                regexp_split_to_table(coalesce(r.""Error"", ''), '\s*\|\s*')
                WITH ORDINALITY AS frag(""Value"", ""ErrorIndex"")
            WHERE r.""DeletedAt"" IS NULL
              AND r.""ImportJobId"" = @p_import
              AND coalesce(r.""Error"", '') <> ''";

        // ✅ Usar frag."Value" (antes estava frag.msg)
        bool hasSearch = !string.IsNullOrWhiteSpace(search);
        if (hasSearch)
        {
            if (includeRaw)
                baseSql += " AND (frag.\"Value\" ILIKE @p_search OR r.\"RawLineJson\" ILIKE @p_search)";
            else
                baseSql += " AND frag.\"Value\" ILIKE @p_search";
        }

        var key = (orderBy ?? "RowNumber").Trim().ToLowerInvariant();
        string orderClause = key switch
        {
            "createdat" => $@" ORDER BY ""CreatedAt"" {(desc ? "DESC" : "ASC")}",
            "errorindex" => $@" ORDER BY ""ErrorIndex"" {(desc ? "DESC" : "ASC")}",
            "linenumber" => $@" ORDER BY ""RowNumber"" {(desc ? "DESC" : "ASC")}",
            _ => $@" ORDER BY ""RowNumber"" {(desc ? "DESC" : "ASC")}, ""ErrorIndex"" {(desc ? "DESC" : "ASC")}",
        };

        string pageSql = baseSql + orderClause + " OFFSET @p_offset LIMIT @p_limit";
        string countSql = $@"SELECT COUNT(*) AS ""Value"" FROM ({baseSql}) AS q"; // sem ';'

        var pImport = new Npgsql.NpgsqlParameter("p_import", importJobId);
        var pSearch = new Npgsql.NpgsqlParameter("p_search", hasSearch ? $"%{search!.Trim()}%" : (object)DBNull.Value)
        { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Text };
        var pOffset = new Npgsql.NpgsqlParameter("p_offset", (page - 1) * pageSize);
        var pLimit = new Npgsql.NpgsqlParameter("p_limit", pageSize);

        var total = await _db.Database.SqlQueryRaw<int>(
            countSql, hasSearch ? new object[] { pImport, pSearch } : new object[] { pImport }
        ).SingleAsync();

        var items = await _db.Database.SqlQueryRaw<B3ErrorFragmentReadModel>(
            pageSql, hasSearch ? new object[] { pImport, pSearch, pOffset, pLimit } : new object[] { pImport, pOffset, pLimit }
        ).ToListAsync();

        return new PagedResult<B3ErrorFragmentReadModel>
        {
            Items = items,
            TotalCount = total,
            Page = page,
            PageSize = pageSize
        };
    }

}

